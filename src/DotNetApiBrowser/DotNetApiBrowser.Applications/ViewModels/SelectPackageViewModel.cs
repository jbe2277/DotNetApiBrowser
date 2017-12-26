using NuGet.Protocol.Core.Types;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Waf.Applications;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Applications.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class SelectPackageViewModel : ViewModel<ISelectPackageView>
    {
        private string searchText;
        private bool includePrerelease;
        private IReadOnlyList<IPackageSearchMetadata> nugetPackages;
        private IPackageSearchMetadata selectedNugetPackage;
        private IReadOnlyList<VersionInfo> packageVersions;
        private VersionInfo selectedPackageVersion;

        [ImportingConstructor]
        public SelectPackageViewModel(ISelectPackageView view) : base(view)
        {
        }
        
        public string SearchText
        {
            get { return searchText; }
            set { SetProperty(ref searchText, value); }
        }
        
        public bool IncludePrerelease
        {
            get { return includePrerelease; }
            set { SetProperty(ref includePrerelease, value); }
        }
        
        public IReadOnlyList<IPackageSearchMetadata> NugetPackages
        {
            get { return nugetPackages; }
            set { SetProperty(ref nugetPackages, value); }
        }
        
        public IPackageSearchMetadata SelectedNugetPackage
        {
            get { return selectedNugetPackage; }
            set { SetProperty(ref selectedNugetPackage, value); }
        }
        
        public IReadOnlyList<VersionInfo> PackageVersions
        {
            get { return packageVersions; }
            set { SetProperty(ref packageVersions, value); }
        }

        public VersionInfo SelectedPackageVersion
        {
            get { return selectedPackageVersion; }
            set { SetProperty(ref selectedPackageVersion, value); }
        }
    }
}
