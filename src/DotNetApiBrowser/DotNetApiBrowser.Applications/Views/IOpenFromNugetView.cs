using System.Threading.Tasks;
using System.Waf.Applications;

namespace Waf.DotNetApiBrowser.Applications.Views
{
    public interface IOpenFromNugetView : IView
    {
        Task ShowDialogAsync(object ownerWindow);
        void Close();
    }
}
