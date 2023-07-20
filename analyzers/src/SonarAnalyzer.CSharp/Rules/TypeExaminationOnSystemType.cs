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
    public sealed class TypeExaminationOnSystemType : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3443";
        private const string MessageFormat = "{0}";
        private const string MessageGetType = "Remove this use of 'GetType' on a 'System.Type'.";
        private const string MessageIsInstanceOfType = "Pass an argument that is not a 'System.Type' or consider using 'IsAssignableFrom'.";
        private const string MessageIsInstanceOfTypeWithGetType = "Consider removing the 'GetType' call, it's suspicious in an 'IsInstanceOfType' call.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
               {
                   var invocation = (InvocationExpressionSyntax)c.Node;

                   if (invocation.Expression.ToStringContainsEitherOr(nameof(Type.IsInstanceOfType), nameof(Type.GetType))
                       && c.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol)
                   {
                       CheckGetTypeCallOnType(c, invocation, methodSymbol);
                       CheckIsInstanceOfTypeCallWithTypeArgument(c, invocation, methodSymbol);
                   }
               },
               SyntaxKind.InvocationExpression);

        private static void CheckIsInstanceOfTypeCallWithTypeArgument(SonarSyntaxNodeReportingContext context, InvocationExpressionSyntax invocation, ISymbol methodSymbol)
        {
            if (methodSymbol.Name != nameof(Type.IsInstanceOfType) || !methodSymbol.ContainingType.Is(KnownType.System_Type))
            {
                return;
            }

            var argument = invocation.ArgumentList.Arguments.First().Expression;

            var typeInfo = context.SemanticModel.GetTypeInfo(argument).Type;
            if (!typeInfo.Is(KnownType.System_Type))
            {
                return;
            }

            var invocationInArgument = argument as InvocationExpressionSyntax;
            var message = invocationInArgument.IsGetTypeCall(context.SemanticModel)
                ? MessageIsInstanceOfTypeWithGetType
                : MessageIsInstanceOfType;

            context.ReportIssue(CreateDiagnostic(Rule, argument.GetLocation(), message));
        }

        private static void CheckGetTypeCallOnType(SonarSyntaxNodeReportingContext context, InvocationExpressionSyntax invocation, IMethodSymbol invokedMethod)
        {
            if (!(invocation.Expression is MemberAccessExpressionSyntax memberCall)
                || IsException(memberCall, context.SemanticModel)
                || !invokedMethod.IsGetTypeCall())
            {
                return;
            }

            var expressionType = context.SemanticModel.GetTypeInfo(memberCall.Expression).Type;
            if (!expressionType.Is(KnownType.System_Type))
            {
                return;
            }

            context.ReportIssue(CreateDiagnostic(Rule, memberCall.OperatorToken.CreateLocation(invocation), MessageGetType));
        }

        private static bool IsException(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel) =>
            memberAccess.Expression is TypeOfExpressionSyntax typeOf
            && typeOf.Type.IsKnownType(KnownType.System_Type, semanticModel);
    }
}
