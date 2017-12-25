using System.ComponentModel.Composition;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Presentation.Views
{
    [Export(typeof(ISelectAssemblyView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class SelectAssemblyView : ISelectAssemblyView
    {
        public SelectAssemblyView()
        {
            InitializeComponent();
        }
    }
}
