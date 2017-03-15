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
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var assignment = (AssignmentExpressionSyntax) c.Node;
                    var typeDerived = c.SemanticModel.GetTypeInfo(assignment.Right).Type;
                    var typeBase = c.SemanticModel.GetTypeInfo(assignment.Left).Type;

                    if (AreCovariantArrayTypes(typeDerived, typeBase))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, assignment.Right.GetLocation()));
                    }
                },
                SyntaxKind.SimpleAssignmentExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var variableDeclaration = (VariableDeclarationSyntax)c.Node;
                    var typeBase = c.SemanticModel.GetTypeInfo(variableDeclaration.Type).Type;

                    foreach (var variable in variableDeclaration.Variables
                        .Where(syntax => syntax.Initializer != null))
                    {
                        var typeDerived = c.SemanticModel.GetTypeInfo(variable.Initializer.Value).Type;

                        if (AreCovariantArrayTypes(typeDerived, typeBase))
                        {
                            c.ReportDiagnostic(Diagnostic.Create(Rule, variable.Initializer.Value.GetLocation()));
                        }
                    }
                },
                SyntaxKind.VariableDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    var methodParameterLookup = new MethodParameterLookup(invocation, c.SemanticModel);

                    foreach (var argument in invocation.ArgumentList.Arguments)
                    {
                        IParameterSymbol parameter;
                        if (!methodParameterLookup.TryGetParameterSymbol(argument, out parameter) ||
                            parameter.IsParams)
                        {
                            continue;
                        }

                        var typeDerived = c.SemanticModel.GetTypeInfo(argument.Expression).Type;
                        if (AreCovariantArrayTypes(typeDerived, parameter.Type))
                        {
                            c.ReportDiagnostic(Diagnostic.Create(Rule, argument.GetLocation()));
                        }
                    }
                },
                SyntaxKind.InvocationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var castExpression = (CastExpressionSyntax) c.Node;
                    var typeDerived = c.SemanticModel.GetTypeInfo(castExpression.Expression).Type;
                    var typeBase = c.SemanticModel.GetTypeInfo(castExpression.Type).Type;

                    if (AreCovariantArrayTypes(typeDerived, typeBase))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, castExpression.Type.GetLocation()));
                    }
                },
                SyntaxKind.CastExpression);
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
