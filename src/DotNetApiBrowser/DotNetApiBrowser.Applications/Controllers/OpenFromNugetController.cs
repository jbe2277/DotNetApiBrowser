using System.ComponentModel.Composition;
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
    }
}
