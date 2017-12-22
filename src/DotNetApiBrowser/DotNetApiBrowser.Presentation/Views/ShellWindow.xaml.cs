using System.ComponentModel.Composition;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Presentation.Views
{
    [Export(typeof(IShellView))]
    public partial class ShellWindow : IShellView
    {
        public ShellWindow()
        {
            InitializeComponent();
        }

        public void SetCode(string code)
        {
            codeEditor.Text = code;
        }
    }
}
