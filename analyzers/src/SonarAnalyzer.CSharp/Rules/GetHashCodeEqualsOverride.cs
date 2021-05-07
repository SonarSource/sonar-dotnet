/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class GetHashCodeEqualsOverride : SonarDiagnosticAnalyzer
    {
        internal const string EqualsName = "Equals";

        private const string DiagnosticId = "S3249";
        private const string MessageFormat = "Remove this 'base' call to 'object.{0}', which is directly based on the object reference.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        private static readonly ISet<string> MethodNames = new HashSet<string> { "GetHashCode", EqualsName };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCodeBlockStartActionInNonGenerated<SyntaxKind>(
                cb =>
                {
                    if (!(cb.CodeBlock is MethodDeclarationSyntax methodDeclaration))
                    {
                        return;
                    }

                    if (methodDeclaration.AttributeLists.Any()
                        || !(cb.OwningSymbol is IMethodSymbol methodSymbol)
                        || !MethodIsRelevant(methodSymbol, MethodNames))
                    {
                        return;
                    }

                    var locations = new List<Location>();

                    cb.RegisterSyntaxNodeAction(
                        c =>
                        {
                            if (TryGetLocationFromInvocationInsideMethod(c, methodSymbol, out var location))
                            {
                                locations.Add(location);
                            }
                        },
                        SyntaxKind.InvocationExpression);

                    cb.RegisterCodeBlockEndAction(
                        c =>
                        {
                            if (!locations.Any())
                            {
                                return;
                            }

                            var firstPosition = locations.Select(loc => loc.SourceSpan.Start).Min();
                            var location = locations.First(loc => loc.SourceSpan.Start == firstPosition);
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, location, methodSymbol.Name));
                        });
                });

        internal static bool IsEqualsCallInGuardCondition(InvocationExpressionSyntax invocation, IMethodSymbol invokedMethod)
        {
            if (invokedMethod.Name != EqualsName)
            {
                return false;
            }

            if (!(invocation.Parent is IfStatementSyntax ifStatement)
                || ifStatement.Condition != invocation
                || !invocation.HasExactlyNArguments(1))
            {
                return false;
            }

            return IfStatementWithSingleReturnTrue(ifStatement);
        }

        internal static bool MethodIsRelevant(IMethodSymbol methodSymbol, ISet<string> methodNames) =>
            methodNames.Contains(methodSymbol.Name) && methodSymbol.IsOverride;

        private static bool TryGetLocationFromInvocationInsideMethod(SyntaxNodeAnalysisContext context, ISymbol symbol, out Location location)
        {
            location = null;
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (!(context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol invokedMethod)
                || invokedMethod.Name != symbol.Name)
            {
                return false;
            }

            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (!(memberAccess?.Expression is BaseExpressionSyntax))
            {
                return false;
            }

            if (invokedMethod.IsInType(KnownType.System_Object)
                && !IsEqualsCallInGuardCondition(invocation, invokedMethod))
            {
                location = invocation.GetLocation();
                return true;
            }

            return false;
        }

        private static bool IfStatementWithSingleReturnTrue(IfStatementSyntax ifStatement)
        {
            var statement = ifStatement.Statement;
            var returnStatement = statement as ReturnStatementSyntax;
            if (statement is BlockSyntax block)
            {
                if (!block.Statements.Any())
                {
                    return false;
                }

                returnStatement = block.Statements.First() as ReturnStatementSyntax;
            }

            if (returnStatement == null)
            {
                return false;
            }

            return returnStatement.Expression != null
                   && CSharpEquivalenceChecker.AreEquivalent(returnStatement.Expression, CSharpSyntaxHelper.TrueLiteralExpression);
        }
    }
}
