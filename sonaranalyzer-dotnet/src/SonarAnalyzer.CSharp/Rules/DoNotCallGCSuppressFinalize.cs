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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System.Collections.Immutable;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DoNotCallGCSuppressFinalize : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3971";
        private const string MessageFormat = "Do not call 'GC.SuppressFinalize'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(ReportIfInvalidMethodIsCalled,
                SyntaxKind.InvocationExpression);
        }

        private void ReportIfInvalidMethodIsCalled(SyntaxNodeAnalysisContext analysisContext)
        {
            var invocation = (InvocationExpressionSyntax)analysisContext.Node;

            var identifier = GetMethodCallIdentifier(invocation);
            if (identifier == null ||
                !identifier.Value.ValueText.Equals("SuppressFinalize"))
            {
                return;
            }

            var methodCallSymbol = analysisContext.SemanticModel.GetSymbolInfo(identifier.Value.Parent);
            if (methodCallSymbol.Symbol == null ||
                !methodCallSymbol.Symbol.ContainingType.ConstructedFrom.Is(KnownType.System_GC))
            {
                return;
            }

            var methodDeclaration = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (methodDeclaration == null)
            {
                analysisContext.ReportDiagnostic(Diagnostic.Create(rule, identifier.Value.GetLocation()));
                return;
            }

            var methodSymbol = analysisContext.SemanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol == null ||
                !methodSymbol.IsIDisposableDispose())
            {
                analysisContext.ReportDiagnostic(Diagnostic.Create(rule, identifier.Value.GetLocation()));
                return;
            }
        }

        private SyntaxToken? GetMethodCallIdentifier(InvocationExpressionSyntax invocation)
        {
            var directMethodCall = invocation.Expression as IdentifierNameSyntax;
            if (directMethodCall != null)
            {
                return directMethodCall.Identifier;
            }

            var memberAccessCall = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccessCall != null)
            {
                return memberAccessCall.Name.Identifier;
            }

            return null;
        }
    }
}