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

using System.Xml.Linq;

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
        context.RegisterNodeAction(c =>
            {
                if (c.Node is LocalDeclarationStatementSyntax { Declaration.Variables: var variables } node
                    && variables.Any(x => IdentifierIsNotTim(x.Identifier)))
                {
                    ReportIssue(c, node);
                }
            }, SyntaxKind.LocalDeclarationStatement);
        context.RegisterNodeAction(c =>
            {
                var node = c.Node;
                if (IdentifierIsNotTim(((SingleVariableDesignationSyntaxWrapper)node).Identifier))
                {
                    ReportIssue(c, node);
                }
            }, SyntaxKindEx.SingleVariableDesignation);
    }

    private static void ReportIssue(SonarSyntaxNodeReportingContext c, SyntaxNode node) =>
        c.ReportIssue(Diagnostic.Create(Rule, node.GetLocation()));
    private static bool IdentifierIsNotTim(SyntaxToken identifier) => identifier is { IsMissing: false, ValueText: not "tim" };
}
