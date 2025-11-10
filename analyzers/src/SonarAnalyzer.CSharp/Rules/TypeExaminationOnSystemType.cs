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
                       && c.Model.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol)
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

            var typeInfo = context.Model.GetTypeInfo(argument).Type;
            if (!typeInfo.Is(KnownType.System_Type))
            {
                return;
            }

            var invocationInArgument = argument as InvocationExpressionSyntax;
            var message = invocationInArgument.IsGetTypeCall(context.Model)
                ? MessageIsInstanceOfTypeWithGetType
                : MessageIsInstanceOfType;

            context.ReportIssue(Rule, argument, message);
        }

        private static void CheckGetTypeCallOnType(SonarSyntaxNodeReportingContext context, InvocationExpressionSyntax invocation, IMethodSymbol invokedMethod)
        {
            if (!(invocation.Expression is MemberAccessExpressionSyntax memberCall)
                || IsException(memberCall, context.Model)
                || !invokedMethod.IsGetTypeCall())
            {
                return;
            }

            var expressionType = context.Model.GetTypeInfo(memberCall.Expression).Type;
            if (!expressionType.Is(KnownType.System_Type))
            {
                return;
            }

            context.ReportIssue(Rule, memberCall.OperatorToken.CreateLocation(invocation), MessageGetType);
        }

        private static bool IsException(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel) =>
            memberAccess.Expression is TypeOfExpressionSyntax typeOf
            && typeOf.Type.IsKnownType(KnownType.System_Type, semanticModel);
    }
}
