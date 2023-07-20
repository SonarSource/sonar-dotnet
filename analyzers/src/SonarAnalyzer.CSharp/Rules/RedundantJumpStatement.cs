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

using SonarAnalyzer.CFG.Sonar;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RedundantJumpStatement : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3626";
        private const string MessageFormat = "Remove this redundant jump.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c => CheckForRedundantJumps(c, ((BaseMethodDeclarationSyntax)c.Node).Body),
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DestructorDeclaration,
                SyntaxKind.ConversionOperatorDeclaration,
                SyntaxKind.OperatorDeclaration);

            context.RegisterNodeAction(
                c => CheckForRedundantJumps(c, ((LocalFunctionStatementSyntaxWrapper)c.Node).Body),
                SyntaxKindEx.LocalFunctionStatement);

            context.RegisterNodeAction(
                c => CheckForRedundantJumps(c, ((AccessorDeclarationSyntax)c.Node).Body),
                SyntaxKind.GetAccessorDeclaration,
                SyntaxKind.SetAccessorDeclaration,
                SyntaxKindEx.InitAccessorDeclaration,
                SyntaxKind.AddAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration);

            context.RegisterNodeAction(
                c => CheckForRedundantJumps(c, ((AnonymousFunctionExpressionSyntax)c.Node).Body),
                SyntaxKind.AnonymousMethodExpression,
                SyntaxKind.SimpleLambdaExpression,
                SyntaxKind.ParenthesizedLambdaExpression);
        }

        private static void CheckForRedundantJumps(SonarSyntaxNodeReportingContext context, CSharpSyntaxNode node)
        {
            if (!CSharpControlFlowGraph.TryGet(node, context.SemanticModel, out var cfg))
            {
                return;
            }

            var yieldStatementCount = node.DescendantNodes().OfType<YieldStatementSyntax>().Count();

            var removableJumps = cfg.Blocks
                                    .OfType<JumpBlock>()
                                    .Where(jumpBlock => IsJumpRemovable(jumpBlock, yieldStatementCount));

            foreach (var jumpBlock in removableJumps)
            {
                context.ReportIssue(CreateDiagnostic(Rule, jumpBlock.JumpNode.GetLocation()));
            }
        }

        private static bool IsJumpRemovable(JumpBlock jumpBlock, int yieldStatementCount) =>
            !IsInsideSwitch(jumpBlock)
            && !IsReturnWithExpression(jumpBlock)
            && !IsThrow(jumpBlock)
            && !IsYieldReturn(jumpBlock)
            && !IsOnlyYieldBreak(jumpBlock, yieldStatementCount)
            && !IsValidJumpInsideTryCatch(jumpBlock)
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
            jumpBlock.JumpNode.IsAnyKind(SyntaxKind.ThrowStatement, SyntaxKindEx.ThrowExpression);

        private static bool IsReturnWithExpression(JumpBlock jumpBlock) =>
            jumpBlock.JumpNode is ReturnStatementSyntax { Expression: { } };
    }
}
