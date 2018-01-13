using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Waf.DotNetApiBrowser.Applications.DataModels;
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

        public void Run(object ownerWindow, IReadOnlyList<AssemblyInfo> availableAssemblies)
        {
            compareAssembliesViewModel.AvailableAssemblies = availableAssemblies;
            compareAssembliesViewModel.SelectedAssembly1 = availableAssemblies.FirstOrDefault();
            compareAssembliesViewModel.SelectedAssembly2 = availableAssemblies.Skip(1).FirstOrDefault();
            compareAssembliesViewModel.ShowDialog(ownerWindow);
        }
    }
}
