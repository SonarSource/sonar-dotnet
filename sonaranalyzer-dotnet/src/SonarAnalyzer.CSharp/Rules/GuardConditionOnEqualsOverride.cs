/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
    public class GuardConditionOnEqualsOverride : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3397";
        private const string MessageFormat = "Change this guard condition to call 'object.ReferenceEquals'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<string> MethodNames = ImmutableHashSet.Create( GetHashCodeEqualsOverride.EqualsName );

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCodeBlockStartActionInNonGenerated<SyntaxKind>(
                cb =>
                {
                    var methodDeclaration = cb.CodeBlock as MethodDeclarationSyntax;
                    if (methodDeclaration == null)
                    {
                        return;
                    }

                    var methodSymbol = cb.OwningSymbol as IMethodSymbol;
                    if (methodSymbol == null ||
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
            var invokedMethod = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (invokedMethod == null ||
                invokedMethod.Name != methodSymbol.Name)
            {
                return;
            }

            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;

            var baseCall = memberAccess?.Expression as BaseExpressionSyntax;
            if (baseCall == null)
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
