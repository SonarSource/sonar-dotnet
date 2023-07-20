/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ArrayCovariance : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2330";
        private const string MessageFormat = "Refactor the code to not rely on potentially unsafe array conversions.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(RaiseOnArrayCovarianceInSimpleAssignmentExpression, SyntaxKind.SimpleAssignmentExpression);
            context.RegisterNodeAction(RaiseOnArrayCovarianceInVariableDeclaration, SyntaxKind.VariableDeclaration);
            context.RegisterNodeAction(RaiseOnArrayCovarianceInInvocationExpression, SyntaxKind.InvocationExpression);
            context.RegisterNodeAction(RaiseOnArrayCovarianceInCastExpression, SyntaxKind.CastExpression);
        }

        private static void RaiseOnArrayCovarianceInSimpleAssignmentExpression(SonarSyntaxNodeReportingContext context)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            VerifyExpression(assignment.Right, context.SemanticModel.GetTypeInfo(assignment.Left).Type, context);
        }

        private static void RaiseOnArrayCovarianceInVariableDeclaration(SonarSyntaxNodeReportingContext context)
        {
            var variableDeclaration = (VariableDeclarationSyntax)context.Node;
            var baseType = context.SemanticModel.GetTypeInfo(variableDeclaration.Type).Type;

            foreach (var declaration in variableDeclaration.Variables.Where(syntax => syntax.Initializer != null))
            {
                VerifyExpression(declaration.Initializer.Value, baseType, context);
            }
        }

        private static void RaiseOnArrayCovarianceInInvocationExpression(SonarSyntaxNodeReportingContext context)
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

        private static void RaiseOnArrayCovarianceInCastExpression(SonarSyntaxNodeReportingContext context)
        {
            var castExpression = (CastExpressionSyntax)context.Node;
            var baseType = context.SemanticModel.GetTypeInfo(castExpression.Type).Type;

            VerifyExpression(castExpression.Expression, baseType, context);
        }

        private static void VerifyExpression(SyntaxNode node, ITypeSymbol baseType, SonarSyntaxNodeReportingContext context)
        {
            foreach (var pair in GetPossibleTypes(node, context.SemanticModel).Where(pair => AreCovariantArrayTypes(pair.Symbol, baseType)))
            {
                context.ReportIssue(CreateDiagnostic(Rule, pair.Node.GetLocation()));
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
