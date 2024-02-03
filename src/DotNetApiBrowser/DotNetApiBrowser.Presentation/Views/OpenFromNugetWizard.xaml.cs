using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Threading;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Presentation.Views;

[Export(typeof(IOpenFromNugetView)), PartCreationPolicy(CreationPolicy.NonShared)]
public partial class OpenFromNugetWizard : IOpenFromNugetView
{
    private TaskCompletionSource<object> showDialogCompletionSource;

    public OpenFromNugetWizard()
    {
        InitializeComponent();
        Closed += ClosedHandler;
    }
    
    public Task ShowDialogAsync(object ownerWindow)
    {
        if (showDialogCompletionSource != null) throw new InvalidOperationException("The dialog is already shown.");
        showDialogCompletionSource = new TaskCompletionSource<object>();
        Dispatcher.BeginInvoke(DispatcherPriority.Send, (Action)(() =>
        {
            Owner = ownerWindow as Window;
            ShowDialog();
        }));
        return showDialogCompletionSource.Task;
    }

    private void ClosedHandler(object sender, EventArgs e)
    {
        showDialogCompletionSource.SetResult(null);
        showDialogCompletionSource = null;
    }
}
