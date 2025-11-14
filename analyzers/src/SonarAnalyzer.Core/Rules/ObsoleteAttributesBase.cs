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

namespace SonarAnalyzer.Core.Rules;

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
                if (c.Model.GetSymbolInfo(c.Node).Symbol is { } attribute
                    && attribute.IsInType(KnownType.System_ObsoleteAttribute))
                {
                    var location = c.Node.GetLocation();
                    c.ReportIssue(RemoveRule, location);

                    if (NoExplanation(c.Node, c.Model))
                    {
                        c.ReportIssue(ExplanationNeededRule, location);
                    }
                }
            },
            Language.SyntaxKind.Attribute);

    private bool NoExplanation(SyntaxNode node, SemanticModel model) =>
        GetExplanationExpression(node) is not { } justification
        || string.IsNullOrWhiteSpace(Language.FindConstantValue(model, justification) as string);
}
