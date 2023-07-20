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

public abstract class ObsoleteAttributesBase<TSyntaxKind> : SonarDiagnosticAnalyzer
    where TSyntaxKind : struct
{
    private const string ExplanationNeededDiagnosticId = "S1123";
    private const string ExplanationNeededMessageFormat = "Add an explanation.";

    private const string RemoveDiagnosticId = "S1133";
    private const string RemoveMessageFormat = "Do not forget to remove this deprecated code someday.";

    protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

    protected abstract SyntaxNode GetExplanationExpression(SyntaxNode node);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

    internal DiagnosticDescriptor ExplanationNeededRule { get; }
    internal DiagnosticDescriptor RemoveRule { get; }

    protected ObsoleteAttributesBase()
    {
        ExplanationNeededRule = Language.CreateDescriptor(ExplanationNeededDiagnosticId, ExplanationNeededMessageFormat);
        RemoveRule = Language.CreateDescriptor(RemoveDiagnosticId, RemoveMessageFormat);
        SupportedDiagnostics = ImmutableArray.Create(ExplanationNeededRule, RemoveRule);
    }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                if (c.SemanticModel.GetSymbolInfo(c.Node).Symbol is { } attribute
                    && attribute.IsInType(KnownType.System_ObsoleteAttribute))
                {
                    var location = c.Node.GetLocation();
                    c.ReportIssue(CreateDiagnostic(RemoveRule, location));

                    if (NoExplanation(c.Node, c.SemanticModel))
                    {
                        c.ReportIssue(CreateDiagnostic(ExplanationNeededRule, location));
                    }
                }
            },
            Language.SyntaxKind.Attribute);

    private bool NoExplanation(SyntaxNode node, SemanticModel model) =>
        GetExplanationExpression(node) is not { } justification
        || string.IsNullOrWhiteSpace(Language.FindConstantValue(model, justification) as string);
}
