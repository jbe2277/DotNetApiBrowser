using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using Microsoft.CodeAnalysis;

namespace Waf.DotNetApiBrowser.Presentation.Controls;

internal sealed class CodeHighlightingColorizer : HighlightingColorizer
{
    private readonly Workspace workspace;
    private readonly Func<Task<SemanticModel>> getSemanticModel;

    public CodeHighlightingColorizer(Workspace workspace, Func<Task<SemanticModel>> getSemanticModel)
    {
        this.workspace = workspace;
        this.getSemanticModel = getSemanticModel;
    }

    protected override IHighlighter CreateHighlighter(TextView textView, ICSharpCode.AvalonEdit.Document.TextDocument document)
    {
        return new CodeHighlighter(document, workspace, getSemanticModel);
    }
}
