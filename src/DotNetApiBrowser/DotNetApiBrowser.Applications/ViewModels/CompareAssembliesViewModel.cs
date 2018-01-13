using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Waf.Applications;
using System.Windows.Input;
using Waf.DotNetApiBrowser.Applications.DataModels;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Applications.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class CompareAssembliesViewModel : ViewModel<ICompareAssembliesView>
    {
        private AssemblyInfo selectedAssembly1;
        private AssemblyInfo selectedAssembly2;
        
        [ImportingConstructor]
        public CompareAssembliesViewModel(ICompareAssembliesView view) : base(view)
        {
        }

        public ICommand CompareCommand { get; set; }

        public IReadOnlyList<AssemblyInfo> AvailableAssemblies { get; set; }
        
        public AssemblyInfo SelectedAssembly1
        {
            get { return selectedAssembly1; }
            set { SetProperty(ref selectedAssembly1, value); }
        }

        public AssemblyInfo SelectedAssembly2
        {
            get { return selectedAssembly2; }
            set { SetProperty(ref selectedAssembly2, value); }
        }
        
        public void ShowDialog(object ownerWindow)
        {
            ViewCore.ShowDialog(ownerWindow);
        }
    }
}
