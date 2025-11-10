/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.Core.Rules
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
             context.RegisterCompilationStartAction(c =>
             {
                 if (!ShouldRegisterAction(c.Compilation))
                 {
                     return;
                 }
                 c.RegisterNodeAction(Language.GeneratedCodeRecognizer, AnalyzeInvocation, Language.SyntaxKind.InvocationExpression);
             });

        private void AnalyzeInvocation(SonarSyntaxNodeReportingContext analysisContext)
        {
            if ((TInvocationExpressionSyntax)analysisContext.Node is var invocation
                && Language.Syntax.InvocationIdentifier(invocation) is { } identifier
                && CheckedMethods.Where(x => x.Name.Equals(identifier.ValueText)) is var nameMatch
                && nameMatch.Any()
                && analysisContext.Model.GetSymbolInfo(identifier.Parent).Symbol is { } methodCallSymbol
                && nameMatch.FirstOrDefault(x => methodCallSymbol.ContainingType.ConstructedFrom.Is(x.ContainingType)) is { } disallowedMethodSignature
                && IsInValidContext(invocation, analysisContext.Model)
                && ShouldReportOnMethodCall(invocation, analysisContext.Model, disallowedMethodSignature))
            {
                analysisContext.ReportIssue(SupportedDiagnostics[0], identifier.GetLocation(), disallowedMethodSignature.ToString());
            }
        }
    }
}
