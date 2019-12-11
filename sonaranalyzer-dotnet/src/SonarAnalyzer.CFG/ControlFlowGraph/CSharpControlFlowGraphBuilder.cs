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
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    public sealed class CSharpControlFlowGraphBuilder : AbstractControlFlowGraphBuilder
    {
        private readonly Stack<Block> breakTarget = new Stack<Block>();
        private readonly Stack<Block> continueTargets = new Stack<Block>();
        private readonly Stack<Dictionary<object, List<JumpBlock>>> switchGotoJumpBlocks = new Stack<Dictionary<object, List<JumpBlock>>>();
        private readonly Dictionary<string, List<JumpBlock>> gotoJumpBlocks = new Dictionary<string, List<JumpBlock>>();
        private readonly Dictionary<string, JumpBlock> labeledStatements = new Dictionary<string, JumpBlock>();
        private static readonly object GotoDefaultEntry = new object();
        private static readonly object GotoNullEntry = new object();

        public CSharpControlFlowGraphBuilder(CSharpSyntaxNode node, SemanticModel semanticModel)
            : base(node, semanticModel)
        {
        }

        #region Fix jump statements

        protected override void PostProcessGraph()
        {
            FixJumps(this.gotoJumpBlocks, this.labeledStatements.ToDictionary(e => e.Key, e => (Block)e.Value));
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

        #endregion Fix jump statements

        #region Top level Build*

        protected override Block Build(SyntaxNode node, Block currentBlock)
        {
            Block block = null;
            if (node is StatementSyntax statement)
            {
                block = BuildStatement(statement, currentBlock);
            }
            else if (node is ArrowExpressionClauseSyntax arrowExpression)
            {
                block = BuildExpression(arrowExpression.Expression, currentBlock);
            }
            else if (node is ExpressionSyntax expression)
            {
                block = BuildExpression(expression, currentBlock);
            }

            if (block == null)
            {
                throw new ArgumentException("Neither a statement, nor an expression", nameof(node));
            }

            if (node.Parent is ConstructorDeclarationSyntax constructorDeclaration &&
                constructorDeclaration.Initializer != null)
            {
                block = BuildConstructorInitializer(constructorDeclaration.Initializer, block);
            }

            return block;
        }

        private Block BuildConstructorInitializer(ConstructorInitializerSyntax initializer, Block currentBlock)
        {
            currentBlock.ReversedInstructions.Add(initializer);

            var arguments = initializer.ArgumentList == null
                ? Enumerable.Empty<ExpressionSyntax>()
                : initializer.ArgumentList.Arguments.Select(a => a.Expression);

            return BuildExpressions(arguments, currentBlock);
        }

        private Block BuildStatement(StatementSyntax statement, Block currentBlock)
        {
            switch (statement.Kind())
            {
                case SyntaxKind.Block:
                    return BuildBlock((BlockSyntax)statement, currentBlock);

                case SyntaxKind.ExpressionStatement:
                    return BuildExpression(((ExpressionStatementSyntax)statement).Expression, currentBlock);

                case SyntaxKind.LocalDeclarationStatement:
                    return BuildVariableDeclaration(((LocalDeclarationStatementSyntax)statement).Declaration,
                        currentBlock);

                case SyntaxKind.IfStatement:
                    return BuildIfStatement((IfStatementSyntax)statement, currentBlock);

                case SyntaxKind.WhileStatement:
                    return BuildWhileStatement((WhileStatementSyntax)statement, currentBlock);

                case SyntaxKind.DoStatement:
                    return BuildDoStatement((DoStatementSyntax)statement, currentBlock);

                case SyntaxKind.ForStatement:
                    return BuildForStatement((ForStatementSyntax)statement, currentBlock);

                case SyntaxKind.ForEachStatement:
                    return BuildForEachStatement((ForEachStatementSyntax)statement, currentBlock);

                case SyntaxKindEx.ForEachVariableStatement:
                    return BuildForEachVariableStatement((ForEachVariableStatementSyntaxWrapper)statement, currentBlock);

                case SyntaxKind.LockStatement:
                    return BuildLockStatement((LockStatementSyntax)statement, currentBlock);

                case SyntaxKind.UsingStatement:
                    return BuildUsingStatement((UsingStatementSyntax)statement, currentBlock);

                case SyntaxKind.FixedStatement:
                    return BuildFixedStatement((FixedStatementSyntax)statement, currentBlock);

                case SyntaxKind.UncheckedStatement:
                case SyntaxKind.CheckedStatement:
                    return BuildCheckedStatement((CheckedStatementSyntax)statement, currentBlock);

                case SyntaxKind.UnsafeStatement:
                    return BuildUnsafeStatement((UnsafeStatementSyntax)statement, currentBlock);

                case SyntaxKind.ReturnStatement:
                    return BuildReturnStatement((ReturnStatementSyntax)statement, currentBlock);

                case SyntaxKind.YieldBreakStatement:
                    return BuildYieldBreakStatement((YieldStatementSyntax)statement, currentBlock);

                case SyntaxKind.ThrowStatement:
                    return BuildThrowStatement((ThrowStatementSyntax)statement, currentBlock);

                case SyntaxKind.YieldReturnStatement:
                    return BuildYieldReturnStatement((YieldStatementSyntax)statement, currentBlock);

                case SyntaxKind.EmptyStatement:
                    return currentBlock;

                case SyntaxKind.BreakStatement:
                    return BuildBreakStatement((BreakStatementSyntax)statement, currentBlock);

                case SyntaxKind.ContinueStatement:
                    return BuildContinueStatement((ContinueStatementSyntax)statement, currentBlock);

                case SyntaxKind.SwitchStatement:
                    return BuildSwitchStatement((SwitchStatementSyntax)statement, currentBlock);

                case SyntaxKind.GotoCaseStatement:
                    return BuildGotoCaseStatement((GotoStatementSyntax)statement, currentBlock);

                case SyntaxKind.GotoDefaultStatement:
                    return BuildGotoDefaultStatement((GotoStatementSyntax)statement, currentBlock);

                case SyntaxKind.GotoStatement:
                    return BuildGotoStatement((GotoStatementSyntax)statement, currentBlock);

                case SyntaxKind.LabeledStatement:
                    return BuildLabeledStatement((LabeledStatementSyntax)statement, currentBlock);

                case SyntaxKind.TryStatement:
                    return BuildTryStatement((TryStatementSyntax)statement, currentBlock);

                case SyntaxKindEx.LocalFunctionStatement:
                    return currentBlock;

                case SyntaxKind.GlobalStatement:
                    throw new NotSupportedException($"{statement.Kind()}");

                default:
                    throw new NotSupportedException($"{statement.Kind()}");
            }
        }

        private Block BuildExpression(ExpressionSyntax expression, Block currentBlock)
        {
            if (expression == null)
            {
                return currentBlock;
            }

            switch (expression.Kind())
            {
                case SyntaxKind.SimpleAssignmentExpression:
                    return BuildSimpleAssignmentExpression((AssignmentExpressionSyntax)expression, currentBlock);

                case SyntaxKindEx.CoalesceAssignmentExpression:
                    return BuildCoalesceAssignmentExpression((AssignmentExpressionSyntax)expression, currentBlock);

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
                    return BuildAssignmentExpression((AssignmentExpressionSyntax)expression, currentBlock);

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
                    return BuildBinaryExpression((BinaryExpressionSyntax)expression, currentBlock);

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
                        return BuildSimpleNestedExpression(parent, currentBlock, parent.Operand);
                    }

                case SyntaxKind.PostIncrementExpression:
                case SyntaxKind.PostDecrementExpression:
                    {
                        var parent = (PostfixUnaryExpressionSyntax)expression;
                        return BuildSimpleNestedExpression(parent, currentBlock, parent.Operand);
                    }

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
                case SyntaxKindEx.DefaultLiteralExpression:
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

                case SyntaxKindEx.TupleExpression:
                    currentBlock.ReversedInstructions.Add(expression);
                    return currentBlock;

                case SyntaxKind.PointerType:
                    return BuildExpression(((PointerTypeSyntax)expression).ElementType, currentBlock);

                case SyntaxKind.ParenthesizedExpression:
                    return BuildExpression(((ParenthesizedExpressionSyntax)expression).Expression, currentBlock);

                case SyntaxKind.AwaitExpression:
                    {
                        var parent = (AwaitExpressionSyntax)expression;
                        return BuildSimpleNestedExpression(parent, currentBlock, parent.Expression);
                    }

                case SyntaxKind.CheckedExpression:
                case SyntaxKind.UncheckedExpression:
                    {
                        var parent = (CheckedExpressionSyntax)expression;
                        return BuildSimpleNestedExpression(parent, currentBlock, parent.Expression);
                    }

                case SyntaxKind.AsExpression:
                case SyntaxKind.IsExpression:
                    {
                        var parent = (BinaryExpressionSyntax)expression;
                        return BuildSimpleNestedExpression(parent, currentBlock, parent.Left);
                    }

                case SyntaxKind.CastExpression:
                    {
                        var parent = (CastExpressionSyntax)expression;
                        return BuildSimpleNestedExpression(parent, currentBlock, parent.Expression);
                    }

                case SyntaxKind.InterpolatedStringExpression:
                    return BuildInterpolatedStringExpression((InterpolatedStringExpressionSyntax)expression,
                        currentBlock);

                case SyntaxKind.InvocationExpression:
                    return BuildInvocationExpression((InvocationExpressionSyntax)expression, currentBlock);

                case SyntaxKind.AnonymousObjectCreationExpression:
                    return BuildAnonymousObjectCreationExpression((AnonymousObjectCreationExpressionSyntax)expression,
                        currentBlock);

                case SyntaxKind.ObjectCreationExpression:
                    return BuildObjectCreationExpression((ObjectCreationExpressionSyntax)expression, currentBlock);

                case SyntaxKind.ElementAccessExpression:
                    return BuildElementAccessExpression((ElementAccessExpressionSyntax)expression, currentBlock);

                case SyntaxKind.ImplicitElementAccess:
                    return BuildImplicitElementAccessExpression((ImplicitElementAccessSyntax)expression, currentBlock);

                case SyntaxKind.LogicalAndExpression:
                    return BuildLogicalAndExpression((BinaryExpressionSyntax)expression, currentBlock);

                case SyntaxKind.LogicalOrExpression:
                    return BuildLogicalOrExpression((BinaryExpressionSyntax)expression, currentBlock);

                case SyntaxKind.ArrayCreationExpression:
                    return BuildArrayCreationExpression((ArrayCreationExpressionSyntax)expression, currentBlock);

                case SyntaxKind.ImplicitArrayCreationExpression:
                    {
                        var parent = (ImplicitArrayCreationExpressionSyntax)expression;

                        var initializerBlock = BuildExpression(parent.Initializer, currentBlock);
                        initializerBlock.ReversedInstructions.Add(parent);
                        return initializerBlock;
                    }

                case SyntaxKind.StackAllocArrayCreationExpression:
                    {
                        var parent = (StackAllocArrayCreationExpressionSyntax)expression;
                        return BuildSimpleNestedExpression(parent, currentBlock, parent.Type, parent.Initializer());
                    }

                case SyntaxKindEx.ImplicitStackAllocArrayCreationExpression:
                    {
                        var parent = (ImplicitStackAllocArrayCreationExpressionSyntaxWrapper)expression;
                        return BuildSimpleNestedExpression(parent, currentBlock, parent.Initializer);
                    }

                case SyntaxKind.SimpleMemberAccessExpression:
                case SyntaxKind.PointerMemberAccessExpression:
                    {
                        var parent = (MemberAccessExpressionSyntax)expression;
                        return BuildSimpleNestedExpression(parent, currentBlock, parent.Expression);
                    }

                case SyntaxKind.ObjectInitializerExpression:
                case SyntaxKind.ArrayInitializerExpression:
                case SyntaxKind.CollectionInitializerExpression:
                case SyntaxKind.ComplexElementInitializerExpression:
                    {
                        var parent = (InitializerExpressionSyntax)expression;
                        return BuildSimpleNestedExpression(parent, currentBlock, parent.Expressions);
                    }

                case SyntaxKind.MakeRefExpression:
                    {
                        var parent = (MakeRefExpressionSyntax)expression;
                        return BuildSimpleNestedExpression(parent, currentBlock, parent.Expression);
                    }

                case SyntaxKind.RefTypeExpression:
                    {
                        var parent = (RefTypeExpressionSyntax)expression;
                        return BuildSimpleNestedExpression(parent, currentBlock, parent.Expression);
                    }

                case SyntaxKind.RefValueExpression:
                    {
                        var parent = (RefValueExpressionSyntax)expression;
                        return BuildSimpleNestedExpression(parent, currentBlock, parent.Expression);
                    }

                case SyntaxKind.ArrayType:
                    return BuildArrayType((ArrayTypeSyntax)expression, currentBlock);

                case SyntaxKind.CoalesceExpression:
                    return BuildCoalesceExpression((BinaryExpressionSyntax)expression, currentBlock);

                case SyntaxKind.ConditionalExpression:
                    return BuildConditionalExpression((ConditionalExpressionSyntax)expression, currentBlock);

                // these look strange in the CFG:
                case SyntaxKind.ConditionalAccessExpression:
                    return BuildConditionalAccessExpression((ConditionalAccessExpressionSyntax)expression, currentBlock);

                case SyntaxKind.MemberBindingExpression:
                    {
                        var parent = (MemberBindingExpressionSyntax)expression;
                        return BuildSimpleNestedExpression(parent, currentBlock, parent.Name);
                    }

                case SyntaxKind.ElementBindingExpression:
                    {
                        var parent = (ElementBindingExpressionSyntax)expression;
                        return BuildSimpleNestedExpression(parent, currentBlock,
                            parent.ArgumentList?.Arguments.Select(a => a.Expression));
                    }

                case SyntaxKindEx.IsPatternExpression:
                    var isPatternExpression = (IsPatternExpressionSyntaxWrapper)expression;

                    currentBlock = BuildIsPatternExpression(isPatternExpression, currentBlock);

                    return BuildExpression(isPatternExpression.Expression, currentBlock);

                case SyntaxKindEx.ThrowExpression:

                    var throwExpression = (ThrowExpressionSyntaxWrapper)expression;
                    return BuildJumpToExitStatement(throwExpression, currentBlock, throwExpression.Expression);

                case SyntaxKindEx.DeclarationExpression:
                    // we ignore 'discard' parameters
                    var declaration = (DeclarationExpressionSyntaxWrapper)expression;
                    if (!declaration.Designation.SyntaxNode.IsKind(SyntaxKindEx.DiscardDesignation))
                    {
                        currentBlock.ReversedInstructions.Add(expression);
                    }
                    return currentBlock;

                case SyntaxKindEx.RefExpression:
                    {
                        var parent = (RefExpressionSyntaxWrapper)expression;
                        return BuildSimpleNestedExpression(parent, currentBlock, parent.Expression);
                    }

                case SyntaxKindEx.SwitchExpression:
                    return BuildSwitchExpression((SwitchExpressionSyntaxWrapper)expression, currentBlock);

                case SyntaxKindEx.RangeExpression:
                    return currentBlock;

                case SyntaxKindEx.IndexExpression:
                    return currentBlock;

                default:
                    throw new NotSupportedException($"{expression.Kind()}");
            }
        }

        private Block BuildIsPatternExpression(IsPatternExpressionSyntaxWrapper isPatternExpression, Block currentBlock)
        {
            currentBlock.ReversedInstructions.Add(isPatternExpression);

            return BuildPatternExpression(isPatternExpression.Pattern, currentBlock);
        }

        private Block BuildPatternExpression(PatternSyntaxWrapper patternSyntaxWrapper, Block currentBlock)
        {
            if (ConstantPatternSyntaxWrapper.IsInstance(patternSyntaxWrapper))
            {
                var constantPattern = (ConstantPatternSyntaxWrapper)patternSyntaxWrapper;

                return BuildExpression(constantPattern.Expression, currentBlock);
            }
            else if (DeclarationPatternSyntaxWrapper.IsInstance(patternSyntaxWrapper))
            {
                // Do nothing, this is just variable assignment and the Pattern itself contains
                // only the new variable(s), which are not enough to evaluate the assignment.
                // The handling should be done in CSharpExplodedGraph and UcfgInstructionFactory.

                return currentBlock;
            }
            else if (DiscardPatternSyntaxWrapper.IsInstance(patternSyntaxWrapper))
            {
                currentBlock.ReversedInstructions.Add(patternSyntaxWrapper);

                return currentBlock;
            }
            else if (RecursivePatternSyntaxWrapper.IsInstance(patternSyntaxWrapper))
            {
                return BuildRecursivePatternExpression((RecursivePatternSyntaxWrapper)patternSyntaxWrapper, currentBlock);
            }

            throw new NotSupportedException($"{patternSyntaxWrapper.SyntaxNode.Kind()}");
        }

        private Block BuildRecursivePatternExpression(RecursivePatternSyntaxWrapper recursivePattern, Block currentBlock)
        {
            // The recursive pattern has PropertyPatternClause and PositionalPatternClause which
            // need to be recursively handled since they can have multiple subpatterns.
            // e.g. for "o is string { Length: 5 }": RecursivePattern -> PropertyPatternClause -> ConstantPattern
            if (recursivePattern.PropertyPatternClause.SyntaxNode != null)
            {
                foreach (var subPattern in recursivePattern.PropertyPatternClause.Subpatterns)
                {
                    currentBlock = BuildPatternExpression(subPattern.Pattern, currentBlock);
                }
            }

            currentBlock.ReversedInstructions.Add(recursivePattern);

            // Support for PositionalPatternClause will be added by https://jira.sonarsource.com/browse/SONARSEC-791
            return currentBlock;
        }

        #endregion Top level Build*

        #region Build*

        #region Build statements

        private Block BuildStatements(IEnumerable<StatementSyntax> statements, Block currentBlock)
        {
            foreach (var statement in statements.Reverse())
            {
                currentBlock = BuildStatement(statement, currentBlock);
            }

            return currentBlock;
        }

        private Block BuildExpressions(IEnumerable<ExpressionSyntax> expressions, Block currentBlock)
        {
            foreach (var expression in expressions.Reverse())
            {
                currentBlock = BuildExpression(expression, currentBlock);
            }

            return currentBlock;
        }

        #region Build label, goto, goto case, goto default

        private Block BuildLabeledStatement(LabeledStatementSyntax labeledStatement, Block currentBlock)
        {
            var statementBlock = BuildStatement(labeledStatement.Statement, currentBlock);
            var jumpBlock = CreateJumpBlock(labeledStatement, statementBlock);

            this.labeledStatements[labeledStatement.Identifier.ValueText] = jumpBlock;

            return CreateBlock(jumpBlock);
        }

        private Block BuildTryStatement(TryStatementSyntax tryStatement, Block currentBlock)
        {
            // successor - either finally of next block after try statement
            var catchSuccessor = currentBlock;

            var hasFinally = tryStatement.Finally?.Block != null;
            if (hasFinally)
            {
                var finallySuccessors = new List<Block>();
                finallySuccessors.Add(CreateBlock(catchSuccessor));
                finallySuccessors.Add(CreateBlock(this.exitTarget.Peek()));

                // Create a finally block that can either go to try-finally successor (happy path) or exit target (exceptional path)
                catchSuccessor = BuildBlock(tryStatement.Finally.Block, CreateBranchBlock(tryStatement.Finally, finallySuccessors));
                // This finally block becomes current exit target stack in case we have a return inside the try/catch block
                this.exitTarget.Push(catchSuccessor);
            }

            var catchBlocks = tryStatement.Catches
                .Reverse()
                .Select(catchClause =>
                {
                    Block catchBlock = BuildBlock(catchClause.Block, CreateBlock(catchSuccessor));
                    if (catchClause.Filter?.FilterExpression != null)
                    {
                        catchBlock = BuildExpression(catchClause.Filter.FilterExpression,
                            CreateBinaryBranchBlock(catchClause.Filter, catchBlock, catchSuccessor));
                    }
                    return catchBlock;
                })
                .ToList();

            // If there is a catch with no Exception filter or equivalent we don't want to
            // join the tryStatement start/end blocks with the exit block because all
            // exceptions will be caught before going to finally
            var areAllExceptionsCaught = tryStatement.Catches.Any(CSharpSyntaxHelper.IsCatchingAllExceptions);

            // try end
            var tryEndStatementConnections = catchBlocks.ToList();
            tryEndStatementConnections.Add(catchSuccessor); // happy path, no exceptions thrown
            if (!areAllExceptionsCaught) // unexpected exception thrown, go to exit (through finally if present)
            {
                tryEndStatementConnections.Add(this.exitTarget.Peek());
            }

            Block tryBody;
            if (tryStatement.Block.Statements.Any(s => s.IsKind(SyntaxKind.ReturnStatement)))
            {
                // there is a return inside the `try`, thus a JumpBlock directly to the finally or exit will be created
                var returnBlock = BuildBlock(tryStatement.Block, catchSuccessor);
                var connections = new List<Block>();
                connections.Add(returnBlock);
                // if an exception is thrown, it will reach the `catch` blocks
                connections.AddRange(catchBlocks);
                tryBody = CreateBranchBlock(tryStatement, connections);
            }
            else
            {
                tryBody = BuildBlock(tryStatement.Block, CreateBranchBlock(tryStatement,
                    tryEndStatementConnections.Distinct()));
            }

            // if this try is inside another try, the `beforeTryBlock` must have edges to the outer catch & finally blocks
            Block beforeTryBlock;
            if (currentBlock is BranchBlock possibleOuterTry &&
                possibleOuterTry.BranchingNode.IsKind(SyntaxKind.TryStatement))
            {
                var beforeTryConnections = possibleOuterTry.SuccessorBlocks.ToList();
                beforeTryConnections.Add(tryBody);
                beforeTryBlock = CreateBranchBlock(tryStatement, beforeTryConnections.Distinct());
            }
            else
            {
                // otherwise, what happens before the try is not handled by any catch or finally
                beforeTryBlock = CreateBlock(tryBody);
            }

            if (hasFinally)
            {
                this.exitTarget.Pop();
            }

            return beforeTryBlock;
        }

        private Block BuildGotoDefaultStatement(GotoStatementSyntax statement, Block currentBlock)
        {
            if (this.switchGotoJumpBlocks.Count == 0)
            {
                throw new InvalidOperationException("goto default; outside a switch");
            }

            var jumpBlock = CreateJumpBlock(statement, CreateTemporaryBlock(), currentBlock);

            var currentJumpBlocks = this.switchGotoJumpBlocks.Peek();
            if (!currentJumpBlocks.ContainsKey(GotoDefaultEntry))
            {
                currentJumpBlocks.Add(GotoDefaultEntry, new List<JumpBlock>());
            }

            currentJumpBlocks[GotoDefaultEntry].Add(jumpBlock);

            return jumpBlock;
        }

        private Block BuildGotoCaseStatement(GotoStatementSyntax statement, Block currentBlock)
        {
            if (this.switchGotoJumpBlocks.Count == 0)
            {
                throw new InvalidOperationException("goto case; outside a switch");
            }

            var jumpBlock = CreateJumpBlock(statement, CreateTemporaryBlock(), currentBlock);
            var currentJumpBlocks = this.switchGotoJumpBlocks.Peek();
            var indexer = GetCaseIndexer(statement.Expression);

            if (!currentJumpBlocks.ContainsKey(indexer))
            {
                currentJumpBlocks.Add(indexer, new List<JumpBlock>());
            }

            currentJumpBlocks[indexer].Add(jumpBlock);

            return jumpBlock;
        }

        private Block BuildGotoStatement(GotoStatementSyntax statement, Block currentBlock)
        {
            var jumpBlock = CreateJumpBlock(statement, CreateTemporaryBlock(), currentBlock);

            if (!(statement.Expression is IdentifierNameSyntax identifier))
            {
                throw new InvalidOperationException("goto with no identifier");
            }

            if (!this.gotoJumpBlocks.ContainsKey(identifier.Identifier.ValueText))
            {
                this.gotoJumpBlocks.Add(identifier.Identifier.ValueText, new List<JumpBlock>());
            }

            this.gotoJumpBlocks[identifier.Identifier.ValueText].Add(jumpBlock);

            return jumpBlock;
        }

        #endregion Build label, goto, goto case, goto default

        #region Build switch

        private Block BuildSwitchStatement(SwitchStatementSyntax switchStatement, Block currentBlock)
        {
            var caseBlocksByValue = new Dictionary<object, Block>();
            this.breakTarget.Push(currentBlock);
            this.switchGotoJumpBlocks.Push(new Dictionary<object, List<JumpBlock>>());

            // Default section is always evaluated last, we are handling it first because
            // the CFG is built in reverse order
            var defaultSection = switchStatement.Sections.FirstOrDefault(ContainsDefaultLabel);
            var defaultSectionBlock = currentBlock;
            if (defaultSection != null)
            {
                defaultSectionBlock = BuildStatements(defaultSection.Statements, CreateBlock(currentBlock));
                caseBlocksByValue[GotoDefaultEntry] = defaultSectionBlock; // All "goto default;" will jump to this block
            }

            var currentSectionBlock = defaultSectionBlock;
            foreach (var section in switchStatement.Sections.Reverse())
            {
                Block sectionBlock;
                if (section == defaultSection)
                {
                    // Skip the default section if it contains a single default label; we already handled it
                    if (section.Labels.Count == 1)
                    {
                        continue;
                    }
                    sectionBlock = defaultSectionBlock;
                }
                else
                {
                    sectionBlock = BuildStatements(section.Statements, CreateBlock(currentBlock));
                }

                foreach (var label in section.Labels.Reverse())
                {
                    // Handle C#7 pattern matching case Block
                    if (CasePatternSwitchLabelSyntaxWrapper.IsInstance(label))
                    {
                        var casePatternSwitchLabel = (CasePatternSwitchLabelSyntaxWrapper)label;
                        currentSectionBlock = BuildCasePattern(casePatternSwitchLabel,
                            trueSuccessor: sectionBlock, falseSuccessor: currentSectionBlock);
                    }
                    else if (label is CaseSwitchLabelSyntax simpleCaseLabel)
                    {
                        currentSectionBlock = BuildExpression(switchStatement.Expression, CreateBinaryBranchBlock(simpleCaseLabel,
                            sectionBlock, currentSectionBlock));
                        var key = GetCaseIndexer(simpleCaseLabel.Value);
                        caseBlocksByValue[key] = sectionBlock;
                    }
                }
            }

            this.breakTarget.Pop();
            var gotosToFix = this.switchGotoJumpBlocks.Pop();
            FixJumps(gotosToFix, caseBlocksByValue);
            var switchBlock = CreateBranchBlock(switchStatement, new[] { currentSectionBlock });
            return BuildExpression(switchStatement.Expression, switchBlock);
            bool ContainsDefaultLabel(SwitchSectionSyntax s) =>
                s.Labels.Any(l => l.IsKind(SyntaxKind.DefaultSwitchLabel));
        }

        private Block BuildSwitchExpression(SwitchExpressionSyntaxWrapper switchExpressionSyntax, Block currentBlock)
        {
            var currentArmBlock = currentBlock;

            foreach (var arm in switchExpressionSyntax.Arms.Reverse())
            {
                var armBlock = BuildExpression(arm.Expression, CreateBlock(currentBlock));

                currentArmBlock = BuildArmBranch(arm, armBlock, currentArmBlock);
            }

            return BuildExpression(switchExpressionSyntax.GoverningExpression, currentArmBlock);
        }

        private Block BuildArmBranch(SwitchExpressionArmSyntaxWrapper switchExpressionArmSyntax, Block trueSuccessor, Block falseSuccessor)
        {
            var newTrueSuccessor = CreateWhenCloseNewTrueSuccessor(switchExpressionArmSyntax.WhenClause, trueSuccessor, falseSuccessor);

            var currentBlock = CreateCurrentBlock(switchExpressionArmSyntax, newTrueSuccessor, falseSuccessor);

            currentBlock = BuildPatternExpression(switchExpressionArmSyntax.Pattern, currentBlock);

            return currentBlock;
        }

        private Block CreateCurrentBlock(SwitchExpressionArmSyntaxWrapper switchExpressionArmSyntax, Block trueSuccessor, Block falseSuccessor) =>
            switchExpressionArmSyntax.Pattern.SyntaxNode.IsKind(SyntaxKindEx.DiscardPattern)
                ? CreateBlock(trueSuccessor)
                : (Block)CreateBinaryBranchBlock(switchExpressionArmSyntax, trueSuccessor, falseSuccessor);

        private Block BuildCasePattern(CasePatternSwitchLabelSyntaxWrapper casePatternSwitchLabel,
            Block trueSuccessor, Block falseSuccessor)
        {
            var newTrueSuccessor = CreateWhenCloseNewTrueSuccessor(casePatternSwitchLabel.WhenClause, trueSuccessor, falseSuccessor);

            var currentBlock = CreateBinaryBranchBlock(casePatternSwitchLabel, newTrueSuccessor, falseSuccessor);

            currentBlock.ReversedInstructions.Add(casePatternSwitchLabel.Pattern);

            return currentBlock;
        }

        private Block CreateWhenCloseNewTrueSuccessor(WhenClauseSyntaxWrapper whenClauseSyntax, Block trueSuccessor, Block falseSuccessor) =>
            whenClauseSyntax.SyntaxNode != null
                ? BuildCondition(whenClauseSyntax.Condition, trueSuccessor, falseSuccessor)
                : trueSuccessor;

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

        #endregion Build switch

        #region Build jumps: break, continue, return, throw, yield break

        private Block BuildBreakStatement(BreakStatementSyntax breakStatement, Block currentBlock)
        {
            if (this.breakTarget.Count == 0)
            {
                throw new InvalidOperationException("break; outside a loop");
            }

            var target = this.breakTarget.Peek();
            if (currentBlock is BranchBlock possibleTryBlock &&
                possibleTryBlock.BranchingNode.IsKind(SyntaxKind.TryStatement))
            {
                var newSuccessors = possibleTryBlock.SuccessorBlocks.ToList();
                newSuccessors.Add(target);
                var branchBlock = CreateBranchBlock(possibleTryBlock.BranchingNode, newSuccessors);
                return branchBlock;
            }
            return CreateJumpBlock(breakStatement, target, currentBlock);
        }

        private Block BuildContinueStatement(ContinueStatementSyntax continueStatement, Block currentBlock)
        {
            if (this.continueTargets.Count == 0)
            {
                throw new InvalidOperationException("continue; outside a loop");
            }

            var target = this.continueTargets.Peek();
            return CreateJumpBlock(continueStatement, target, currentBlock);
        }

        private Block BuildReturnStatement(ReturnStatementSyntax returnStatement, Block currentBlock)
        {
            return BuildJumpToExitStatement(returnStatement, currentBlock, returnStatement.Expression);
        }

        private Block BuildThrowStatement(ThrowStatementSyntax throwStatement, Block currentBlock)
        {
            return BuildJumpToExitStatement(throwStatement, currentBlock, throwStatement.Expression);
        }

        private Block BuildYieldBreakStatement(YieldStatementSyntax yieldBreakStatement, Block currentBlock)
        {
            return BuildJumpToExitStatement(yieldBreakStatement, currentBlock);
        }

        private Block BuildYieldReturnStatement(YieldStatementSyntax yieldReturnStatement, Block currentBlock)
        {
            return BuildExpression(yieldReturnStatement.Expression, CreateJumpBlock(yieldReturnStatement, currentBlock, currentBlock));
        }

        private Block BuildJumpToExitStatement(StatementSyntax statement, Block currentBlock, ExpressionSyntax expression = null)
        {
            // When there is a `throw` inside a `try`, and there is a `catch` with a filter,
            // the `throw` block should point to both the `catch` and the `exit` blocks.
            if (currentBlock.SuccessorBlocks.Any(b => b is BinaryBranchBlock x && x.BranchingNode.IsKind(SyntaxKind.CatchFilterClause)) &&
                currentBlock.SuccessorBlocks.Contains(this.exitTarget.Peek()))
            {
                return BuildExpression(expression, currentBlock);
            }
            return BuildExpression(expression, CreateJumpBlock(statement, this.exitTarget.Peek(), currentBlock));
        }

        private Block BuildJumpToExitStatement(ExpressionSyntax expression, Block currentBlock, ExpressionSyntax innerExpression)
        {
            return BuildExpression(innerExpression, CreateJumpBlock(expression, this.exitTarget.Peek(), currentBlock));
        }

        #endregion Build jumps: break, continue, return, throw, yield break

        #region Build lock, using, fixed, unsafe checked statements

        private Block BuildLockStatement(LockStatementSyntax lockStatement, Block currentBlock)
        {
            var lockStatementBlock = BuildStatement(lockStatement.Statement, CreateBlock(currentBlock));

            return BuildExpression(lockStatement.Expression, CreateLockBlock(lockStatement, lockStatementBlock));
        }

        private Block BuildUsingStatement(UsingStatementSyntax usingStatement, Block currentBlock)
        {
            var usingStatementBlock = BuildStatement(usingStatement.Statement, CreateUsingFinalizerBlock(usingStatement, currentBlock));
            var usingBlock = CreateJumpBlock(usingStatement, usingStatementBlock);

            return usingStatement.Expression != null
                ? BuildExpression(usingStatement.Expression, usingBlock)
                : BuildVariableDeclaration(usingStatement.Declaration, usingBlock);
        }

        private Block BuildFixedStatement(FixedStatementSyntax fixedStatement, Block currentBlock)
        {
            var fixedStatementBlock = BuildStatement(fixedStatement.Statement, CreateBlock(currentBlock));
            return BuildVariableDeclaration(fixedStatement.Declaration, CreateJumpBlock(fixedStatement, fixedStatementBlock));
        }

        private Block BuildUnsafeStatement(UnsafeStatementSyntax statement, Block currentBlock)
        {
            var unsafeStatement = BuildStatement(statement.Block, CreateBlock(currentBlock));
            return CreateJumpBlock(statement, unsafeStatement);
        }

        private Block BuildCheckedStatement(CheckedStatementSyntax statement, Block currentBlock)
        {
            var statementBlock = BuildStatement(statement.Block, CreateBlock(currentBlock));
            return CreateJumpBlock(statement, statementBlock);
        }

        #endregion Build lock, using, fixed, unsafe checked statements

        #region Build loops - do, for, foreach, while

        private Block BuildDoStatement(DoStatementSyntax doStatement, Block currentBlock)
        {
            //// while (A) { B; }
            var conditionBlockTemp = CreateTemporaryBlock();

            var conditionBlock = BuildCondition(doStatement.Condition, conditionBlockTemp, currentBlock); // A

            this.breakTarget.Push(currentBlock);
            this.continueTargets.Push(conditionBlock);

            var loopBody = BuildStatement(doStatement.Statement, CreateBlock(conditionBlock)); // B
            conditionBlockTemp.SuccessorBlock = loopBody;

            this.breakTarget.Pop();
            this.continueTargets.Pop();

            return CreateBlock(loopBody);
        }

        private Block BuildForStatement(ForStatementSyntax forStatement, Block currentBlock)
        {
            //// for (A; B; C) { D; }

            var tempLoopBlock = CreateTemporaryBlock();

            var incrementorBlock = BuildExpressions(forStatement.Incrementors, CreateBlock(tempLoopBlock)); // C

            this.breakTarget.Push(currentBlock);
            this.continueTargets.Push(incrementorBlock);

            var forBlock = BuildStatement(forStatement.Statement, CreateBlock(incrementorBlock)); // D

            this.breakTarget.Pop();
            this.continueTargets.Pop();

            var conditionBlock = BuildExpression(forStatement.Condition,
                CreateBinaryBranchBlock(forStatement, forBlock, currentBlock)); // B
            tempLoopBlock.SuccessorBlock = conditionBlock;

            Block forInitializer = AddBlock(new ForInitializerBlock(forStatement, conditionBlock)); // A
            if (forStatement.Declaration != null)
            {
                forInitializer = BuildVariableDeclaration(forStatement.Declaration, forInitializer);
            }

            forInitializer = BuildExpressions(forStatement.Initializers, forInitializer);

            return forInitializer;
        }

        private Block BuildForEachVariableStatement(ForEachVariableStatementSyntaxWrapper foreachStatement, Block currentBlock)
            => BuildForEachStatement(foreachStatement, foreachStatement.Statement, foreachStatement.Expression, currentBlock);

        private Block BuildForEachStatement(ForEachStatementSyntax foreachStatement, Block currentBlock)
            => BuildForEachStatement(foreachStatement, foreachStatement.Statement, foreachStatement.Expression, currentBlock);

        private Block BuildForEachStatement(StatementSyntax foreachStatement, StatementSyntax foreachBodyStatement, ExpressionSyntax foreachExpression, Block currentBlock)
        {
            var temp = CreateTemporaryBlock();

            this.breakTarget.Push(currentBlock);
            this.continueTargets.Push(temp);

            var foreachBlock = BuildStatement(foreachBodyStatement, CreateBlock(temp));

            this.breakTarget.Pop();
            this.continueTargets.Pop();

            // Variable declaration in a foreach statement is not a VariableDeclarator, otherwise it would be added here.
            temp.SuccessorBlock = CreateBinaryBranchBlock(foreachStatement, foreachBlock, currentBlock);

            return BuildExpression(foreachExpression, AddBlock(new ForeachCollectionProducerBlock(foreachStatement, temp)));
        }

        private Block BuildWhileStatement(WhileStatementSyntax whileStatement, Block currentBlock)
        {
            var loopTempBlock = CreateTemporaryBlock();

            this.breakTarget.Push(currentBlock);
            this.continueTargets.Push(loopTempBlock);

            var bodyBlock = BuildStatement(whileStatement.Statement, CreateBlock(loopTempBlock));

            this.breakTarget.Pop();
            this.continueTargets.Pop();

            var loopCondition = BuildCondition(whileStatement.Condition, bodyBlock, currentBlock);

            loopTempBlock.SuccessorBlock = loopCondition;

            return CreateBlock(loopCondition);
        }

        #endregion Build loops - do, for, foreach, while

        #region Build if statement

        private Block BuildIfStatement(IfStatementSyntax ifStatement, Block currentBlock)
        {
            var elseBlock = ifStatement.Else?.Statement != null
                ? BuildStatement(ifStatement.Else.Statement, CreateBlock(currentBlock))
                : currentBlock;
            var trueBlock = BuildStatement(ifStatement.Statement, CreateBlock(currentBlock));

            var ifConditionBlock = BuildCondition(ifStatement.Condition, trueBlock, elseBlock);

            return ifConditionBlock;
        }

        #endregion Build if statement

        #region Build block

        private Block BuildBlock(BlockSyntax block, Block currentBlock)
        {
            return BuildStatements(block.Statements, currentBlock);
        }

        #endregion Build block

        #endregion Build statements

        #region Build expressions

        private Block BuildConditionalAccessExpression(ConditionalAccessExpressionSyntax conditionalAccess,
            Block currentBlock)
        {
            var whenNotNull = BuildExpression(conditionalAccess.WhenNotNull, CreateBlock(currentBlock));

            return BuildExpression(conditionalAccess.Expression,
                CreateBinaryBranchBlock(conditionalAccess, currentBlock, whenNotNull));
        }

        private Block BuildConditionalExpression(ConditionalExpressionSyntax conditional, Block currentBlock)
        {
            var falseBlock = BuildExpression(conditional.WhenFalse, CreateBlock(currentBlock));
            var trueBlock = BuildExpression(conditional.WhenTrue, CreateBlock(currentBlock));

            return BuildCondition(conditional.Condition, trueBlock, falseBlock);
        }

        private Block BuildCoalesceExpression(BinaryExpressionSyntax expression, Block currentBlock)
        {
            var rightBlock = BuildExpression(expression.Right, CreateBlock(currentBlock));

            return BuildExpression(expression.Left, CreateBinaryBranchBlock(expression, rightBlock, currentBlock));
        }

        private Block BuildLogicalAndExpression(BinaryExpressionSyntax expression, Block currentBlock)
        {
            var rightBlock = BuildExpression(expression.Right,
                AddBlock(new BinaryBranchingSimpleBlock(expression.Right, currentBlock)));

            return BuildExpression(expression.Left, CreateBinaryBranchBlock(expression, rightBlock, currentBlock));
        }

        private Block BuildLogicalOrExpression(BinaryExpressionSyntax expression, Block currentBlock)
        {
            var rightBlock = BuildExpression(expression.Right,
                AddBlock(new BinaryBranchingSimpleBlock(expression.Right, currentBlock)));

            return BuildExpression(expression.Left, CreateBinaryBranchBlock(expression, currentBlock, rightBlock));
        }

        private Block BuildArrayCreationExpression(ArrayCreationExpressionSyntax expression, Block currentBlock)
        {
            var arrayInitializerBlock = BuildExpression(expression.Initializer, currentBlock);
            arrayInitializerBlock.ReversedInstructions.Add(expression);

            return BuildExpression(expression.Type, arrayInitializerBlock);
        }

        private Block BuildElementAccessExpression(ElementAccessExpressionSyntax expression, Block currentBlock)
        {
            return BuildInvocationLikeExpression(expression, currentBlock, expression.Expression,
                expression.ArgumentList?.Arguments);
        }

        private Block BuildImplicitElementAccessExpression(ImplicitElementAccessSyntax expression, Block currentBlock)
        {
            return BuildInvocationLikeExpression(expression, currentBlock, null, expression.ArgumentList?.Arguments);
        }

        private Block BuildInvocationLikeExpression(ExpressionSyntax parent, Block currentBlock,
            ExpressionSyntax child, IEnumerable<ArgumentSyntax> arguments)
        {
            currentBlock.ReversedInstructions.Add(parent);
            var isNameof = parent is InvocationExpressionSyntax invocation
                && invocation.IsNameof(this.semanticModel);

            // The nameof arguments are not evaluated at runtime and should not be added
            // to the block as instructions
            if (isNameof)
                return currentBlock;

            // ref arguments should be added at the end since they remove
            // the constraints on the arguments after all the other arguments are evaluated
            foreach (var arg in arguments.Reverse())
            {
                if (arg.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword))
                {
                    currentBlock = BuildExpression(arg.Expression, currentBlock);
                }
            }

            foreach (var arg in arguments.Reverse())
            {
                if (!arg.RefOrOutKeyword.IsKind(SyntaxKind.RefKeyword))
                {
                    currentBlock = BuildExpression(arg.Expression, currentBlock);
                }
            }
            return BuildExpression(child, currentBlock);
        }

        private Block BuildObjectCreationExpression(ObjectCreationExpressionSyntax expression, Block currentBlock)
        {
            var objectInitializerBlock = BuildExpression(expression.Initializer, currentBlock);
            objectInitializerBlock.ReversedInstructions.Add(expression);

            var arguments = expression.ArgumentList == null
                ? Enumerable.Empty<ExpressionSyntax>()
                : expression.ArgumentList.Arguments.Select(a => a.Expression);

            return BuildExpressions(arguments, objectInitializerBlock);
        }

        private Block BuildAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax expression,
            Block currentBlock)
        {
            return BuildSimpleNestedExpression(expression, currentBlock,
                expression.Initializers.Select(i => i.Expression));
        }

        private Block BuildInvocationExpression(InvocationExpressionSyntax expression, Block currentBlock)
        {
            return BuildInvocationLikeExpression(expression, currentBlock, expression.Expression,
                expression.ArgumentList?.Arguments);
        }

        private Block BuildInterpolatedStringExpression(InterpolatedStringExpressionSyntax expression,
            Block currentBlock)
        {
            return BuildSimpleNestedExpression(expression, currentBlock,
                expression.Contents.OfType<InterpolationSyntax>().Select(i => i.Expression));
        }

        private Block BuildSimpleNestedExpression(ExpressionSyntax parent, Block currentBlock,
            params ExpressionSyntax[] children)
        {
            return BuildSimpleNestedExpression(parent, currentBlock, (IEnumerable<ExpressionSyntax>)children);
        }

        private Block BuildSimpleNestedExpression(ExpressionSyntax parent, Block currentBlock,
            IEnumerable<ExpressionSyntax> children)
        {
            currentBlock.ReversedInstructions.Add(parent);

            // The nameof arguments are not evaluated at runtime and should not be added
            // to the block as instructions
            var isNameof = parent is InvocationExpressionSyntax invocation
                && invocation.IsNameof(this.semanticModel);

            return children == null || isNameof
                ? currentBlock
                : BuildExpressions(children, currentBlock);
        }

        private Block BuildBinaryExpression(BinaryExpressionSyntax expression, Block currentBlock)
        {
            currentBlock.ReversedInstructions.Add(expression);
            var binaryExpressionBlock = BuildExpression(expression.Right, currentBlock);
            return BuildExpression(expression.Left, binaryExpressionBlock);
        }

        private Block BuildAssignmentExpression(AssignmentExpressionSyntax expression, Block currentBlock)
        {
            currentBlock.ReversedInstructions.Add(expression);
            var binaryExpressionBlock = BuildExpression(expression.Right, currentBlock);
            return BuildExpression(expression.Left, binaryExpressionBlock);
        }

        private Block BuildCoalesceAssignmentExpression(AssignmentExpressionSyntax expression, Block currentBlock)
        {
            currentBlock.ReversedInstructions.Add(expression);
            var rightBlock = BuildExpression(expression.Right, CreateBlock(currentBlock));
            return BuildExpression(expression.Left, CreateBinaryBranchBlock(expression, rightBlock, currentBlock));
        }

        private Block BuildSimpleAssignmentExpression(AssignmentExpressionSyntax expression, Block currentBlock)
        {
            currentBlock.ReversedInstructions.Add(expression);

            var assignmentBlock = BuildExpression(expression.Right, currentBlock);
            if (!IsAssignmentWithSimpleLeftSide(expression))
            {
                assignmentBlock = BuildExpression(expression.Left, assignmentBlock);
            }
            return assignmentBlock;
        }

        public static bool IsAssignmentWithSimpleLeftSide(AssignmentExpressionSyntax assignment)
        {
            return assignment.Left.RemoveParentheses() is IdentifierNameSyntax;
        }

        private Block BuildArrayType(ArrayTypeSyntax arrayType, Block currentBlock)
        {
            currentBlock.ReversedInstructions.Add(arrayType);

            var arraySizes = arrayType.RankSpecifiers.SelectMany(rs => rs.Sizes);
            return BuildExpressions(arraySizes, currentBlock);
        }

        #endregion Build expressions

        #region Build variable declaration

        private Block BuildVariableDeclaration(VariableDeclarationSyntax declaration, Block currentBlock)
        {
            if (declaration == null)
            {
                return currentBlock;
            }

            var variableDeclaratorBlock = currentBlock;
            foreach (var variable in declaration.Variables.Reverse())
            {
                variableDeclaratorBlock = BuildVariableDeclarator(variable, variableDeclaratorBlock);
            }

            return variableDeclaratorBlock;
        }

        private Block BuildVariableDeclarator(VariableDeclaratorSyntax variableDeclarator, Block currentBlock)
        {
            // There are variable declarations which implicitly get a value, such as foreach (var x in xs)
            currentBlock.ReversedInstructions.Add(variableDeclarator);

            var initializer = variableDeclarator.Initializer?.Value;
            return initializer == null
                ? currentBlock
                : BuildExpression(initializer, currentBlock);
        }

        #endregion Build variable declaration

        #region Create*

        internal LockBlock CreateLockBlock(LockStatementSyntax lockStatement, Block successor) =>
            AddBlock(new LockBlock(lockStatement, successor));

        internal UsingEndBlock CreateUsingFinalizerBlock(UsingStatementSyntax usingStatement, Block successor) =>
            AddBlock(new UsingEndBlock(usingStatement, successor));

        #endregion Create*

        #region Condition

        /// <summary>
        /// Builds a conditional expression with two successor blocks. The BuildExpression method
        /// creates a tree with only one successor.
        /// </summary>
        private Block BuildCondition(ExpressionSyntax expression, Block trueSuccessor, Block falseSuccessor)
        {
            expression = expression.RemoveParentheses();

            if (expression is BinaryExpressionSyntax binaryExpression)
            {
                switch (expression.Kind())
                {
                    case SyntaxKind.LogicalOrExpression:
                        return BuildCondition(
                            binaryExpression.Left,
                            trueSuccessor,
                            BuildCondition(binaryExpression.Right, trueSuccessor, falseSuccessor));

                    case SyntaxKind.LogicalAndExpression:
                        return BuildCondition(
                            binaryExpression.Left,
                            BuildCondition(binaryExpression.Right, trueSuccessor, falseSuccessor),
                            falseSuccessor);

                    case SyntaxKind.CoalesceExpression:
                        return BuildCondition(
                             binaryExpression.Left,
                             BuildCondition(binaryExpression.Right, trueSuccessor, falseSuccessor),
                             CreateBranchBlock(binaryExpression.Left, successors: new[] { trueSuccessor, falseSuccessor }));
                }
            }

            // Fallback to generating an additional branch block for the if statement itself.
            return BuildExpression(expression,
                    AddBlock(new BinaryBranchBlock(expression, trueSuccessor, falseSuccessor)));
        }

        #endregion Condition

        #endregion Build*
    }
}
