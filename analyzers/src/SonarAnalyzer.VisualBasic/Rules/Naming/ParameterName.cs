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

namespace SonarAnalyzer.Rules.VisualBasic;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class ParameterName : ParametrizedDiagnosticAnalyzer
{
    private const string DiagnosticId = "S1654";
    private const string MessageFormat = "Rename this parameter to match the regular expression: '{0}'.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat, isEnabledByDefault: false);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    [RuleParameter("format", PropertyType.String, "Regular expression used to check the parameter names against.", NamingHelper.CamelCasingPattern)]
    public string Pattern { get; set; } = NamingHelper.CamelCasingPattern;

    protected override void Initialize(SonarParametrizedAnalysisContext context) =>
        context.RegisterNodeAction(
            c =>
            {
                var parameter = (ParameterSyntax)c.Node;
                if (parameter.Identifier is not null
                    && !HasPredefinedName(parameter)
                    && !NamingHelper.IsRegexMatch(parameter.Identifier.Identifier.ValueText, Pattern))
                {
                    c.ReportIssue(Rule, parameter.Identifier.Identifier, Pattern);
                }
            },
            SyntaxKind.Parameter);

    private static bool HasPredefinedName(SyntaxNode node)
    {
        while (node is not null)
        {
            if (node is MethodStatementSyntax method)
            {
                return method.Modifiers.Any(SyntaxKind.OverridesKeyword) || method.ImplementsClause is not null || method.HandlesClause is not null;
            }
            else if (node is PropertyStatementSyntax property)
            {
                return property.Modifiers.Any(SyntaxKind.OverridesKeyword) || property.ImplementsClause is not null;
            }
            else
            {
                node = node.Parent;
            }
        }
        return false;
    }
}
