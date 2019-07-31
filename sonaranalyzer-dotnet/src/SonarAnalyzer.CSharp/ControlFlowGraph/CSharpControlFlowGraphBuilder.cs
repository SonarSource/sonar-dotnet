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
    internal sealed class CSharpControlFlowGraphBuilder : AbstractControlFlowGraphBuilder
    {
        private readonly Stack<Block> BreakTarget = new Stack<Block>();
        private readonly Stack<Block> ContinueTargets = new Stack<Block>();

        private readonly Stack<Dictionary<object, List<JumpBlock>>> SwitchGotoJumpBlocks =
            new Stack<Dictionary<object, List<JumpBlock>>>();

        private readonly Dictionary<string, List<JumpBlock>> GotoJumpBlocks = new Dictionary<string, List<JumpBlock>>();
        private readonly Dictionary<string, JumpBlock> LabeledStatements = new Dictionary<string, JumpBlock>();
        private static readonly object GotoDefaultEntry = new object();
        private static readonly object GotoNullEntry = new object();


        internal CSharpControlFlowGraphBuilder(CSharpSyntaxNode node, SemanticModel semanticModel)
            : base(node, semanticModel)
        {
        }

        #region Fix jump statements

        protected override void PostProcessGraph()
        {
            FixJumps(this.GotoJumpBlocks, this.LabeledStatements.ToDictionary(e => e.Key, e => (Block)e.Value));
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
                    this.reversedBlocks.Remove(jumpBlock.SuccessorBlock);
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
                        return BuildSimpleNestedExpression(parent, currentBlock, parent.Type);
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

                    currentBlock.ReversedInstructions.Add(expression);

                    if (ConstantPatternSyntaxWrapper.IsInstance(isPatternExpression.Pattern))
                    {
                        var constantPattern = (ConstantPatternSyntaxWrapper)isPatternExpression.Pattern;

                        currentBlock = BuildExpression(constantPattern.Expression, currentBlock);
                    }
                    else if (DeclarationPatternSyntaxWrapper.IsInstance(isPatternExpression.Pattern))
                    {
                        // Do nothing, this is just variable assignment and the Pattern itself contains
                        // only the new variable(s), which are not enough to evaluate the assignment.
                        // The handling should be done in CSharpExplodedGraph and UcfgInstructionFactory.
                    }
                    else
                    {
                        throw new NotSupportedException($"{isPatternExpression.Pattern.SyntaxNode.Kind()}");
                    }

                    return BuildExpression(isPatternExpression.Expression, currentBlock);

                default:
                    throw new NotSupportedException($"{expression.Kind()}");
            }
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

            this.LabeledStatements[labeledStatement.Identifier.ValueText] = jumpBlock;

            return CreateBlock(jumpBlock);
        }

        private Block BuildTryStatement(TryStatementSyntax tryStatement, Block currentBlock)
        {
            // successor - either finally of next block after try statement
            var catchSuccessor = currentBlock;

            var hasFinally = tryStatement.Finally?.Block != null;
            if (hasFinally)
            {
                // Create a finally block for the happy path where no exceptions are thrown
                catchSuccessor = BuildBlock(tryStatement.Finally.Block, CreateBlock(catchSuccessor));

                // Wire another finally block to the exit target stack in case we have a return inside the try/catch block
                this.ExitTarget.Push(BuildBlock(tryStatement.Finally.Block, CreateBlock(this.ExitTarget.Peek())));
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
                tryEndStatementConnections.Add(this.ExitTarget.Peek());
            }

            Block tryBody;
            if (tryStatement.Block.Statements.Any(s=>s.IsKind(SyntaxKind.ReturnStatement)))
            {
                // there is a return inside the `try`, thus a JumpBlock directly to exit will be created
                var returnBlock = BuildBlock(tryStatement.Block, currentBlock);
                var connections = new List<Block>();
                connections.Add(returnBlock);
                // if an exception is thrown, it will reach the `catch` blocks
                connections.AddRange(catchBlocks);
                // if there is a finally, add it; otherwise, the `return` will jump to exit
                if (catchSuccessor != currentBlock)
                {
                    connections.Add(catchSuccessor);
                }
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
                this.ExitTarget.Pop();
            }

            return beforeTryBlock;
        }

        private Block BuildGotoDefaultStatement(GotoStatementSyntax statement, Block currentBlock)
        {
            if (this.SwitchGotoJumpBlocks.Count == 0)
            {
                throw new InvalidOperationException("goto default; outside a switch");
            }

            var jumpBlock = CreateJumpBlock(statement, CreateTemporaryBlock(), currentBlock);

            var currentJumpBlocks = this.SwitchGotoJumpBlocks.Peek();
            if (!currentJumpBlocks.ContainsKey(GotoDefaultEntry))
            {
                currentJumpBlocks.Add(GotoDefaultEntry, new List<JumpBlock>());
            }

            currentJumpBlocks[GotoDefaultEntry].Add(jumpBlock);

            return jumpBlock;
        }

        private Block BuildGotoCaseStatement(GotoStatementSyntax statement, Block currentBlock)
        {
            if (this.SwitchGotoJumpBlocks.Count == 0)
            {
                throw new InvalidOperationException("goto case; outside a switch");
            }

            var jumpBlock = CreateJumpBlock(statement, CreateTemporaryBlock(), currentBlock);
            var currentJumpBlocks = this.SwitchGotoJumpBlocks.Peek();
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

            if (!this.GotoJumpBlocks.ContainsKey(identifier.Identifier.ValueText))
            {
                this.GotoJumpBlocks.Add(identifier.Identifier.ValueText, new List<JumpBlock>());
            }

            this.GotoJumpBlocks[identifier.Identifier.ValueText].Add(jumpBlock);

            return jumpBlock;
        }

        #endregion Build label, goto, goto case, goto default

        #region Build switch

        private Block BuildSwitchStatement(SwitchStatementSyntax switchStatement, Block currentBlock)
        {
            var caseBlocksByValue = new Dictionary<object, Block>();
            BreakTarget.Push(currentBlock);
            SwitchGotoJumpBlocks.Push(new Dictionary<object, List<JumpBlock>>());

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

            BreakTarget.Pop();
            var gotosToFix = SwitchGotoJumpBlocks.Pop();
            FixJumps(gotosToFix, caseBlocksByValue);
            var switchBlock = CreateBranchBlock(switchStatement, new[] { currentSectionBlock });
            return BuildExpression(switchStatement.Expression, switchBlock);
            bool ContainsDefaultLabel(SwitchSectionSyntax s) =>
                s.Labels.Any(l => l.IsKind(SyntaxKind.DefaultSwitchLabel));
        }

        private Block BuildCasePattern(CasePatternSwitchLabelSyntaxWrapper casePatternSwitchLabel,
            Block trueSuccessor, Block falseSuccessor)
        {
            var newTrueSuccessor = casePatternSwitchLabel.WhenClause.SyntaxNode != null
                ? BuildCondition(casePatternSwitchLabel.WhenClause.Condition, trueSuccessor, falseSuccessor)
                : trueSuccessor;

            var currentBlock = CreateBinaryBranchBlock(casePatternSwitchLabel, newTrueSuccessor, falseSuccessor);

            currentBlock.ReversedInstructions.Add(casePatternSwitchLabel.Pattern);

            return currentBlock;
        }

        private object GetCaseIndexer(ExpressionSyntax expression)
        {
            var constValue = this.semanticModel.GetConstantValue(expression);
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
            if (this.BreakTarget.Count == 0)
            {
                throw new InvalidOperationException("break; outside a loop");
            }

            var target = this.BreakTarget.Peek();
            if (currentBlock is BranchBlock possibleTryBlock &&
                possibleTryBlock.BranchingNode.IsKind(SyntaxKind.TryStatement))
            {
                var newSuccessors = possibleTryBlock.SuccessorBlocks.ToList();
                newSuccessors.Add(target);
                var branchBlock = CreateBranchBlock(possibleTryBlock.BranchingNode, newSuccessors);
                branchBlock.ReversedInstructions.Add(breakStatement);
                return branchBlock;
            }
            return CreateJumpBlock(breakStatement, target, currentBlock);
        }

        private Block BuildContinueStatement(ContinueStatementSyntax continueStatement, Block currentBlock)
        {
            if (this.ContinueTargets.Count == 0)
            {
                throw new InvalidOperationException("continue; outside a loop");
            }

            var target = this.ContinueTargets.Peek();
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
                currentBlock.SuccessorBlocks.Contains(this.ExitTarget.Peek()))
            {
                return BuildExpression(expression, currentBlock);
            }
            return BuildExpression(expression, CreateJumpBlock(statement, this.ExitTarget.Peek(), currentBlock));
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

            this.BreakTarget.Push(currentBlock);
            this.ContinueTargets.Push(conditionBlock);

            var loopBody = BuildStatement(doStatement.Statement, CreateBlock(conditionBlock)); // B
            conditionBlockTemp.SuccessorBlock = loopBody;

            this.BreakTarget.Pop();
            this.ContinueTargets.Pop();

            return CreateBlock(loopBody);
        }

        private Block BuildForStatement(ForStatementSyntax forStatement, Block currentBlock)
        {
            //// for (A; B; C) { D; }

            var tempLoopBlock = CreateTemporaryBlock();

            var incrementorBlock = BuildExpressions(forStatement.Incrementors, CreateBlock(tempLoopBlock)); // C

            this.BreakTarget.Push(currentBlock);
            this.ContinueTargets.Push(incrementorBlock);

            var forBlock = BuildStatement(forStatement.Statement, CreateBlock(incrementorBlock)); // D

            this.BreakTarget.Pop();
            this.ContinueTargets.Pop();

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

        private Block BuildForEachStatement(ForEachStatementSyntax foreachStatement, Block currentBlock)
        {
            var temp = CreateTemporaryBlock();

            this.BreakTarget.Push(currentBlock);
            this.ContinueTargets.Push(temp);

            var foreachBlock = BuildStatement(foreachStatement.Statement, CreateBlock(temp));

            this.BreakTarget.Pop();
            this.ContinueTargets.Pop();

            // Variable declaration in a foreach statement is not a VariableDeclarator, otherwise it would be added here.
            temp.SuccessorBlock = CreateBinaryBranchBlock(foreachStatement, foreachBlock, currentBlock);

            return BuildExpression(foreachStatement.Expression,
                    AddBlock(new ForeachCollectionProducerBlock(foreachStatement, temp)));
        }

        private Block BuildWhileStatement(WhileStatementSyntax whileStatement, Block currentBlock)
        {
            var loopTempBlock = CreateTemporaryBlock();

            this.BreakTarget.Push(currentBlock);
            this.ContinueTargets.Push(loopTempBlock);

            var bodyBlock = BuildStatement(whileStatement.Statement, CreateBlock(loopTempBlock));

            this.BreakTarget.Pop();
            this.ContinueTargets.Pop();

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
            var expression = BuildExpression(conditionalAccess.Expression, CreateBlock(currentBlock));
            return BuildExpression(conditionalAccess.WhenNotNull,
                CreateBinaryBranchBlock(conditionalAccess, currentBlock, expression));
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

        internal static bool IsAssignmentWithSimpleLeftSide(AssignmentExpressionSyntax assignment)
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
                        CreateBranchBlock(binaryExpression.Left,successors: new[] { trueSuccessor, falseSuccessor }));

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
