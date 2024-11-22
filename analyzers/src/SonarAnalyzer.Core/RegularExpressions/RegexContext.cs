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

using System.Text.RegularExpressions;

namespace SonarAnalyzer.RegularExpressions;

internal sealed class RegexContext
{
    private static readonly RegexOptions ValidationMask = (RegexOptions)int.MaxValue ^ RegexOptions.Compiled;

    private static readonly string[] MatchMethods = new[]
    {
        nameof(Regex.IsMatch),
        nameof(Regex.Match),
        nameof(Regex.Matches),
        nameof(Regex.Replace),
        nameof(Regex.Split),
        "EnumerateSplits",  // https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.enumeratesplits?view=net-9.0
        "EnumerateMatches", // https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.enumeratematches?view=net-9.0
    };

    public SyntaxNode PatternNode { get; }
    public string Pattern { get; }
    public SyntaxNode OptionsNode { get; }
    public RegexOptions? Options { get; }
    public Regex Regex { get; }
    public Exception ParseError { get; }

    public RegexContext(SyntaxNode patternNode, string pattern, SyntaxNode optionsNode, RegexOptions? options)
    {
        PatternNode = patternNode;
        Pattern = pattern;
        OptionsNode = optionsNode;
        Options = options;

        if (!string.IsNullOrEmpty(pattern))
        {
            try
            {
                Regex = new(Pattern, options.GetValueOrDefault() & ValidationMask, RegexConstants.DefaultTimeout);
            }
            catch (Exception ex)
            {
                ParseError = ex;
            }
        }
    }

    public static RegexContext FromAttribute<TSyntaxKind>(ILanguageFacade<TSyntaxKind> language, SemanticModel model, SyntaxNode node) where TSyntaxKind : struct
    {
        if (model.GetSymbolInfo(node).Symbol is IMethodSymbol method
            && method.IsInType(KnownType.System_ComponentModel_DataAnnotations_RegularExpressionAttribute))
        {
            var parameters = language.MethodParameterLookup(node, method);
            var pattern = TryGetNonParamsSyntax(method, parameters, "pattern");
            return new RegexContext(pattern, language.FindConstantValue(model, pattern) as string, null, null);
        }
        else
        {
            return null;
        }
    }

    public static RegexContext FromCtor<TSyntaxKind>(ILanguageFacade<TSyntaxKind> language, SemanticModel model, SyntaxNode node) where TSyntaxKind : struct =>
        model.GetSymbolInfo(node).Symbol is IMethodSymbol method
        && method.IsConstructor()
        && method.ContainingType.Is(KnownType.System_Text_RegularExpressions_Regex)
            ? FromMethod(language, model, node, method)
            : null;

    public static RegexContext FromMethod<TSyntaxKind>(ILanguageFacade<TSyntaxKind> language, SemanticModel model, SyntaxNode node) where TSyntaxKind : struct =>
        language.Syntax.NodeIdentifier(node).GetValueOrDefault().Text is { } name
        && MatchMethods.Any(x => name.Equals(x, language.NameComparison))
        && model.GetSymbolInfo(node).Symbol is IMethodSymbol { IsStatic: true } method
        && method.ContainingType.Is(KnownType.System_Text_RegularExpressions_Regex)
            ? FromMethod(language, model, node, method)
            : null;

    private static RegexContext FromMethod<TSyntaxKind>(ILanguageFacade<TSyntaxKind> language, SemanticModel model, SyntaxNode node, IMethodSymbol method) where TSyntaxKind : struct
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
}
