using System;
using System.ComponentModel.Composition;
using System.Waf.Applications;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Applications.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class OpenFromNugetViewModel : ViewModel<IOpenFromNugetView>
    {
        [ImportingConstructor]
        public OpenFromNugetViewModel(IOpenFromNugetView view) : base(view)
        {
        }

        public void ShowDialog(object ownerWindow)
        {
            ViewCore.ShowDialog(ownerWindow);
        }
    }
}
