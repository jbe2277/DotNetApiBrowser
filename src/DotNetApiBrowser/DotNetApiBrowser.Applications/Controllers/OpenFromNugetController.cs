using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using Waf.DotNetApiBrowser.Applications.ViewModels;

namespace Waf.DotNetApiBrowser.Applications.Controllers
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    internal class OpenFromNugetController
    {
        private readonly IMessageService messageService;
        private readonly OpenFromNugetViewModel openFromNugetViewModel;
        private readonly SelectPackageViewModel selectPackageViewModel;
        private readonly SelectAssemblyViewModel selectAssemblyViewModel;
        private readonly Lazy<Task<PackageSearchResource>> searchResource;
        private readonly DelegateCommand backCommand;
        private readonly DelegateCommand nextCommand;
        private (string fileName, Stream assemblyStream) result;
        private bool updateSelectAssemblyView;

        [ImportingConstructor]
        public OpenFromNugetController(IMessageService messageService, OpenFromNugetViewModel openFromNugetViewModel, SelectPackageViewModel selectPackageViewModel, SelectAssemblyViewModel selectAssemblyViewModel)
        {
            this.messageService = messageService;
            this.openFromNugetViewModel = openFromNugetViewModel;
            this.selectPackageViewModel = selectPackageViewModel;
            this.selectAssemblyViewModel = selectAssemblyViewModel;
            selectPackageViewModel.NugetPackages = Array.Empty<IPackageSearchMetadata>();
            selectPackageViewModel.PackageVersions = Array.Empty<VersionInfo>();
            openFromNugetViewModel.PropertyChanged += OpenFromNugetViewModelPropertyChanged;
            selectPackageViewModel.PropertyChanged += SelectPackageViewModelPropertyChanged;
            selectAssemblyViewModel.PropertyChanged += SelectAssemblyViewModelPropertyChanged;
            searchResource = new Lazy<Task<PackageSearchResource>>(() => 
                new SourceRepository(new PackageSource("https://api.nuget.org/v3/index.json"), Repository.Provider.GetCoreV3()).GetResourceAsync<PackageSearchResource>());
            backCommand = new DelegateCommand(Back, CanBack);
            nextCommand = new DelegateCommand(Next, CanNext);
        }

        public async Task<(string fileName, Stream assemblyStream)> RunAsync(object ownerWindow)
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
                selectAssemblyViewModel.Assemblies?.FirstOrDefault()?.Archive.Dispose();
                if (searchResource.IsValueCreated) searchResource.Value.Dispose();
            }            
        }

        private bool CanBack() => openFromNugetViewModel.ContentView == selectAssemblyViewModel.View;

        private void Back()
        {
            ShowSelectPackageView();
        }

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

        private void ShowSelectPackageView()
        {
            openFromNugetViewModel.ContentView = selectPackageViewModel.View;
        }

        private async void ShowSelectAssemblyView()
        {
            openFromNugetViewModel.ContentView = selectAssemblyViewModel.View;
            if (updateSelectAssemblyView)
            {
                selectAssemblyViewModel.Assemblies?.FirstOrDefault()?.Archive.Dispose();
                selectAssemblyViewModel.Assemblies = null;
                selectAssemblyViewModel.SelectedAssembly = null;
                try
                {
                    var nugetPackage = await DownloadNugetPackage(selectPackageViewModel.SelectedNugetPackage.Identity.Id, selectPackageViewModel.SelectedPackageVersion.Version.ToString());
                    selectAssemblyViewModel.Assemblies = nugetPackage.Entries.Where(x => new[] { ".dll", ".exe" }.Contains(Path.GetExtension(x.Name))).ToArray();
                }
                catch (Exception ex)
                {
                    selectAssemblyViewModel.Assemblies = Array.Empty<ZipArchiveEntry>();
                    ShowError("Error: " + ex.Message);
                }
                selectAssemblyViewModel.SelectedAssembly = selectAssemblyViewModel.Assemblies.FirstOrDefault();
                updateSelectAssemblyView = false;
            }
        }

        private async void Finish()
        {
            result = (selectAssemblyViewModel.SelectedAssembly.Name, new MemoryStream());
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

        private void OpenFromNugetViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(openFromNugetViewModel.ContentView))
            {
                backCommand.RaiseCanExecuteChanged();
                nextCommand.RaiseCanExecuteChanged();
            }
        }

        private async void SelectPackageViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (new[] { nameof(selectPackageViewModel.SearchText), nameof(selectPackageViewModel.IncludePrerelease) }.Contains(e.PropertyName))
            {
                selectPackageViewModel.NugetPackages = null;
                selectPackageViewModel.SelectedNugetPackage = null;
                try
                {
                    selectPackageViewModel.NugetPackages = await GetNugetPackages(selectPackageViewModel.SearchText, selectPackageViewModel.IncludePrerelease);
                }
                catch (Exception ex)
                {
                    selectPackageViewModel.NugetPackages = Array.Empty<IPackageSearchMetadata>();
                    ShowError("Error: " + ex.Message);
                }
                selectPackageViewModel.SelectedNugetPackage = selectPackageViewModel.NugetPackages.FirstOrDefault();
            }
            else if (e.PropertyName == nameof(selectPackageViewModel.SelectedNugetPackage))
            {
                selectPackageViewModel.PackageVersions = null;
                selectPackageViewModel.SelectedPackageVersion = null;
                try
                {
                    selectPackageViewModel.PackageVersions = await GetVersionInfos(selectPackageViewModel.SelectedNugetPackage);
                }
                catch (Exception ex)
                {
                    selectPackageViewModel.PackageVersions = Array.Empty<VersionInfo>();
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

        private void SelectAssemblyViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(selectAssemblyViewModel.SelectedAssembly))
            {
                nextCommand.RaiseCanExecuteChanged();
            }
        }

        private async Task<IReadOnlyList<IPackageSearchMetadata>> GetNugetPackages(string searchText, bool includePrerelease)
        {
            if (string.IsNullOrEmpty(searchText)) return Array.Empty<IPackageSearchMetadata>();
            var resource = await searchResource.Value.ConfigureAwait(false);
            return (await resource.SearchAsync(searchText, new SearchFilter(includePrerelease), 0, 50, new Logger(), CancellationToken.None).ConfigureAwait(false)).ToArray();
        }

        private async Task<IReadOnlyList<VersionInfo>> GetVersionInfos(IPackageSearchMetadata packageSearchMetadata)
        {
            if (packageSearchMetadata == null) return Array.Empty<VersionInfo>();
            return (await packageSearchMetadata.GetVersionsAsync().ConfigureAwait(false)).Reverse().ToArray();
        }

        private async Task<ZipArchive> DownloadNugetPackage(string packageId, string version)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"https://www.nuget.org/api/v2/package/{packageId}/{version}").ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                var packageStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                return new ZipArchive(packageStream, ZipArchiveMode.Read, leaveOpen: false);
            }
        }


        private sealed class Logger : ILogger
        {
            public void LogDebug(string data) => Trace.WriteLine($"DEBUG: {data}");
            public void LogVerbose(string data) => Trace.WriteLine($"VERBOSE: {data}");
            public void LogInformation(string data) => Trace.WriteLine($"INFORMATION: {data}");
            public void LogMinimal(string data) => Trace.WriteLine($"MINIMAL: {data}");
            public void LogWarning(string data) => Trace.WriteLine($"WARNING: {data}");
            public void LogError(string data) => Trace.WriteLine($"ERROR: {data}");
            public void LogInformationSummary(string data) => Trace.WriteLine($"INFO SUMMARY: {data}");
            public void LogErrorSummary(string data) => Trace.WriteLine($"ERROR SUMMARY: {data}");
        }
    }
}
