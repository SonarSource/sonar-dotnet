/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

#nullable enable

using System.Text.RegularExpressions;

namespace SonarAnalyzer.RegularExpressions;

internal sealed class RegexNode
{
    public RegexNode(
        SyntaxNodeValue<string?> pattern,
        SyntaxNodeValue<RegexOptions?> options)
    {
        Pattern = pattern;
        Options = options;
    }

    public SyntaxNodeValue<string?> Pattern { get; }
    public SyntaxNodeValue<RegexOptions?> Options { get; }

    public static RegexNode? FromCtor<TSyntaxKind>(SyntaxNode node, SemanticModel model, ILanguageFacade<TSyntaxKind> language) where TSyntaxKind : struct =>
        language.Syntax.IsAnyKind(node, language.SyntaxKind.ObjectCreationExpressions)
        && model.GetSymbolInfo(node).Symbol is IMethodSymbol method
        && method.ContainingType.Is(KnownType.System_Text_RegularExpressions_Regex)
        && method.IsConstructor()
        ? FromSymbol(method, node, model, language)
        : null;

    public static RegexNode? FromMethod<TSyntaxKind>(SyntaxNode node, SemanticModel model, ILanguageFacade<TSyntaxKind> language) where TSyntaxKind : struct =>
        language.Syntax.IsKind(node, language.SyntaxKind.InvocationExpression)
        && language.Syntax.NodeIdentifier(node).GetValueOrDefault().Text is { } name
        && MatchMethods.Any(x => x.Equals(name, language.NameComparison))
        && model.GetSymbolInfo(node).Symbol is IMethodSymbol method
        && method.ContainingType.Is(KnownType.System_Text_RegularExpressions_Regex)
        ? FromSymbol(method, node, model, language)
        : null;

    public static RegexNode? FromAttribute<TSyntaxKind>(SyntaxNode node, SemanticModel model, ILanguageFacade<TSyntaxKind> language) where TSyntaxKind : struct
    {
        if (model.GetSymbolInfo(node).Symbol is IMethodSymbol method
            && method.IsInType(KnownType.System_ComponentModel_DataAnnotations_RegularExpressionAttribute))
        {
            var parameters = language.MethodParameterLookup(node, method);
            var pattern = TryGetNonParamsSyntax(method, parameters, "pattern");
            return new RegexNode(
             new(pattern, language.FindConstantValue(model, pattern) as string),
             new(null, null));
        }
        return null;
    }

    private static RegexNode FromSymbol<TSyntaxKind>(IMethodSymbol method, SyntaxNode node, SemanticModel model, ILanguageFacade<TSyntaxKind> language) where TSyntaxKind : struct
    {
        var parameters = language.MethodParameterLookup(node, method);
        var pattern = TryGetNonParamsSyntax(method, parameters, "pattern");
        var options = TryGetNonParamsSyntax(method, parameters, "options");

        return new RegexNode(
            new(pattern, language.FindConstantValue(model, pattern) as string),
            new(options, language.FindConstantValue(model, options) is RegexOptions value ? value : null));
    }

    private static SyntaxNode? TryGetNonParamsSyntax(IMethodSymbol method, IMethodParameterLookup parameters, string paramName) =>
        method.Parameters.SingleOrDefault(x => x.Name == paramName) is { } param
        && parameters.TryGetNonParamsSyntax(param, out var node)
        ? node
        : null;

    private static readonly IReadOnlyList<string> MatchMethods = new[]
    {
        nameof(Regex.IsMatch),
        nameof(Regex.Match),
        nameof(Regex.Matches),
        nameof(Regex.Replace),
        nameof(Regex.Split),
    };
}
