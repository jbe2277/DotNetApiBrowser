using System.ComponentModel.Composition;
using System.Waf.Applications;
using System.Windows.Input;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Applications.ViewModels
{
    [Export]
    public class ShellViewModel : ViewModel<IShellView>
    {
        [ImportingConstructor]
        public ShellViewModel(IShellView view) : base(view)
        {
            ExitCommand = new DelegateCommand(Close);
        }

        public ICommand ExitCommand { get; }

        public ICommand OpenCommand { get; set; }

        public void Show()
        {
            ViewCore.Show();
        }

        private void Close()
        {
            ViewCore.Close();
        }

        public void SetCode(string code)
        {
            ViewCore.SetCode(code);
        }
    }
}
