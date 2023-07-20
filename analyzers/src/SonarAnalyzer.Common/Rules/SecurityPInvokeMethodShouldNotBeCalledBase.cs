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
    public abstract class SecurityPInvokeMethodShouldNotBeCalledBase<TSyntaxKind, TInvocationExpressionSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
        where TInvocationExpressionSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S3884";
        protected const string InteropName = "ole32";
        protected const string InteropDllName = InteropName + ".dll";

        protected abstract IMethodSymbol MethodSymbolForInvalidInvocation(SyntaxNode syntaxNode, SemanticModel semanticModel);

        protected override string MessageFormat => "Refactor the code to remove this use of '{0}'.";

        protected ISet<string> InvalidMethods { get; } = new HashSet<string>
        {
            "CoSetProxyBlanket",
            "CoInitializeSecurity"
        };

        protected SecurityPInvokeMethodShouldNotBeCalledBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckForIssue, Language.SyntaxKind.InvocationExpression);

        protected virtual bool IsImportFromInteropDll(IMethodSymbol symbol, SemanticModel semanticModel) =>
            (symbol.IsExtern
                ? symbol.GetAttributes(KnownType.System_Runtime_InteropServices_DllImportAttribute)
                : symbol.GetAttributes(KnownType.System_Runtime_InteropServices_LibraryImportAttribute))
                    .SelectMany(x => x.ConstructorArguments) // Both attributes have a single constructor which takes a single string "library" argument
                    .Any(x => x.Value is string stringValue && IsInterop(stringValue));

        protected virtual string GetMethodName(ISymbol symbol, SemanticModel semanticModel) =>
            symbol.Name;

        protected static bool IsInterop(string dllName) =>
            dllName.Equals(InteropName, StringComparison.OrdinalIgnoreCase)
            || dllName.Equals(InteropDllName, StringComparison.OrdinalIgnoreCase);

        private void CheckForIssue(SonarSyntaxNodeReportingContext analysisContext)
        {
            if (analysisContext.Node is TInvocationExpressionSyntax invocation
                && Language.Syntax.NodeExpression(invocation) is { } directMethodCall
                && MethodSymbolForInvalidInvocation(directMethodCall, analysisContext.SemanticModel) is IMethodSymbol methodSymbol
                && methodSymbol.IsStatic
                && IsImportFromInteropDll(methodSymbol, analysisContext.SemanticModel))
            {
                analysisContext.ReportIssue(CreateDiagnostic(Rule, Language.Syntax.NodeIdentifier(directMethodCall).Value.GetLocation(), GetMethodName(methodSymbol, analysisContext.SemanticModel)));
            }
        }
    }
}
