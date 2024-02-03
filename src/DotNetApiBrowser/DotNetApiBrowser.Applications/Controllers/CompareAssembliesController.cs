using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Text;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using Waf.DotNetApiBrowser.Applications.DataModels;
using Waf.DotNetApiBrowser.Applications.Services;
using Waf.DotNetApiBrowser.Applications.ViewModels;

namespace Waf.DotNetApiBrowser.Applications.Controllers;

[Export, PartCreationPolicy(CreationPolicy.NonShared)]
internal class CompareAssembliesController
{
    private readonly IMessageService messageService;
    private readonly IEnvironmentService environmentService;
    private readonly CompareAssembliesViewModel compareAssembliesViewModel;
    private readonly AsyncDelegateCommand compareCommand;

    [ImportingConstructor]
    public CompareAssembliesController(IMessageService messageService, IEnvironmentService environmentService, CompareAssembliesViewModel compareAssembliesViewModel)
    {
        this.messageService = messageService;
        this.environmentService = environmentService;
        this.compareAssembliesViewModel = compareAssembliesViewModel;
        compareCommand = new AsyncDelegateCommand(CompareAsync, CanCompare);
        compareAssembliesViewModel.Model.PropertyChanged += CompareAssembliesDataModelPropertyChanged;
    }

    public async void Run(object ownerWindow, IReadOnlyList<AssemblyInfo> availableAssemblies)
    {
        var viewModel = compareAssembliesViewModel;
        viewModel.CompareCommand = compareCommand;
        viewModel.Model.AvailableAssemblies = availableAssemblies;
        viewModel.Model.SelectedAssembly1 = availableAssemblies.FirstOrDefault();
        viewModel.Model.SelectedAssembly2 = availableAssemblies.Skip(1).FirstOrDefault();
        viewModel.Model.Validate();
        var dialogTask = viewModel.ShowDialogAsync(ownerWindow);

        var path = await environmentService.GetDefaultDiffToolPathAsync();
        if (!string.IsNullOrEmpty(path.path) && string.IsNullOrEmpty(viewModel.Model.DiffToolPath)) viewModel.Model.DiffToolPath = path.path;
        if (!string.IsNullOrEmpty(path.arguments) && string.IsNullOrEmpty(viewModel.Model.DiffToolArguments)) viewModel.Model.DiffToolArguments = path.arguments;

        await dialogTask;
    }

    private bool CanCompare() => !compareAssembliesViewModel.Model.HasErrors;
   
    private async Task CompareAsync()
    {
        compareAssembliesViewModel.IsClosing = true;
        var assemblyApi1FileName = environmentService.GetTempFileName();
        var assemblyApi2FileName = environmentService.GetTempFileName();
        var task1 = WriteTextAsync(assemblyApi1FileName, compareAssembliesViewModel.Model.SelectedAssembly1!.AssemblyApi);
        var task2 = WriteTextAsync(assemblyApi2FileName, compareAssembliesViewModel.Model.SelectedAssembly2!.AssemblyApi);

        try
        {
            await Task.WhenAll(task1, task2);
        }
        catch (Exception ex)
        {
            ShowError("Could not create the temporary files: " + ex.Message);
            compareAssembliesViewModel.Close();
            return;
        }
        
        var devenv = compareAssembliesViewModel.Model.DiffToolPath;
        var args = (compareAssembliesViewModel.Model.DiffToolArguments ?? "") + " \"" + assemblyApi1FileName + "\" \"" + assemblyApi2FileName + "\"";

        var process = new Process
        {
            StartInfo = { FileName = devenv, Arguments = args },
            EnableRaisingEvents = true
        };
        process.Exited += async (sender, e) =>
        {
            // Note: devenv will exit immediately; another process will open the files but it still works
            process.Dispose();
            try
            {
                await Task.Run(() =>
                {
                    File.Delete(assemblyApi1FileName);
                    File.Delete(assemblyApi2FileName);
                });
            }
            catch (Exception)
            {
                // Just try to delete temp files -> don't care if it does not work
            }
        };
        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            ShowError("Could not start the diff tool: " + ex.Message);
        }
        compareAssembliesViewModel.Close();
    }

    private void CompareAssembliesDataModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CompareAssembliesDataModel.HasErrors))
        {
            compareCommand.RaiseCanExecuteChanged();
        }
    }

    private void ShowError(string message)
    {
        messageService.ShowError(compareAssembliesViewModel.View, message);
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
