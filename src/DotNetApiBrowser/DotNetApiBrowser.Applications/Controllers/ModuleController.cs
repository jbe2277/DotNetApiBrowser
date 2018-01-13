using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using Waf.CodeAnalysis.AssemblyReaders;
using Waf.DotNetApiBrowser.Applications.DataModels;
using Waf.DotNetApiBrowser.Applications.ViewModels;

namespace Waf.DotNetApiBrowser.Applications.Controllers
{
    [Export(typeof(IModuleController))]
    internal class ModuleController : IModuleController
    {
        private readonly IMessageService messageService;
        private readonly IFileDialogService fileDialogService;
        private readonly ExportFactory<OpenFromNugetController> openFromNugetController;
        private readonly ExportFactory<CompareAssembliesController> compareAssembliesController;
        private readonly Lazy<ShellViewModel> shellViewModel;
        private readonly ExportFactory<CodeEditorViewModel> codeEditorViewModel;
        private readonly AsyncDelegateCommand openFileCommand;
        private readonly AsyncDelegateCommand openFromNugetCommand;
        private readonly DelegateCommand compareAssembliesCommand;
        private readonly DelegateCommand closeAssemblyApiCommand;
        private readonly ObservableCollection<CodeEditorViewModel> codeEditorViewModels;

        [ImportingConstructor]
        public ModuleController(IMessageService messageService, IFileDialogService fileDialogService, ExportFactory<OpenFromNugetController> openFromNugetController,
            ExportFactory<CompareAssembliesController> compareAssembliesController, Lazy<ShellViewModel> shellViewModel, ExportFactory<CodeEditorViewModel> codeEditorViewModel)
        {
            this.messageService = messageService;
            this.fileDialogService = fileDialogService;
            this.openFromNugetController = openFromNugetController;
            this.compareAssembliesController = compareAssembliesController;
            this.shellViewModel = shellViewModel;
            this.codeEditorViewModel = codeEditorViewModel;
            openFileCommand = new AsyncDelegateCommand(OpenFile);
            openFromNugetCommand = new AsyncDelegateCommand(OpenFromNuget);
            compareAssembliesCommand = new DelegateCommand(CompareAssemblies);
            closeAssemblyApiCommand = new DelegateCommand(CloseAssemblyApi);
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
            ShellViewModel.CodeEditorViewModels = codeEditorViewModels;
            ShellViewModel.Show();
        }

        public void Shutdown()
        {
        }

        private async Task OpenFile()
        {
            var assemblyFileType = new FileType(".NET Assembly (*.dll, *.exe)", ".dll;*.exe");
            var allFileType = new FileType("All files (*.*)", ".*");
            var result = fileDialogService.ShowOpenFileDialog(ShellViewModel.View, new[] { assemblyFileType, allFileType });
            if (!result.IsValid) return;

            try
            {
                using (ShellViewModel.SetApplicationBusy())
                {
                    AddAndSelectAssemblyApi(result.FileName, await Task.Run(() => AssemblyReader.Read(result.FileName)));
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
                    AddAndSelectAssemblyApi(result.fileName, await Task.Run(() => AssemblyReader.Read(result.assemblyStream, null)));
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

        private void AddAndSelectAssemblyApi(string fileName, string assemblyApi)
        {
            var viewModel = codeEditorViewModel.CreateExport().Value;
            viewModel.AssemblyInfo = new AssemblyInfo(fileName, Path.GetFileNameWithoutExtension(fileName), assemblyApi);
            codeEditorViewModels.Add(viewModel);
            ShellViewModel.SelectedCodeEditorViewModel = viewModel;
        }
    }
}
