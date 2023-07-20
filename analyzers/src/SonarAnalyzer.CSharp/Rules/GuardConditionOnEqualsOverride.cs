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
    public sealed class GuardConditionOnEqualsOverride : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3397";
        private const string MessageFormat = "Change this guard condition to call 'object.ReferenceEquals'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly ISet<string> MethodNames = new HashSet<string> { GetHashCodeEqualsOverride.EqualsName };

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCodeBlockStartAction<SyntaxKind>(
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
            if (!(context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol invokedMethod)
                || invokedMethod.Name != symbol.Name
                || !invocation.IsOnBase())
            {
                return;
            }

            if (!invokedMethod.ContainingType.Is(KnownType.System_Object)
                && GetHashCodeEqualsOverride.IsEqualsCallInGuardCondition(invocation, invokedMethod))
            {
                context.ReportIssue(CreateDiagnostic(Rule, invocation.GetLocation()));
            }
        }
    }
}
