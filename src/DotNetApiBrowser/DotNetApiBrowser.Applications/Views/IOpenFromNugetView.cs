using System.Waf.Applications;

namespace Waf.DotNetApiBrowser.Applications.Views
{
    public interface IOpenFromNugetView : IView
    {
        void ShowDialog(object ownerWindow);
    }
}
