/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.ControlFlowGraph;
using SonarAnalyzer.ControlFlowGraph.CSharp;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class RedundantJumpStatement : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3626";
        private const string MessageFormat = "Remove this redundant jump.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (BaseMethodDeclarationSyntax)c.Node;
                    CheckForRedundantJumps(declaration.Body, c);
                },
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DestructorDeclaration,
                SyntaxKind.ConversionOperatorDeclaration,
                SyntaxKind.OperatorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (LocalFunctionStatementSyntaxWrapper)c.Node;
                    CheckForRedundantJumps(declaration.Body, c);
                },
                SyntaxKindEx.LocalFunctionStatement);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (AccessorDeclarationSyntax)c.Node;
                    CheckForRedundantJumps(declaration.Body, c);
                },
                SyntaxKind.GetAccessorDeclaration,
                SyntaxKind.SetAccessorDeclaration,
                SyntaxKind.AddAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (AnonymousFunctionExpressionSyntax)c.Node;
                    CheckForRedundantJumps(declaration.Body, c);
                },
                SyntaxKind.AnonymousMethodExpression,
                SyntaxKind.SimpleLambdaExpression,
                SyntaxKind.ParenthesizedLambdaExpression);
        }

        private static void CheckForRedundantJumps(CSharpSyntaxNode node, SyntaxNodeAnalysisContext context)
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
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, jumpBlock.JumpNode.GetLocation()));
            }
        }

        private static bool IsJumpRemovable(JumpBlock jumpBlock, int yieldStatementCount)
        {
            return !IsInsideSwitch(jumpBlock) &&
                   !IsReturnWithExpression(jumpBlock) &&
                   !IsThrow(jumpBlock) &&
                   !IsYieldReturn(jumpBlock) &&
                   !IsOnlyYieldBreak(jumpBlock, yieldStatementCount) &&
                   !IsValidJumpInsideTryCatch(jumpBlock) &&
                   jumpBlock.SuccessorBlock == jumpBlock.WouldBeSuccessor;
        }

        private static bool IsValidJumpInsideTryCatch(JumpBlock jumpBlock)
        {
            return jumpBlock.WouldBeSuccessor is BranchBlock branchBlock &&
                branchBlock.BranchingNode is FinallyClauseSyntax &&
                branchBlock.AllSuccessorBlocks.Count > 1;
        }

        private static bool IsInsideSwitch(JumpBlock jumpBlock)
        {
            // Not reporting inside switch, as the jumps might not be removable
            return jumpBlock.JumpNode.AncestorsAndSelf().OfType<SwitchStatementSyntax>().Any();
        }

        private static bool IsYieldReturn(JumpBlock jumpBlock)
        {
            // yield return cannot be redundant
            return jumpBlock.JumpNode is YieldStatementSyntax yieldStatement &&
                yieldStatement.IsKind(SyntaxKind.YieldReturnStatement);
        }

        private static bool IsOnlyYieldBreak(JumpBlock jumpBlock, int yieldStatementCount)
        {
            return jumpBlock.JumpNode is YieldStatementSyntax yieldStatement &&
                yieldStatement.IsKind(SyntaxKind.YieldBreakStatement) &&
                yieldStatementCount == 1;
        }

        private static bool IsThrow(JumpBlock jumpBlock)
        {
            return jumpBlock.JumpNode is ThrowStatementSyntax;
        }

        private static bool IsReturnWithExpression(JumpBlock jumpBlock)
        {
            return jumpBlock.JumpNode is ReturnStatementSyntax returnStatement && returnStatement.Expression != null;
        }
    }
}
