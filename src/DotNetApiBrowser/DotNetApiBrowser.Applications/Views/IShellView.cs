using System.Waf.Applications;

namespace Waf.DotNetApiBrowser.Applications.Views
{
    public interface IShellView : IView
    {
        void Show();

        void Close();
    }
}
