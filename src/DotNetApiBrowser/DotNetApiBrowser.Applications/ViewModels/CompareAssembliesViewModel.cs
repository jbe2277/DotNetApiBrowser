using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Waf.Applications;
using System.Windows.Input;
using Waf.DotNetApiBrowser.Applications.DataModels;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Applications.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class CompareAssembliesViewModel : ViewModel<ICompareAssembliesView>
    {
        private bool isClosing;

        [ImportingConstructor]
        public CompareAssembliesViewModel(ICompareAssembliesView view) : base(view)
        {
            Model = new CompareAssembliesDataModel();
        }

        public CompareAssembliesDataModel Model { get; }

        public ICommand CompareCommand { get; set; }
        
        public bool IsClosing
        {
            get => isClosing;
            set => SetProperty(ref isClosing, value);
        }

        public Task ShowDialogAsync(object ownerWindow)
        {
            return ViewCore.ShowDialogAsync(ownerWindow);
        }

        public void Close()
        {
            ViewCore.Close();
        }
    }
}
