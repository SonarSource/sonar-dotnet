/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ArrayCovariance : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2330";
        private const string MessageFormat = "Refactor the code to not rely on potentially unsafe array conversions.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(RaiseOnArrayCovarianceInSimpleAssignmentExpression, SyntaxKind.SimpleAssignmentExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(RaiseOnArrayCovarianceInVariableDeclaration, SyntaxKind.VariableDeclaration);
            context.RegisterSyntaxNodeActionInNonGenerated(RaiseOnArrayCovarianceInInvocationExpression, SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(RaiseOnArrayCovarianceInCastExpression, SyntaxKind.CastExpression);
        }

        private static void RaiseOnArrayCovarianceInSimpleAssignmentExpression(SyntaxNodeAnalysisContext context)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            VerifyExpression(assignment.Right, context.SemanticModel.GetTypeInfo(assignment.Left).Type, context);
        }

        private static void RaiseOnArrayCovarianceInVariableDeclaration(SyntaxNodeAnalysisContext context)
        {
            var variableDeclaration = (VariableDeclarationSyntax)context.Node;
            var baseType = context.SemanticModel.GetTypeInfo(variableDeclaration.Type).Type;

            foreach (var declaration in variableDeclaration.Variables.Where(syntax => syntax.Initializer != null))
            {
                VerifyExpression(declaration.Initializer.Value, baseType, context);
            }
        }

        private static void RaiseOnArrayCovarianceInInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var methodParameterLookup = new CSharpMethodParameterLookup(invocation, context.SemanticModel);

            foreach (var argument in invocation.ArgumentList.Arguments)
            {
                if (!methodParameterLookup.TryGetSymbol(argument, out var parameter) || parameter.IsParams)
                {
                    continue;
                }

                VerifyExpression(argument.Expression, parameter.Type, context);
            }
        }

        private static void RaiseOnArrayCovarianceInCastExpression(SyntaxNodeAnalysisContext context)
        {
            var castExpression = (CastExpressionSyntax)context.Node;
            var baseType = context.SemanticModel.GetTypeInfo(castExpression.Type).Type;

            VerifyExpression(castExpression.Expression, baseType, context);
        }

        private static void VerifyExpression(SyntaxNode node, ITypeSymbol baseType, SyntaxNodeAnalysisContext context)
        {
            foreach (var pair in GetPossibleTypes(node, context.SemanticModel).Where(pair => AreCovariantArrayTypes(pair.Symbol, baseType)))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, pair.Node.GetLocation()));
            }
        }

        private static bool AreCovariantArrayTypes(ITypeSymbol typeDerivedArray, ITypeSymbol typeBaseArray)
        {
            if (typeDerivedArray == null
                || !(typeBaseArray is {Kind: SymbolKind.ArrayType})
                || typeDerivedArray.Kind != SymbolKind.ArrayType)
            {
                return false;
            }

            var typeDerivedElement = ((IArrayTypeSymbol)typeDerivedArray).ElementType;
            var typeBaseElement = ((IArrayTypeSymbol)typeBaseArray).ElementType;

            return typeDerivedElement.BaseType?.ConstructedFrom.DerivesFrom(typeBaseElement) == true;
        }

        private static IEnumerable<NodeTypePair> GetPossibleTypes(SyntaxNode syntax, SemanticModel semanticModel)
        {
            while (syntax is ParenthesizedExpressionSyntax parenthesizedExpression)
            {
                syntax = parenthesizedExpression.Expression;
            }

            if (syntax is ConditionalExpressionSyntax conditionalExpression)
            {
                yield return new NodeTypePair(conditionalExpression.WhenTrue, semanticModel);
                yield return new NodeTypePair(conditionalExpression.WhenFalse, semanticModel);
            }
            else if (syntax.IsKind(SyntaxKind.CoalesceExpression))
            {
                var binaryExpression = (BinaryExpressionSyntax)syntax;
                yield return new NodeTypePair(binaryExpression.Left, semanticModel);
                yield return new NodeTypePair(binaryExpression.Right, semanticModel);
            }
            else
            {
                yield return new NodeTypePair(syntax, semanticModel);
            }
        }

        private readonly struct NodeTypePair
        {
            public ITypeSymbol Symbol { get; }

            public SyntaxNode Node { get; }

            public NodeTypePair(SyntaxNode node, SemanticModel model)
            {
                Symbol = model.GetTypeInfo(node).Type;
                Node = node;
            }
        }
    }
}
