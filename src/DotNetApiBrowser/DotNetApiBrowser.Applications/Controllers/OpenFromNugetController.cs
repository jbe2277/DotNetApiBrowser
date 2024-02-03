using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO.Compression;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using Waf.DotNetApiBrowser.Applications.ViewModels;

namespace Waf.DotNetApiBrowser.Applications.Controllers;

[Export, PartCreationPolicy(CreationPolicy.NonShared)]
internal class OpenFromNugetController
{
    private readonly IMessageService messageService;
    private readonly OpenFromNugetViewModel openFromNugetViewModel;
    private readonly SelectPackageViewModel selectPackageViewModel;
    private readonly SelectAssemblyViewModel selectAssemblyViewModel;
    private readonly DelegateCommand backCommand;
    private readonly DelegateCommand nextCommand;
    private PackageSearchResource? searchResource;
    private (string? fileName, Stream? assemblyStream) result;
    private CancellationTokenSource? getNugetPackagesCancellation;
    private CancellationTokenSource? downloadNugetPackageCancellation;
    private bool updateSelectAssemblyView;

    [ImportingConstructor]
    public OpenFromNugetController(IMessageService messageService, OpenFromNugetViewModel openFromNugetViewModel, SelectPackageViewModel selectPackageViewModel, SelectAssemblyViewModel selectAssemblyViewModel)
    {
        this.messageService = messageService;
        this.openFromNugetViewModel = openFromNugetViewModel;
        this.selectPackageViewModel = selectPackageViewModel;
        this.selectAssemblyViewModel = selectAssemblyViewModel;
        backCommand = new(Back, CanBack);
        nextCommand = new(Next, CanNext);
        openFromNugetViewModel.PropertyChanged += OpenFromNugetViewModelPropertyChanged;
        selectPackageViewModel.PropertyChanged += SelectPackageViewModelPropertyChanged;
        selectAssemblyViewModel.PropertyChanged += SelectAssemblyViewModelPropertyChanged;
    }

    public async Task<(string? fileName, Stream? assemblyStream)> RunAsync(object ownerWindow)
    {
        try
        {
            openFromNugetViewModel.BackCommand = backCommand;
            openFromNugetViewModel.NextCommand = nextCommand;
            ShowSelectPackageView();
            await openFromNugetViewModel.ShowDialogAsync(ownerWindow);
            return result;
        }
        finally
        {
            selectAssemblyViewModel.Assemblies?.FirstOrDefault()?.Archive?.Dispose();
            getNugetPackagesCancellation?.Cancel();
            getNugetPackagesCancellation?.Dispose();
            downloadNugetPackageCancellation?.Cancel();
            downloadNugetPackageCancellation?.Dispose();
        }            
    }

    private bool CanBack() => openFromNugetViewModel.ContentView == selectAssemblyViewModel.View;

    private void Back() => ShowSelectPackageView();

    private bool CanNext() => openFromNugetViewModel.ContentView == selectPackageViewModel.View && selectPackageViewModel.SelectedPackageVersion != null
        || openFromNugetViewModel.ContentView == selectAssemblyViewModel.View && selectAssemblyViewModel.SelectedAssembly != null;

    private void Next()
    {
        if (openFromNugetViewModel.ContentView == selectPackageViewModel.View)
        {
            ShowSelectAssemblyView();
        }
        else if (openFromNugetViewModel.ContentView == selectAssemblyViewModel.View)
        {
            Finish();
        }
    }

    private void ShowSelectPackageView() => openFromNugetViewModel.ContentView = selectPackageViewModel.View;

    private async void ShowSelectAssemblyView()
    {
        openFromNugetViewModel.ContentView = selectAssemblyViewModel.View;
        if (updateSelectAssemblyView)
        {
            selectAssemblyViewModel.Assemblies?.FirstOrDefault()?.Archive?.Dispose();
            selectAssemblyViewModel.Assemblies = [];
            selectAssemblyViewModel.SelectedAssembly = null;
            try
            {
                downloadNugetPackageCancellation?.Cancel();
                downloadNugetPackageCancellation = new CancellationTokenSource();
                var nugetPackage = await DownloadNugetPackage(selectPackageViewModel.SelectedNugetPackage!.Identity.Id, selectPackageViewModel.SelectedPackageVersion!.Version.ToString(), 
                    downloadNugetPackageCancellation.Token);
                selectAssemblyViewModel.Assemblies = nugetPackage.Entries.Where(x => Path.GetExtension(x.Name) is ".dll" or ".exe").ToArray();
            }
            catch (Exception ex)
            {
                selectAssemblyViewModel.Assemblies = [];
                ShowError("Error: " + ex.Message);
            }
            selectAssemblyViewModel.SelectedAssembly = selectAssemblyViewModel.Assemblies.FirstOrDefault();
            updateSelectAssemblyView = false;
        }
    }

    private async void Finish()
    {
        openFromNugetViewModel.IsClosing = true;
        result = (selectAssemblyViewModel.SelectedAssembly!.Name, new MemoryStream());
        try
        {
            await selectAssemblyViewModel.SelectedAssembly.Open().CopyToAsync(result.assemblyStream);
            result.assemblyStream.Position = 0;
        }
        catch (Exception ex)
        {
            result = (null, null);
            ShowError("Error: " + ex.Message);
        }
        openFromNugetViewModel.Close();
    }

    private void ShowError(string message)
    {
        if (openFromNugetViewModel.IsVisible) messageService.ShowError(openFromNugetViewModel.View, message);
    }

    private void OpenFromNugetViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(openFromNugetViewModel.ContentView)) DelegateCommand.RaiseCanExecuteChanged(backCommand, nextCommand);
    }

    private async void SelectPackageViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(selectPackageViewModel.SearchText) or nameof(selectPackageViewModel.IncludePrerelease))
        {
            selectPackageViewModel.NugetPackages = [];
            selectPackageViewModel.SelectedNugetPackage = null;
            try
            {
                getNugetPackagesCancellation?.Cancel();
                getNugetPackagesCancellation = new CancellationTokenSource();
                selectPackageViewModel.NugetPackages = await GetNugetPackages(selectPackageViewModel.SearchText, selectPackageViewModel.IncludePrerelease, getNugetPackagesCancellation.Token);
            }
            catch (Exception ex)
            {
                selectPackageViewModel.NugetPackages = [];
                ShowError("Error: " + ex.Message);
            }
            selectPackageViewModel.SelectedNugetPackage = selectPackageViewModel.NugetPackages.FirstOrDefault();
        }
        else if (e.PropertyName == nameof(selectPackageViewModel.SelectedNugetPackage))
        {
            selectPackageViewModel.PackageVersions = [];
            selectPackageViewModel.SelectedPackageVersion = null;
            try
            {
                selectPackageViewModel.PackageVersions = await GetVersionInfos(selectPackageViewModel.SelectedNugetPackage!);
            }
            catch (Exception ex)
            {
                selectPackageViewModel.PackageVersions = [];
                ShowError("Error: " + ex.Message);
            }
            selectPackageViewModel.SelectedPackageVersion = selectPackageViewModel.PackageVersions.FirstOrDefault();
        }
        else if (e.PropertyName == nameof(selectPackageViewModel.SelectedPackageVersion))
        {
            updateSelectAssemblyView = true;
            nextCommand.RaiseCanExecuteChanged();
        }
    }

    private void SelectAssemblyViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(selectAssemblyViewModel.SelectedAssembly)) nextCommand.RaiseCanExecuteChanged();
    }

    private async Task<PackageSearchResource> GetPackageSearchResource()
    {
        return searchResource ??= await new SourceRepository(new PackageSource("https://api.nuget.org/v3/index.json"), Repository.Provider.GetCoreV3())
            .GetResourceAsync<PackageSearchResource>();
    }

    private async Task<IReadOnlyList<IPackageSearchMetadata>> GetNugetPackages(string? searchText, bool includePrerelease, CancellationToken cancellation)
    {
        if (string.IsNullOrEmpty(searchText)) return [];
        var resource = await GetPackageSearchResource().ConfigureAwait(false);
        return (await resource.SearchAsync(searchText, new SearchFilter(includePrerelease), 0, 50, new Logger(), cancellation).ConfigureAwait(false)).ToArray();
    }

    private static async Task<IReadOnlyList<VersionInfo>> GetVersionInfos(IPackageSearchMetadata packageSearchMetadata)
    {
        if (packageSearchMetadata == null) return [];
        return (await packageSearchMetadata.GetVersionsAsync().ConfigureAwait(false)).Reverse().ToArray();
    }

    private static async Task<ZipArchive> DownloadNugetPackage(string packageId, string version, CancellationToken cancellation)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync(new Uri($"https://www.nuget.org/api/v2/package/{packageId}/{version}"), cancellation).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var packageStream = await response.Content.ReadAsStreamAsync(cancellation).ConfigureAwait(false);
        return new ZipArchive(packageStream, ZipArchiveMode.Read, leaveOpen: false);
    }


    private sealed class Logger : LoggerBase
    {
        public override void Log(ILogMessage message) => Trace.WriteLine($"{message.WarningLevel} {message.Code} {message.Message}");

        public override Task LogAsync(ILogMessage message) { Log(message); return Task.CompletedTask; }
    }
}
