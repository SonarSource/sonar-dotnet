/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class GuardConditionOnEqualsOverride : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3397";
        private const string MessageFormat = "Change this guard condition to call 'object.ReferenceEquals'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<string> MethodNames = new HashSet<string> { GetHashCodeEqualsOverride.EqualsName };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCodeBlockStartActionInNonGenerated<SyntaxKind>(
                cb =>
                {
                    if (!(cb.CodeBlock is MethodDeclarationSyntax methodDeclaration))
                    {
                        return;
                    }

                    if (!(cb.OwningSymbol is IMethodSymbol methodSymbol) ||
                        !GetHashCodeEqualsOverride.MethodIsRelevant(methodSymbol, MethodNames))
                    {
                        return;
                    }

                    cb.RegisterSyntaxNodeAction(
                        c =>
                        {
                            CheckInvocationInsideMethod(c, methodSymbol);
                        },
                        SyntaxKind.InvocationExpression);
                });
        }
        private static void CheckInvocationInsideMethod(SyntaxNodeAnalysisContext context,
            IMethodSymbol methodSymbol)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (!(context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol invokedMethod) ||
                invokedMethod.Name != methodSymbol.Name)
            {
                return;
            }

            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;

            if (!(memberAccess?.Expression is BaseExpressionSyntax baseCall))
            {
                return;
            }

            var objectType = invokedMethod.ContainingType;
            if (objectType != null &&
                !objectType.Is(KnownType.System_Object) &&
                GetHashCodeEqualsOverride.IsEqualsCallInGuardCondition(invocation, invokedMethod))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, invocation.GetLocation()));
            }
        }
    }
}
