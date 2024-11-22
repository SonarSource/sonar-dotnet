/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Core.Extensions;

public static class SyntaxNodeExtensions
{
    public static bool IsKnownType(this SyntaxNode syntaxNode, KnownType knownType, SemanticModel semanticModel)
    {
        var type = semanticModel.GetSymbolInfo(syntaxNode).Symbol.GetSymbolType();
        return type.Is(knownType) || type?.OriginalDefinition?.Is(knownType) == true;
    }

    public static bool IsDeclarationKnownType(this SyntaxNode syntaxNode, KnownType knownType, SemanticModel semanticModel) =>
        semanticModel.GetDeclaredSymbol(syntaxNode)?.GetSymbolType().Is(knownType) ?? false;

    public static SemanticModel EnsureCorrectSemanticModelOrDefault(this SyntaxNode node, SemanticModel model) =>
        node.SyntaxTree.GetSemanticModelOrDefault(model);

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

    public static SecondaryLocation ToSecondaryLocation(this SyntaxNode node, string message = null) =>
        new(node.GetLocation(), message);
}
