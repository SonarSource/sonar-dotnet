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
using SonarAnalyzer.Helpers.FlowAnalysis.Common;
using SonarAnalyzer.Helpers.FlowAnalysis.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class EmptyCollectionsShouldNotBeEnumerated : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4158";
        private const string MessageFormat = "Remove this call the collection can only be empty here.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<KnownType> CollectionTypes = new HashSet<KnownType>
        {
            KnownType.System_Collections_Generic_Dictionary_TKey_TValue,
            KnownType.System_Collections_Generic_List_T,
            KnownType.System_Collections_Generic_Queue_T,
            KnownType.System_Collections_Generic_Stack_T,
            KnownType.System_Collections_Generic_HashSet_T,
            KnownType.System_Collections_ObjectModel_ObservableCollection_T,
            KnownType.System_Array,
        };

        private static readonly HashSet<string> AddMethods = new HashSet<string>
        {
            nameof(List<object>.Add),
            nameof(List<object>.AddRange),
            nameof(List<object>.Insert),
            nameof(List<object>.InsertRange),
            nameof(Queue<object>.Enqueue),
            nameof(Stack<object>.Push),
            nameof(HashSet<object>.Add),
            nameof(HashSet<object>.UnionWith),
        };

        private static readonly HashSet<string> IgnoredMethods = new HashSet<string>
        {
            nameof(List<object>.GetHashCode),
            nameof(List<object>.Equals),
            nameof(List<object>.GetType),
            nameof(List<object>.ToString),
            nameof(List<object>.ToArray),
            nameof(Array.GetLength),
            nameof(Array.GetLongLength),
            nameof(Array.GetLowerBound),
            nameof(Array.GetUpperBound),
            nameof(Dictionary<object, object>.ContainsKey),
            nameof(Dictionary<object, object>.ContainsValue),
            nameof(Dictionary<object, object>.GetObjectData),
            nameof(Dictionary<object, object>.OnDeserialization),
            nameof(Dictionary<object, object>.TryGetValue),
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterExplodedGraphBasedAnalysis(CheckForEmptyCollectionAccess);
        }

        private void CheckForEmptyCollectionAccess(ExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context)
        {
            var check = new EmptyCollectionAccessedCheck(explodedGraph);
            explodedGraph.AddExplodedGraphCheck(check);

            var empty = new HashSet<SyntaxNode>();
            var full = new HashSet<SyntaxNode>();

            EventHandler<CollectionAccessedEventArgs> collectionAccessedHandler = (sender, args) =>
                {
                    (args.IsEmpty ? empty : full).Add(args.Node);
                };

            check.CollectionAccessed += collectionAccessedHandler;
            try
            {
                explodedGraph.Walk();
            }
            finally
            {
                check.CollectionAccessed -= collectionAccessedHandler;
            }

            foreach (var node in empty.Except(full))
            {
                context.ReportDiagnostic(Diagnostic.Create(rule, node.GetLocation()));
            }
        }


        internal sealed class EmptyCollectionAccessedCheck : ExplodedGraphCheck
        {
            public event EventHandler<CollectionAccessedEventArgs> CollectionAccessed;

            public EmptyCollectionAccessedCheck(ExplodedGraph explodedGraph)
                : base(explodedGraph)
            {
            }

            private void OnCollectionAccessed(SyntaxNode node, bool empty)
            {
                CollectionAccessed?.Invoke(this, new CollectionAccessedEventArgs(node, empty));
            }

            public override ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState)
            {
                var instruction = programPoint.Block.Instructions[programPoint.Offset];

                switch (instruction.Kind())
                {
                    case SyntaxKind.InvocationExpression:
                        return ProcessInvocation(programState, (InvocationExpressionSyntax)instruction);
                    case SyntaxKind.ElementAccessExpression:
                        return ProcessElementAccess(programState, (ElementAccessExpressionSyntax)instruction);
                    default:
                        return programState;
                }
            }

            private ProgramState ProcessElementAccess(ProgramState programState, ElementAccessExpressionSyntax instruction)
            {
                var newProgramState = programState;

                var collectionSymbol = semanticModel.GetSymbolInfo(instruction.Expression).Symbol;
                if (collectionSymbol == null)
                {
                    return newProgramState;
                }

                var collectionType = GetCollectionType(collectionSymbol);
                if (collectionType?.ConstructedFrom != null &&
                    collectionType.ConstructedFrom.IsAny(CollectionTypes))
                {
                    if (collectionType.ConstructedFrom.Is(KnownType.System_Collections_Generic_Dictionary_TKey_TValue) &&
                        IsDictionarySetItem(instruction))
                    {
                        newProgramState = collectionSymbol.SetConstraint(CollectionConstraint.NotEmpty, newProgramState);
                    }
                    else
                    {
                        OnCollectionAccessed(instruction, collectionSymbol.HasConstraint(CollectionConstraint.Empty, programState));
                    }
                }

                return newProgramState;
            }

            private ProgramState ProcessInvocation(ProgramState programState, InvocationExpressionSyntax instruction)
            {
                var newProgramState = programState;

                var memberAccess = instruction.Expression as MemberAccessExpressionSyntax;
                if (memberAccess != null)
                {
                    var collectionSymbol = semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
                    var collectionType = GetCollectionType(collectionSymbol);

                    if (collectionType.IsAny(CollectionTypes))
                    {
                        var methodSymbol = semanticModel.GetSymbolInfo(instruction).Symbol as IMethodSymbol;
                        if (methodSymbol == null ||
                            methodSymbol.IsExtensionMethod ||
                            IgnoredMethods.Contains(methodSymbol.Name))
                        {
                            return newProgramState;
                        }

                        if (AddMethods.Contains(methodSymbol.Name))
                        {
                            newProgramState = collectionSymbol.SetConstraint(CollectionConstraint.NotEmpty, newProgramState);
                        }
                        else
                        {
                            OnCollectionAccessed(instruction, collectionSymbol.HasConstraint(CollectionConstraint.Empty, programState));
                        }
                    }
                }

                newProgramState = RemoveCollectionConstraintsFromArguments(instruction, newProgramState);

                return newProgramState;
            }

            private static ProgramState RemoveCollectionConstraintsFromArguments(InvocationExpressionSyntax instruction, ProgramState newProgramState)
            {
                var values = instruction.ArgumentList.Arguments.Select((node, index) =>
                {
                    SymbolicValue value;
                    newProgramState.ExpressionStack.Pop(out value);
                    return value;
                });
                foreach (var value in values)
                {
                    newProgramState = value.RemoveConstraint(CollectionConstraint.Empty, newProgramState);
                }

                return newProgramState;
            }

            public override ProgramState ObjectCreated(ProgramState programState, SymbolicValue symbolicValue, SyntaxNode instruction)
            {
                var newProgramState = programState;
                switch (instruction.Kind())
                {
                    case SyntaxKind.ObjectCreationExpression:
                        var objectCreation = (ObjectCreationExpressionSyntax)instruction;
                        var constructorSymbol = semanticModel.GetSymbolInfo(objectCreation).Symbol as IMethodSymbol;

                        if (IsCollectionConstructor(constructorSymbol) &&
                            (!constructorSymbol.Parameters.Any()
                            || constructorSymbol.Parameters.Count(p => p.IsType(KnownType.System_Int32)) == 1))
                        {
                            newProgramState = symbolicValue.SetConstraint(CollectionConstraint.Empty, newProgramState);
                        }
                        if (objectCreation.Initializer != null)
                        {
                            var constraint = objectCreation.Initializer.Expressions.Count == 0
                                ? CollectionConstraint.Empty
                                : CollectionConstraint.NotEmpty;
                            newProgramState = symbolicValue.SetConstraint(constraint, newProgramState);
                        }
                        break;
                    case SyntaxKind.ArrayCreationExpression:
                        var arrayCreation = (ArrayCreationExpressionSyntax)instruction;
                        if (arrayCreation.Type.RankSpecifiers.Count == 1 &&
                            arrayCreation.Type.RankSpecifiers[0].Sizes.Count == 1)
                        {
                            var size = arrayCreation.Type.RankSpecifiers[0].Sizes[0] as LiteralExpressionSyntax;
                            if (size?.Token.ValueText == "0")
                            {
                                newProgramState = symbolicValue.SetConstraint(CollectionConstraint.Empty, newProgramState);
                            }
                        }
                        if (arrayCreation.Initializer != null)
                        {
                            var constraint = arrayCreation.Initializer.Expressions.Count == 0
                                ? CollectionConstraint.Empty
                                : CollectionConstraint.NotEmpty;
                            newProgramState = symbolicValue.SetConstraint(constraint, newProgramState);
                        }
                        break;
                    default:
                        break;
                }
                return newProgramState;
            }

            private static bool IsDictionarySetItem(ElementAccessExpressionSyntax instruction) =>
                (instruction.GetSelfOrTopParenthesizedExpression().Parent as AssignmentExpressionSyntax)
                    ?.Left.RemoveParentheses() == instruction;

            private static bool IsCollectionConstructor(IMethodSymbol constructorSymbol) =>
                constructorSymbol?.ContainingType?.ConstructedFrom != null && 
                constructorSymbol.ContainingType.ConstructedFrom.IsAny(CollectionTypes);

            private static INamedTypeSymbol GetCollectionType(ISymbol collectionSymbol) =>
                (collectionSymbol.GetSymbolType() as INamedTypeSymbol)?.ConstructedFrom ?? // collections
                collectionSymbol.GetSymbolType()?.BaseType; // arrays
        }

        internal sealed class CollectionAccessedEventArgs : EventArgs
        {
            public SyntaxNode Node { get; }
            public bool IsEmpty { get; }

            public CollectionAccessedEventArgs(SyntaxNode node, bool isEmpty)
            {
                Node = node;
                IsEmpty = isEmpty;
            }
        }
    }
}
