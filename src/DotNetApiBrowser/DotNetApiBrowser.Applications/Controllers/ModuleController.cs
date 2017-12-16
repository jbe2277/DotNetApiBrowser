using System;
using System.ComponentModel.Composition;
using System.Waf.Applications;
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

        public void Run()
        {
            ShellViewModel.Show();
        }

        public void Shutdown()
        {
        }
    }
}
