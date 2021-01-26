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
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class SecurityPInvokeMethodShouldNotBeCalledBase<TSyntaxKind, TInvocationExpressionSyntax, TIdentifierNameSyntax> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TInvocationExpressionSyntax : SyntaxNode
        where TIdentifierNameSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S3884";
        protected const string MessageFormat = "Refactor the code to remove this use of '{0}'.";
        protected const string InteropDllName = "ole32.dll";

        protected static readonly ISet<string> InvalidMethods = new HashSet<string>
        {
            "CoSetProxyBlanket",
            "CoInitializeSecurity"
        };

        protected abstract TSyntaxKind SyntaxKind { get; }
        protected abstract ILanguageFacade LanguageFacade { get; }

        protected abstract SyntaxNode Expression(TInvocationExpressionSyntax invocationExpression);
        protected abstract SyntaxToken Identifier(TIdentifierNameSyntax identifierName);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected DiagnosticDescriptor Rule { get; }

        protected SecurityPInvokeMethodShouldNotBeCalledBase(System.Resources.ResourceManager rspecResources) =>
            Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(LanguageFacade.GeneratedCodeRecognizer, CheckForIssue, SyntaxKind);

        private void CheckForIssue(SyntaxNodeAnalysisContext analysisContext)
        {
            var invocation = (TInvocationExpressionSyntax)analysisContext.Node;

            if (!(Expression(invocation) is TIdentifierNameSyntax directMethodCall))
            {
                return;
            }

            if (!InvalidMethods.Contains(Identifier(directMethodCall).ValueText))
            {
                return;
            }

            var methodCallSymbol = analysisContext.SemanticModel.GetSymbolInfo(directMethodCall);

            if (methodCallSymbol.Symbol == null)
            {
                return;
            }

            if (!methodCallSymbol.Symbol.IsExtern || !methodCallSymbol.Symbol.IsStatic)
            {
                return;
            }

            var dllImportAttribute = methodCallSymbol.Symbol.GetAttributes(KnownType.System_Runtime_InteropServices_DllImportAttribute).FirstOrDefault();

            if (dllImportAttribute == null)
            {
                return;
            }

            if (dllImportAttribute.ConstructorArguments.Any(x => x.Value.Equals(InteropDllName)))
            {
                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, Identifier(directMethodCall).GetLocation(), Identifier(directMethodCall).ValueText));
            }
        }
    }
}
