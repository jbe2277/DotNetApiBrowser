using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Waf.Applications;
using System.Windows.Input;
using Waf.DotNetApiBrowser.Applications.DataModels;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Applications.ViewModels
{
    [Export]
    public class ShellViewModel : ViewModel<IShellView>
    {
        private AssemblyApiDataModel selectedAssemblyApiDataModel;

        [ImportingConstructor]
        public ShellViewModel(IShellView view) : base(view)
        {
            ExitCommand = new DelegateCommand(Close);
        }

        public static string Title => ApplicationInfo.ProductName;

        public ICommand ExitCommand { get; }

        public ICommand OpenCommand { get; set; }

        public ICommand CloseAssemblyApiCommand { get; set; }

        public IReadOnlyList<AssemblyApiDataModel> AssemblyApiDataModels { get; set; }

        public AssemblyApiDataModel SelectedAssemblyApiDataModel
        {
            get => selectedAssemblyApiDataModel;
            set => SetProperty(ref selectedAssemblyApiDataModel, value);
        }

        public void Show()
        {
            ViewCore.Show();
        }

        private void Close()
        {
            ViewCore.Close();
        }
    }
}
