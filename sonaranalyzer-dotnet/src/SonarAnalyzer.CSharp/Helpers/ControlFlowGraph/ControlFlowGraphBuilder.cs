/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers.FlowAnalysis.Common;

namespace SonarAnalyzer.Helpers.FlowAnalysis.CSharp
{
    internal sealed class ControlFlowGraphBuilder : Common.ControlFlowGraphBuilder
    {
        private readonly Stack<Block> BreakTarget = new Stack<Block>();
        private readonly Stack<Block> ContinueTargets = new Stack<Block>();
        private readonly Stack<Dictionary<object, List<JumpBlock>>> SwitchGotoJumpBlocks = new Stack<Dictionary<object, List<JumpBlock>>>();
        private readonly Dictionary<string, List<JumpBlock>> GotoJumpBlocks = new Dictionary<string, List<JumpBlock>>();
        private readonly Dictionary<string, JumpBlock> LabeledStatements = new Dictionary<string, JumpBlock>();
        private static readonly object GotoDefaultEntry = new object();
        private static readonly object GotoNullEntry = new object();

        internal ControlFlowGraphBuilder(CSharpSyntaxNode node, SemanticModel semanticModel)
            : base(node, semanticModel)
        {
        }

        #region Fix jump statements

        protected override void PostProcessGraph()
        {
            FixJumps(GotoJumpBlocks, LabeledStatements.ToDictionary(e => e.Key, e => (Block)e.Value));

            base.PostProcessGraph();
        }

        private void FixJumps<TLabel>(Dictionary<TLabel, List<JumpBlock>> jumpsToFix,
            Dictionary<TLabel, Block> collectedJumpTargets)
        {
            foreach (var jumpToFix in jumpsToFix)
            {
                if (!collectedJumpTargets.ContainsKey(jumpToFix.Key))
                {
                    throw new InvalidOperationException("Jump to non-existent location");
                }

                foreach (var jumpBlock in jumpToFix.Value)
                {
                    reversedBlocks.Remove(jumpBlock.SuccessorBlock);
                    jumpBlock.SuccessorBlock = collectedJumpTargets[jumpToFix.Key];
                }
            }
        }

        #endregion

        #region Top level Build*

        protected override void Build(SyntaxNode node)
        {
            var statement = node as StatementSyntax;
            if (statement != null)
            {
                BuildStatement(statement);
                return;
            }
            var expression = node as ExpressionSyntax;
            if (expression != null)
            {
                BuildExpression(expression);
                return;
            }

            throw new ArgumentException("Neither a statement, nor an expression", nameof(node));
        }

        private void BuildStatement(StatementSyntax statement)
        {
            switch (statement.Kind())
            {
                case SyntaxKind.Block:
                    BuildBlock((BlockSyntax)statement);
                    break;
                case SyntaxKind.ExpressionStatement:
                    BuildExpression(((ExpressionStatementSyntax)statement).Expression);
                    break;
                case SyntaxKind.LocalDeclarationStatement:
                    BuildVariableDeclaration(((LocalDeclarationStatementSyntax)statement).Declaration);
                    break;

                case SyntaxKind.IfStatement:
                    BuildIfStatement((IfStatementSyntax)statement);
                    break;
                case SyntaxKind.WhileStatement:
                    BuildWhileStatement((WhileStatementSyntax)statement);
                    break;
                case SyntaxKind.DoStatement:
                    BuildDoStatement((DoStatementSyntax)statement);
                    break;
                case SyntaxKind.ForStatement:
                    BuildForStatement((ForStatementSyntax)statement);
                    break;
                case SyntaxKind.ForEachStatement:
                    BuildForEachStatement((ForEachStatementSyntax)statement);
                    break;

                case SyntaxKind.LockStatement:
                    BuildLockStatement((LockStatementSyntax)statement);
                    break;
                case SyntaxKind.UsingStatement:
                    BuildUsingStatement((UsingStatementSyntax)statement);
                    break;
                case SyntaxKind.FixedStatement:
                    BuildFixedStatement((FixedStatementSyntax)statement);
                    break;
                case SyntaxKind.UncheckedStatement:
                case SyntaxKind.CheckedStatement:
                    BuildCheckedStatement(statement);
                    break;
                case SyntaxKind.UnsafeStatement:
                    BuildUnsafeStatement(statement);
                    break;

                case SyntaxKind.ReturnStatement:
                    BuildReturnStatement((ReturnStatementSyntax)statement);
                    break;
                case SyntaxKind.YieldBreakStatement:
                    BuildYieldBreakStatement((YieldStatementSyntax)statement);
                    break;
                case SyntaxKind.ThrowStatement:
                    BuildThrowStatement((ThrowStatementSyntax)statement);
                    break;

                case SyntaxKind.YieldReturnStatement:
                    {
                        // A JumpBlock could be used, just to mark that something special is happening here.
                        // But for the time being we wouldn't do anything with that information.
                        var yieldReturn = (YieldStatementSyntax)statement;
                        BuildExpression(yieldReturn.Expression);
                    }
                    break;

                case SyntaxKind.EmptyStatement:
                    break;

                case SyntaxKind.BreakStatement:
                    BuildBreakStatement((BreakStatementSyntax)statement);
                    break;
                case SyntaxKind.ContinueStatement:
                    BuildContinueStatement((ContinueStatementSyntax)statement);
                    break;

                case SyntaxKind.SwitchStatement:
                    BuildSwitchStatement((SwitchStatementSyntax)statement);
                    break;

                case SyntaxKind.GotoCaseStatement:
                    BuildGotoCaseStatement((GotoStatementSyntax)statement);
                    break;
                case SyntaxKind.GotoDefaultStatement:
                    BuildGotoDefaultStatement((GotoStatementSyntax)statement);
                    break;


                case SyntaxKind.GotoStatement:
                    BuildGotoStatement((GotoStatementSyntax)statement);
                    break;

                case SyntaxKind.LabeledStatement:
                    BuildLabeledStatement((LabeledStatementSyntax)statement);
                    break;

                case SyntaxKind.TryStatement:
                    BuildTryStatement((TryStatementSyntax)statement);
                    break;

                case SyntaxKind.GlobalStatement:
                    throw new NotSupportedException($"{statement.Kind()}");

                default:
                    throw new NotImplementedException($"{statement.Kind()}");
            }
        }

        private void BuildExpression(ExpressionSyntax expression)
        {
            if (expression == null)
            {
                return;
            }

            switch (expression.Kind())
            {
                case SyntaxKind.SimpleAssignmentExpression:
                    BuildSimpleAssignmentExpression((AssignmentExpressionSyntax)expression);
                    break;

                case SyntaxKind.OrAssignmentExpression:
                case SyntaxKind.AndAssignmentExpression:
                case SyntaxKind.ExclusiveOrAssignmentExpression:

                case SyntaxKind.SubtractAssignmentExpression:
                case SyntaxKind.AddAssignmentExpression:
                case SyntaxKind.DivideAssignmentExpression:
                case SyntaxKind.MultiplyAssignmentExpression:
                case SyntaxKind.ModuloAssignmentExpression:

                case SyntaxKind.LeftShiftAssignmentExpression:
                case SyntaxKind.RightShiftAssignmentExpression:
                    BuildAssignmentExpression((AssignmentExpressionSyntax)expression);
                    break;

                case SyntaxKind.LessThanExpression:
                case SyntaxKind.LessThanOrEqualExpression:
                case SyntaxKind.GreaterThanExpression:
                case SyntaxKind.GreaterThanOrEqualExpression:
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.NotEqualsExpression:

                case SyntaxKind.BitwiseOrExpression:
                case SyntaxKind.BitwiseAndExpression:
                case SyntaxKind.ExclusiveOrExpression:

                case SyntaxKind.SubtractExpression:
                case SyntaxKind.AddExpression:
                case SyntaxKind.DivideExpression:
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.ModuloExpression:

                case SyntaxKind.LeftShiftExpression:
                case SyntaxKind.RightShiftExpression:
                    BuildBinaryExpression((BinaryExpressionSyntax)expression);
                    break;

                case SyntaxKind.LogicalNotExpression:
                case SyntaxKind.BitwiseNotExpression:
                case SyntaxKind.UnaryMinusExpression:
                case SyntaxKind.UnaryPlusExpression:
                case SyntaxKind.PreIncrementExpression:
                case SyntaxKind.PreDecrementExpression:
                case SyntaxKind.AddressOfExpression:
                case SyntaxKind.PointerIndirectionExpression:
                    {
                        var parent = (PrefixUnaryExpressionSyntax)expression;
                        BuildSimpleNestedExpression(parent, parent.Operand);
                    }
                    break;

                case SyntaxKind.PostIncrementExpression:
                case SyntaxKind.PostDecrementExpression:
                    {
                        var parent = (PostfixUnaryExpressionSyntax)expression;
                        BuildSimpleNestedExpression(parent, parent.Operand);
                    }
                    break;

                case SyntaxKind.IdentifierName:
                case SyntaxKind.GenericName:
                case SyntaxKind.AliasQualifiedName:
                case SyntaxKind.QualifiedName:
                case SyntaxKind.CharacterLiteralExpression:
                case SyntaxKind.StringLiteralExpression:
                case SyntaxKind.NumericLiteralExpression:
                case SyntaxKind.TrueLiteralExpression:
                case SyntaxKind.FalseLiteralExpression:
                case SyntaxKind.NullLiteralExpression:
                case SyntaxKind.ThisExpression:
                case SyntaxKind.BaseExpression:

                case SyntaxKind.DefaultExpression:
                case SyntaxKind.SizeOfExpression:
                case SyntaxKind.TypeOfExpression:

                case SyntaxKind.PredefinedType:
                case SyntaxKind.NullableType:

                case SyntaxKind.OmittedArraySizeExpression:

                case SyntaxKind.AnonymousMethodExpression:
                case SyntaxKind.ParenthesizedLambdaExpression:
                case SyntaxKind.SimpleLambdaExpression:
                case SyntaxKind.QueryExpression:

                case SyntaxKind.ArgListExpression:
                    currentBlock.ReversedInstructions.Add(expression);
                    break;

                case SyntaxKind.PointerType:
                    BuildExpression(((PointerTypeSyntax)expression).ElementType);
                    break;

                case SyntaxKind.ParenthesizedExpression:
                    BuildExpression(((ParenthesizedExpressionSyntax)expression).Expression);
                    break;

                case SyntaxKind.AwaitExpression:
                    {
                        var parent = (AwaitExpressionSyntax)expression;
                        BuildSimpleNestedExpression(parent, parent.Expression);
                    }
                    break;

                case SyntaxKind.CheckedExpression:
                case SyntaxKind.UncheckedExpression:
                    {
                        var parent = (CheckedExpressionSyntax)expression;
                        BuildSimpleNestedExpression(parent, parent.Expression);
                    }
                    break;

                case SyntaxKind.AsExpression:
                case SyntaxKind.IsExpression:
                    {
                        var parent = (BinaryExpressionSyntax)expression;
                        BuildSimpleNestedExpression(parent, parent.Left);
                    }
                    break;
                case SyntaxKind.CastExpression:
                    {
                        var parent = (CastExpressionSyntax)expression;
                        BuildSimpleNestedExpression(parent, parent.Expression);
                    }
                    break;

                case SyntaxKind.InterpolatedStringExpression:
                    BuildInterpolatedStringExpression((InterpolatedStringExpressionSyntax)expression);
                    break;

                case SyntaxKind.InvocationExpression:
                    BuildInvocationExpression((InvocationExpressionSyntax)expression);
                    break;

                case SyntaxKind.AnonymousObjectCreationExpression:
                    BuildAnonymousObjectCreationExpression((AnonymousObjectCreationExpressionSyntax)expression);
                    break;

                case SyntaxKind.ObjectCreationExpression:
                    BuildObjectCreationExpression((ObjectCreationExpressionSyntax)expression);
                    break;

                case SyntaxKind.ElementAccessExpression:
                    BuildElementAccessExpression((ElementAccessExpressionSyntax)expression);
                    break;
                case SyntaxKind.ImplicitElementAccess:
                    BuildImplicitElementAccessExpression((ImplicitElementAccessSyntax)expression);
                    break;

                case SyntaxKind.LogicalAndExpression:
                    BuildLogicalAndExpression((BinaryExpressionSyntax)expression);
                    break;

                case SyntaxKind.LogicalOrExpression:
                    BuildLogicalOrExpression((BinaryExpressionSyntax)expression);
                    break;

                case SyntaxKind.ArrayCreationExpression:
                    BuildArrayCreationExpression((ArrayCreationExpressionSyntax)expression);
                    break;
                case SyntaxKind.ImplicitArrayCreationExpression:
                    {
                        var parent = (ImplicitArrayCreationExpressionSyntax)expression;
                        BuildExpression(parent.Initializer);
                        currentBlock.ReversedInstructions.Add(parent);
                    }
                    break;
                case SyntaxKind.StackAllocArrayCreationExpression:
                    {
                        var parent = (StackAllocArrayCreationExpressionSyntax)expression;
                        BuildSimpleNestedExpression(parent, parent.Type);
                    }
                    break;

                case SyntaxKind.SimpleMemberAccessExpression:
                case SyntaxKind.PointerMemberAccessExpression:
                    {
                        var parent = (MemberAccessExpressionSyntax)expression;
                        BuildSimpleNestedExpression(parent, parent.Expression);
                    }
                    break;
                case SyntaxKind.ObjectInitializerExpression:
                case SyntaxKind.ArrayInitializerExpression:
                case SyntaxKind.CollectionInitializerExpression:
                case SyntaxKind.ComplexElementInitializerExpression:
                    {
                        var parent = (InitializerExpressionSyntax)expression;
                        BuildSimpleNestedExpression(parent, parent.Expressions);
                    }
                    break;

                case SyntaxKind.MakeRefExpression:
                    {
                        var parent = (MakeRefExpressionSyntax)expression;
                        BuildSimpleNestedExpression(parent, parent.Expression);
                    }
                    break;
                case SyntaxKind.RefTypeExpression:
                    {
                        var parent = (RefTypeExpressionSyntax)expression;
                        BuildSimpleNestedExpression(parent, parent.Expression);
                    }
                    break;
                case SyntaxKind.RefValueExpression:
                    {
                        var parent = (RefValueExpressionSyntax)expression;
                        BuildSimpleNestedExpression(parent, parent.Expression);
                    }
                    break;

                case SyntaxKind.ArrayType:
                    BuildArrayType((ArrayTypeSyntax)expression);
                    break;

                case SyntaxKind.CoalesceExpression:
                    BuildCoalesceExpression((BinaryExpressionSyntax)expression);
                    break;

                case SyntaxKind.ConditionalExpression:
                    BuildConditionalExpression((ConditionalExpressionSyntax)expression);
                    break;

                // these look strange in the CFG:
                case SyntaxKind.ConditionalAccessExpression:
                    BuildConditionalAccessExpression((ConditionalAccessExpressionSyntax)expression);
                    break;
                case SyntaxKind.MemberBindingExpression:
                    {
                        var parent = (MemberBindingExpressionSyntax)expression;
                        BuildSimpleNestedExpression(parent, parent.Name);
                    }
                    break;
                case SyntaxKind.ElementBindingExpression:
                    {
                        var parent = (ElementBindingExpressionSyntax)expression;
                        BuildSimpleNestedExpression(parent, parent.ArgumentList?.Arguments.Select(a => a.Expression));
                    }
                    break;

                default:
                    throw new NotImplementedException($"{expression.Kind()}");
            }
        }

        #endregion

        #region Build*

        #region Build statements

        #region Build label, goto, goto case, goto default

        private void BuildLabeledStatement(LabeledStatementSyntax labeledStatement)
        {
            BuildStatement(labeledStatement.Statement);
            var jumpBlock = CreateJumpBlock(labeledStatement, currentBlock);
            currentBlock = jumpBlock;

            LabeledStatements[labeledStatement.Identifier.ValueText] = jumpBlock;

            currentBlock = CreateBlock(currentBlock);
        }

        private void BuildTryStatement(TryStatementSyntax tryStatement)
        {
            if (tryStatement.Finally?.Block != null)
            {
                // Wire exit in case we have a return inside the try/catch block
                currentBlock = CreateBranchBlock(tryStatement.Finally, new[] { currentBlock, ExitTarget.Peek() });
                BuildBlock(tryStatement.Finally.Block);
                ExitTarget.Push(currentBlock);
            }

            var finallyBlock = currentBlock;

            var catchBlocks = new List<Block>();
            foreach (var catchClause in tryStatement.Catches.Reverse())
            {
                currentBlock = CreateBlock(finallyBlock);

                BuildBlock(catchClause.Block);

                catchBlocks.Add(currentBlock);
            }

            // try end
            currentBlock = CreateBranchBlock(tryStatement, catchBlocks.Union(new[] { finallyBlock }).ToList());

            BuildBlock(tryStatement.Block);

            // try start
            currentBlock = CreateBranchBlock(tryStatement, catchBlocks.Union(new[] { currentBlock, finallyBlock }).ToList());
            if (tryStatement.Finally?.Block != null)
            {
                ExitTarget.Pop();
            }
        }

        private void BuildGotoDefaultStatement(GotoStatementSyntax statement)
        {
            if (SwitchGotoJumpBlocks.Count == 0)
            {
                throw new InvalidOperationException("goto default; outside a switch");
            }

            var jumpBlock = CreateJumpBlock(statement, CreateTemporaryBlock(), currentBlock);
            currentBlock = jumpBlock;

            var currentJumpBlocks = SwitchGotoJumpBlocks.Peek();
            if (!currentJumpBlocks.ContainsKey(GotoDefaultEntry))
            {
                currentJumpBlocks.Add(GotoDefaultEntry, new List<JumpBlock>());
            }

            currentJumpBlocks[GotoDefaultEntry].Add(jumpBlock);
        }

        private void BuildGotoCaseStatement(GotoStatementSyntax statement)
        {
            if (SwitchGotoJumpBlocks.Count == 0)
            {
                throw new InvalidOperationException("goto case; outside a switch");
            }

            var jumpBlock = CreateJumpBlock(statement, CreateTemporaryBlock(), currentBlock);
            currentBlock = jumpBlock;

            var currentJumpBlocks = SwitchGotoJumpBlocks.Peek();
            var indexer = GetCaseIndexer(statement.Expression);

            if (!currentJumpBlocks.ContainsKey(indexer))
            {
                currentJumpBlocks.Add(indexer, new List<JumpBlock>());
            }

            currentJumpBlocks[indexer].Add(jumpBlock);
        }

        private void BuildGotoStatement(GotoStatementSyntax statement)
        {
            var jumpBlock = CreateJumpBlock(statement, CreateTemporaryBlock(), currentBlock);
            currentBlock = jumpBlock;

            var identifier = statement.Expression as IdentifierNameSyntax;
            if (identifier == null)
            {
                throw new InvalidOperationException("goto with no identifier");
            }

            if (!GotoJumpBlocks.ContainsKey(identifier.Identifier.ValueText))
            {
                GotoJumpBlocks.Add(identifier.Identifier.ValueText, new List<JumpBlock>());
            }

            GotoJumpBlocks[identifier.Identifier.ValueText].Add(jumpBlock);
        }

        #endregion

        #region Build switch

        private void BuildSwitchStatement(SwitchStatementSyntax switchStatement)
        {
            var successorBlock = currentBlock;
            var caseBlocks = new List<Block>();

            var caseBlocksByValue = new Dictionary<object, Block>();

            BreakTarget.Push(successorBlock);
            SwitchGotoJumpBlocks.Push(new Dictionary<object, List<JumpBlock>>());

            foreach (var section in switchStatement.Sections)
            {
                var blocks = new List<Block>();
                Block fallThroughBlock = null;
                foreach (var label in section.Labels.Reverse())
                {
                    if (fallThroughBlock == null)
                    {
                        currentBlock = CreateBlock(successorBlock);
                        foreach (var st in section.Statements.Reverse())
                        {
                            BuildStatement(st);
                        }
                        fallThroughBlock = currentBlock;
                    }
                    else
                    {
                        fallThroughBlock = CreateJumpBlock(label, fallThroughBlock);
                    }
                    blocks.Add(fallThroughBlock);

                    var defaultLabel = label as DefaultSwitchLabelSyntax;
                    if (defaultLabel != null)
                    {
                        caseBlocksByValue[GotoDefaultEntry] = fallThroughBlock;
                    }
                    else
                    {
                        var caseLabel = label as CaseSwitchLabelSyntax;
                        if (caseLabel == null)
                        {
                            throw new NotSupportedException("C# 7 features are not supported yet.");
                        }

                        var indexer = GetCaseIndexer(caseLabel.Value);
                        caseBlocksByValue[indexer] = fallThroughBlock;
                    }
                }

                caseBlocks.AddRange(blocks.Reverse<Block>());
            }

            BreakTarget.Pop();
            var gotosToFix = SwitchGotoJumpBlocks.Pop();
            FixJumps(gotosToFix, caseBlocksByValue);

            if (!caseBlocksByValue.ContainsKey(GotoDefaultEntry))
            {
                caseBlocks.Add(successorBlock);
            }

            currentBlock = CreateBranchBlock(switchStatement, caseBlocks);

            BuildExpression(switchStatement.Expression);
        }

        private object GetCaseIndexer(ExpressionSyntax expression)
        {
            var constValue = semanticModel.GetConstantValue(expression);
            if (!constValue.HasValue)
            {
                throw new InvalidOperationException("Expression has no constant value");
            }

            var indexer = constValue.Value;
            if (indexer == null)
            {
                indexer = GotoNullEntry;
            }

            return indexer;
        }

        #endregion

        #region Build jumps: break, continue, return, throw, yield break

        private void BuildBreakStatement(BreakStatementSyntax breakStatement)
        {
            if (BreakTarget.Count == 0)
            {
                throw new InvalidOperationException("break; outside a loop");
            }

            var target = BreakTarget.Peek();
            currentBlock = CreateJumpBlock(breakStatement, target, currentBlock);
        }

        private void BuildContinueStatement(ContinueStatementSyntax continueStatement)
        {
            if (ContinueTargets.Count == 0)
            {
                throw new InvalidOperationException("continue; outside a loop");
            }

            var target = ContinueTargets.Peek();
            currentBlock = CreateJumpBlock(continueStatement, target, currentBlock);
        }

        private void BuildReturnStatement(ReturnStatementSyntax returnStatement)
        {
            BuildJumpToExitStatement(returnStatement, returnStatement.Expression);
        }

        private void BuildThrowStatement(ThrowStatementSyntax throwStatement)
        {
            BuildJumpToExitStatement(throwStatement, throwStatement.Expression);
        }

        private void BuildYieldBreakStatement(YieldStatementSyntax yieldBreakStatement)
        {
            BuildJumpToExitStatement(yieldBreakStatement);
        }

        private void BuildJumpToExitStatement(StatementSyntax statement, ExpressionSyntax expression = null)
        {
            currentBlock = CreateJumpBlock(statement, ExitTarget.Peek(), currentBlock);

            BuildExpression(expression);
        }

        #endregion

        #region Build lock, using, fixed, unsafe checked statements

        private void BuildLockStatement(LockStatementSyntax lockStatement)
        {
            currentBlock = CreateBlock(currentBlock);
            BuildStatement(lockStatement.Statement);

            currentBlock = CreateLockBlock(lockStatement, currentBlock);
            BuildExpression(lockStatement.Expression);
        }

        private void BuildUsingStatement(UsingStatementSyntax usingStatement)
        {
            currentBlock = CreateBlock(currentBlock);
            BuildStatement(usingStatement.Statement);

            currentBlock = CreateJumpBlock(usingStatement, currentBlock);
            if (usingStatement.Expression != null)
            {
                BuildExpression(usingStatement.Expression);
            }
            else
            {
                BuildVariableDeclaration(usingStatement.Declaration);
            }
        }

        private void BuildFixedStatement(FixedStatementSyntax fixedStatement)
        {
            currentBlock = CreateBlock(currentBlock);
            BuildStatement(fixedStatement.Statement);

            currentBlock = CreateJumpBlock(fixedStatement, currentBlock);
            BuildVariableDeclaration(fixedStatement.Declaration);
        }

        private void BuildUnsafeStatement(StatementSyntax statement)
        {
            var checkedStatement = (UnsafeStatementSyntax)statement;
            currentBlock = CreateBlock(currentBlock);
            BuildStatement(checkedStatement.Block);

            currentBlock = CreateJumpBlock(checkedStatement, currentBlock);
        }

        private void BuildCheckedStatement(StatementSyntax statement)
        {
            var checkedStatement = (CheckedStatementSyntax)statement;
            currentBlock = CreateBlock(currentBlock);
            BuildStatement(checkedStatement.Block);

            currentBlock = CreateJumpBlock(checkedStatement, currentBlock);
        }

        #endregion

        #region Build loops - do, for, foreach, while

        private void BuildDoStatement(DoStatementSyntax doStatement)
        {
            var afterBlock = currentBlock;
            var loopTempBlock = CreateTemporaryBlock();

            var doBlock = CreateBinaryBranchBlock(doStatement, loopTempBlock, afterBlock);

            currentBlock = doBlock;
            BuildExpression(doStatement.Condition);

            BreakTarget.Push(afterBlock);
            ContinueTargets.Push(currentBlock);

            currentBlock = CreateBlock(currentBlock);
            BuildStatement(doStatement.Statement);
            loopTempBlock.SuccessorBlock = currentBlock;

            BreakTarget.Pop();
            ContinueTargets.Pop();

            currentBlock = CreateBlock(currentBlock);
        }

        private void BuildForStatement(ForStatementSyntax forStatement)
        {
            var afterBlock = currentBlock;
            var tempLoopBlock = CreateTemporaryBlock();

            currentBlock = CreateBlock(tempLoopBlock);
            foreach (var incrementor in forStatement.Incrementors.Reverse())
            {
                BuildExpression(incrementor);
            }

            var incrementorBlock = currentBlock;

            BreakTarget.Push(afterBlock);
            ContinueTargets.Push(incrementorBlock);

            currentBlock = CreateBlock(incrementorBlock);
            BuildStatement(forStatement.Statement);

            BreakTarget.Pop();
            ContinueTargets.Pop();

            var forBlock = CreateBinaryBranchBlock(forStatement, currentBlock, afterBlock);

            currentBlock = forBlock;
            BuildExpression(forStatement.Condition);
            tempLoopBlock.SuccessorBlock = currentBlock;

            currentBlock = AddBlock(new ForInitializerBlock(forStatement, currentBlock));
            if (forStatement.Declaration != null)
            {
                BuildVariableDeclaration(forStatement.Declaration);
            }
            else
            {
                foreach (var initializer in forStatement.Initializers.Reverse())
                {
                    BuildExpression(initializer);
                }
            }
        }

        private void BuildForEachStatement(ForEachStatementSyntax foreachStatement)
        {
            var afterBlock = currentBlock;
            var temp = CreateTemporaryBlock();

            BreakTarget.Push(afterBlock);
            ContinueTargets.Push(temp);

            currentBlock = CreateBlock(temp);
            BuildStatement(foreachStatement.Statement);

            BreakTarget.Pop();
            ContinueTargets.Pop();

            currentBlock = CreateBinaryBranchBlock(foreachStatement, currentBlock, afterBlock);
            // Variable declaration in a foreach statement is not a VariableDeclarator, otherwise it would be added here.

            temp.SuccessorBlock = currentBlock;

            currentBlock = AddBlock(new ForeachCollectionProducerBlock(foreachStatement, temp));
            BuildExpression(foreachStatement.Expression);
        }

        private void BuildWhileStatement(WhileStatementSyntax whileStatement)
        {
            var afterBlock = currentBlock;
            var loopTempBlock = CreateTemporaryBlock();

            BreakTarget.Push(afterBlock);
            ContinueTargets.Push(loopTempBlock);

            var bodyBlock = CreateBlock(loopTempBlock);
            currentBlock = bodyBlock;
            BuildStatement(whileStatement.Statement);

            BreakTarget.Pop();
            ContinueTargets.Pop();

            currentBlock = CreateBinaryBranchBlock(whileStatement, currentBlock, afterBlock);
            BuildExpression(whileStatement.Condition);
            loopTempBlock.SuccessorBlock = currentBlock;

            currentBlock = CreateBlock(currentBlock);
        }

        #endregion

        #region Build if statement

        private void BuildIfStatement(IfStatementSyntax ifStatement)
        {
            var successor = currentBlock;
            var elseBlock = currentBlock;

            if (ifStatement.Else?.Statement != null)
            {
                currentBlock = CreateBlock(successor);
                BuildStatement(ifStatement.Else.Statement);
                elseBlock = currentBlock;
            }

            currentBlock = CreateBlock(successor);
            BuildStatement(ifStatement.Statement);
            var trueBlock = currentBlock;

            currentBlock = CreateBinaryBranchBlock(ifStatement, trueBlock, elseBlock);
            BuildExpression(ifStatement.Condition);
        }

        #endregion

        #region Build block

        private void BuildBlock(BlockSyntax block)
        {
            foreach (var node in block.Statements.Reverse())
            {
                BuildStatement(node);
            }
        }

        #endregion

        #endregion

        #region Build expressions

        private void BuildConditionalAccessExpression(ConditionalAccessExpressionSyntax conditionalAccess)
        {
            var successorBlock = currentBlock;

            currentBlock = CreateBlock(currentBlock);
            BuildExpression(conditionalAccess.WhenNotNull);

            currentBlock = CreateBinaryBranchBlock(conditionalAccess, successorBlock, currentBlock);
            BuildExpression(conditionalAccess.Expression);
        }

        private void BuildConditionalExpression(ConditionalExpressionSyntax conditional)
        {
            var successor = currentBlock;

            currentBlock = CreateBlock(successor);
            BuildExpression(conditional.WhenFalse);
            var falseBlock = currentBlock;

            currentBlock = CreateBlock(successor);
            BuildExpression(conditional.WhenTrue);
            var trueBlock = currentBlock;

            currentBlock = CreateBinaryBranchBlock(conditional, trueBlock, falseBlock);
            BuildExpression(conditional.Condition);
        }

        private void BuildCoalesceExpression(BinaryExpressionSyntax expression)
        {
            var successor = currentBlock;
            currentBlock = CreateBlock(currentBlock);
            BuildExpression(expression.Right);

            currentBlock = CreateBinaryBranchBlock(expression, currentBlock, successor);
            BuildExpression(expression.Left);
        }

        private void BuildLogicalAndExpression(BinaryExpressionSyntax expression)
        {
            var successor = currentBlock;
            currentBlock = AddBlock(new BinaryBranchingSimpleBlock(expression.Right, successor));
            BuildExpression(expression.Right);

            currentBlock = CreateBinaryBranchBlock(expression, currentBlock, successor);
            BuildExpression(expression.Left);
        }

        private void BuildLogicalOrExpression(BinaryExpressionSyntax expression)
        {
            var successor = currentBlock;
            currentBlock = AddBlock(new BinaryBranchingSimpleBlock(expression.Right, successor));
            BuildExpression(expression.Right);

            currentBlock = CreateBinaryBranchBlock(expression, successor, currentBlock);
            BuildExpression(expression.Left);
        }

        private void BuildArrayCreationExpression(ArrayCreationExpressionSyntax expression)
        {
            BuildExpression(expression.Initializer);

            currentBlock.ReversedInstructions.Add(expression);

            BuildExpression(expression.Type);
        }

        private void BuildElementAccessExpression(ElementAccessExpressionSyntax expression)
        {
            BuildInvocationLikeExpression(expression, expression.Expression, expression.ArgumentList?.Arguments);
        }

        private void BuildImplicitElementAccessExpression(ImplicitElementAccessSyntax expression)
        {
            BuildInvocationLikeExpression(expression, null, expression.ArgumentList?.Arguments);
        }

        private void BuildInvocationLikeExpression(ExpressionSyntax parent, ExpressionSyntax child, IEnumerable<ArgumentSyntax> arguments)
        {
            var args = arguments == null
                ? Enumerable.Empty<ExpressionSyntax>()
                : arguments.Select(a => a.Expression);

            BuildSimpleNestedExpression(parent, new[] { child }.Concat(args));
        }

        private void BuildObjectCreationExpression(ObjectCreationExpressionSyntax expression)
        {
            BuildExpression(expression.Initializer);

            currentBlock.ReversedInstructions.Add(expression);

            var arguments = expression.ArgumentList == null
                ? Enumerable.Empty<ExpressionSyntax>()
                : expression.ArgumentList.Arguments.Select(a => a.Expression);

            foreach (var argument in arguments.Reverse())
            {
                BuildExpression(argument);
            }
        }

        private void BuildAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax expression)
        {
            BuildSimpleNestedExpression(expression, expression.Initializers.Select(i => i.Expression));
        }

        private void BuildInvocationExpression(InvocationExpressionSyntax expression)
        {
            BuildInvocationLikeExpression(expression, expression.Expression, expression.ArgumentList?.Arguments);
        }

        private void BuildInterpolatedStringExpression(InterpolatedStringExpressionSyntax expression)
        {
            BuildSimpleNestedExpression(expression, expression.Contents.OfType<InterpolationSyntax>().Select(i => i.Expression));
        }

        private void BuildSimpleNestedExpression(ExpressionSyntax parent, params ExpressionSyntax[] children)
        {
            BuildSimpleNestedExpression(parent, (IEnumerable<ExpressionSyntax>)children);
        }

        private void BuildSimpleNestedExpression(ExpressionSyntax parent, IEnumerable<ExpressionSyntax> children)
        {
            currentBlock.ReversedInstructions.Add(parent);

            if (children == null)
            {
                return;
            }

            foreach (var child in children.Reverse())
            {
                BuildExpression(child);
            }
        }

        private void BuildBinaryExpression(BinaryExpressionSyntax expression)
        {
            currentBlock.ReversedInstructions.Add(expression);
            BuildExpression(expression.Right);
            BuildExpression(expression.Left);
        }

        private void BuildAssignmentExpression(AssignmentExpressionSyntax expression)
        {
            currentBlock.ReversedInstructions.Add(expression);
            BuildExpression(expression.Right);
            BuildExpression(expression.Left);
        }

        private void BuildSimpleAssignmentExpression(AssignmentExpressionSyntax expression)
        {
            currentBlock.ReversedInstructions.Add(expression);
            BuildExpression(expression.Right);
            if (!IsAssignmentWithSimpleLeftSide(expression))
            {
                BuildExpression(expression.Left);
            }
        }

        internal static bool IsAssignmentWithSimpleLeftSide(AssignmentExpressionSyntax assignment)
        {
            return assignment.Left.RemoveParentheses() is IdentifierNameSyntax;
        }

        private void BuildArrayType(ArrayTypeSyntax arrayType)
        {
            currentBlock.ReversedInstructions.Add(arrayType);

            var arraySizes = arrayType.RankSpecifiers.SelectMany(rs => rs.Sizes);
            foreach (var arraySize in arraySizes.Reverse())
            {
                BuildExpression(arraySize);
            }
        }

        #endregion

        #region Build variable declaration

        private void BuildVariableDeclaration(VariableDeclarationSyntax declaration)
        {
            if (declaration == null)
            {
                return;
            }

            foreach (var variable in declaration.Variables.Reverse())
            {
                BuildVariableDeclarator(variable);
            }
        }

        private void BuildVariableDeclarator(VariableDeclaratorSyntax variableDeclarator)
        {
            // There are variable declarations which implicitly get a value, such as foreach (var x in xs)
            currentBlock.ReversedInstructions.Add(variableDeclarator);

            var initializer = variableDeclarator.Initializer?.Value;
            if (initializer != null)
            {
                BuildExpression(initializer);
            }
        }

        #endregion

        #region Create*

        internal LockBlock CreateLockBlock(LockStatementSyntax lockStatement, Block successor) =>
            AddBlock(new LockBlock(lockStatement, successor));

        #endregion

        #endregion
    }
}
