using System.ComponentModel.Composition;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using Waf.CodeAnalysis.AssemblyReaders;
using Waf.DotNetApiBrowser.Applications.DataModels;
using Waf.DotNetApiBrowser.Applications.ViewModels;

namespace Waf.DotNetApiBrowser.Applications.Controllers;

[Export(typeof(IModuleController))]
internal class ModuleController : IModuleController
{
    private readonly IMessageService messageService;
    private readonly IFileDialogService fileDialogService;
    private readonly ExportFactory<OpenFromNugetController> openFromNugetController;
    private readonly ExportFactory<CompareAssembliesController> compareAssembliesController;
    private readonly Lazy<ShellViewModel> shellViewModel;
    private readonly ExportFactory<CodeEditorViewModel> codeEditorViewModel;
    private readonly ExportFactory<InfoViewModel> infoViewModel;
    private readonly DelegateCommand openFileCommand;
    private readonly AsyncDelegateCommand openFromNugetCommand;
    private readonly DelegateCommand compareAssembliesCommand;
    private readonly DelegateCommand closeAssemblyApiCommand;
    private readonly DelegateCommand infoCommand;
    private readonly ObservableCollection<CodeEditorViewModel> codeEditorViewModels;

    [ImportingConstructor]
    public ModuleController(IMessageService messageService, IFileDialogService fileDialogService, ExportFactory<OpenFromNugetController> openFromNugetController,
        ExportFactory<CompareAssembliesController> compareAssembliesController, Lazy<ShellViewModel> shellViewModel, ExportFactory<CodeEditorViewModel> codeEditorViewModel,
        ExportFactory<InfoViewModel> infoViewModel)
    {
        this.messageService = messageService;
        this.fileDialogService = fileDialogService;
        this.openFromNugetController = openFromNugetController;
        this.compareAssembliesController = compareAssembliesController;
        this.shellViewModel = shellViewModel;
        this.codeEditorViewModel = codeEditorViewModel;
        this.infoViewModel = infoViewModel;
        openFileCommand = new DelegateCommand(OpenFile);
        openFromNugetCommand = new AsyncDelegateCommand(OpenFromNuget);
        compareAssembliesCommand = new DelegateCommand(CompareAssemblies);
        closeAssemblyApiCommand = new DelegateCommand(CloseAssemblyApi);
        infoCommand = new DelegateCommand(ShowInfo);
        codeEditorViewModels = new ObservableCollection<CodeEditorViewModel>();
    }

    private ShellViewModel ShellViewModel => shellViewModel.Value;

    public void Initialize()
    {
    }

    public void Run()
    {
        ShellViewModel.OpenFileCommand = openFileCommand;
        ShellViewModel.OpenFromNugetCommand = openFromNugetCommand;
        ShellViewModel.CompareAssembliesCommand = compareAssembliesCommand;
        ShellViewModel.CloseAssemblyApiCommand = closeAssemblyApiCommand;
        ShellViewModel.InfoCommand = infoCommand;
        ShellViewModel.CodeEditorViewModels = codeEditorViewModels;
        ShellViewModel.Show();
    }

    public void Shutdown()
    {
    }

    private async void OpenFile()
    {
        var assemblyFileType = new FileType(".NET Assembly (*.dll, *.exe)", ".dll;*.exe");
        var allFileType = new FileType("All files (*.*)", ".*");
        var result = fileDialogService.ShowOpenFileDialog(ShellViewModel.View, new[] { assemblyFileType, allFileType });
        if (!result.IsValid) return;

        try
        {
            using (ShellViewModel.SetApplicationBusy())
            {
                var assemblyApi = await Task.Run(() => AssemblyReader.Read(result.FileName));
                AddAndSelectAssemblyApi(result.FileName, assemblyApi.version, assemblyApi.api);
            }
        }
        catch (Exception e)
        {
            messageService.ShowError(ShellViewModel.View, "Could not read the file. Error: " + e);
        }
    }

    private async Task OpenFromNuget()
    {
        using (var controller = openFromNugetController.CreateExport())
        {
            var result = await controller.Value.RunAsync(ShellViewModel.View);
            if (result.assemblyStream == null) return;
            using (ShellViewModel.SetApplicationBusy())
            {
                var assemblyApi = await Task.Run(() => AssemblyReader.Read(result.assemblyStream));
                AddAndSelectAssemblyApi(result.fileName, assemblyApi.version, assemblyApi.api);
            }
        }
    }

    private void CompareAssemblies()
    {
        using (var controller = compareAssembliesController.CreateExport())
        {
            controller.Value.Run(ShellViewModel.View, codeEditorViewModels.Select(x => x.AssemblyInfo).ToArray());
        }
    }

    private void CloseAssemblyApi()
    {
        codeEditorViewModels.Remove(ShellViewModel.SelectedCodeEditorViewModel);
    }

    private void AddAndSelectAssemblyApi(string fileName, Version assemblyVersion, string assemblyApi)
    {
        var viewModel = codeEditorViewModel.CreateExport().Value;
        viewModel.AssemblyInfo = new AssemblyInfo(fileName, Path.GetFileNameWithoutExtension(fileName), assemblyVersion, assemblyApi);
        codeEditorViewModels.Add(viewModel);
        ShellViewModel.SelectedCodeEditorViewModel = viewModel;
    }

    private void ShowInfo()
    {
        var viewModel = infoViewModel.CreateExport().Value;
        viewModel.ShowDialog(ShellViewModel.View);
    }
}
