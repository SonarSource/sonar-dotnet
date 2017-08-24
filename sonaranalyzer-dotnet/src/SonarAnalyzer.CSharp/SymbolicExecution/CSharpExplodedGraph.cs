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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.SymbolicExecution;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.SymbolicExecution.CFG;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.SymbolicValues;

namespace SonarAnalyzer.SymbolicExecution
{
    internal class CSharpExplodedGraph : AbstractExplodedGraph
    {
        public CSharpExplodedGraph(IControlFlowGraph cfg, ISymbol declaration, SemanticModel semanticModel, AbstractLiveVariableAnalysis lva)
            : base(cfg, declaration, semanticModel, lva)
        {
            // Add mandatory checks
            AddExplodedGraphCheck(new NullPointerDereference.NullPointerCheck(this));
            AddExplodedGraphCheck(new EmptyNullableValueAccess.NullValueAccessedCheck(this));
            AddExplodedGraphCheck(new InvalidCastToInterface.NullableCastCheck(this));
        }

        #region Visit*

        protected override void VisitSimpleBlock(SimpleBlock block, ExplodedGraphNode node)
        {
            var newProgramState = node.ProgramState;

            var usingFinalizerBlock = block as UsingEndBlock;
            if (usingFinalizerBlock != null)
            {
                newProgramState = InvokeChecks(newProgramState, (ps, check) => check.PreProcessUsingStatement(node.ProgramPoint, ps));
                newProgramState = CleanStateAfterBlock(newProgramState, block);
                EnqueueAllSuccessors(block, newProgramState);
                return;
            }

            newProgramState = CleanStateAfterBlock(newProgramState, block);

            if (block is ForeachCollectionProducerBlock)
            {
                newProgramState = newProgramState.PopValue();
                EnqueueAllSuccessors(block, newProgramState);
                return;
            }

            var forInitializerBlock = block as ForInitializerBlock;
            if (forInitializerBlock != null)
            {
                newProgramState = newProgramState.PopValues(
                    forInitializerBlock.ForNode.Initializers.Count);

                newProgramState = newProgramState.PushValues(
                    Enumerable
                        .Range(0, forInitializerBlock.ForNode.Incrementors.Count)
                        .Select(i => new SymbolicValue()));

                EnqueueAllSuccessors(forInitializerBlock, newProgramState);
                return;
            }

            var lockBlock = block as LockBlock;
            if (lockBlock != null)
            {
                newProgramState = newProgramState.PopValue();

                newProgramState = newProgramState.RemoveSymbols(IsFieldSymbol);

                EnqueueAllSuccessors(block, newProgramState);
                return;
            }

            base.VisitSimpleBlock(block, node);
        }

        protected override void VisitBinaryBranch(BinaryBranchBlock binaryBranchBlock, ExplodedGraphNode node)
        {
            var newProgramState = CleanStateAfterBlock(node.ProgramState, node.ProgramPoint.Block);

            switch (binaryBranchBlock.BranchingNode.Kind())
            {
                case SyntaxKind.ForEachStatement:
                    VisitForeachBinaryBranch(binaryBranchBlock, newProgramState);
                    return;
                case SyntaxKind.CoalesceExpression:
                    VisitCoalesceExpressionBinaryBranch(binaryBranchBlock, newProgramState);
                    return;
                case SyntaxKind.ConditionalAccessExpression:
                    VisitConditionalAccessBinaryBranch(binaryBranchBlock, newProgramState);
                    return;

                case SyntaxKind.LogicalAndExpression:
                case SyntaxKind.LogicalOrExpression:
                    VisitBinaryBranch(binaryBranchBlock, node, ((BinaryExpressionSyntax)binaryBranchBlock.BranchingNode).Left);
                    return;

                case SyntaxKind.WhileStatement:
                    VisitBinaryBranch(binaryBranchBlock, node, ((WhileStatementSyntax)binaryBranchBlock.BranchingNode).Condition);
                    return;
                case SyntaxKind.DoStatement:
                    VisitBinaryBranch(binaryBranchBlock, node, ((DoStatementSyntax)binaryBranchBlock.BranchingNode).Condition);
                    return;
                case SyntaxKind.ForStatement:
                    VisitBinaryBranch(binaryBranchBlock, node, ((ForStatementSyntax)binaryBranchBlock.BranchingNode).Condition);
                    return;

                case SyntaxKind.IfStatement:
                    VisitBinaryBranch(binaryBranchBlock, node, ((IfStatementSyntax)binaryBranchBlock.BranchingNode).Condition);
                    return;
                case SyntaxKind.ConditionalExpression:
                    VisitBinaryBranch(binaryBranchBlock, node, ((ConditionalExpressionSyntax)binaryBranchBlock.BranchingNode).Condition);
                    return;

                default:
                    System.Diagnostics.Debug.Fail($"Branch kind '{binaryBranchBlock.BranchingNode.Kind()}' not handled");
                    VisitBinaryBranch(binaryBranchBlock, node, null);
                    return;
            }
        }

        protected override void VisitInstruction(ExplodedGraphNode node)
        {
            var instruction = node.ProgramPoint.Block.Instructions[node.ProgramPoint.Offset];
            var expression = instruction as ExpressionSyntax;
            var parenthesizedExpression = expression?.GetSelfOrTopParenthesizedExpression();
            var newProgramPoint = new ProgramPoint(node.ProgramPoint.Block, node.ProgramPoint.Offset + 1);
            var newProgramState = node.ProgramState;

            newProgramState = InvokeChecks(newProgramState, (ps, check) => check.PreProcessInstruction(node.ProgramPoint, ps));
            if (newProgramState == null)
            {
                return;
            }

            switch (instruction.Kind())
            {
                case SyntaxKind.VariableDeclarator:
                    newProgramState = VisitVariableDeclarator((VariableDeclaratorSyntax)instruction, newProgramState);
                    break;
                case SyntaxKind.SimpleAssignmentExpression:
                    newProgramState = VisitSimpleAssignment((AssignmentExpressionSyntax)instruction, newProgramState);
                    break;

                case SyntaxKind.OrAssignmentExpression:
                    newProgramState = VisitBooleanBinaryOpAssignment(newProgramState, (AssignmentExpressionSyntax)instruction, (l, r) => new OrSymbolicValue(l, r));
                    break;
                case SyntaxKind.AndAssignmentExpression:
                    newProgramState = VisitBooleanBinaryOpAssignment(newProgramState, (AssignmentExpressionSyntax)instruction, (l, r) => new AndSymbolicValue(l, r));
                    break;
                case SyntaxKind.ExclusiveOrAssignmentExpression:
                    newProgramState = VisitBooleanBinaryOpAssignment(newProgramState, (AssignmentExpressionSyntax)instruction, (l, r) => new XorSymbolicValue(l, r));
                    break;

                case SyntaxKind.SubtractAssignmentExpression:
                case SyntaxKind.AddAssignmentExpression:
                case SyntaxKind.DivideAssignmentExpression:
                case SyntaxKind.MultiplyAssignmentExpression:
                case SyntaxKind.ModuloAssignmentExpression:

                case SyntaxKind.LeftShiftAssignmentExpression:
                case SyntaxKind.RightShiftAssignmentExpression:
                    newProgramState = VisitOpAssignment((AssignmentExpressionSyntax)instruction, newProgramState);
                    break;

                case SyntaxKind.PreIncrementExpression:
                case SyntaxKind.PreDecrementExpression:
                    newProgramState = VisitPrefixIncrement((PrefixUnaryExpressionSyntax)instruction, newProgramState);
                    break;

                case SyntaxKind.PostIncrementExpression:
                case SyntaxKind.PostDecrementExpression:
                    newProgramState = VisitPostfixIncrement((PostfixUnaryExpressionSyntax)instruction, newProgramState);
                    break;

                case SyntaxKind.IdentifierName:
                    newProgramState = VisitIdentifier((IdentifierNameSyntax)instruction, newProgramState);
                    break;

                case SyntaxKind.BitwiseOrExpression:
                    newProgramState = VisitBinaryOperator(newProgramState, (l, r) => new OrSymbolicValue(l, r));
                    break;
                case SyntaxKind.BitwiseAndExpression:
                    newProgramState = VisitBinaryOperator(newProgramState, (l, r) => new AndSymbolicValue(l, r));
                    break;
                case SyntaxKind.ExclusiveOrExpression:
                    newProgramState = VisitBinaryOperator(newProgramState, (l, r) => new XorSymbolicValue(l, r));
                    break;

                case SyntaxKind.LessThanExpression:
                    newProgramState = VisitComparisonBinaryOperator(newProgramState, (BinaryExpressionSyntax)instruction, (l, r) => new ComparisonSymbolicValue(ComparisonKind.Less, l, r));
                    break;
                case SyntaxKind.LessThanOrEqualExpression:
                    newProgramState = VisitComparisonBinaryOperator(newProgramState, (BinaryExpressionSyntax)instruction, (l, r) => new ComparisonSymbolicValue(ComparisonKind.LessOrEqual, l, r));
                    break;
                case SyntaxKind.GreaterThanExpression:
                    newProgramState = VisitComparisonBinaryOperator(newProgramState, (BinaryExpressionSyntax)instruction, (l, r) => new ComparisonSymbolicValue(ComparisonKind.Less, r, l));
                    break;
                case SyntaxKind.GreaterThanOrEqualExpression:
                    newProgramState = VisitComparisonBinaryOperator(newProgramState, (BinaryExpressionSyntax)instruction, (l, r) => new ComparisonSymbolicValue(ComparisonKind.LessOrEqual, r, l));
                    break;

                case SyntaxKind.SubtractExpression:
                case SyntaxKind.AddExpression:
                case SyntaxKind.DivideExpression:
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.ModuloExpression:

                case SyntaxKind.LeftShiftExpression:
                case SyntaxKind.RightShiftExpression:

                    newProgramState = newProgramState.PopValues(2);
                    newProgramState = newProgramState.PushValue(new SymbolicValue());
                    break;

                case SyntaxKind.EqualsExpression:
                    var binary = (BinaryExpressionSyntax)instruction;
                    newProgramState = IsOperatorOnObject(instruction)
                        ? VisitReferenceEquals(binary, newProgramState)
                        : VisitValueEquals(newProgramState);

                    break;

                case SyntaxKind.NotEqualsExpression:
                    newProgramState = IsOperatorOnObject(instruction)
                        ? VisitBinaryOperator(newProgramState, (l, r) => new ReferenceNotEqualsSymbolicValue(l, r))
                        : VisitBinaryOperator(newProgramState, (l, r) => new ValueNotEqualsSymbolicValue(l, r));
                    break;

                case SyntaxKind.BitwiseNotExpression:
                case SyntaxKind.UnaryMinusExpression:
                case SyntaxKind.UnaryPlusExpression:
                case SyntaxKind.AddressOfExpression:
                case SyntaxKind.PointerIndirectionExpression:

                case SyntaxKind.MakeRefExpression:
                case SyntaxKind.RefTypeExpression:
                case SyntaxKind.RefValueExpression:

                case SyntaxKind.MemberBindingExpression:

                case SyntaxKind.AwaitExpression:
                    newProgramState = newProgramState.PopValue();
                    newProgramState = newProgramState.PushValue(new SymbolicValue());
                    break;

                case SyntaxKind.AsExpression:
                case SyntaxKind.IsExpression:
                    newProgramState = VisitSafeCastExpression((BinaryExpressionSyntax)instruction, newProgramState);
                    break;

                case SyntaxKind.SimpleMemberAccessExpression:
                    {
                        var memberAccess = (MemberAccessExpressionSyntax)instruction;
                        var check = explodedGraphChecks.OfType<EmptyNullableValueAccess.NullValueAccessedCheck>().FirstOrDefault();
                        if (check == null ||
                            !check.TryProcessInstruction(memberAccess, newProgramState, out newProgramState))
                        {
                            // Default behavior
                            newProgramState = VisitMemberAccess(memberAccess, newProgramState);
                        }
                    }
                    break;

                case SyntaxKind.PointerMemberAccessExpression:
                    {
                        newProgramState = VisitMemberAccess((MemberAccessExpressionSyntax)instruction, newProgramState);
                    }
                    break;

                case SyntaxKind.GenericName:
                case SyntaxKind.AliasQualifiedName:
                case SyntaxKind.QualifiedName:

                case SyntaxKind.PredefinedType:
                case SyntaxKind.NullableType:

                case SyntaxKind.OmittedArraySizeExpression:

                case SyntaxKind.AnonymousMethodExpression:
                case SyntaxKind.ParenthesizedLambdaExpression:
                case SyntaxKind.SimpleLambdaExpression:
                case SyntaxKind.QueryExpression:

                case SyntaxKind.ArgListExpression:
                    newProgramState = newProgramState.PushValue(new SymbolicValue());
                    break;
                case SyntaxKind.LogicalNotExpression:
                    {
                        SymbolicValue sv;
                        newProgramState = newProgramState.PopValue(out sv);
                        newProgramState = newProgramState.PushValue(new LogicalNotSymbolicValue(sv));
                    }
                    break;

                case SyntaxKind.TrueLiteralExpression:
                    newProgramState = newProgramState.PushValue(SymbolicValue.True);
                    break;
                case SyntaxKind.FalseLiteralExpression:
                    newProgramState = newProgramState.PushValue(SymbolicValue.False);
                    break;
                case SyntaxKind.NullLiteralExpression:
                    newProgramState = newProgramState.PushValue(SymbolicValue.Null);
                    break;

                case SyntaxKind.ThisExpression:
                    newProgramState = newProgramState.PushValue(SymbolicValue.This);
                    break;
                case SyntaxKind.BaseExpression:
                    newProgramState = newProgramState.PushValue(SymbolicValue.Base);
                    break;

                case SyntaxKind.CharacterLiteralExpression:
                case SyntaxKind.StringLiteralExpression:
                case SyntaxKind.NumericLiteralExpression:

                case SyntaxKind.SizeOfExpression:
                case SyntaxKind.TypeOfExpression:

                case SyntaxKind.ArrayCreationExpression:
                case SyntaxKind.ImplicitArrayCreationExpression:
                case SyntaxKind.StackAllocArrayCreationExpression:
                    {
                        var sv = new SymbolicValue();
                        newProgramState = newProgramState.SetConstraint(sv, ObjectConstraint.NotNull);
                        newProgramState = newProgramState.PushValue(sv);
                        newProgramState = InvokeChecks(newProgramState, 
                            (ps, check) => check.ObjectCreated(ps, sv, instruction));
                    }
                    break;

                case SyntaxKind.DefaultExpression:
                    newProgramState = VisitDefaultExpression((DefaultExpressionSyntax)instruction, newProgramState);
                    break;

                case SyntaxKind.AnonymousObjectCreationExpression:
                    {
                        var creation = (AnonymousObjectCreationExpressionSyntax)instruction;
                        newProgramState = newProgramState.PopValues(creation.Initializers.Count);

                        var sv = new SymbolicValue();
                        newProgramState = newProgramState.SetConstraint(sv, ObjectConstraint.NotNull);
                        newProgramState = newProgramState.PushValue(sv);
                        newProgramState = InvokeChecks(newProgramState, 
                            (ps, check) => check.ObjectCreated(ps, sv, instruction));
                    }
                    break;

                case SyntaxKind.CastExpression:

                case SyntaxKind.CheckedExpression:
                case SyntaxKind.UncheckedExpression:
                    // Do nothing
                    break;

                case SyntaxKind.InterpolatedStringExpression:
                    newProgramState = newProgramState.PopValues(((InterpolatedStringExpressionSyntax)instruction).Contents.OfType<InterpolationSyntax>().Count());
                    newProgramState = newProgramState.PushValue(new SymbolicValue());
                    break;

                case SyntaxKind.ObjectCreationExpression:
                    newProgramState = VisitObjectCreation((ObjectCreationExpressionSyntax)instruction, newProgramState);
                    break;

                case SyntaxKind.ElementAccessExpression:
                    newProgramState = newProgramState.PopValues((((ElementAccessExpressionSyntax)instruction).ArgumentList?.Arguments.Count ?? 0) + 1);
                    newProgramState = newProgramState.PushValue(new SymbolicValue());
                    break;

                case SyntaxKind.ImplicitElementAccess:
                    newProgramState = newProgramState
                        .PopValues(((ImplicitElementAccessSyntax)instruction).ArgumentList?.Arguments.Count ?? 0)
                        .PushValue(new SymbolicValue());
                    break;

                case SyntaxKind.ObjectInitializerExpression:
                case SyntaxKind.ArrayInitializerExpression:
                case SyntaxKind.CollectionInitializerExpression:
                case SyntaxKind.ComplexElementInitializerExpression:
                    newProgramState = VisitInitializer(instruction, parenthesizedExpression, newProgramState);
                    break;

                case SyntaxKind.ArrayType:
                    newProgramState = newProgramState.PopValues(((ArrayTypeSyntax)instruction).RankSpecifiers.SelectMany(rs => rs.Sizes).Count());
                    break;

                case SyntaxKind.ElementBindingExpression:
                    newProgramState = newProgramState.PopValues(((ElementBindingExpressionSyntax)instruction).ArgumentList?.Arguments.Count ?? 0);
                    newProgramState = newProgramState.PushValue(new SymbolicValue());
                    break;

                case SyntaxKind.InvocationExpression:
                    {
                        var invocation = (InvocationExpressionSyntax)instruction;
                        var invocationVisitor = new InvocationVisitor(invocation, SemanticModel, newProgramState);
                        newProgramState = invocationVisitor.ProcessInvocation();

                        if (invocation.Expression.IsOnThis() && !invocation.IsNameof(SemanticModel))
                        {
                            newProgramState = newProgramState.RemoveSymbols(IsFieldSymbol);
                        }
                    }
                    break;

                default:
                    throw new NotImplementedException($"{instruction.Kind()}");
            }

            newProgramState = EnsureStackState(parenthesizedExpression, newProgramState);
            OnInstructionProcessed(instruction, node.ProgramPoint, newProgramState);
            EnqueueNewNode(newProgramPoint, newProgramState);
        }

        private ProgramState InvokeChecks(ProgramState programState, Func<ProgramState, ExplodedGraphCheck, ProgramState> invoke)
        {
            // If a check returns null, we will skip the next checks and return null
            return explodedGraphChecks.Aggregate(programState, (ps, check) => ps == null ? null : invoke(ps, check));
        }

        private ProgramState EnsureStackState(ExpressionSyntax parenthesizedExpression, ProgramState programState)
        {
            if (ShouldConsumeValue(parenthesizedExpression))
            {
                var newProgramState = programState.PopValue();
                System.Diagnostics.Debug.Assert(!newProgramState.HasValue);

                return newProgramState;
            }

            return programState;
        }

        #region Handle VisitBinaryBranch cases

        private void VisitForeachBinaryBranch(BinaryBranchBlock binaryBranchBlock, ProgramState programState)
        {
            // foreach variable is not a VariableDeclarator, so we need to assign a value to it
            var foreachVariableSymbol = SemanticModel.GetDeclaredSymbol(binaryBranchBlock.BranchingNode);
            var sv = new SymbolicValue();
            var newProgramState = SetNonNullConstraintIfValueType(foreachVariableSymbol, sv, programState);
            newProgramState = SetNewSymbolicValueIfTracked(foreachVariableSymbol, sv, newProgramState);

            EnqueueAllSuccessors(binaryBranchBlock, newProgramState);
        }

        private void VisitCoalesceExpressionBinaryBranch(BinaryBranchBlock binaryBranchBlock, ProgramState programState)
        {
            SymbolicValue sv;
            var ps = programState.PopValue(out sv);

            foreach (var newProgramState in sv.TrySetConstraint(ObjectConstraint.Null, ps))
            {
                EnqueueNewNode(new ProgramPoint(binaryBranchBlock.TrueSuccessorBlock), newProgramState);
            }

            foreach (var newProgramState in sv.TrySetConstraint(ObjectConstraint.NotNull, ps))
            {
                var nps = newProgramState;

                if (!ShouldConsumeValue((BinaryExpressionSyntax)binaryBranchBlock.BranchingNode))
                {
                    nps = nps.PushValue(sv);
                }
                EnqueueNewNode(new ProgramPoint(binaryBranchBlock.FalseSuccessorBlock), nps);
            }
        }

        private void VisitConditionalAccessBinaryBranch(BinaryBranchBlock binaryBranchBlock, ProgramState programState)
        {
            SymbolicValue sv;
            var ps = programState.PopValue(out sv);

            foreach (var newProgramState in sv.TrySetConstraint(ObjectConstraint.Null, ps))
            {
                var nps = newProgramState;

                if (!ShouldConsumeValue((ConditionalAccessExpressionSyntax)binaryBranchBlock.BranchingNode))
                {
                    nps = nps.PushValue(SymbolicValue.Null);
                }
                EnqueueNewNode(new ProgramPoint(binaryBranchBlock.TrueSuccessorBlock), nps);
            }

            foreach (var newProgramState in sv.TrySetConstraint(ObjectConstraint.NotNull, ps))
            {
                EnqueueNewNode(new ProgramPoint(binaryBranchBlock.FalseSuccessorBlock), newProgramState);
            }
        }

        private void VisitBinaryBranch(BinaryBranchBlock binaryBranchBlock, ExplodedGraphNode node, SyntaxNode instruction)
        {
            var ps = node.ProgramState;
            SymbolicValue sv;

            var forStatement = binaryBranchBlock.BranchingNode as ForStatementSyntax;
            if (forStatement != null)
            {
                if (forStatement.Condition == null)
                {
                    ps = ps.PushValue(SymbolicValue.True);
                }
                ps = ps.PopValue(out sv);
                ps = ps.PopValues(forStatement.Incrementors.Count);
            }
            else
            {
                ps = ps.PopValue(out sv);
            }

            foreach (var newProgramState in sv.TrySetConstraint(BoolConstraint.True, ps))
            {
                OnConditionEvaluated(instruction, evaluationValue: true);

                var nps = binaryBranchBlock.BranchingNode.IsKind(SyntaxKind.LogicalOrExpression)
                    ? newProgramState.PushValue(SymbolicValue.True)
                    : newProgramState;

                EnqueueNewNode(new ProgramPoint(binaryBranchBlock.TrueSuccessorBlock), CleanStateAfterBlock(nps, node.ProgramPoint.Block));
            }

            foreach (var newProgramState in sv.TrySetConstraint(BoolConstraint.False, ps))
            {
                OnConditionEvaluated(instruction, evaluationValue: false);

                var nps = binaryBranchBlock.BranchingNode.IsKind(SyntaxKind.LogicalAndExpression)
                    ? newProgramState.PushValue(SymbolicValue.False)
                    : newProgramState;

                EnqueueNewNode(new ProgramPoint(binaryBranchBlock.FalseSuccessorBlock), CleanStateAfterBlock(nps, node.ProgramPoint.Block));
            }
        }

        #endregion

        #region VisitExpression

        private ProgramState VisitMemberAccess(MemberAccessExpressionSyntax memberAccess, ProgramState programState)
        {
            SymbolicValue memberExpression;
            var newProgramState = programState.PopValue(out memberExpression);
            SymbolicValue sv = null;
            var identifier = memberAccess.Name as IdentifierNameSyntax;
            if (identifier != null)
            {
                var symbol = SemanticModel.GetSymbolInfo(identifier).Symbol;
                var fieldSymbol = symbol as IFieldSymbol;
                if (fieldSymbol != null && (memberAccess.IsOnThis() || fieldSymbol.IsConst))
                {
                    sv = newProgramState.GetSymbolValue(symbol);
                    if (sv == null)
                    {
                        sv = fieldSymbol.CreateFieldSymbolicValue();
                        newProgramState = newProgramState.StoreSymbolicValue(symbol, sv);
                    }
                }
            }
            if (sv == null)
            {
                sv = new MemberAccessSymbolicValue(memberExpression, memberAccess.Name.Identifier.ValueText);
            }

            newProgramState = SetNonNullConstraintIfValueType(memberAccess, sv, newProgramState);
            return newProgramState.PushValue(sv);
        }

        private static ProgramState VisitSafeCastExpression(BinaryExpressionSyntax instruction, ProgramState programState)
        {
            SymbolicValue sv;
            var newProgramState = programState.PopValue(out sv);
            var resultValue = new SymbolicValue();
            if (newProgramState.HasConstraint(sv, ObjectConstraint.Null))
            {
                var constraint = instruction.IsKind(SyntaxKind.IsExpression)
                    ? (SymbolicValueConstraint)BoolConstraint.False
                    : ObjectConstraint.Null;
                newProgramState = newProgramState.SetConstraint(resultValue, constraint);
            }

            return newProgramState.PushValue(resultValue);
        }

        private ProgramState VisitDefaultExpression(DefaultExpressionSyntax instruction, ProgramState programState)
        {
            var sv = new SymbolicValue();
            var typeSymbol = SemanticModel.GetTypeInfo(instruction).Type;

            var isReferenceOrNullable = typeSymbol.IsReferenceType ||
                typeSymbol.OriginalDefinition.Is(KnownType.System_Nullable_T);

            var newProgramState = isReferenceOrNullable
                ? programState.SetConstraint(sv, ObjectConstraint.Null)
                : SetNonNullConstraintIfValueType(typeSymbol, sv, programState);

            return newProgramState.PushValue(sv);
        }

        private bool IsOperatorOnObject(SyntaxNode instruction)
        {
            var operatorSymbol = SemanticModel.GetSymbolInfo(instruction).Symbol as IMethodSymbol;
            return operatorSymbol != null &&
                operatorSymbol.ContainingType.Is(KnownType.System_Object);
        }

        private static ProgramState VisitValueEquals(ProgramState programState)
        {
            SymbolicValue leftSymbol;
            SymbolicValue rightSymbol;

            var newProgramState = programState
                .PopValue(out rightSymbol)
                .PopValue(out leftSymbol);

            var equals = new ValueEqualsSymbolicValue(leftSymbol, rightSymbol);
            newProgramState = newProgramState.PushValue(equals);
            return InvocationVisitor.SetConstraintOnValueEquals(equals, newProgramState);
        }

        private ProgramState VisitReferenceEquals(BinaryExpressionSyntax equals, ProgramState programState)
        {
            SymbolicValue leftSymbol;
            SymbolicValue rightSymbol;

            var newProgramState = programState
                .PopValue(out rightSymbol)
                .PopValue(out leftSymbol);

            return new InvocationVisitor.ReferenceEqualsConstraintHandler(leftSymbol, rightSymbol,
                equals.Left, equals.Right, newProgramState, SemanticModel).PushWithConstraint();
        }

        private ProgramState VisitComparisonBinaryOperator(ProgramState programState, BinaryExpressionSyntax comparison,
            Func<SymbolicValue, SymbolicValue, SymbolicValue> svFactory)
        {
            SymbolicValue leftSymbol;
            SymbolicValue rightSymbol;

            var newProgramState = programState
                .PopValue(out rightSymbol)
                .PopValue(out leftSymbol);

            var op = SemanticModel.GetSymbolInfo(comparison).Symbol as IMethodSymbol;

            var isValueTypeOperator = op?.ContainingType?.IsValueType ?? false;

            var isLiftedOperator = isValueTypeOperator &&
                (leftSymbol.IsNull(programState) || rightSymbol.IsNull(programState));

            var comparisonValue = isLiftedOperator ? SymbolicValue.False : svFactory(leftSymbol, rightSymbol);

            return newProgramState.PushValue(comparisonValue);
        }

        private static ProgramState VisitBinaryOperator(ProgramState programState,
            Func<SymbolicValue, SymbolicValue, SymbolicValue> svFactory)
        {
            SymbolicValue leftSymbol;
            SymbolicValue rightSymbol;

            return programState
                .PopValue(out rightSymbol)
                .PopValue(out leftSymbol)
                .PushValue(svFactory(leftSymbol, rightSymbol));
        }

        private ProgramState VisitBooleanBinaryOpAssignment(ProgramState programState, AssignmentExpressionSyntax assignment,
            Func<SymbolicValue, SymbolicValue, SymbolicValue> symbolicValueFactory)
        {
            var symbol = SemanticModel.GetSymbolInfo(assignment.Left).Symbol;

            SymbolicValue leftSymbol;
            SymbolicValue rightSymbol;

            var newProgramState = programState
                .PopValue(out rightSymbol)
                .PopValue(out leftSymbol);

            var sv = symbolicValueFactory(leftSymbol, rightSymbol);
            newProgramState = newProgramState.PushValue(sv);
            newProgramState = SetNonNullConstraintIfValueType(symbol, sv, newProgramState);
            return SetNewSymbolicValueIfTracked(symbol, sv, newProgramState);
        }

        private ProgramState VisitObjectCreation(ObjectCreationExpressionSyntax ctor, ProgramState programState)
        {
            var newProgramState = InvokeChecks(programState, (ps, check) => check.ObjectCreating(ps, ctor));

            var sv = new SymbolicValue();
            newProgramState = newProgramState.PopValues(ctor.ArgumentList?.Arguments.Count ?? 0);

            var ctorSymbol = SemanticModel.GetSymbolInfo(ctor).Symbol as IMethodSymbol;
            if (ctorSymbol == null)
            {
                // Add no constraint
            }
            else if (IsEmptyNullableCtorCall(ctorSymbol))
            {
                newProgramState = newProgramState.SetConstraint(sv, ObjectConstraint.Null);
            }
            else
            {
                newProgramState = newProgramState.SetConstraint(sv, ObjectConstraint.NotNull);
            }

            newProgramState = newProgramState.PushValue(sv);

            return InvokeChecks(newProgramState, (ps, check) => check.ObjectCreated(ps, sv, ctor));
        }

        private static ProgramState VisitInitializer(SyntaxNode instruction, ExpressionSyntax parenthesizedExpression, ProgramState programState)
        {
            var init = (InitializerExpressionSyntax)instruction;
            var newProgramState = programState.PopValues(init.Expressions.Count);

            if (!(parenthesizedExpression.Parent is ObjectCreationExpressionSyntax) &&
                !(parenthesizedExpression.Parent is ArrayCreationExpressionSyntax) &&
                !(parenthesizedExpression.Parent is AnonymousObjectCreationExpressionSyntax) &&
                !(parenthesizedExpression.Parent is ImplicitArrayCreationExpressionSyntax))
            {
                newProgramState = newProgramState.PushValue(new SymbolicValue());
            }

            return newProgramState;
        }

        private ProgramState VisitIdentifier(IdentifierNameSyntax identifier, ProgramState programState)
        {
            var newProgramState = programState;
            var symbol = SemanticModel.GetSymbolInfo(identifier).Symbol;
            var typeSymbol = SemanticModel.GetTypeInfo(identifier).Type;
            var sv = newProgramState.GetSymbolValue(symbol);

            if (sv == null)
            {
                var fieldSymbol = symbol as IFieldSymbol;
                if (fieldSymbol != null) // TODO: Fix me when implementing SLVS-1130
                {
                    sv = fieldSymbol.CreateFieldSymbolicValue();
                    newProgramState = newProgramState.StoreSymbolicValue(symbol, sv);
                }
                else
                {
                    sv = SymbolicValue.Create(typeSymbol);
                }
            }
            newProgramState = newProgramState.PushValue(sv);

            var parenthesized = identifier.GetSelfOrTopParenthesizedExpression();
            var argument = parenthesized.Parent as ArgumentSyntax;
            if (argument == null ||
                argument.RefOrOutKeyword.IsKind(SyntaxKind.None))
            {
                return SetNonNullConstraintIfValueType(symbol, sv, newProgramState);
            }

            newProgramState = newProgramState.PopValue();
            sv = SymbolicValue.Create(typeSymbol);
            newProgramState = newProgramState.PushValue(sv);
            newProgramState = SetNonNullConstraintIfValueType(symbol, sv, newProgramState);
            return SetNewSymbolicValueIfTracked(symbol, sv, newProgramState);
        }

        private ProgramState VisitPostfixIncrement(PostfixUnaryExpressionSyntax unary, ProgramState programState)
        {
            var symbol = SemanticModel.GetSymbolInfo(unary.Operand).Symbol;

            // Do not change the stacked value
            var sv = new SymbolicValue();
            var newProgramState = SetNonNullConstraintIfValueType(symbol, sv, programState);
            return SetNewSymbolicValueIfTracked(symbol, sv, newProgramState);
        }

        private ProgramState VisitPrefixIncrement(PrefixUnaryExpressionSyntax unary, ProgramState programState)
        {
            var newProgramState = programState;
            var symbol = SemanticModel.GetSymbolInfo(unary.Operand).Symbol;
            newProgramState = newProgramState.PopValue();

            var sv = new SymbolicValue();
            newProgramState = newProgramState.PushValue(sv);
            newProgramState = SetNonNullConstraintIfValueType(symbol, sv, newProgramState);
            return SetNewSymbolicValueIfTracked(symbol, sv, newProgramState);
        }

        private ProgramState VisitOpAssignment(AssignmentExpressionSyntax assignment, ProgramState programState)
        {
            var newProgramState = programState;
            var leftSymbol = SemanticModel.GetSymbolInfo(assignment.Left).Symbol;
            newProgramState = newProgramState.PopValues(2);

            var sv = new SymbolicValue();
            newProgramState = newProgramState.PushValue(sv);
            newProgramState = SetNonNullConstraintIfValueType(leftSymbol, sv, newProgramState);
            return SetNewSymbolicValueIfTracked(leftSymbol, sv, newProgramState);
        }

        private ProgramState VisitSimpleAssignment(AssignmentExpressionSyntax assignment, ProgramState programState)
        {
            var newProgramState = programState;
            SymbolicValue sv;
            newProgramState = newProgramState.PopValue(out sv);
            if (!CSharpControlFlowGraphBuilder.IsAssignmentWithSimpleLeftSide(assignment))
            {
                newProgramState = newProgramState.PopValue();
            }

            var leftSymbol = SemanticModel.GetSymbolInfo(assignment.Left).Symbol;
            newProgramState = newProgramState.PushValue(sv);
            newProgramState = SetNewSymbolicValueIfTracked(leftSymbol, sv, newProgramState);
            return SetNonNullConstraintIfValueType(leftSymbol, sv, newProgramState);
        }

        private ProgramState VisitVariableDeclarator(VariableDeclaratorSyntax declarator, ProgramState programState)
        {
            var newProgramState = programState;

            var sv = new SymbolicValue();
            if (declarator.Initializer?.Value != null)
            {
                newProgramState = newProgramState.PopValue(out sv);
            }

            var leftSymbol = SemanticModel.GetDeclaredSymbol(declarator);
            if (leftSymbol != null &&
                IsSymbolTracked(leftSymbol))
            {
                newProgramState = newProgramState.StoreSymbolicValue(leftSymbol, sv);
            }

            return SetNonNullConstraintIfValueType(leftSymbol, sv, newProgramState);
        }

        #endregion

        protected override bool IsValueConsumingStatement(SyntaxNode jumpNode)
        {
            if (jumpNode.IsKind(SyntaxKind.LockStatement))
            {
                return true;
            }

            var usingStatement = jumpNode as UsingStatementSyntax;
            if (usingStatement != null)
            {
                return usingStatement.Expression != null;
            }

            var throwStatement = jumpNode as ThrowStatementSyntax;
            if (throwStatement != null)
            {
                return throwStatement.Expression != null;
            }

            var returnStatement = jumpNode as ReturnStatementSyntax;
            if (returnStatement != null)
            {
                return returnStatement.Expression != null;
            }

            var switchStatement = jumpNode as SwitchStatementSyntax;
            if (switchStatement != null)
            {
                return switchStatement.Expression != null;
            }

            // goto is not putting the expression to the CFG

            return false;
        }

        private static bool ShouldConsumeValue(ExpressionSyntax expression)
        {
            if (expression == null)
            {
                return false;
            }

            var parent = expression.Parent;
            var conditionalAccess = parent as ConditionalAccessExpressionSyntax;
            if (conditionalAccess != null &&
                conditionalAccess.WhenNotNull == expression)
            {
                return ShouldConsumeValue(conditionalAccess.GetSelfOrTopParenthesizedExpression());
            }

            return parent is ExpressionStatementSyntax ||
                parent is YieldStatementSyntax;
        }

        private static bool IsEmptyNullableCtorCall(IMethodSymbol nullableConstructorCall)
        {
            return nullableConstructorCall != null &&
                nullableConstructorCall.MethodKind == MethodKind.Constructor &&
                nullableConstructorCall.ReceiverType.OriginalDefinition.Is(KnownType.System_Nullable_T) &&
                nullableConstructorCall.Parameters.Length == 0;
        }

        #endregion
    }
}