using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Search;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Waf;
using System.Windows;

namespace Waf.DotNetApiBrowser.Presentation.Controls;

public class CodeEditor : TextEditor
{
    public static DependencyProperty CodeProperty { get; } = DependencyProperty.Register("Code", typeof(string), typeof(CodeEditor), new PropertyMetadata(null, 
        propertyChangedCallback: (d, e) => ((CodeEditor)d).Text = (string)e.NewValue));

    private static readonly MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

    private readonly AdhocWorkspace adhocWorkspace;
    private Task<SemanticModel> semanticModel;

    public CodeEditor()
    {
        // TODO: This must be a Roslin document now
        //if (!WafConfiguration.IsInDesignMode)
        //{
        //    adhocWorkspace = new AdhocWorkspace();
        //    TextArea.TextView.LineTransformers.Insert(0, new CodeHighlightingColorizer(adhocWorkspace, () => semanticModel));
        //}
        SearchPanel.Install(TextArea);
    }

    public string Code
    {
        get => (string)GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
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
