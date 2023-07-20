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
    public sealed class GenericReadonlyFieldPropertyAssignment : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2934";
        private const string MessageFormat = "Restrict '{0}' to be a reference type or remove this assignment of '{1}'; it is useless if '{0}' is a value type.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(c =>
                {
                    var assignment = (AssignmentExpressionSyntax)c.Node;
                    var expression = assignment.Left;

                    ProcessPropertyChange(c, c.SemanticModel, expression);
                },
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxKind.AddAssignmentExpression,
                SyntaxKind.SubtractAssignmentExpression,
                SyntaxKind.MultiplyAssignmentExpression,
                SyntaxKind.DivideAssignmentExpression,
                SyntaxKind.ModuloAssignmentExpression,
                SyntaxKind.AndAssignmentExpression,
                SyntaxKind.ExclusiveOrAssignmentExpression,
                SyntaxKind.OrAssignmentExpression,
                SyntaxKind.LeftShiftAssignmentExpression,
                SyntaxKind.RightShiftAssignmentExpression,
                SyntaxKindEx.CoalesceAssignmentExpression,
                SyntaxKindEx.UnsignedRightShiftAssignmentExpression);

            context.RegisterNodeAction(c =>
                {
                    var unary = (PrefixUnaryExpressionSyntax)c.Node;
                    ProcessPropertyChange(c, c.SemanticModel, unary.Operand);
                },
                SyntaxKind.PreDecrementExpression,
                SyntaxKind.PreIncrementExpression);

            context.RegisterNodeAction(c =>
                {
                    var unary = (PostfixUnaryExpressionSyntax)c.Node;
                    ProcessPropertyChange(c, c.SemanticModel, unary.Operand);
                },
                SyntaxKind.PostDecrementExpression,
                SyntaxKind.PostIncrementExpression);
        }

        private static void ProcessPropertyChange(SonarSyntaxNodeReportingContext context, SemanticModel semanticModel, ExpressionSyntax expression)
        {
            if (TupleExpressionSyntaxWrapper.IsInstance(expression))
            {
                foreach (var tupleArgument in ((TupleExpressionSyntaxWrapper)expression).Arguments)
                {
                    ProcessPropertyChange(context, semanticModel, tupleArgument.Expression);
                }
            }
            else if (expression is MemberAccessExpressionSyntax memberAccess
                && semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol is IFieldSymbol fieldSymbol
                && IsFieldReadonlyAndPossiblyValueType(fieldSymbol)
                && !IsInsideConstructorDeclaration(expression, fieldSymbol.ContainingType, semanticModel)
                && semanticModel.GetSymbolInfo(expression).Symbol is IPropertySymbol propertySymbol)
            {
                context.ReportIssue(CreateDiagnostic(Rule, expression.GetLocation(), fieldSymbol.Name, propertySymbol.Name));
            }
        }

        private static bool IsFieldReadonlyAndPossiblyValueType(IFieldSymbol fieldSymbol) =>
            fieldSymbol is { IsReadOnly: true }
            && GenericParameterMightBeValueType(fieldSymbol.Type as ITypeParameterSymbol);

        private static bool IsInsideConstructorDeclaration(ExpressionSyntax expression, INamedTypeSymbol currentType, SemanticModel semanticModel) =>
            semanticModel.GetEnclosingSymbol(expression.SpanStart) is IMethodSymbol { MethodKind: MethodKind.Constructor } constructorSymbol
            && constructorSymbol.ContainingType.Equals(currentType);

        private static bool GenericParameterMightBeValueType(ITypeParameterSymbol typeParameterSymbol) =>
            typeParameterSymbol is
            {
                HasReferenceTypeConstraint: false,
                HasValueTypeConstraint: false, // CS1648 is raised, if constrained by 'struct'.
                ConstraintTypes: { } constraintTypes
            }
            && constraintTypes.All(MightBeValueType);

        private static bool MightBeValueType(ITypeSymbol type) =>
            type.IsInterface()
            || GenericParameterMightBeValueType(type as ITypeParameterSymbol);
    }
}
