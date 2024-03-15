/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.MessageTemplates;

public sealed class LoggingTemplatePlaceHoldersShouldBeInOrder : IMessageTemplateCheck
{
    private const string DiagnosticId = "S6673";
    private const string MessageFormat = "Template placeholders should be in the right order: placeholder '{0}' does not match with argument '{1}'.";

    internal static readonly DiagnosticDescriptor S6673 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public DiagnosticDescriptor Rule => S6673;

    public void Execute(SonarSyntaxNodeReportingContext context, InvocationExpressionSyntax invocation, ArgumentSyntax templateArgument, MessageTemplatesParser.Placeholder[] placeholders)
    {
        var methodSymbol = (IMethodSymbol)context.SemanticModel.GetSymbolInfo(invocation).Symbol;
        var placeholderValues = PlaceholderValues(invocation, methodSymbol).ToImmutableArray();
        for (var i = 0; i < placeholders.Length; i++)
        {
            var placeholder = placeholders[i];
            if (placeholder.Name != "_"
                && !int.TryParse(placeholder.Name, out _)
                && Array.FindIndex(placeholders, x => x.Name == placeholder.Name) == i // don't raise for duplicate placeholders
                && OutOfOrderPlaceholderValue(placeholder, i, placeholderValues) is { } outOfOrderArgument)
            {
                var templateStart = templateArgument.Expression.GetLocation().SourceSpan.Start;
                var primaryLocation = Location.Create(context.Tree, new(templateStart + placeholder.Start, placeholder.Length));
                var secondaryLocation = outOfOrderArgument.GetLocation();
                context.ReportIssue(Diagnostic.Create(Rule, primaryLocation, [secondaryLocation], placeholder.Name, outOfOrderArgument.ToString()));
                return; // only raise on the first out-of-order placeholder to make the rule less noisy
            }
        }
    }

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

    private static SyntaxNode OutOfOrderPlaceholderValue(MessageTemplatesParser.Placeholder placeholder, int placeholderIndex, ImmutableArray<SyntaxNode> placeholderValues)
    {
        if (placeholderIndex < placeholderValues.Length && MatchesName(placeholder.Name, placeholderValues[placeholderIndex]) is not false)
        {
            return null;
        }
        else
        {
            for (var i = 0; i < placeholderValues.Length; i++)
            {
                if (i != placeholderIndex && placeholderIndex < placeholderValues.Length && MatchesName(placeholder.Name, placeholderValues[i]) is true)
                {
                    return placeholderValues[placeholderIndex];
                }
            }
        }
        return null;
    }

    private static bool? MatchesName(string placeholderName, SyntaxNode placeholderValue) =>
        placeholderValue switch
        {
            MemberAccessExpressionSyntax memberAccess => MatchesName(placeholderName, memberAccess.Name),
            CastExpressionSyntax cast => MatchesName(placeholderName, cast.Expression),
            NameSyntax name => new string(placeholderName.Where(char.IsLetterOrDigit).ToArray()).Equals(name.GetName(), StringComparison.OrdinalIgnoreCase),
            _ => null
        };
}
