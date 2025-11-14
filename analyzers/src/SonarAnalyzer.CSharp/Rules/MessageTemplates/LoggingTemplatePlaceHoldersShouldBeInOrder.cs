/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Text;
using SonarAnalyzer.CSharp.Core.RegularExpressions;

namespace SonarAnalyzer.CSharp.Rules.MessageTemplates;

public sealed class LoggingTemplatePlaceHoldersShouldBeInOrder : IMessageTemplateCheck
{
    private const string DiagnosticId = "S6673";
    private const string MessageFormat = "Template placeholders should be in the right order: placeholder '{0}' does not match with argument '{1}'.";
    private const string SecondaryMessageFormat = "The argument should be '{0}' to match placeholder '{1}'.";

    internal static readonly DiagnosticDescriptor S6673 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public DiagnosticDescriptor Rule => S6673;

    public void Execute(SonarSyntaxNodeReportingContext context, InvocationExpressionSyntax invocation, ArgumentSyntax templateArgument, MessageTemplatesParser.Placeholder[] placeholders)
    {
        var methodSymbol = (IMethodSymbol)context.Model.GetSymbolInfo(invocation).Symbol;
        var placeholderValues = PlaceholderValues(invocation, methodSymbol).ToImmutableArray();
        for (var i = 0; i < placeholders.Length; i++)
        {
            var placeholder = placeholders[i];
            if (placeholder.Name != "_"
                && !int.TryParse(placeholder.Name, out _)
                && Array.FindIndex(placeholders, x => x.Name == placeholder.Name) == i // don't raise for duplicate placeholders
                && OutOfOrderPlaceholderValue(placeholder, i, placeholderValues, out var betterFitArgument) is { } outOfOrderArgument)
            {
                var templateStart = templateArgument.Expression.GetLocation().SourceSpan.Start;
                var primaryLocation = Location.Create(context.Tree, new(templateStart + placeholder.Start, placeholder.Length));
                context.ReportIssue(Rule, primaryLocation, SecondaryLocations(outOfOrderArgument, betterFitArgument, placeholder), placeholder.Name, outOfOrderArgument.ToString());
                return; // only raise on the first out-of-order placeholder to make the rule less noisy
            }
        }
    }

    private static IEnumerable<SecondaryLocation> SecondaryLocations(SyntaxNode node, SyntaxNode betterFitArgument, MessageTemplatesParser.Placeholder placeholder) =>
        [node.ToSecondaryLocation(SecondaryMessageFormat, BetterFitName(betterFitArgument), placeholder.Name)];

    private static string BetterFitName(SyntaxNode node) =>
        node is MemberAccessExpressionSyntax or CastExpressionSyntax ? node.ToString() : node.GetName();

    private static IEnumerable<SyntaxNode> PlaceholderValues(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol)
    {
        var parameters = methodSymbol.Parameters.Where(x => x.Name == "args"
            || x.Name.StartsWith("argument")
            || x.Name.StartsWith("propertyValue"))
            .ToArray();
        if (parameters.Length == 0)
        {
            yield break;
        }
        var parameterLookup = CSharpFacade.Instance.MethodParameterLookup(invocation, methodSymbol);
        foreach (var parameter in parameters)
        {
            if (parameterLookup.TryGetSyntax(parameter, out var expressions))
            {
                foreach (var item in expressions)
                {
                    yield return item;
                }
            }
        }
    }

    private static SyntaxNode OutOfOrderPlaceholderValue(
        MessageTemplatesParser.Placeholder placeholder,
        int placeholderIndex,
        ImmutableArray<SyntaxNode> placeholderValues,
        out SyntaxNode betterFitArgument)
    {
        betterFitArgument = null;
        if (placeholderIndex < placeholderValues.Length && MatchesName(placeholder.Name, placeholderValues[placeholderIndex], isStrict: false) is not false)
        {
            return null;
        }
        else
        {
            for (var i = 0; i < placeholderValues.Length; i++)
            {
                if (i != placeholderIndex && placeholderIndex < placeholderValues.Length && MatchesName(placeholder.Name, placeholderValues[i], isStrict: true) is true)
                {
                    betterFitArgument = placeholderValues.Skip(i).FirstOrDefault(x => MatchesName(placeholder.Name, x, isStrict: true) is true);
                    return placeholderValues[placeholderIndex];
                }
            }
        }
        return null;
    }

    private static bool? MatchesName(string placeholderName, SyntaxNode placeholderValue, bool isStrict) =>
        placeholderValue switch
        {
            MemberAccessExpressionSyntax memberAccess => MatchesName(placeholderName, memberAccess.Name, isStrict),
            CastExpressionSyntax cast => MatchesName(placeholderName, cast.Expression, isStrict),
            NameSyntax name => SimpleStringMatches(placeholderName, name.GetName(), isStrict),
            _ => null
        };

    private static bool SimpleStringMatches(string placeholderName, string argumentName, bool isStrict)
    {
        if (isStrict)
        {
            return OnlyLetters(placeholderName).Equals(OnlyLetters(argumentName), StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            var placeholderComponents = SplitByCamelCase(placeholderName);
            var argumentComponents = SplitByCamelCase(argumentName);
            return placeholderComponents.Intersect(argumentComponents, StringComparer.OrdinalIgnoreCase).Any();
        }
    }

    private static IEnumerable<string> SplitByCamelCase(string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            if (char.IsUpper(ch) && builder.Length > 0)
            {
                yield return builder.ToString();
                builder.Clear();
            }
            if (char.IsLetter(ch))
            {
                builder.Append(ch);
            }
        }
        if (builder.Length > 0)
        {
            yield return builder.ToString();
        }
    }

    private static string OnlyLetters(string text) =>
        new(text.Where(char.IsLetter).ToArray());
}
