/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Core.Syntax.Extensions;

public static class SyntaxNodeExtensions
{
    public static bool IsKnownType(this SyntaxNode node, KnownType knownType, SemanticModel model)
    {
        var type = model.GetSymbolInfo(node).Symbol.GetSymbolType();
        return type.Is(knownType) || type?.OriginalDefinition?.Is(knownType) == true;
    }

    public static bool IsDeclarationKnownType(this SyntaxNode node, KnownType knownType, SemanticModel model) =>
        model.GetDeclaredSymbol(node)?.GetSymbolType().Is(knownType) ?? false;

    public static SemanticModel EnsureCorrectSemanticModelOrDefault(this SyntaxNode node, SemanticModel model) =>
        node.SyntaxTree.SemanticModelOrDefault(model);

    public static bool ToStringContains(this SyntaxNode node, string s) =>
        node.ToString().Contains(s);

    public static bool ToStringContains(this SyntaxNode node, string s, StringComparison comparison) =>
        node.ToString().IndexOf(s, comparison) != -1;

    public static bool ToStringContainsEitherOr(this SyntaxNode node, string a, string b)
    {
        var toString = node.ToString();
        return toString.Contains(a) || toString.Contains(b);
    }

    public static string GetMappedFilePathFromRoot(this SyntaxNode root)
    {
        if (root.DescendantTrivia().FirstOrDefault() is { RawKind: (ushort)SyntaxKindEx.PragmaChecksumDirectiveTrivia } pragmaChecksum)
        {
            // The format is: #pragma checksum "filename" "{guid}" "checksum bytes"
            // See https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives#pragma-checksum
            var content = pragmaChecksum.ToString();
            var firstIndex = content.IndexOf('"');
            var start = firstIndex + 1;
            var secondIndex = content.IndexOf('"', start);
            return content.Substring(start, secondIndex - start);
        }

        return root.SyntaxTree.FilePath;
    }

    public static TSyntaxKind Kind<TSyntaxKind>(this SyntaxNode node) where TSyntaxKind : struct, Enum =>
        node is null ? default : (TSyntaxKind)Enum.ToObject(typeof(TSyntaxKind), node.RawKind);

    public static SecondaryLocation ToSecondaryLocation(this SyntaxNode node, string message = null, params string[] messageArgs) =>
        message is not null && messageArgs?.Length > 0
            ? new(node.GetLocation(), string.Format(message, messageArgs))
            : new(node.GetLocation(), message);

    public static int LineNumberToReport(this SyntaxNode node) =>
        node.GetLocation().LineNumberToReport();

    public static bool HasFlagsAttribute(this SyntaxNode node, SemanticModel model) =>
        model.GetDeclaredSymbol(node).HasAttribute(KnownType.System_FlagsAttribute);

    public static Location CreateLocation(this SyntaxNode from, SyntaxNode to) =>
        Location.Create(from.SyntaxTree, TextSpan.FromBounds(from.SpanStart, to.Span.End));

    public static Location CreateLocation(this SyntaxNode from, SyntaxToken to) =>
        Location.Create(from.SyntaxTree, TextSpan.FromBounds(from.SpanStart, to.Span.End));
}
