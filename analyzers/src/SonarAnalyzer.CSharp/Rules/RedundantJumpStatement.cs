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

using SonarAnalyzer.CFG.Sonar;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantJumpStatement : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S3626";
    private const string MessageFormat = "Remove this redundant jump.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            CheckForRedundantJumps,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.DestructorDeclaration,
            SyntaxKind.ConversionOperatorDeclaration,
            SyntaxKind.OperatorDeclaration,
            SyntaxKindEx.LocalFunctionStatement,
            SyntaxKind.GetAccessorDeclaration,
            SyntaxKind.SetAccessorDeclaration,
            SyntaxKindEx.InitAccessorDeclaration,
            SyntaxKind.AddAccessorDeclaration,
            SyntaxKind.RemoveAccessorDeclaration,
            SyntaxKind.AnonymousMethodExpression,
            SyntaxKind.SimpleLambdaExpression,
            SyntaxKind.ParenthesizedLambdaExpression);

    private static void CheckForRedundantJumps(SonarSyntaxNodeReportingContext context)
    {
        if (!CSharpControlFlowGraph.TryGet(context.Node, context.Model, out var cfg))
        {
            return;
        }

        var yieldStatementCount = context.Node.DescendantNodes().OfType<YieldStatementSyntax>().Count();

        var removableJumps = cfg.Blocks.OfType<JumpBlock>().Where(x => IsJumpRemovable(x, yieldStatementCount));

        foreach (var jumpBlock in removableJumps)
        {
            context.ReportIssue(Rule, jumpBlock.JumpNode);
        }
    }

    private static bool IsJumpRemovable(JumpBlock jumpBlock, int yieldStatementCount) =>
        !IsInsideSwitch(jumpBlock)
        && !IsReturnWithExpression(jumpBlock)
        && !IsThrow(jumpBlock)
        && !IsYieldReturn(jumpBlock)
        && !IsOnlyYieldBreak(jumpBlock, yieldStatementCount)
        && !IsValidJumpInsideTryCatch(jumpBlock)
        && !IsReturnWithFollowingLocalFunction(jumpBlock)
        && jumpBlock.SuccessorBlock == jumpBlock.WouldBeSuccessor;

    private static bool IsValidJumpInsideTryCatch(JumpBlock jumpBlock) =>
        jumpBlock.WouldBeSuccessor is BranchBlock branchBlock
        && branchBlock.BranchingNode is FinallyClauseSyntax
        && branchBlock.AllSuccessorBlocks.Count > 1;

    private static bool IsInsideSwitch(JumpBlock jumpBlock) =>
        // Not reporting inside switch, as the jumps might not be removable
        jumpBlock.JumpNode.AncestorsAndSelf().OfType<SwitchStatementSyntax>().Any();

    private static bool IsYieldReturn(JumpBlock jumpBlock) =>
        // yield return cannot be redundant
        jumpBlock.JumpNode is YieldStatementSyntax yieldStatement
        && yieldStatement.IsKind(SyntaxKind.YieldReturnStatement);

    private static bool IsOnlyYieldBreak(JumpBlock jumpBlock, int yieldStatementCount) =>
        jumpBlock.JumpNode is YieldStatementSyntax yieldStatement
        && yieldStatement.IsKind(SyntaxKind.YieldBreakStatement)
        && yieldStatementCount == 1;

    private static bool IsThrow(JumpBlock jumpBlock) =>
        jumpBlock.JumpNode.Kind() is SyntaxKind.ThrowStatement or SyntaxKindEx.ThrowExpression;

    private static bool IsReturnWithExpression(JumpBlock jumpBlock) =>
        jumpBlock.JumpNode is ReturnStatementSyntax { Expression: not null };

    private static bool IsReturnWithFollowingLocalFunction(JumpBlock jumpBlock) =>
        jumpBlock.JumpNode.IsKind(SyntaxKind.ReturnStatement)
        && jumpBlock.JumpNode.Parent.ChildNodes().Reverse().TakeWhile(x => x != jumpBlock.JumpNode).Any(x => x.IsKind(SyntaxKindEx.LocalFunctionStatement));
}
