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
    public class ArrayCovariance : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2330";
        private const string MessageFormat = "Refactor the code to not rely on potentially unsafe array conversions.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        protected sealed override DiagnosticDescriptor Rule => rule;

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(RaiseOnArrayCovarianceInSimpleAssignmentExpression,
                SyntaxKind.SimpleAssignmentExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(RaiseOnArrayCovarianceInVariableDeclaration,
                SyntaxKind.VariableDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(RaiseOnArrayCovarianceInInvocationExpression,
                SyntaxKind.InvocationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(RaiseOnArrayCovarianceInCastExpression,
                SyntaxKind.CastExpression);
        }

        private void RaiseOnArrayCovarianceInSimpleAssignmentExpression(SyntaxNodeAnalysisContext context)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            var typeDerived = context.SemanticModel.GetTypeInfo(assignment.Right).Type;
            var typeBase = context.SemanticModel.GetTypeInfo(assignment.Left).Type;

            if (AreCovariantArrayTypes(typeDerived, typeBase))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, assignment.Right.GetLocation()));
            }
        }

        private void RaiseOnArrayCovarianceInVariableDeclaration(SyntaxNodeAnalysisContext context)
        {
            var variableDeclaration = (VariableDeclarationSyntax)context.Node;
            var typeBase = context.SemanticModel.GetTypeInfo(variableDeclaration.Type).Type;

            foreach (var variable in variableDeclaration.Variables
                .Where(syntax => syntax.Initializer != null))
            {
                var typeDerived = context.SemanticModel.GetTypeInfo(variable.Initializer.Value).Type;

                if (AreCovariantArrayTypes(typeDerived, typeBase))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, variable.Initializer.Value.GetLocation()));
                }
            }
        }

        private void RaiseOnArrayCovarianceInInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var methodParameterLookup = new MethodParameterLookup(invocation, context.SemanticModel);

            foreach (var argument in invocation.ArgumentList.Arguments)
            {
                IParameterSymbol parameter;
                if (!methodParameterLookup.TryGetParameterSymbol(argument, out parameter) ||
                    parameter.IsParams)
                {
                    continue;
                }

                var typeDerived = context.SemanticModel.GetTypeInfo(argument.Expression).Type;
                if (AreCovariantArrayTypes(typeDerived, parameter.Type))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, argument.GetLocation()));
                }
            }
        }

        private void RaiseOnArrayCovarianceInCastExpression(SyntaxNodeAnalysisContext context)
        {
            var castExpression = (CastExpressionSyntax)context.Node;
            var typeDerived = context.SemanticModel.GetTypeInfo(castExpression.Expression).Type;
            var typeBase = context.SemanticModel.GetTypeInfo(castExpression.Type).Type;

            if (AreCovariantArrayTypes(typeDerived, typeBase))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, castExpression.Type.GetLocation()));
            }
        }

        private static bool AreCovariantArrayTypes(ITypeSymbol typeDerivedArray, ITypeSymbol typeBaseArray)
        {
            if (typeDerivedArray == null ||
                typeBaseArray == null ||
                typeBaseArray.Kind != SymbolKind.ArrayType ||
                typeDerivedArray.Kind != SymbolKind.ArrayType)
            {
                return false;
            }

            var typeDerivedElement = ((IArrayTypeSymbol) typeDerivedArray).ElementType;
            var typeBaseElement = ((IArrayTypeSymbol)typeBaseArray).ElementType;

            return typeDerivedElement.BaseType != null &&
                typeDerivedElement.BaseType.ConstructedFrom.DerivesFrom(typeBaseElement);
        }
    }
}
