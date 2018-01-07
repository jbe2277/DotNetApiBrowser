using System.ComponentModel.Composition;
using System.Waf.Applications;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Applications.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class CompareAssembliesViewModel : ViewModel<ICompareAssembliesView>
    {
        [ImportingConstructor]
        public CompareAssembliesViewModel(ICompareAssembliesView view) : base(view)
        {
        }

        public void ShowDialog(object ownerWindow)
        {
            ViewCore.ShowDialog(ownerWindow);
        }
    }
}
