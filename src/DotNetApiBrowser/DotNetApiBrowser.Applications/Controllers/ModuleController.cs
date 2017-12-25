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
        private readonly Lazy<ShellViewModel> shellViewModel;
        private readonly ExportFactory<CodeEditorViewModel> codeEditorViewModel;
        private readonly AsyncDelegateCommand openCommand;
        private readonly DelegateCommand closeAssemblyApiCommand;
        private readonly ObservableCollection<CodeEditorViewModel> assemblyApis;

        [ImportingConstructor]
        public ModuleController(IMessageService messageService, IFileDialogService fileDialogService, Lazy<ShellViewModel> shellViewModel, ExportFactory<CodeEditorViewModel> codeEditorViewModel)
        {
            this.messageService = messageService;
            this.fileDialogService = fileDialogService;
            this.shellViewModel = shellViewModel;
            this.codeEditorViewModel = codeEditorViewModel;
            openCommand = new AsyncDelegateCommand(Open);
            closeAssemblyApiCommand = new DelegateCommand(CloseAssemblyApi);
            assemblyApis = new ObservableCollection<CodeEditorViewModel>();
        }

        private ShellViewModel ShellViewModel => shellViewModel.Value;

        public void Initialize()
        {
        }

        public async void Run()
        {
            ShellViewModel.OpenCommand = openCommand;
            ShellViewModel.CloseAssemblyApiCommand = closeAssemblyApiCommand;
            ShellViewModel.AssemblyApis = assemblyApis;
            ShellViewModel.Show();
            var assembly = typeof(ExportAttribute).Assembly;
            AddAndSelectAssemblyApi(assembly.GetName().Name, await Task.Run(() => AssemblyReader.Read(assembly.Location)));
        }

        public void Shutdown()
        {
        }

        private async Task Open()
        {
            var assemblyFileType = new FileType(".NET Assembly (*.dll, *.exe)", ".dll;*.exe");
            var allFileType = new FileType("All files (*.*)", ".*");
            var result = fileDialogService.ShowOpenFileDialog(ShellViewModel.View, new[] { assemblyFileType, allFileType });
            if (!result.IsValid) return;

            try
            {
                AddAndSelectAssemblyApi(Path.GetFileNameWithoutExtension(result.FileName), await Task.Run(() => AssemblyReader.Read(result.FileName)));
            }
            catch (Exception e)
            {
                messageService.ShowError(ShellViewModel.View, "Could not read the file. Error: " + e);
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
