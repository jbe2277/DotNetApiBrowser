using System.ComponentModel.Composition;
using Waf.DotNetApiBrowser.Applications.ViewModels;

namespace Waf.DotNetApiBrowser.Applications.Controllers
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    internal class OpenFromNugetController
    {
        private readonly OpenFromNugetViewModel openFromNugetViewModel;
        private readonly SelectPackageViewModel selectPackageViewModel;
        private readonly SelectAssemblyViewModel selectAssemblyViewModel;

        [ImportingConstructor]
        public OpenFromNugetController(OpenFromNugetViewModel openFromNugetViewModel, SelectPackageViewModel selectPackageViewModel, SelectAssemblyViewModel selectAssemblyViewModel)
        {
            this.openFromNugetViewModel = openFromNugetViewModel;
            this.selectPackageViewModel = selectPackageViewModel;
            this.selectAssemblyViewModel = selectAssemblyViewModel;
        }

        public void Run(object ownerWindow)
        {
            openFromNugetViewModel.ShowDialog(ownerWindow);
        }
    }
}
