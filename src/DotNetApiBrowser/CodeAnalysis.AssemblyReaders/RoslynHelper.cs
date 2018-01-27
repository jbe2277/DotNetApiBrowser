using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Waf.CodeAnalysis.AssemblyReaders
{
    internal static class RoslynHelper
    {
        private static readonly string[] attributesToExclude = {
            "System.Diagnostics.CodeAnalysis.SuppressMessageAttribute",
            "System.Runtime.CompilerServices.IteratorStateMachine",
            "System.Runtime.CompilerServices.AsyncStateMachine",
            "System.ComponentModel.EditorBrowsable"
        };

        public static IEnumerable<AttributeData> DefaultFilter(this IEnumerable<AttributeData> attributes)
        {
            return attributes.Where(x => !attributesToExclude.Any(y => x.AttributeClass.ToString().StartsWith(y, StringComparison.Ordinal)));
        }

        public static string ToCSharpString(this INamespaceSymbol symbol)
        {
            return symbol.IsGlobalNamespace ? "<global namespace>" : string.Join(".", GetNamespaceParts(symbol));
        }

        public static string ToCSharpString(this AttributeData attributeData, ISymbol attachedTo = null)
        {
            var x = attributeData.ToString();
            var result = x.EndsWith("Attribute", StringComparison.Ordinal) ? x.Substring(0, x.Length - 9) : x.Replace("Attribute(", "(");
            return attachedTo == null ? result : result.OptimizeNamespaces(attachedTo.ContainingNamespace);
        }

        public static string ToCSharpString(this ITypeSymbol symbol)
        {
            var defaultFormat = symbol.ToDisplayString(SymbolDisplayFormats.DefaultFormat);
            var defaultParts = defaultFormat.Split(new[] { "where" }, StringSplitOptions.RemoveEmptyEntries);
            var containingTypePath = GetContainingTypePath(symbol).ToArray();
            var firstDefaultPart = defaultParts[0];
            if (containingTypePath.Length > 1)  // Nested type
            {
                var nestedTypePath = string.Join(".", containingTypePath.Reverse().Select(x => x.Name));
                firstDefaultPart = firstDefaultPart.Replace(nestedTypePath, symbol.Name);  // Replace with short type name.
            }

            var results = new[] {
                symbol.DeclaredAccessibility.ToCSharpString(),
                symbol.TypeFlagsToCSharpString(),
                firstDefaultPart,
                AddPrefix(": ", string.Join(", ", new[] { symbol.BaseType }.Concat(symbol.Interfaces)
                    .Where(x => x != null && !x.SpecialType.AnyItem(SpecialType.System_Object, SpecialType.System_ValueType, SpecialType.System_Enum, SpecialType.System_Delegate, SpecialType.System_MulticastDelegate))
                    .Select(x => x.ToDisplayString(SymbolDisplayFormats.BaseTypeFormat)))),
            }.Concat(defaultParts.Skip(1).Select(x => "where" + x)).Select(x => x.Trim(' '));
            return string.Join(" ", results.Where(x => !string.IsNullOrEmpty(x))).OptimizeNamespaces(symbol.ContainingNamespace);
        }

        public static bool IsConstructor(this ISymbol member)
        {
            return member.Kind == SymbolKind.Method && member.MetadataName == ".ctor";
        }

        public static IOrderedEnumerable<ISymbol> OrderMembers(this IEnumerable<ISymbol> members)
        {
            return members.OrderBy(x =>
                x.Kind == SymbolKind.Field ? (x.IsStatic ? 0 : 1) :
                x.IsConstructor() ? 3 :
                x.Kind == SymbolKind.Property ? (x.IsStatic ? 4 : 5) :
                x.Kind == SymbolKind.Event ? (x.IsStatic ? 6 : 7) :
                x.Kind == SymbolKind.Method ? (x.IsStatic ? 8 : 9) :
                100).ThenBy(x => x.ToString());
        }

        public static string ToCSharpString(this ISymbol symbol)
        {
            return symbol.ToDisplayString(SymbolDisplayFormats.DefaultFormat).OptimizeNamespaces(symbol.ContainingNamespace);
        }

        private static IEnumerable<string> GetNamespaceParts(INamespaceSymbol symbol)
        {
            return GetContainingNamespacePath(symbol).Where(x => !string.IsNullOrEmpty(x.Name)).Reverse().Select(x => x.Name);
        }

        private static IEnumerable<INamespaceSymbol> GetContainingNamespacePath(INamespaceSymbol symbol)
        {
            var next = symbol;
            yield return symbol;
            while ((next = next.ContainingNamespace) != null)
            {
                yield return next;
            }
        }

        private static IEnumerable<ITypeSymbol> GetContainingTypePath(ITypeSymbol type)
        {
            var next = type;
            yield return next;
            while ((next = next.ContainingType) != null)
            {
                yield return next;
            }
        }

        private static string ToCSharpString(this Accessibility accessability)
        {
            switch (accessability)
            {
                case Accessibility.Internal: return "internal";
                case Accessibility.Private: return "private";
                case Accessibility.Protected: return "protected";
                case Accessibility.Public: return "public";
                case Accessibility.ProtectedOrInternal: return "protected internal";
                default: return "private";
            }
        }

        private static string TypeFlagsToCSharpString(this ITypeSymbol symbol)
        {
            var results = new List<string>();
            if (symbol.IsStatic) results.Add("static");
            else if (symbol.TypeKind == TypeKind.Class)
            {
                if (symbol.IsAbstract) results.Add("abstract");
                if (symbol.IsSealed) results.Add("sealed");
            }
            return string.Join(" ", results);
        }

        private static string OptimizeNamespaces(this string declaration, INamespaceSymbol namespaceContext)
        {
            var namespaceParts = GetNamespaceParts(namespaceContext).ToArray();
            for (int i = namespaceParts.Length; i > 0; i--)
            {
                declaration = declaration.Replace(string.Join(".", namespaceParts.Take(i)) + ".", "");
            }
            return declaration;

            // TODO: Still not correct: Threading.CancellationToken is wrong :-(
            // return declaration.Replace("System.Threading.Tasks.", "").Replace("System.Collections.Generic.", "").Replace("System.", "");
        }

        private static string AddPrefix(string prefix, string value) => string.IsNullOrEmpty(value) ? value : prefix + value;

        public static bool AnyItem<T>(this T value, params T[] values) where T : struct => values?.Contains(value) ?? false;
    }
}
