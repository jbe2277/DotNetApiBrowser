using System;
using System.ComponentModel.Composition;
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
        private readonly AsyncDelegateCommand openCommand;

        [ImportingConstructor]
        public ModuleController(IMessageService messageService, IFileDialogService fileDialogService, Lazy<ShellViewModel> shellViewModel)
        {
            this.messageService = messageService;
            this.fileDialogService = fileDialogService;
            this.shellViewModel = shellViewModel;
            openCommand = new AsyncDelegateCommand(Open);
        }

        private ShellViewModel ShellViewModel => shellViewModel.Value;

        public void Initialize()
        {
        }

        public async void Run()
        {
            ShellViewModel.OpenCommand = openCommand;
            ShellViewModel.Show();
            ShellViewModel.SetCode(await Task.Run(() => AssemblyReader.Read(typeof(ExportAttribute).Assembly.Location)));
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
                ShellViewModel.SetCode(await Task.Run(() => AssemblyReader.Read(result.FileName)));
            }
            catch (Exception e)
            {
                messageService.ShowError(ShellViewModel.View, "Could not read the file. Error: " + e);
            }
        }
    }
}
