using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using Waf.CodeAnalysis.AssemblyReaders;
using Waf.DotNetApiBrowser.Applications.ViewModels;

namespace Waf.DotNetApiBrowser.Applications.Controllers
{
    [Export(typeof(IModuleController))]
    internal class ModuleController : IModuleController
    {
        private readonly IMessageService messageService;
        private readonly IFileDialogService fileDialogService;
        private readonly ExportFactory<OpenFromNugetController> openFromNugetController;
        private readonly Lazy<ShellViewModel> shellViewModel;
        private readonly ExportFactory<CodeEditorViewModel> codeEditorViewModel;
        private readonly AsyncDelegateCommand openFileCommand;
        private readonly DelegateCommand openFromNugetCommand;
        private readonly DelegateCommand closeAssemblyApiCommand;
        private readonly ObservableCollection<CodeEditorViewModel> assemblyApis;

        [ImportingConstructor]
        public ModuleController(IMessageService messageService, IFileDialogService fileDialogService, ExportFactory<OpenFromNugetController> openFromNugetController,
            Lazy<ShellViewModel> shellViewModel, ExportFactory<CodeEditorViewModel> codeEditorViewModel)
        {
            this.messageService = messageService;
            this.fileDialogService = fileDialogService;
            this.openFromNugetController = openFromNugetController;
            this.shellViewModel = shellViewModel;
            this.codeEditorViewModel = codeEditorViewModel;
            openFileCommand = new AsyncDelegateCommand(OpenFile);
            openFromNugetCommand = new DelegateCommand(OpenFromNuget);
            closeAssemblyApiCommand = new DelegateCommand(CloseAssemblyApi);
            assemblyApis = new ObservableCollection<CodeEditorViewModel>();
        }

        private ShellViewModel ShellViewModel => shellViewModel.Value;

        public void Initialize()
        {
        }

        public void Run()
        {
            ShellViewModel.OpenFileCommand = openFileCommand;
            ShellViewModel.OpenFromNugetCommand = openFromNugetCommand;
            ShellViewModel.CloseAssemblyApiCommand = closeAssemblyApiCommand;
            ShellViewModel.AssemblyApis = assemblyApis;
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
                    AddAndSelectAssemblyApi(Path.GetFileNameWithoutExtension(result.FileName), await Task.Run(() => AssemblyReader.Read(result.FileName)));
                }
            }
            catch (Exception e)
            {
                messageService.ShowError(ShellViewModel.View, "Could not read the file. Error: " + e);
            }
        }

        private async void OpenFromNuget()
        {
            using (var controller = openFromNugetController.CreateExport())
            {
                var result = await controller.Value.RunAsync(ShellViewModel.View);
                if (result.assemblyStream == null) return;
                using (ShellViewModel.SetApplicationBusy())
                {
                    AddAndSelectAssemblyApi(Path.GetFileNameWithoutExtension(result.fileName), await Task.Run(() => AssemblyReader.Read(result.assemblyStream, null)));
                }
            }
        }

        private void CloseAssemblyApi()
        {
            assemblyApis.Remove(ShellViewModel.SelectedAssemblyApi);
        }

        private void AddAndSelectAssemblyApi(string header, string code)
        {
            var viewModel = codeEditorViewModel.CreateExport().Value;
            viewModel.Header = header;
            viewModel.Code = code;
            assemblyApis.Add(viewModel);
            ShellViewModel.SelectedAssemblyApi = viewModel;
        }
    }
}
