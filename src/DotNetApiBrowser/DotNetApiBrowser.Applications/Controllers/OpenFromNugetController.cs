using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Waf.Applications;
using Waf.DotNetApiBrowser.Applications.ViewModels;

namespace Waf.DotNetApiBrowser.Applications.Controllers
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    internal class OpenFromNugetController
    {
        private readonly OpenFromNugetViewModel openFromNugetViewModel;
        private readonly SelectPackageViewModel selectPackageViewModel;
        private readonly SelectAssemblyViewModel selectAssemblyViewModel;
        private readonly DelegateCommand backCommand;
        private readonly DelegateCommand nextCommand;

        [ImportingConstructor]
        public OpenFromNugetController(OpenFromNugetViewModel openFromNugetViewModel, SelectPackageViewModel selectPackageViewModel, SelectAssemblyViewModel selectAssemblyViewModel)
        {
            this.openFromNugetViewModel = openFromNugetViewModel;
            this.selectPackageViewModel = selectPackageViewModel;
            this.selectAssemblyViewModel = selectAssemblyViewModel;
            selectPackageViewModel.PropertyChanged += SelectPackageViewModelPropertyChanged;
            backCommand = new DelegateCommand(Back, CanBack);
            nextCommand = new DelegateCommand(Next, CanNext);
        }

        public void Run(object ownerWindow)
        {
            openFromNugetViewModel.BackCommand = backCommand;
            openFromNugetViewModel.NextCommand = nextCommand;
            openFromNugetViewModel.ContentView = selectPackageViewModel.View;
            openFromNugetViewModel.ShowDialog(ownerWindow);
        }

        private bool CanBack()
        {
            return true;
        }

        private void Back()
        {
        }

        private bool CanNext() { return true; }

        private void Next()
        {
            openFromNugetViewModel.Close();
        }

        private async void SelectPackageViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (new[] { nameof(selectPackageViewModel.SearchText), nameof(selectPackageViewModel.IncludePrerelease) }.Contains(e.PropertyName))
            {
                selectPackageViewModel.NugetPackages = await GetNugetPackages(selectPackageViewModel.SearchText, selectPackageViewModel.IncludePrerelease);
                selectPackageViewModel.SelectedNugetPackage = selectPackageViewModel.NugetPackages.FirstOrDefault();
                // TODO: error handling
            }
            else if (e.PropertyName == nameof(selectPackageViewModel.SelectedNugetPackage))
            {
                selectPackageViewModel.PackageVersions = Array.Empty<VersionInfo>();
                selectPackageViewModel.SelectedPackageVersion = null;
                selectPackageViewModel.PackageVersions = await GetVersionInfos(selectPackageViewModel.SelectedNugetPackage);
                selectPackageViewModel.SelectedPackageVersion = selectPackageViewModel.PackageVersions.FirstOrDefault();
                // TODO: error handling
            }
        }

        private async Task<IReadOnlyList<IPackageSearchMetadata>> GetNugetPackages(string searchText, bool includePrerelease)
        {
            if (string.IsNullOrEmpty(searchText)) return Array.Empty<IPackageSearchMetadata>();
            var packageSource = new PackageSource("https://api.nuget.org/v3/index.json");
            var sourceRepository = new SourceRepository(packageSource, Repository.Provider.GetCoreV3());

            var searchResource = await sourceRepository.GetResourceAsync<PackageSearchResource>().ConfigureAwait(false);
            return (await searchResource.SearchAsync(searchText, new SearchFilter(includePrerelease), 0, 10, new Logger(), CancellationToken.None).ConfigureAwait(false)).ToArray();
        }

        private async Task<IReadOnlyList<VersionInfo>> GetVersionInfos(IPackageSearchMetadata packageSearchMetadata)
        {
            if (packageSearchMetadata == null) return Array.Empty<VersionInfo>();
            return (await packageSearchMetadata.GetVersionsAsync().ConfigureAwait(false)).Reverse().ToArray();
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
