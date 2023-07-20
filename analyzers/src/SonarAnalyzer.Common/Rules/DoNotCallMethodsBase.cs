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
    public abstract class DoNotCallMethodsBase<TSyntaxKind, TInvocationExpressionSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
        where TInvocationExpressionSyntax : SyntaxNode
    {
        protected abstract IEnumerable<MemberDescriptor> CheckedMethods { get; }

        protected virtual bool ShouldReportOnMethodCall(TInvocationExpressionSyntax invocation, SemanticModel semanticModel, MemberDescriptor memberDescriptor) => true;

        protected virtual bool IsInValidContext(TInvocationExpressionSyntax invocationSyntax, SemanticModel semanticModel) => true;

        protected virtual bool ShouldRegisterAction(Compilation compilation) => true;

        protected DoNotCallMethodsBase(string diagnosticId) : base(diagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
             context.RegisterCompilationStartAction(start =>
             {
                 if (!ShouldRegisterAction(start.Compilation))
                 {
                     return;
                 }
                 context.RegisterNodeAction(Language.GeneratedCodeRecognizer, AnalyzeInvocation, Language.SyntaxKind.InvocationExpression);
             });

        private void AnalyzeInvocation(SonarSyntaxNodeReportingContext analysisContext)
        {
            if ((TInvocationExpressionSyntax)analysisContext.Node is var invocation
                && Language.Syntax.InvocationIdentifier(invocation) is { } identifier
                && CheckedMethods.Where(x => x.Name.Equals(identifier.ValueText)) is var nameMatch
                && nameMatch.Any()
                && analysisContext.SemanticModel.GetSymbolInfo(identifier.Parent).Symbol is { } methodCallSymbol
                && nameMatch.FirstOrDefault(x => methodCallSymbol.ContainingType.ConstructedFrom.Is(x.ContainingType)) is { } disallowedMethodSignature
                && IsInValidContext(invocation, analysisContext.SemanticModel)
                && ShouldReportOnMethodCall(invocation, analysisContext.SemanticModel, disallowedMethodSignature))
            {
                analysisContext.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], identifier.GetLocation(), disallowedMethodSignature.ToString()));
            }
        }
    }
}
