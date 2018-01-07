using System.ComponentModel.Composition;
using System.Windows;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Presentation.Views
{
    [Export(typeof(ICompareAssembliesView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class CompareAssembliesWindow : ICompareAssembliesView
    {
        public CompareAssembliesWindow()
        {
            InitializeComponent();
        }

        public void ShowDialog(object ownerWindow)
        {
            Owner = ownerWindow as Window;
            ShowDialog();
        }
    }
}
