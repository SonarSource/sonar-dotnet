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
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class InvalidCastToInterface : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1944";
        private const string MessageFormat = "{0}";
        internal const string MessageReviewFormat = "Review this cast; in this project there's no type that {0}.";
        internal const string MessageDefinite = "Nullable is known to be empty, this cast throws an exception.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            //FIXME: Temporary silence for CFG defork
            return;

            context.RegisterCompilationStartAction(
                compilationStartContext =>
                {
                    var allNamedTypeSymbols = compilationStartContext.Compilation.GlobalNamespace.GetAllNamedTypes();
                    var typeInterfaceMappings = allNamedTypeSymbols.Select(type =>
                        new
                        {
                            Type = type.OriginalDefinition,
                            Interfaces = type.OriginalDefinition.AllInterfaces.Select(i => i.OriginalDefinition)
                        });

                    var interfaceImplementerMappings = new Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>>();
                    foreach (var typeInterfaceMapping in typeInterfaceMappings)
                    {
                        if (typeInterfaceMapping.Type.IsInterface())
                        {
                            if (!interfaceImplementerMappings.ContainsKey(typeInterfaceMapping.Type))
                            {
                                interfaceImplementerMappings.Add(typeInterfaceMapping.Type, new HashSet<INamedTypeSymbol>());
                            }

                            interfaceImplementerMappings[typeInterfaceMapping.Type].Add(typeInterfaceMapping.Type);
                        }

                        foreach (var @interface in typeInterfaceMapping.Interfaces)
                        {
                            if (!interfaceImplementerMappings.ContainsKey(@interface))
                            {
                                interfaceImplementerMappings.Add(@interface, new HashSet<INamedTypeSymbol>());
                            }

                            interfaceImplementerMappings[@interface].Add(typeInterfaceMapping.Type);
                        }
                    }

                    compilationStartContext.RegisterSyntaxNodeActionInNonGenerated(
                        c =>
                        {
                            var cast = (CastExpressionSyntax)c.Node;
                            var interfaceType = c.SemanticModel.GetTypeInfo(cast.Type).Type as INamedTypeSymbol;
                            var expressionType = c.SemanticModel.GetTypeInfo(cast.Expression).Type as INamedTypeSymbol;

                            CheckTypesForInvalidCast(interfaceType, expressionType, interfaceImplementerMappings,
                                cast.Type.GetLocation(), c);
                        },
                        SyntaxKind.CastExpression);
                });

            context.RegisterExplodedGraphBasedAnalysis((e, c) => CheckEmptyNullableCast(e, c));
        }

        private static void CheckEmptyNullableCast(CSharpExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context)
        {
            explodedGraph.AddExplodedGraphCheck(new NullableCastCheck(explodedGraph, context));
            explodedGraph.Walk();
        }

        internal sealed class NullableCastCheck : ExplodedGraphCheck
        {
            private readonly SyntaxNodeAnalysisContext context;

            public NullableCastCheck(CSharpExplodedGraph explodedGraph)
                : base(explodedGraph)
            {
            }

            public NullableCastCheck(CSharpExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context)
                : this(explodedGraph)
            {
                this.context = context;
            }

            public override ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState)
            {
                var instruction = programPoint.Block.Instructions[programPoint.Offset];

                return instruction.IsKind(SyntaxKind.CastExpression)
                    ? ProcessCastAccess(programState, (CastExpressionSyntax)instruction)
                    : programState;
            }

            private ProgramState ProcessCastAccess(ProgramState programState, CastExpressionSyntax castExpression)
            {
                var typeExpression = this.semanticModel.GetTypeInfo(castExpression.Expression).Type;
                if (typeExpression == null ||
                    !typeExpression.OriginalDefinition.Is(KnownType.System_Nullable_T))
                {
                    return programState;
                }

                var type = this.semanticModel.GetTypeInfo(castExpression.Type).Type;

                if (type == null ||
                    type.OriginalDefinition.Is(KnownType.System_Nullable_T) ||
                    !this.semanticModel.Compilation.ClassifyConversion(typeExpression, type).IsNullable ||
                    !programState.HasConstraint(programState.PeekValue(), ObjectConstraint.Null))
                {
                    return programState;
                }

                if (!this.context.Equals(default(SyntaxNodeAnalysisContext)))
                {
                    this.context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, castExpression.GetLocation(), MessageDefinite));
                }

                return null;
            }
        }

        private static void CheckTypesForInvalidCast(INamedTypeSymbol interfaceType, INamedTypeSymbol expressionType,
            Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> interfaceImplementerMappings, Location issueLocation,
            SyntaxNodeAnalysisContext context)
        {
            if (interfaceType == null ||
                expressionType == null ||
                !interfaceType.IsInterface() ||
                expressionType.Is(KnownType.System_Object))
            {
                return;
            }

            if (!HasExistingConcreteImplementation(interfaceType, interfaceImplementerMappings))
            {
                return;
            }

            if (expressionType.IsInterface() &&
                !HasExistingConcreteImplementation(expressionType, interfaceImplementerMappings))
            {
                return;
            }

            if (interfaceImplementerMappings.ContainsKey(interfaceType.OriginalDefinition) &&
                !interfaceImplementerMappings[interfaceType.OriginalDefinition].Any(t => t.DerivesOrImplements(expressionType.OriginalDefinition)) &&
                !expressionType.IsSealed)
            {
                ReportIssue(interfaceType, expressionType, issueLocation, context);
            }
        }

        private static bool HasExistingConcreteImplementation(INamedTypeSymbol type,
            Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> interfaceImplementerMappings)
        {
            return interfaceImplementerMappings.ContainsKey(type) &&
                interfaceImplementerMappings[type].Any(t => t.IsClassOrStruct());
        }

        private static void ReportIssue(INamedTypeSymbol interfaceType, INamedTypeSymbol expressionType, Location issueLocation,
            SyntaxNodeAnalysisContext context)
        {
            var interfaceTypeName = interfaceType.ToMinimalDisplayString(context.SemanticModel, issueLocation.SourceSpan.Start);
            var expressionTypeName = expressionType.ToMinimalDisplayString(context.SemanticModel, issueLocation.SourceSpan.Start);

            var messageReasoning = expressionType.IsInterface()
                ? $"implements both '{expressionTypeName}' and '{interfaceTypeName}'"
                : $"extends '{expressionTypeName}' and implements '{interfaceTypeName}'";

            context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, issueLocation, string.Format(MessageReviewFormat, messageReasoning)));
        }
    }
}
