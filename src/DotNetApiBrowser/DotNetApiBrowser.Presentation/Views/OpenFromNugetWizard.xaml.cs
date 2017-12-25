using System.ComponentModel.Composition;
using System.Windows;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Presentation.Views
{
    [Export(typeof(IOpenFromNugetView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class OpenFromNugetWizard : IOpenFromNugetView
    {
        public OpenFromNugetWizard()
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
