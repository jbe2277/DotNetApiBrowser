using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
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
        private readonly Lazy<ShellViewModel> shellViewModel;
        private readonly AsyncDelegateCommand openCommand;
        private readonly DelegateCommand closeAssemblyApiCommand;
        private readonly ObservableCollection<AssemblyApiDataModel> assemblyApiDataModels;

        [ImportingConstructor]
        public ModuleController(IMessageService messageService, IFileDialogService fileDialogService, Lazy<ShellViewModel> shellViewModel)
        {
            this.messageService = messageService;
            this.fileDialogService = fileDialogService;
            this.shellViewModel = shellViewModel;
            openCommand = new AsyncDelegateCommand(Open);
            closeAssemblyApiCommand = new DelegateCommand(CloseAssemblyApi);
            assemblyApiDataModels = new ObservableCollection<AssemblyApiDataModel>();
        }

        private ShellViewModel ShellViewModel => shellViewModel.Value;

        public void Initialize()
        {
        }

        public async void Run()
        {
            ShellViewModel.OpenCommand = openCommand;
            ShellViewModel.CloseAssemblyApiCommand = closeAssemblyApiCommand;
            ShellViewModel.AssemblyApiDataModels = assemblyApiDataModels;
            ShellViewModel.Show();
            var assembly = typeof(ExportAttribute).Assembly;
            AddAndSelectAssemblyApi(new AssemblyApiDataModel(assembly.GetName().Name, await Task.Run(() => AssemblyReader.Read(assembly.Location))));
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
                AddAndSelectAssemblyApi(new AssemblyApiDataModel(Path.GetFileNameWithoutExtension(result.FileName), await Task.Run(() => AssemblyReader.Read(result.FileName))));
            }
            catch (Exception e)
            {
                messageService.ShowError(ShellViewModel.View, "Could not read the file. Error: " + e);
            }
        }

        private void CloseAssemblyApi()
        {
            assemblyApiDataModels.Remove(ShellViewModel.SelectedAssemblyApiDataModel);
        }

        private void AddAndSelectAssemblyApi(AssemblyApiDataModel dataModel)
        {
            assemblyApiDataModels.Add(dataModel);
            ShellViewModel.SelectedAssemblyApiDataModel = dataModel;
        }
    }
}
