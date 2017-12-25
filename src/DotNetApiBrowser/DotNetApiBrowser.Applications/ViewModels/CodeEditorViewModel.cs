using System.ComponentModel.Composition;
using System.Waf.Applications;
using Waf.DotNetApiBrowser.Applications.Views;

namespace Waf.DotNetApiBrowser.Applications.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class CodeEditorViewModel : ViewModel<ICodeEditorView>
    {
        [ImportingConstructor]
        public CodeEditorViewModel(ICodeEditorView view) : base(view)
        {
        }
        
        public string Header { get; set; }

        public string Code { get; set; }
    }
}
