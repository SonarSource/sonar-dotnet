/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using SonarAnalyzer.CSharp.Core.Syntax.Utilities;

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidDeconstruction : StylingAnalyzer
{
    private const string DiscardMessage = "It's pointless.";
    private const string VariableMessage = "Reference the member from the instance instead.";

    public AvoidDeconstruction() : base("T0035", "Don't use this deconstruction. {0}") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var varPattern = (VarPatternSyntax)c.Node;
                if (varPattern.Parent is not IsPatternExpressionSyntax and not SwitchExpressionArmSyntax)
                {
                    if (varPattern.Designation is DiscardDesignationSyntax)
                    {
                        c.ReportIssue(Rule, c.Node, DiscardMessage);
                    }
                    else if (varPattern.Designation is SingleVariableDesignationSyntax variable && !IsUsedEnough(c, variable))
                    {
                        c.ReportIssue(Rule, c.Node, VariableMessage);
                    }
                }
            },
            SyntaxKind.VarPattern);

    private static bool IsUsedEnough(SonarSyntaxNodeReportingContext context, SingleVariableDesignationSyntax variable)
    {
        if (context.Node.Ancestors().FirstOrDefault(x => x is BlockSyntax or ArrowExpressionClauseSyntax or LambdaExpressionSyntax) is { } root
            && context.Model.GetDeclaredSymbol(variable) is { } symbol)
        {
            var walker = new IdentifierWalker(context.Model, symbol);
            walker.SafeVisit(root);
            return walker.IsUsedEnough;
        }
        else
        {
            return false;
        }
    }
}

file sealed class IdentifierWalker : SafeCSharpSyntaxWalker
{
    private const int MinUsageCount = 3;

    private readonly SemanticModel model;
    private readonly ISymbol symbol;
    private int usageCount;

    public bool IsUsedEnough => usageCount >= MinUsageCount;

    public IdentifierWalker(SemanticModel model, ISymbol symbol)
    {
        this.model = model;
        this.symbol = symbol;
    }

    public override void Visit(SyntaxNode node)
    {
        if (!IsUsedEnough)  // Performance: Stop searching once we know the answer
        {
            base.Visit(node);
        }
    }

    public override void VisitIdentifierName(IdentifierNameSyntax node)
    {
        if (node.Identifier.ValueText == symbol.Name
            && model.GetSymbolInfo(node).Symbol is { } nodeSymbol
            && symbol.Equals(nodeSymbol, SymbolEqualityComparer.Default))
        {
            usageCount++;
        }
        base.VisitIdentifierName(node);
    }
}
