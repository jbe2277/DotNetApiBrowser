using System;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Search;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Waf;
using System.Threading.Tasks;

namespace Waf.DotNetApiBrowser.Presentation.Controls
{
    public class CodeEditor : TextEditor
    {
        private static readonly MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

        private readonly AdhocWorkspace adhocWorkspace;
        private Task<SemanticModel> semanticModel;

        public CodeEditor()
        {
            if (!WafConfiguration.IsInDesignMode)
            {
                adhocWorkspace = new AdhocWorkspace();
                TextArea.TextView.LineTransformers.Insert(0, new CodeHighlightingColorizer(adhocWorkspace, () => semanticModel));
            }
            SearchPanel.Install(TextArea);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            var code = Text;
            semanticModel = Task.Run(() =>
            {
                var tree = SyntaxFactory.ParseSyntaxTree(code);
                var compilation = CSharpCompilation.Create("MyCompilation").AddReferences(mscorlib).AddSyntaxTrees(tree);
                return compilation.GetSemanticModel(tree);
            });
        }
    }
}
