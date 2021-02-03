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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class SecurityPInvokeMethodShouldNotBeCalledBase<TSyntaxKind, TInvocationExpressionSyntax> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TInvocationExpressionSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S3884";
        protected const string MessageFormat = "Refactor the code to remove this use of '{0}'.";
        protected const string InteropName = "ole32";
        protected const string InteropDllName = InteropName + ".dll";

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        protected abstract IMethodSymbol MethodSymbolForInvalidInvocation(SyntaxNode syntaxNode, SemanticModel semanticModel);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected DiagnosticDescriptor Rule { get; }

        protected ISet<string> InvalidMethods { get; } = new HashSet<string>
        {
            "CoSetProxyBlanket",
            "CoInitializeSecurity"
        };

        protected SecurityPInvokeMethodShouldNotBeCalledBase(System.Resources.ResourceManager rspecResources) =>
            Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer, CheckForIssue, Language.SyntaxKind.InvocationExpression);

        protected virtual bool IsImportFromInteropDll(ISymbol symbol) =>
            symbol.GetAttributes(KnownType.System_Runtime_InteropServices_DllImportAttribute).FirstOrDefault() is AttributeData attributeData
            && attributeData.ConstructorArguments.Any(x => x.Value is string stringValue && IsInterop(stringValue));

        protected virtual string GetMethodName(ISymbol symbol) =>
            symbol.Name;

        protected static bool IsInterop(string dllName) =>
            dllName.Equals(InteropName, StringComparison.OrdinalIgnoreCase)
            || dllName.Equals(InteropDllName, StringComparison.OrdinalIgnoreCase);

        private void CheckForIssue(SyntaxNodeAnalysisContext analysisContext)
        {
            if (analysisContext.Node is TInvocationExpressionSyntax invocation
                && Language.Syntax.NodeExpression(invocation) is { } directMethodCall
                && MethodSymbolForInvalidInvocation(directMethodCall, analysisContext.SemanticModel) is IMethodSymbol methodSymbol
                && methodSymbol.IsExtern
                && methodSymbol.IsStatic
                && IsImportFromInteropDll(methodSymbol))
            {
                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, Language.Syntax.NodeIdentifier(directMethodCall).Value.GetLocation(), GetMethodName(methodSymbol)));
            }
        }
    }
}
