using System.ComponentModel.Composition;
using System.Waf.Applications;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Applications.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class SelectAssemblyViewModel : ViewModel<ISelectAssemblyView>
    {
        [ImportingConstructor]
        public SelectAssemblyViewModel(ISelectAssemblyView view) : base(view)
        {
        }
    }
}
