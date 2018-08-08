using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Waf.Foundation;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using Waf.DotNetApiBrowser.Applications;

namespace Waf.DotNetApiBrowser.Presentation.Controls
{
    public sealed class CodeHighlighter : IHighlighter
    {
        private readonly TaskScheduler uiTaskScheduler;
        private readonly Workspace workspace;
        private readonly Func<Task<SemanticModel>> getSemanticModel;
        private readonly List<VersionedHighlightedLine> cachedLines;

        public CodeHighlighter(IDocument document, Workspace workspace, Func<Task<SemanticModel>> getSemanticModel)
        {
            uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Document = document;
            this.workspace = workspace;
            this.getSemanticModel = getSemanticModel;
            cachedLines = new List<VersionedHighlightedLine>();
        }

        public IDocument Document { get; }

        public HighlightingColor DefaultTextColor => CodeHighlightColors.DefaultHighlightingColor;
        
        public event HighlightingStateChangedEventHandler HighlightingStateChanged;

        public HighlightedLine HighlightLine(int lineNumber)
        {
            var documentLine = Document.GetLineByNumber(lineNumber);
            var currentVersion = Document.Version;

            EnlargeList(cachedLines, lineNumber + 1);
            var cachedLine = cachedLines[lineNumber];

            if (cachedLine != null && currentVersion != null && cachedLine.Version.CompareAge(currentVersion) == 0
                && currentVersion.BelongsToSameDocumentAs(cachedLine.Version))
            {
                return cachedLine;
            }

            cachedLines[lineNumber]?.Cancel();
            var newLine = new VersionedHighlightedLine(Document, documentLine, Document.Version, cachedLine);
            cachedLines[lineNumber] = newLine;
            UpdateHighlightLineAsync(newLine);

            foreach (var line in cachedLines.ToArray().Reverse())
            {
                if (!line?.DocumentLine?.IsDeleted == true)
                {
                    break;
                }
                cachedLines.Remove(line);
            }

            return newLine;
        }

        private async void UpdateHighlightLineAsync(VersionedHighlightedLine line)
        {
            try
            {
                await Task.Run(async () =>
                {
                    if (CancelUpdate(Document, line)) return;

                    var documentLine = line.DocumentLine;
                    var spans = await GetClassifiedSpansAsync(documentLine, line.CancellationToken).ConfigureAwait(false);

                    await TaskHelper.Run(() =>
                    {
                        if (CancelUpdate(Document, line)) return;

                        var newLineSections = new List<HighlightedSection>();
                        foreach (var classifiedSpan in spans)
                        {
                            if (IsOutsideLine(documentLine, classifiedSpan.TextSpan.Start, classifiedSpan.TextSpan.Length))
                            {
                                continue;
                            }
                            newLineSections.Add(new HighlightedSection
                            {
                                Color = CodeHighlightColors.GetHighlightingColor(classifiedSpan.ClassificationType),
                                Offset = classifiedSpan.TextSpan.Start,
                                Length = classifiedSpan.TextSpan.Length
                            });
                        }
                        if (!line.Sections.SequenceEqual(newLineSections, HighlightedSectionComparer.Default))
                        {
                            line.Sections.Clear();
                            foreach (var newSection in newLineSections) { line.Sections.Add(newSection); }
                            HighlightingStateChanged?.Invoke(documentLine.LineNumber, documentLine.LineNumber);
                        }
                    }, uiTaskScheduler).ConfigureAwait(false);
                }, line.CancellationToken);
            }
            catch (OperationCanceledException) { }
        }

        private static bool CancelUpdate(IDocument document, VersionedHighlightedLine line)
        {
            var currentVersion = document.Version;
            return line.CancellationToken.IsCancellationRequested || line.Version == null || !currentVersion.BelongsToSameDocumentAs(line.Version) || currentVersion.CompareAge(line.Version) != 0;
        }

        private static bool IsOutsideLine(IDocumentLine documentLine, int offset, int length)
        {
            return offset < documentLine.Offset || offset + length > documentLine.EndOffset;
        }

        private async Task<IEnumerable<ClassifiedSpan>> GetClassifiedSpansAsync(IDocumentLine documentLine, CancellationToken cancellationToken)
        {
            int textLength = await TaskHelper.Run(() => Document.TextLength, uiTaskScheduler).ConfigureAwait(false);
            if (textLength >= documentLine.Offset + documentLine.TotalLength)
            {
                var semanticModel = await getSemanticModel().ConfigureAwait(false);
                return Classifier.GetClassifiedSpans(semanticModel, new TextSpan(documentLine.Offset, documentLine.TotalLength), workspace, cancellationToken);
            }
            return Array.Empty<ClassifiedSpan>();
        }

        public void BeginHighlighting() { }

        public void EndHighlighting() { }

        public HighlightingColor GetNamedColor(string name) => null;

        public IEnumerable<HighlightingColor> GetColorStack(int lineNumber) => null;
        
        public void UpdateHighlightingState(int lineNumber) { }

        public void Dispose()
        {
            cachedLines.Clear();
        }

        private static void EnlargeList<T>(List<T> list, int newCount)
        {
            if (newCount > list.Count)
            {
                list.AddRange(Enumerable.Repeat(default(T), newCount - list.Count));
            }
        }


        private sealed class VersionedHighlightedLine : HighlightedLine
        {
            private readonly CancellationTokenSource cancellationTokenSource;

            public VersionedHighlightedLine(IDocument document, IDocumentLine documentLine, ITextSourceVersion version, VersionedHighlightedLine oldVersion)
                : base(document, documentLine)
            {
                Version = version;
                cancellationTokenSource = new CancellationTokenSource();
                CancellationToken = cancellationTokenSource.Token;
                if (oldVersion != null)
                {
                    foreach (var oldSection in oldVersion.Sections)
                    {
                        if (IsOutsideLine(documentLine, oldSection.Offset, oldSection.Length))
                        {
                            continue;
                        }
                        Sections.Add(new HighlightedSection
                        {
                            Color = oldSection.Color,
                            Offset = oldSection.Offset,
                            Length = oldSection.Length
                        });
                    }
                }
            }

            public ITextSourceVersion Version { get; }

            public CancellationToken CancellationToken { get; }
            
            public void Cancel()
            {
                cancellationTokenSource.Cancel();
            }
        }

        private sealed class HighlightedSectionComparer : IEqualityComparer<HighlightedSection>
        {
            public static HighlightedSectionComparer Default { get; } = new HighlightedSectionComparer();

            public bool Equals(HighlightedSection x, HighlightedSection y)
            {
                if (x == y) { return true; }
                if (x == null || y == null) { return false; }
                return Equals(x.Color, y.Color) && x.Length == y.Length && x.Offset == y.Offset;
            }

            public int GetHashCode(HighlightedSection obj)
            {
                return (obj.Color?.GetHashCode() ?? 0) ^ obj.Length.GetHashCode() ^ obj.Offset.GetHashCode();
            }
        }
    }
}
