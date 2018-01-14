using System.Waf.Applications;

namespace Waf.DotNetApiBrowser.Applications.Views
{
    public interface ICompareAssembliesView : IView
    {
        void ShowDialog(object ownerWindow);

        void Close();
    }
}
