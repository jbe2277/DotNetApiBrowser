using System.ComponentModel.Composition;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Presentation.Views
{
    [Export(typeof(ICodeEditorView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class CodeEditorView : ICodeEditorView
    {
        public CodeEditorView()
        {
            InitializeComponent();
        }
    }
}
