/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
    public sealed class GuardConditionOnEqualsOverride : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3397";
        private const string MessageFormat = "Change this guard condition to call 'object.ReferenceEquals'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly ISet<string> MethodNames = new HashSet<string> { GetHashCodeEqualsOverride.EqualsName };

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCodeBlockStartAction(
                cb =>
                {
                    if (!(cb.OwningSymbol is IMethodSymbol methodSymbol)
                        || !(cb.CodeBlock is MethodDeclarationSyntax)
                        || !GetHashCodeEqualsOverride.MethodIsRelevant(methodSymbol, MethodNames))
                    {
                        return;
                    }

                    cb.RegisterNodeAction(c => CheckInvocationInsideMethod(c, methodSymbol), SyntaxKind.InvocationExpression);
                });

        private static void CheckInvocationInsideMethod(SonarSyntaxNodeReportingContext context, ISymbol symbol)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (!(context.Model.GetSymbolInfo(invocation).Symbol is IMethodSymbol invokedMethod)
                || invokedMethod.Name != symbol.Name
                || !invocation.IsOnBase())
            {
                return;
            }

            if (!invokedMethod.ContainingType.Is(KnownType.System_Object)
                && GetHashCodeEqualsOverride.IsEqualsCallInGuardCondition(invocation, invokedMethod))
            {
                context.ReportIssue(Rule, invocation);
            }
        }
    }
}
