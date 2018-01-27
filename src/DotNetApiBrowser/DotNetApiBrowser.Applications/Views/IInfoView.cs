using System.Waf.Applications;

namespace Waf.DotNetApiBrowser.Applications.Views
{
    public interface IInfoView : IView
    {
        void ShowDialog(object owner);
    }
}
