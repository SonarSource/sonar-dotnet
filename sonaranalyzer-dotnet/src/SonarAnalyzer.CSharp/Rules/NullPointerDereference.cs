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
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.ControlFlowGraph;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class NullPointerDereference : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2259";
        private const string MessageFormat = "'{0}' is null on at least one execution path.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterExplodedGraphBasedAnalysis((e, c) => CheckForNullDereference(e, c));
        }

        private static void CheckForNullDereference(CSharpExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context)
        {
            var nullPointerCheck = new NullPointerCheck(explodedGraph);
            explodedGraph.AddExplodedGraphCheck(nullPointerCheck);

            var nullIdentifiers = new HashSet<IdentifierNameSyntax>();

            EventHandler<MemberAccessedEventArgs> memberAccessedHandler =
                (sender, args) => CollectMemberAccesses(args, nullIdentifiers, context.SemanticModel);

            nullPointerCheck.MemberAccessed += memberAccessedHandler;

            try
            {
                explodedGraph.Walk();
            }
            finally
            {
                nullPointerCheck.MemberAccessed -= memberAccessedHandler;
            }

            foreach (var nullIdentifier in nullIdentifiers)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, nullIdentifier.GetLocation(), nullIdentifier.Identifier.ValueText));
            }
        }

        private static void CollectMemberAccesses(MemberAccessedEventArgs args, HashSet<IdentifierNameSyntax> nullIdentifiers,
            SemanticModel semanticModel)
        {
            if (!NullPointerCheck.IsExtensionMethod(args.Identifier.Parent, semanticModel))
            {
                nullIdentifiers.Add(args.Identifier);
            }
        }

        internal sealed class NullPointerCheck : ExplodedGraphCheck
        {
            public event EventHandler<MemberAccessingEventArgs> MemberAccessing;

            public event EventHandler<MemberAccessedEventArgs> MemberAccessed;

            public NullPointerCheck(CSharpExplodedGraph explodedGraph)
                : base(explodedGraph)
            {

            }

            private void OnMemberAccessing(IdentifierNameSyntax identifier, ISymbol symbol, ProgramState programState)
            {
                MemberAccessing?.Invoke(this, new MemberAccessingEventArgs(identifier, symbol, programState));
            }

            private void OnMemberAccessed(IdentifierNameSyntax identifier)
            {
                MemberAccessed?.Invoke(this, new MemberAccessedEventArgs(identifier));
            }

            public override ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState)
            {
                var instruction = programPoint.Block.Instructions[programPoint.Offset];
                switch (instruction.Kind())
                {
                    case SyntaxKind.IdentifierName:
                        return ProcessIdentifier(programPoint, programState, (IdentifierNameSyntax)instruction);

                    case SyntaxKind.AwaitExpression:
                        return ProcessAwait(programState, (AwaitExpressionSyntax)instruction);

                    case SyntaxKind.SimpleMemberAccessExpression:
                    case SyntaxKind.PointerMemberAccessExpression:
                        return ProcessMemberAccess(programState, (MemberAccessExpressionSyntax)instruction);

                    case SyntaxKind.ElementAccessExpression:
                        return ProcessElementAccess(programState, (ElementAccessExpressionSyntax)instruction);

                    default:
                        return programState;
                }
            }

            private ProgramState ProcessAwait(ProgramState programState, AwaitExpressionSyntax awaitExpression)
            {
                var identifier = awaitExpression.Expression as IdentifierNameSyntax;
                if (identifier == null)
                {
                    return programState;
                }

                var symbol = semanticModel.GetSymbolInfo(identifier).Symbol;
                return ProcessIdentifier(programState, identifier, symbol);
            }

            private ProgramState ProcessElementAccess(ProgramState programState, ElementAccessExpressionSyntax elementAccess)
            {
                var identifier = elementAccess.Expression as IdentifierNameSyntax;
                if (identifier == null)
                {
                    return programState;
                }

                var symbol = semanticModel.GetSymbolInfo(identifier).Symbol;
                if (symbol == null)
                {
                    return programState;
                }

                return ProcessIdentifier(programState, identifier, symbol);
            }

            private static MemberAccessIdentifierScope GetIdentifierFromMemberAccess(MemberAccessExpressionSyntax memberAccess)
            {
                var expressionWithoutParentheses = memberAccess.Expression.RemoveParentheses();

                var identifier = expressionWithoutParentheses as IdentifierNameSyntax;
                if (identifier != null)
                {
                    return new MemberAccessIdentifierScope(identifier, true);
                }

                var subMemberBinding = expressionWithoutParentheses as MemberBindingExpressionSyntax;
                if (subMemberBinding != null)
                {
                    return new MemberAccessIdentifierScope(subMemberBinding.Name as IdentifierNameSyntax, true);
                }

                var subMemberAccess = expressionWithoutParentheses as MemberAccessExpressionSyntax;
                if (subMemberAccess != null &&
                    subMemberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                {
                    var isThisAccess = subMemberAccess.Expression.RemoveParentheses() is ThisExpressionSyntax;
                    return new MemberAccessIdentifierScope(subMemberAccess.Name as IdentifierNameSyntax, isThisAccess);
                }

                return new MemberAccessIdentifierScope(null, false);
            }

            private ProgramState ProcessMemberAccess(ProgramState programState, MemberAccessExpressionSyntax memberAccess)
            {
                var memberAccessIdentifierScope = GetIdentifierFromMemberAccess(memberAccess);
                if (memberAccessIdentifierScope.Identifier == null)
                {
                    return programState;
                }

                var symbol = semanticModel.GetSymbolInfo(memberAccessIdentifierScope.Identifier).Symbol;
                if (symbol == null)
                {
                    return programState;
                }

                var fieldSymbol = symbol as IFieldSymbol;
                if (fieldSymbol != null && !fieldSymbol.IsConst && !memberAccessIdentifierScope.IsOnCurrentInstance)
                {
                    return programState;
                }

                if ((IsNullableValueType(symbol) && !IsGetTypeCall(memberAccess)) ||
                    IsExtensionMethod(memberAccess, semanticModel))
                {
                    return programState;
                }

                return ProcessIdentifier(programState, memberAccessIdentifierScope.Identifier, symbol);
            }

            private static bool IsGetTypeCall(MemberAccessExpressionSyntax memberAccess)
            {
                return memberAccess.Name.Identifier.ValueText == "GetType";
            }

            private ProgramState ProcessIdentifier(ProgramPoint programPoint, ProgramState programState, IdentifierNameSyntax identifier)
            {
                if (programPoint.Block.Instructions.Last() != identifier ||
                    programPoint.Block.SuccessorBlocks.Count != 1 ||
                    (!IsSuccessorForeachBranch(programPoint) && !IsExceptionThrow(identifier)))
                {
                    return programState;
                }

                var symbol = semanticModel.GetSymbolInfo(identifier).Symbol;
                return ProcessIdentifier(programState, identifier, symbol);
            }

            private ProgramState ProcessIdentifier(ProgramState programState, IdentifierNameSyntax identifier, ISymbol symbol)
            {
                if (explodedGraph.IsSymbolTracked(symbol))
                {
                    OnMemberAccessing(identifier, symbol, programState);

                    if (symbol.HasConstraint(ObjectConstraint.Null, programState))
                    {
                        OnMemberAccessed(identifier);
                        return null;
                    }
                }

                return SetNotNullConstraintOnSymbol(symbol, programState);
            }

            private static ProgramState SetNotNullConstraintOnSymbol(ISymbol symbol, ProgramState programState)
            {
                if (programState == null)
                {
                    return null;
                }

                if (symbol == null)
                {
                    return programState;
                }

                if (!IsNullableValueType(symbol))
                {
                    return symbol.SetConstraint(ObjectConstraint.NotNull, programState);
                }

                return programState;
            }

            private static bool IsNullableValueType(ISymbol symbol)
            {
                var type = symbol.GetSymbolType();
                return type.IsStruct() &&
                    type.OriginalDefinition.Is(KnownType.System_Nullable_T);
            }

            private static bool IsExceptionThrow(IdentifierNameSyntax identifier)
            {
                return identifier.GetSelfOrTopParenthesizedExpression().Parent.IsKind(SyntaxKind.ThrowStatement);
            }

            private static bool IsSuccessorForeachBranch(ProgramPoint programPoint)
            {
                var successorBlock = programPoint.Block.SuccessorBlocks.First() as BinaryBranchBlock;
                return successorBlock != null &&
                    successorBlock.BranchingNode.IsKind(SyntaxKind.ForEachStatement);
            }

            internal static bool IsExtensionMethod(SyntaxNode expression, SemanticModel semanticModel)
            {
                var memberSymbol = semanticModel.GetSymbolInfo(expression).Symbol as IMethodSymbol;
                return memberSymbol != null && memberSymbol.IsExtensionMethod;
            }

            private class MemberAccessIdentifierScope
            {
                public MemberAccessIdentifierScope(IdentifierNameSyntax identifier, bool isOnCurrentInstance)
                {
                    Identifier = identifier;
                    IsOnCurrentInstance = isOnCurrentInstance;
                }

                public IdentifierNameSyntax Identifier { get; }
                public bool IsOnCurrentInstance { get; }
            }
        }
    }
}