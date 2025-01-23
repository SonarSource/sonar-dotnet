/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Rules
{
    public abstract class DoNotCallInsecureSecurityAlgorithmBase<TSyntaxKind, TInvocationExpressionSyntax, TArgumentListSyntax, TArgumentSyntax>
        : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TInvocationExpressionSyntax : SyntaxNode
        where TArgumentListSyntax : SyntaxNode
        where TArgumentSyntax : SyntaxNode
    {
        protected abstract ISet<string> AlgorithmParameterlessFactoryMethods { get; }
        protected abstract ISet<string> AlgorithmParameterizedFactoryMethods { get; }
        protected abstract ISet<string> FactoryParameterNames { get; }
        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
        private protected abstract ImmutableArray<KnownType> AlgorithmTypes { get; }

        protected abstract Location Location(SyntaxNode objectCreation);
        protected abstract TArgumentListSyntax ArgumentList(TInvocationExpressionSyntax invocationExpression);
        protected abstract SeparatedSyntaxList<TArgumentSyntax> Arguments(TArgumentListSyntax argumentList);
        protected abstract bool IsStringLiteralArgument(TArgumentSyntax argument);
        protected abstract SyntaxNode Expression(TArgumentSyntax argument);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckObjectCreation, Language.SyntaxKind.ObjectCreationExpressions);
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckInvocation, Language.SyntaxKind.InvocationExpression);
        }

        private void CheckInvocation(SonarSyntaxNodeReportingContext context)
        {
            var invocation = (TInvocationExpressionSyntax)context.Node;

            if (Language.Syntax.NodeExpression(invocation) is { } expression
                && (context.Model.GetSymbolInfo(expression).Symbol is IMethodSymbol methodSymbol)
                && (methodSymbol.ReturnType.DerivesFromAny(AlgorithmTypes) || IsInsecureBaseAlgorithmCreationFactoryCall(methodSymbol, invocation)))
            {
                ReportAllDiagnostics(context, invocation.GetLocation());
            }
        }

        private void CheckObjectCreation(SonarSyntaxNodeReportingContext context)
        {
            var objectCreation = context.Node;

            var typeInfo = context.Model.GetTypeInfo(objectCreation);
            if (typeInfo.Type == null || typeInfo.Type is IErrorTypeSymbol)
            {
                return;
            }

            if (typeInfo.Type.DerivesFromAny(AlgorithmTypes))
            {
                ReportAllDiagnostics(context, Location(objectCreation));
            }
        }

        private bool IsInsecureBaseAlgorithmCreationFactoryCall(IMethodSymbol methodSymbol, TInvocationExpressionSyntax invocationExpression)
        {
            var argumentList = ArgumentList(invocationExpression);

            if (argumentList == null || methodSymbol.ContainingType == null)
            {
                return false;
            }

            var methodFullName = $"{methodSymbol.ContainingType}.{methodSymbol.Name}";

            if (Arguments(argumentList).Count == 0)
            {
                return AlgorithmParameterlessFactoryMethods.Contains(methodFullName);
            }

            if (Arguments(argumentList).Count > 1 || !IsStringLiteralArgument(Arguments(argumentList).First()))
            {
                return false;
            }

            if (!AlgorithmParameterizedFactoryMethods.Contains(methodFullName))
            {
                return false;
            }

            return FactoryParameterNames.Any(x => x.Equals(Language.Syntax.LiteralText(Expression(Arguments(argumentList).First())), StringComparison.Ordinal));
        }

        private void ReportAllDiagnostics(SonarSyntaxNodeReportingContext context, Location location)
        {
            foreach (var supportedDiagnostic in SupportedDiagnostics)
            {
                context.ReportIssue(supportedDiagnostic, location);
            }
        }
    }
}
