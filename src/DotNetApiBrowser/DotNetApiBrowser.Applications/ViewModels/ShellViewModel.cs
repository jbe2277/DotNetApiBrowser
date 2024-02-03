using System.ComponentModel.Composition;
using System.Waf.Applications;
using System.Windows.Input;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Applications.ViewModels;

[Export]
public class ShellViewModel : ViewModel<IShellView>
{
    private readonly List<ApplicationBusyContext> applicationBusyContext;
    private CodeEditorViewModel selectedCodeEditorViewModel;
    private bool isApplicationBusy;

    [ImportingConstructor]
    public ShellViewModel(IShellView view) : base(view)
    {
        applicationBusyContext = [];
        ExitCommand = new DelegateCommand(Close);
    }

    public static string Title => ApplicationInfo.ProductName;

    public ICommand ExitCommand { get; }

    public ICommand OpenFileCommand { get; set; }

    public ICommand OpenFromNugetCommand { get; set; }

    public ICommand CompareAssembliesCommand { get; set; }

    public ICommand CloseAssemblyApiCommand { get; set; }

    public ICommand InfoCommand { get; set; }

    public IReadOnlyList<CodeEditorViewModel> CodeEditorViewModels { get; set; }

    public CodeEditorViewModel SelectedCodeEditorViewModel
    {
        get => selectedCodeEditorViewModel;
        set => SetProperty(ref selectedCodeEditorViewModel, value);
    }
    
    public bool IsApplicationBusy
    {
        get => isApplicationBusy;
        private set => SetProperty(ref isApplicationBusy, value);
    }
    
    public void Show()
    {
        ViewCore.Show();
    }

    public IDisposable SetApplicationBusy()
    {
        var context = new ApplicationBusyContext(ApplicationBusyContextDisposeCallback);
        applicationBusyContext.Add(context);
        IsApplicationBusy = true;
        return context;
    }

    private void ApplicationBusyContextDisposeCallback(ApplicationBusyContext context)
    {
        applicationBusyContext.Remove(context);
        IsApplicationBusy = applicationBusyContext.Any();
    }

    private void Close()
    {
        ViewCore.Close();
    }


    private sealed class ApplicationBusyContext(Action<ApplicationBusyContext> disposeCallback) : IDisposable
    {
        public void Dispose()
        {
            disposeCallback(this);
        }
    }
}
