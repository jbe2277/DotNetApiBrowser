using System.ComponentModel.Composition;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Presentation.Views
{
    [Export(typeof(ISelectPackageView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class SelectPackageView : ISelectPackageView
    {
        public SelectPackageView()
        {
            InitializeComponent();
        }
    }
}
