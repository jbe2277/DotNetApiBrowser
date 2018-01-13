using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Waf.Applications;
using Waf.DotNetApiBrowser.Applications.DataModels;
using Waf.DotNetApiBrowser.Applications.Services;
using Waf.DotNetApiBrowser.Applications.ViewModels;

namespace Waf.DotNetApiBrowser.Applications.Controllers
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    internal class CompareAssembliesController
    {
        private readonly IEnvironmentService environmentService;
        private readonly CompareAssembliesViewModel compareAssembliesViewModel;
        private readonly AsyncDelegateCommand compareCommand;

        [ImportingConstructor]
        public CompareAssembliesController(IEnvironmentService environmentService, CompareAssembliesViewModel compareAssembliesViewModel)
        {
            this.environmentService = environmentService;
            this.compareAssembliesViewModel = compareAssembliesViewModel;
            compareCommand = new AsyncDelegateCommand(CompareAsync);
        }

        public void Run(object ownerWindow, IReadOnlyList<AssemblyInfo> availableAssemblies)
        {
            compareAssembliesViewModel.CompareCommand = compareCommand;
            compareAssembliesViewModel.AvailableAssemblies = availableAssemblies;
            compareAssembliesViewModel.SelectedAssembly1 = availableAssemblies.FirstOrDefault();
            compareAssembliesViewModel.SelectedAssembly2 = availableAssemblies.Skip(1).FirstOrDefault();
            compareAssembliesViewModel.ShowDialog(ownerWindow);
        }

        private async Task CompareAsync()
        {
            var assemblyApi1FileName = environmentService.GetTempFileName();
            var assemblyApi2FileName = environmentService.GetTempFileName();
            var task1 = WriteTextAsync(assemblyApi1FileName, compareAssembliesViewModel.SelectedAssembly1.AssemblyApi);
            var task2 = WriteTextAsync(assemblyApi2FileName, compareAssembliesViewModel.SelectedAssembly2.AssemblyApi);

            await Task.WhenAll(task1, task2).ConfigureAwait(false);
            
            var devenv = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe";
            var args = "/Diff \"" + assemblyApi1FileName + "\" \"" + assemblyApi2FileName + "\"";
            Process.Start(devenv, args);
        }

        private static async Task WriteTextAsync(string fileName, string text)
        {
            byte[] encodedText = Encoding.UTF8.GetBytes(text);
            using (var fileStream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await fileStream.WriteAsync(encodedText, 0, encodedText.Length).ConfigureAwait(false);
            }
        }
    }
}
