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
        protected abstract string StringLiteralValue(TArgumentSyntax argument);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckObjectCreation, Language.SyntaxKind.ObjectCreationExpressions);
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckInvocation, Language.SyntaxKind.InvocationExpression);
        }

        private void CheckInvocation(SonarSyntaxNodeReportingContext context)
        {
            var invocation = (TInvocationExpressionSyntax)context.Node;

            if (Language.Syntax.NodeExpression(invocation) is { } expression
                && (context.SemanticModel.GetSymbolInfo(expression).Symbol is IMethodSymbol methodSymbol)
                && (methodSymbol.ReturnType.DerivesFromAny(AlgorithmTypes) || IsInsecureBaseAlgorithmCreationFactoryCall(methodSymbol, invocation)))
            {
                ReportAllDiagnostics(context, invocation.GetLocation());
            }
        }

        private void CheckObjectCreation(SonarSyntaxNodeReportingContext context)
        {
            var objectCreation = context.Node;

            var typeInfo = context.SemanticModel.GetTypeInfo(objectCreation);
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

            return FactoryParameterNames.Any(alg => alg.Equals(StringLiteralValue(Arguments(argumentList).First()), StringComparison.Ordinal));
        }

        private void ReportAllDiagnostics(SonarSyntaxNodeReportingContext context, Location location)
        {
            foreach (var supportedDiagnostic in SupportedDiagnostics)
            {
                context.ReportIssue(CreateDiagnostic(supportedDiagnostic, location));
            }
        }
    }
}
