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

using System.Text.RegularExpressions;

namespace SonarAnalyzer.RegularExpressions;

internal sealed class RegexContext
{
    private static readonly RegexOptions TestMask = (RegexOptions)int.MinValue ^ RegexOptions.Compiled;

    public SyntaxNode PatternNode { get; }
    public string Pattern { get; }
    public SyntaxNode OptionsNode { get; }
    public RegexOptions? Options { get; }
    public Regex Regex { get; }
    public Exception ParseError { get; }

    public RegexContext(
        SyntaxNode patternNode,
        string pattern,
        SyntaxNode optionsNode,
        RegexOptions? options)
    {
        PatternNode = patternNode;
        Pattern = pattern;
        OptionsNode = optionsNode;
        Options = options;

        if (!string.IsNullOrEmpty(pattern))
        {
            try
            {
                Regex = new(Pattern, options.GetValueOrDefault() & TestMask, TimeSpan.FromMilliseconds(100));
            }
            catch (Exception x)
            {
                ParseError = x;
            }
        }
    }

    public static RegexContext FromCtor<TSyntaxKind>(SyntaxNode node, SemanticModel model, ILanguageFacade<TSyntaxKind> language) where TSyntaxKind : struct =>
        model.GetSymbolInfo(node).Symbol is IMethodSymbol method
        && method.IsConstructor()
        && method.ContainingType.Is(KnownType.System_Text_RegularExpressions_Regex)
            ? FromSymbol(method, node, model, language)
            : null;

    public static RegexContext FromMethod<TSyntaxKind>(SyntaxNode node, SemanticModel model, ILanguageFacade<TSyntaxKind> language) where TSyntaxKind : struct =>
        language.Syntax.NodeIdentifier(node).GetValueOrDefault().Text is { } name
        && MatchMethods.Any(x => x.Equals(name, language.NameComparison))
        && model.GetSymbolInfo(node).Symbol is IMethodSymbol { IsStatic: true } method
        && method.ContainingType.Is(KnownType.System_Text_RegularExpressions_Regex)
            ? FromSymbol(method, node, model, language)
            : null;

    public static RegexContext FromAttribute<TSyntaxKind>(SyntaxNode node, SemanticModel model, ILanguageFacade<TSyntaxKind> language) where TSyntaxKind : struct
    {
        if (model.GetSymbolInfo(node).Symbol is IMethodSymbol method
            && method.IsInType(KnownType.System_ComponentModel_DataAnnotations_RegularExpressionAttribute))
        {
            var parameters = language.MethodParameterLookup(node, method);
            var pattern = TryGetNonParamsSyntax(method, parameters, "pattern");
            return new RegexContext(
                pattern,
                language.FindConstantValue(model, pattern) as string,
                null,
                null);
        }
        return null;
    }

    private static RegexContext FromSymbol<TSyntaxKind>(IMethodSymbol method, SyntaxNode node, SemanticModel model, ILanguageFacade<TSyntaxKind> language) where TSyntaxKind : struct
    {
        var parameters = language.MethodParameterLookup(node, method);
        var pattern = TryGetNonParamsSyntax(method, parameters, "pattern");
        var options = TryGetNonParamsSyntax(method, parameters, "options");

        return new RegexContext(
            pattern,
            language.FindConstantValue(model, pattern) as string,
            options,
            language.FindConstantValue(model, options) is RegexOptions value ? value : null);
    }

    private static SyntaxNode TryGetNonParamsSyntax(IMethodSymbol method, IMethodParameterLookup parameters, string paramName) =>
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
