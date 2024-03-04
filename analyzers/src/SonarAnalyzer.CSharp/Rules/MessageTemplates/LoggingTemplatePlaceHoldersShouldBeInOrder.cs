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
        var templateArguments = PlaceholderValues(invocation, methodSymbol).ToImmutableArray();
        for (var i = 0; i < placeholders.Length; i++)
        {
            var placeholder = placeholders[i];
            if (placeholder.Name != "_"
                && !int.TryParse(placeholder.Name, out _)
                && OutOfOrderTemplateArgumentForPlaceHolder(placeholder, i, templateArguments) is { } outOfOrderArgument
                && !placeholders.Take(i).Any(x => x.Name == placeholder.Name)) // don't raise for duplicate placeholders
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
        var placeholderArguments = methodSymbol.Parameters.Where(x => x.Name is "args"
            || x.Name.StartsWith("argument")
            || x.Name.StartsWith("propertyValue"));
        foreach (var placeholderArgument in placeholderArguments)
        {
            var paramIndex = methodSymbol.Parameters.IndexOf(placeholderArgument);
            if (invocation.ArgumentList.Arguments.FirstOrDefault(x => x.NameColon?.GetName() == placeholderArgument.Name) is { } argumentValue)
            {
                yield return argumentValue.Expression;
            }
            else if (paramIndex < invocation.ArgumentList.Arguments.Count)
            {
                var argumentLimit = placeholderArgument.IsParams
                    ? invocation.ArgumentList.Arguments.Count
                    : paramIndex + 1;
                for (var i = paramIndex; i < argumentLimit; i++)
                {
                    yield return invocation.ArgumentList.Arguments[i].Expression;
                }
            }
        }
    }

    private static SyntaxNode OutOfOrderTemplateArgumentForPlaceHolder(MessageTemplatesParser.Placeholder placeholder, int placeholderIndex, ImmutableArray<SyntaxNode> templateArguments)
    {
        if (placeholderIndex < templateArguments.Length && PlaceholderMatchesTemplateArgument(placeholder.Name, templateArguments[placeholderIndex]))
        {
            return null;
        }
        else
        {
            for (var i = 0; i < templateArguments.Length; i++)
            {
                if (i != placeholderIndex && PlaceholderMatchesTemplateArgument(placeholder.Name, templateArguments[i]))
                {
                    return templateArguments[placeholderIndex];
                }
            }
        }
        return null;
    }

    private static bool PlaceholderMatchesTemplateArgument(string placeholderName, SyntaxNode templateArgument)
    {
        if (templateArgument is MemberAccessExpressionSyntax memberAccess)
        {
            return PlaceholderMatchesTemplateArgument(placeholderName, memberAccess.Expression)
                || PlaceholderMatchesTemplateArgument(placeholderName, memberAccess.Name);
        }
        else if (templateArgument is ObjectCreationExpressionSyntax)
        {
            return false;
        }
        else
        {
            var filteredName = new string(placeholderName.Where(char.IsLetterOrDigit).ToArray());
            return filteredName.Equals(templateArgument.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
