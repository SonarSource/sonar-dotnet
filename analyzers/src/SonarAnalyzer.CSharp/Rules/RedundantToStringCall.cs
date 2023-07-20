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
    public sealed class RedundantToStringCall : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1858";
        private const string MessageFormat = "There's no need to call 'ToString()'{0}.";
        internal const string MessageCallOnString = " on a string";
        internal const string MessageCompiler = ", the compiler will do it for you";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const string additionOperatorName = "op_Addition";

        protected override void Initialize(SonarAnalysisContext context)
        {
            CheckToStringInvocationsOnStringAndInStringFormat(context);
            CheckSidesOfAddExpressionsForToStringCall(context);
            CheckRightSideOfAddAssignmentsForToStringCall(context);
        }

        private static void CheckRightSideOfAddAssignmentsForToStringCall(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var assignment = (AssignmentExpressionSyntax)c.Node;
                    var operation = c.SemanticModel.GetSymbolInfo(assignment).Symbol as IMethodSymbol;
                    if (!IsOperationAddOnString(operation))
                    {
                        return;
                    }

                    CheckRightExpressionForRemovableToStringCall(c, assignment);
                },
                SyntaxKind.AddAssignmentExpression);
        }

        private static void CheckSidesOfAddExpressionsForToStringCall(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var binary = (BinaryExpressionSyntax)c.Node;
                    var operation = c.SemanticModel.GetSymbolInfo(binary).Symbol as IMethodSymbol;
                    if (!IsOperationAddOnString(operation))
                    {
                        return;
                    }

                    CheckLeftExpressionForRemovableToStringCall(c, binary);
                    CheckRightExpressionForRemovableToStringCall(c, binary);
                },
                SyntaxKind.AddExpression);
        }

        private static void CheckToStringInvocationsOnStringAndInStringFormat(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    if (!IsArgumentlessToStringCallNotOnBaseExpression(invocation, c.SemanticModel, out var location, out var methodSymbol))
                    {
                        return;
                    }

                    if (methodSymbol.IsInType(KnownType.System_String))
                    {
                        c.ReportIssue(CreateDiagnostic(rule, location, MessageCallOnString));
                        return;
                    }

                    if (!TryGetExpressionTypeOfOwner(invocation, c.SemanticModel, out var subExpressionType) ||
                        subExpressionType.IsValueType)
                    {
                        return;
                    }

                    var stringFormatArgument = invocation?.Parent as ArgumentSyntax;
                    if (!(stringFormatArgument?.Parent?.Parent is InvocationExpressionSyntax stringFormatInvocation) ||
                        !IsStringFormatCall(c.SemanticModel.GetSymbolInfo(stringFormatInvocation).Symbol as IMethodSymbol))
                    {
                        return;
                    }

                    var parameterLookup = new CSharpMethodParameterLookup(stringFormatInvocation, c.SemanticModel);
                    if (parameterLookup.TryGetSymbol(stringFormatArgument, out var argParameter) &&
                        argParameter.Name.StartsWith("arg", StringComparison.Ordinal))
                    {
                        c.ReportIssue(CreateDiagnostic(rule, location, MessageCompiler));
                    }
                },
                SyntaxKind.InvocationExpression);
        }

        private static void CheckLeftExpressionForRemovableToStringCall(SonarSyntaxNodeReportingContext context,
            BinaryExpressionSyntax binary)
        {
            CheckExpressionForRemovableToStringCall(context, binary.Left, binary.Right, 0);
        }
        private static void CheckRightExpressionForRemovableToStringCall(SonarSyntaxNodeReportingContext context,
            BinaryExpressionSyntax binary)
        {
            CheckExpressionForRemovableToStringCall(context, binary.Right, binary.Left, 1);
        }
        private static void CheckRightExpressionForRemovableToStringCall(SonarSyntaxNodeReportingContext context,
            AssignmentExpressionSyntax assignment)
        {
            CheckExpressionForRemovableToStringCall(context, assignment.Right, assignment.Left, 1);
        }

        private static void CheckExpressionForRemovableToStringCall(SonarSyntaxNodeReportingContext context,
            ExpressionSyntax expressionWithToStringCall, ExpressionSyntax otherOperandOfAddition, int checkedSideIndex)
        {
            if (!IsArgumentlessToStringCallNotOnBaseExpression(expressionWithToStringCall, context.SemanticModel, out var location, out var methodSymbol) ||
                methodSymbol.IsInType(KnownType.System_String))
            {
                return;
            }

            var sideBType = context.SemanticModel.GetTypeInfo(otherOperandOfAddition).Type;
            if (!sideBType.Is(KnownType.System_String))
            {
                return;
            }

            if (!TryGetExpressionTypeOfOwner((InvocationExpressionSyntax)expressionWithToStringCall, context.SemanticModel, out var subExpressionType) ||
                subExpressionType.IsValueType)
            {
                return;
            }

            var stringParameterIndex = (checkedSideIndex + 1) % 2;
            if (!DoesCollidingAdditionExist(subExpressionType, stringParameterIndex))
            {
                context.ReportIssue(CreateDiagnostic(rule, location, MessageCompiler));
            }
        }

        private static bool TryGetExpressionTypeOfOwner(InvocationExpressionSyntax invocation, SemanticModel semanticModel,
            out ITypeSymbol subExpressionType)
        {
            subExpressionType = null;

            var subExpression = (invocation.Expression as MemberAccessExpressionSyntax)?.Expression;
            if (subExpression == null)
            {
                return false;
            }

            subExpressionType = semanticModel.GetTypeInfo(subExpression).Type;
            return subExpressionType != null;
        }

        private static bool DoesCollidingAdditionExist(ITypeSymbol subExpressionType, int stringParameterIndex)
        {
            return subExpressionType.GetMembers(additionOperatorName)
                .OfType<IMethodSymbol>()
                .Where(method =>
                    method.MethodKind == MethodKind.BuiltinOperator ||
                    method.MethodKind == MethodKind.UserDefinedOperator)
                .Any(method =>
                    method.Parameters.Length == 2 &&
                    method.Parameters[stringParameterIndex].IsType(KnownType.System_String));
        }

        private static bool IsStringFormatCall(IMethodSymbol stringFormatSymbol)
        {
            return stringFormatSymbol != null &&
                stringFormatSymbol.Name == "Format" &&
                (stringFormatSymbol.ContainingType == null || stringFormatSymbol.IsInType(KnownType.System_String));
        }

        private static bool IsOperationAddOnString(IMethodSymbol operation)
        {
            return operation != null &&
                operation.Name == additionOperatorName &&
                operation.IsInType(KnownType.System_String);
        }

        private static bool IsArgumentlessToStringCallNotOnBaseExpression(ExpressionSyntax expression, SemanticModel semanticModel,
            out Location location, out IMethodSymbol methodSymbol)
        {
            location = null;
            methodSymbol = null;
            if (!(expression is InvocationExpressionSyntax invocation) ||
                invocation.ArgumentList.CloseParenToken.IsMissing)
            {
                return false;
            }

            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess) ||
                memberAccess.Expression is BaseExpressionSyntax)
            {
                return false;
            }

            methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (!IsParameterlessToString(methodSymbol))
            {
                return false;
            }

            location = memberAccess.OperatorToken.CreateLocation(invocation);
            return true;
        }

        private static bool IsParameterlessToString(IMethodSymbol methodSymbol)
        {
            return methodSymbol != null &&
                methodSymbol.Name == "ToString" &&
                !methodSymbol.Parameters.Any();
        }
    }
}
