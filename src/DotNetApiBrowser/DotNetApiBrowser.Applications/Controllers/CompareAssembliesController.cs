using System.ComponentModel.Composition;
using Waf.DotNetApiBrowser.Applications.ViewModels;

namespace Waf.DotNetApiBrowser.Applications.Controllers
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    internal class CompareAssembliesController
    {
        private readonly CompareAssembliesViewModel compareAssembliesViewModel;

        [ImportingConstructor]
        public CompareAssembliesController(CompareAssembliesViewModel compareAssembliesViewModel)
        {
            this.compareAssembliesViewModel = compareAssembliesViewModel;
        }

        public void Run(object ownerWindow)
        {
            compareAssembliesViewModel.ShowDialog(ownerWindow);
        }
    }
}
