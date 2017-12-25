using System.ComponentModel.Composition;
using System.Waf.Applications;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Applications.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class SelectPackageViewModel : ViewModel<ISelectPackageView>
    {
        [ImportingConstructor]
        public SelectPackageViewModel(ISelectPackageView view) : base(view)
        {
        }
    }
}
