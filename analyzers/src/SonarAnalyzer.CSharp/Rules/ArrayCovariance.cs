/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
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
            VerifyExpression(assignment.Right, context.Model.GetTypeInfo(assignment.Left).Type, context);
        }

        private static void RaiseOnArrayCovarianceInVariableDeclaration(SonarSyntaxNodeReportingContext context)
        {
            var variableDeclaration = (VariableDeclarationSyntax)context.Node;
            var baseType = context.Model.GetTypeInfo(variableDeclaration.Type).Type;

            foreach (var declaration in variableDeclaration.Variables.Where(syntax => syntax.Initializer != null))
            {
                VerifyExpression(declaration.Initializer.Value, baseType, context);
            }
        }

        private static void RaiseOnArrayCovarianceInInvocationExpression(SonarSyntaxNodeReportingContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var methodParameterLookup = new CSharpMethodParameterLookup(invocation, context.Model);

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
            var baseType = context.Model.GetTypeInfo(castExpression.Type).Type;

            VerifyExpression(castExpression.Expression, baseType, context);
        }

        private static void VerifyExpression(SyntaxNode node, ITypeSymbol baseType, SonarSyntaxNodeReportingContext context)
        {
            foreach (var pair in GetPossibleTypes(node, context.Model).Where(pair => AreCovariantArrayTypes(pair.Symbol, baseType)))
            {
                context.ReportIssue(Rule, pair.Node);
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
