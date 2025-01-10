/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PubliclyWritableDirectories : PubliclyWritableDirectoriesBase<SyntaxKind, InvocationExpressionSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        public PubliclyWritableDirectories() : this(AnalyzerConfiguration.Hotspot) { }

        internal PubliclyWritableDirectories(IAnalyzerConfiguration configuration) : base(configuration) { }

        private protected override bool IsGetTempPathAssignment(InvocationExpressionSyntax invocationExpression, KnownType type, string methodName, SemanticModel semanticModel) =>
            invocationExpression.IsMethodInvocation(type, methodName, semanticModel)
            && invocationExpression.Parent?.Kind() is
                SyntaxKind.EqualsValueClause or SyntaxKind.SimpleAssignmentExpression or SyntaxKind.ArrowExpressionClause or SyntaxKind.ReturnStatement;

        private protected override bool IsInsecureEnvironmentVariableRetrieval(InvocationExpressionSyntax invocation, KnownType type, string methodName, SemanticModel semanticModel) =>
            invocation.IsMethodInvocation(type, methodName, semanticModel)
            && invocation.ArgumentList?.Arguments.FirstOrDefault() is { } firstArgument
            && InsecureEnvironmentVariables.Any(x => x.Equals(firstArgument.Expression?.StringValue(semanticModel), StringComparison.OrdinalIgnoreCase));
    }
}
