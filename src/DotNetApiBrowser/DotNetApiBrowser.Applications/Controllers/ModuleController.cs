using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Waf.Applications;
using Waf.CodeAnalysis.AssemblyReaders;
using Waf.DotNetApiBrowser.Applications.ViewModels;

namespace Waf.DotNetApiBrowser.Applications.Controllers
{
    [Export(typeof(IModuleController))]
    internal class ModuleController : IModuleController
    {
        private readonly Lazy<ShellViewModel> shellViewModel;

        [ImportingConstructor]
        public ModuleController(Lazy<ShellViewModel> shellViewModel)
        {
            this.shellViewModel = shellViewModel;
        }

        private ShellViewModel ShellViewModel => shellViewModel.Value;

        public void Initialize()
        {
        }

        public async void Run()
        {
            ShellViewModel.Show();
            ShellViewModel.SetCode(await Task.Run(() => AssemblyReader.Read(typeof(ExportAttribute).Assembly.Location)));
        }

        public void Shutdown()
        {
        }
    }
}
