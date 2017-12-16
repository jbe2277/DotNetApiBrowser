using System.ComponentModel.Composition;
using System.Waf.Applications;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Applications.ViewModels
{
    [Export]
    public class ShellViewModel : ViewModel<IShellView>
    {
        [ImportingConstructor]
        public ShellViewModel(IShellView view) : base(view)
        {

        }

        public void Show()
        {
            ViewCore.Show();
        }
    }
}
