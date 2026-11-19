/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Linq.Expressions;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Core.Syntax.Extensions;

public static class SyntaxNodeExtensions
{
    extension(SyntaxNode node)
    {
        public string MappedFilePathFromRoot
        {
            get
            {
                // Directives are always leading trivia, so this is the first trivia of the root without allocating a DescendantTrivia iterator
                if (node.GetLeadingTrivia() is { Count: > 0 } leading
                    && leading[0] is { RawKind: (ushort)Microsoft.CodeAnalysis.CSharp.SyntaxKind.PragmaChecksumDirectiveTrivia } pragmaChecksum
                    && SyntaxTreeExtensions.FileFromDirective(pragmaChecksum.GetStructure()) is { } file)
                {
                    return file;
                }

                // Since .NET 11, Razor also generates *_razor.decl.g.cs files without #pragma checksum
                return GeneratedCodeRecognizer.IsRazorGeneratedFile(node.SyntaxTree) ? node.SyntaxTree.OriginalFilePath : node.SyntaxTree.FilePath;
            }
        }

        public int LineNumberToReport => node.GetLocation().LineNumberToReport;

        public bool IsKnownType(KnownType knownType, SemanticModel model)
        {
            var type = model.GetSymbolInfo(node).Symbol.SymbolType;
            return type.Is(knownType) || type?.OriginalDefinition?.Is(knownType) == true;
        }

        public bool IsDeclarationKnownType(KnownType knownType, SemanticModel model) =>
            model.GetDeclaredSymbol(node)?.SymbolType.Is(knownType) ?? false;

        public SemanticModel EnsureCorrectSemanticModelOrDefault(SemanticModel model) =>
            node.SyntaxTree.SemanticModelOrDefault(model);

        public bool ToStringContains(string s) =>
            node.ToString().Contains(s);

        public bool ToStringContains(string s, StringComparison comparison) =>
            node.ToString().IndexOf(s, comparison) != -1;

        public bool ToStringContainsEitherOr(string a, string b)
        {
            var toString = node.ToString();
            return toString.Contains(a) || toString.Contains(b);
        }

        public TSyntaxKind Kind<TSyntaxKind>() where TSyntaxKind : struct, Enum =>
            node is null ? default : EnumConverter<TSyntaxKind>.FromInt32(node.RawKind);

        public SecondaryLocation ToSecondaryLocation(string message = null, params string[] messageArgs) =>
            message is not null && messageArgs?.Length > 0
                ? new(node.GetLocation(), string.Format(message, messageArgs))
                : new(node.GetLocation(), message);

        public bool HasFlagsAttribute(SemanticModel model) =>
            model.GetDeclaredSymbol(node).HasAttribute(KnownType.System_FlagsAttribute);

        public Location CreateLocation(SyntaxNode to) =>
            Location.Create(node.SyntaxTree, TextSpan.FromBounds(node.SpanStart, to.Span.End));

        public Location CreateLocation(SyntaxToken to) =>
            Location.Create(node.SyntaxTree, TextSpan.FromBounds(node.SpanStart, to.Span.End));
    }

    // Converts a SyntaxNode.RawKind (int) to the strongly-typed C#/VB SyntaxKind enum without boxing.
    // Enum.ToObject(...) boxes the result on every call; here a direct int -> TSyntaxKind conversion is
    // compiled once per enum type. System.Runtime.CompilerServices.Unsafe is not part of the netstandard2.0 /
    // Roslyn 1.3 surface we must support, so a compiled expression is used instead.
    private static class EnumConverter<TSyntaxKind>
        where TSyntaxKind : struct, Enum
    {
        public static readonly Func<int, TSyntaxKind> FromInt32 = Build();

        private static Func<int, TSyntaxKind> Build()
        {
            var rawKind = Expression.Parameter(typeof(int), "rawKind");
            return Expression.Lambda<Func<int, TSyntaxKind>>(Expression.Convert(rawKind, typeof(TSyntaxKind)), rawKind).Compile();
        }
    }
}
