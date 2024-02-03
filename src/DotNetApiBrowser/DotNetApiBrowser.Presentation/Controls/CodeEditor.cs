using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Search;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Windows;

namespace Waf.DotNetApiBrowser.Presentation.Controls;

public class CodeEditor : TextEditor
{
    public static DependencyProperty CodeProperty { get; } = DependencyProperty.Register("Code", typeof(string), typeof(CodeEditor));

    private readonly AdhocWorkspace workspace;
    private readonly Project project;
    private Document? document;

    public CodeEditor()
    {
        workspace = new AdhocWorkspace();
        project = workspace.AddProject("AdhocProject", LanguageNames.CSharp);
        
        TextArea.TextView.LineTransformers.Insert(0, new CodeHighlightingColorizer(() => document ?? throw new InvalidOperationException("Document is not initialized")));
        SearchPanel.Install(TextArea);
    }

    public string Code
    {
        get => (string)GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == CodeProperty)
        {
            Text = Code;
            document = workspace.AddDocument(project.Id, "Adhoc.cs", SourceText.From(Code, System.Text.Encoding.UTF8));
        }
    }
}
