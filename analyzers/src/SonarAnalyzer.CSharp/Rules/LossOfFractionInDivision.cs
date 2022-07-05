/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class LossOfFractionInDivision : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2184";
        private const string MessageFormat = "Cast one of the operands of this division to '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var division = (BinaryExpressionSyntax)c.Node;

                    if (!(c.SemanticModel.GetSymbolInfo(division).Symbol is IMethodSymbol symbol)
                        || symbol.ContainingType == null
                        || !symbol.ContainingType.IsAny(KnownType.IntegralNumbersIncludingNative))
                    {
                        return;
                    }

                    if (TryGetTypeFromAssignmentToFloatType(division, c.SemanticModel, out var assignedToType)
                        || TryGetTypeFromArgumentMappedToFloatType(division, c.SemanticModel, out assignedToType)
                        || TryGetTypeFromReturnMappedToFloatType(division, c.SemanticModel, out assignedToType))
                    {
                        var diagnostic = Diagnostic.Create(Rule, division.GetLocation(), assignedToType.ToMinimalDisplayString(c.SemanticModel, division.SpanStart));
                        c.ReportIssue(diagnostic);
                    }
                },
                SyntaxKind.DivideExpression);

        private static bool TryGetTypeFromReturnMappedToFloatType(SyntaxNode division, SemanticModel semanticModel, out ITypeSymbol type)
        {
            if (division.Parent is ReturnStatementSyntax
                || division.Parent is LambdaExpressionSyntax)
            {
                type = (semanticModel.GetEnclosingSymbol(division.SpanStart) as IMethodSymbol)?.ReturnType;
                return type.IsAny(KnownType.NonIntegralNumbers);
            }

            type = null;
            return false;
        }

        private static bool TryGetTypeFromArgumentMappedToFloatType(SyntaxNode division, SemanticModel semanticModel, out ITypeSymbol type)
        {
            if (division.Parent is not ArgumentSyntax argument)
            {
                type = null;
                return false;
            }

            if (argument.Parent.Parent is not InvocationExpressionSyntax invocation)
            {
                type = null;
                return false;
            }

            var lookup = new CSharpMethodParameterLookup(invocation, semanticModel);
            if (!lookup.TryGetSymbol(argument, out var parameter))
            {
                type = null;
                return false;
            }

            type = parameter.Type;
            return type.IsAny(KnownType.NonIntegralNumbers);
        }

        private static bool TryGetTypeFromAssignmentToFloatType(SyntaxNode division, SemanticModel semanticModel, out ITypeSymbol type)
        {
            if (division.Parent is AssignmentExpressionSyntax assignment)
            {
                type = semanticModel.GetTypeInfo(assignment.Left).Type;
                return type.IsAny(KnownType.NonIntegralNumbers);
            }

            var outerTuple = GetMostOuterTuple(division);
            if (outerTuple != null && GetFirstAncenstorOfType<AssignmentExpressionSyntax>(outerTuple) is { } assignmentSyntax
                && assignmentSyntax.MapAssignmentArguments() is { } assignmentMappings)
            {
                var assignementLeft = assignmentMappings.Where(x => x.Right.Equals(division)).FirstOrDefault().Left;
                if (assignementLeft != null)
                {
                    type = semanticModel.GetTypeInfo(assignementLeft).Type;
                    return type.IsAny(KnownType.NonIntegralNumbers);
                }
            }
            else if (outerTuple != null && GetFirstAncenstorOfType<VariableDeclarationSyntax>(outerTuple) is { } variableDeclaration)
            {
                var tupleArguments = ((TupleExpressionSyntaxWrapper)outerTuple).AllArguments();
                var declarationType = semanticModel.GetTypeInfo(variableDeclaration.Type).Type;
                var flattenTupleTypes = AllTupleElements(declarationType);
                if (flattenTupleTypes.Any() && DivisionArgumentIndex(tupleArguments, division) is { } argumentIndex)
                {
                    type = flattenTupleTypes[argumentIndex];
                    return type.IsAny(KnownType.NonIntegralNumbers);
                }
            }

            if (division is { Parent: EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax variableDecl } } })
            {
                type = semanticModel.GetTypeInfo(variableDecl.Type).Type;
                return type.IsAny(KnownType.NonIntegralNumbers);
            }

            type = null;
            return false;
        }

        private static SyntaxNode GetMostOuterTuple(SyntaxNode node) =>
            node.Ancestors().LastOrDefault(x => TupleExpressionSyntaxWrapper.IsInstance(x));

        private static T GetFirstAncenstorOfType<T>(SyntaxNode node) where T : CSharpSyntaxNode =>
            node.AncestorsAndSelf().OfType<T>().FirstOrDefault();

        private static int? DivisionArgumentIndex(ImmutableArray<ArgumentSyntax> arguments, SyntaxNode division)
        {
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
                if (argument.Expression.Equals(division))
                {
                    return i;
                }
            }
            return null;
        }

        private static List<ITypeSymbol> AllTupleElements(ITypeSymbol typeSymbol)
        {
            List<ITypeSymbol> flattenTupleTypes = new();
            CollectTupleTypes(flattenTupleTypes, typeSymbol);
            return flattenTupleTypes;
            static void CollectTupleTypes(List<ITypeSymbol> symbolList, ITypeSymbol typeSymbol)
            {
                if (typeSymbol.IsTupleType())
                {
                    var types = ((INamedTypeSymbol)typeSymbol).TupleElements();
                    foreach (var type in types)
                    {
                        CollectTupleTypes(symbolList, type.Type);
                    }
                }
                else
                {
                    symbolList.Add(typeSymbol.GetSymbolType());
                }
            }
        }
    }
}
