using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Waf.CodeAnalysis.AssemblyReaders
{
    public static class AssemblyReader
    {
        private static readonly string nl = Environment.NewLine;
        private static MetadataReference Mscorlib { get; } = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        private static MetadataReference SystemRuntime { get; } = MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "System.Runtime.dll"));

        public static (Version version, string api) Read(string assemblyPath, bool showXmlComments = false)
        {
            var documentation = showXmlComments ? new FileBasedXmlDocumentationProvider(Path.ChangeExtension(assemblyPath, ".xml")) : null;
            using (var fileStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Read(fileStream, documentation);
            }
        }

        public static (Version version, string api) Read(Stream assemblyStream, DocumentationProvider documentation)
        {
            var assemblyReference = MetadataReference.CreateFromStream(assemblyStream, documentation: documentation);

            var tree = SyntaxFactory.ParseSyntaxTree(@"class Program { static void Main() { } }");
            var compilation = CSharpCompilation.Create("MyCompilation").AddReferences(Mscorlib).AddReferences(SystemRuntime).AddReferences(assemblyReference).AddSyntaxTrees(tree);
            var assemblySymbol = (IAssemblySymbol)compilation.GetAssemblyOrModuleSymbol(assemblyReference);
            if (assemblySymbol == null) throw new InvalidOperationException("Could not read the assembly.");

            var result = new StringBuilder();
            result.AppendLine("// " + assemblySymbol);

            foreach (var attribute in assemblySymbol.GetAttributes().DefaultFilter().OrderBy(x => x.ToString()))
            {
                result.Append(nl + "[assembly: " + attribute.ToCSharpString() + "]");
            }

            foreach (var n in GetAllNamespaces(assemblySymbol.GlobalNamespace).Select(x => (ns:x, name:x.ToCSharpString())).OrderBy(x => x.name))
            {
                var typeMembers = n.ns.GetTypeMembers().Where(x => x.CanBeReferencedByName && !x.IsImplicitlyDeclared && x.DeclaredAccessibility == Accessibility.Public).ToArray();
                if (!typeMembers.Any()) continue;

                result.AppendLine(nl);
                result.Append(n.name);

                foreach (var t in typeMembers.Select(x => (type:x, name:x.ToString())).OrderBy(x => x.name))
                {
                    ReadTypeSymbol(t.type, result);
                }
            }

            return (assemblySymbol.Identity.Version, result.ToString());
        }

        private static void ReadTypeSymbol(INamedTypeSymbol type, StringBuilder results, int indent = 4)
        {
            var indentString = new string(' ', indent);
            results.AppendLine();
            // TODO: Doc for everything
            AppendDocumentation(results, type, indentString);
            AppendAttributes(results, type, indentString);

            results.AppendLine();
            results.Append(indentString + type.ToCSharpString());

            if (!type.TypeKind.AnyItem(TypeKind.Class, TypeKind.Struct, TypeKind.Interface, TypeKind.Enum)) return;

            foreach (var member in type.GetMembers().Where(x =>
                (x.CanBeReferencedByName || x.IsConstructor())
                && x.Kind != SymbolKind.NamedType
                && !x.IsImplicitlyDeclared
                && x.DeclaredAccessibility.AnyItem(Accessibility.Public, Accessibility.Protected, Accessibility.ProtectedOrFriend)).OrderMembers())
            {
                AppendDocumentation(results, member, indentString + "    ");
                AppendAttributes(results, member, indentString + "    ");
                results.Append(nl + indentString + "    " + member.ToCSharpString());
                if (member.Kind == SymbolKind.Field)
                {
                    var field = (IFieldSymbol)member;
                    if (field.ConstantValue != null) results.Append(" = " + field.ConstantValue);
                }
            }

            foreach (var nestedType in type.GetMembers().Where(x =>
                x.CanBeReferencedByName
                && x.Kind == SymbolKind.NamedType
                && !x.IsImplicitlyDeclared
                && x.DeclaredAccessibility == Accessibility.Public))
            {
                ReadTypeSymbol((INamedTypeSymbol)nestedType, results, indent + 4);
            }
        }

        private static void AppendDocumentation(StringBuilder results, ISymbol symbol, string indentString)
        {
            var doc = symbol.GetDocumentationCommentXml();
            if (string.IsNullOrEmpty(doc)) return;

            try
            {
                doc = string.Join(nl, XDocument.Parse("<Root>" + doc + "</Root>").Root.Elements()
                        .Select(x => indentString + "/// " + x.ToString()));
            }
            catch (Exception)
            {
                // Ignore exceptions
            }
            results.Append(nl + doc);
        }

        private static void AppendAttributes(StringBuilder results, ISymbol symbol, string indentString)
        {
            var attributes = symbol.GetAttributes().DefaultFilter().ToArray();
            if (attributes.Length > 0)
            {
                results.Append(nl + indentString + "[" + string.Join(", ", attributes.Select(x => x.ToCSharpString(symbol))) + "]");
            }
        }

        private static IEnumerable<INamespaceSymbol> GetAllNamespaces(INamespaceSymbol rootNamespace)
        {
            yield return rootNamespace;
            foreach (var childNs in rootNamespace.GetNamespaceMembers())
            {
                foreach (var childOfChildNs in GetAllNamespaces(childNs))
                {
                    yield return childOfChildNs;
                }
            }
        }
    }
}
