/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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
                    c.ReportIssue(Rule, c.Node);
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
