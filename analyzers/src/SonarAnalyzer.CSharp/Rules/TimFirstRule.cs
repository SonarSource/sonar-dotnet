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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TimFirstRule : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S2552";
    private const string MessageFormat = "Tim's first rule!";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        // Part 1
        context.RegisterNodeAction(c =>
            {
                if (c.Node is VariableDeclaratorSyntax node)
                {
                    ReportIfIdentifierIsNotTim(c, node.Identifier, lowerCase: true);
                }
            }, SyntaxKind.VariableDeclarator);
        context.RegisterNodeAction(
            c => ReportIfIdentifierIsNotTim(c, ((SingleVariableDesignationSyntaxWrapper)c.Node).Identifier, lowerCase: true),
            SyntaxKindEx.SingleVariableDesignation);
        context.RegisterNodeAction(c =>
            {
                if (c.Node is ForEachStatementSyntax { Identifier: var identifier })
                {
                    ReportIfIdentifierIsNotTim(c, identifier, lowerCase: true);
                }
            }, SyntaxKind.ForEachStatement);
        // Part 2
        context.RegisterNodeAction(c =>
            {
                if (c.Node is IdentifierNameSyntax { Identifier: var identifier } node
                    && c.SemanticModel.GetSymbolInfo(node).Symbol is { Kind: SymbolKind.Property })
                {
                    ReportIfIdentifierIsNotTim(c, identifier);
                }
            }, SyntaxKind.IdentifierName);
    }

    private static void ReportIfIdentifierIsNotTim(SonarSyntaxNodeReportingContext c, SyntaxToken identifier, bool lowerCase = false)
    {
        var expected = lowerCase switch
        {
            true => "tim",
            false => "Tim"
        };
        if (identifier is { IsMissing: false, ValueText: var valueText } && valueText != expected)
        {
            c.ReportIssue(Diagnostic.Create(Rule, identifier.GetLocation()));
        }
    }
}
