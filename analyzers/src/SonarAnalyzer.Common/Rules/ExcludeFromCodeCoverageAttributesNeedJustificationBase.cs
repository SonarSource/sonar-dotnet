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

namespace SonarAnalyzer.Rules;

public abstract class ExcludeFromCodeCoverageAttributesNeedJustificationBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6513";

    protected const string JustificationPropertyName = "Justification";

    protected override string MessageFormat => "Add a justification.";

    protected abstract SyntaxNode GetJustificationExpression(SyntaxNode node);

    protected ExcludeFromCodeCoverageAttributesNeedJustificationBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                if (NoJustification(c.Node, c.SemanticModel)
                    && c.SemanticModel.GetSymbolInfo(c.Node).Symbol is IMethodSymbol attribute
                    && attribute.IsInType(KnownType.System_Diagnostics_CodeAnalysis_ExcludeFromCodeCoverageAttribute)
                    && HasJustificationProperty(attribute.ContainingType))
                {
                    c.ReportIssue(CreateDiagnostic(Rule, c.Node.GetLocation()));
                }
            },
            Language.SyntaxKind.Attribute);

    private bool NoJustification(SyntaxNode node, SemanticModel model) =>
        GetJustificationExpression(node) is not { } justification
        || string.IsNullOrWhiteSpace(Language.FindConstantValue(model, justification) as string);

    /// <summary>"Justification" was added in .Net 5, while ExcludeFromCodeCoverage in netstandard2.0.</summary>
    private static bool HasJustificationProperty(INamedTypeSymbol symbol) =>
        symbol.MemberNames.Contains(JustificationPropertyName);
}
