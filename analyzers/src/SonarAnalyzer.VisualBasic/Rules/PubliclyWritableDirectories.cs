/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.VisualBasic.Rules;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class PubliclyWritableDirectories : PubliclyWritableDirectoriesBase<SyntaxKind, InvocationExpressionSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    private protected override bool IsGetTempPathAssignment(InvocationExpressionSyntax invocationExpression, KnownType type, string methodName, SemanticModel model) =>
        invocationExpression.IsMethodInvocation(type, methodName, model)
        && invocationExpression.Parent?.Kind() is SyntaxKind.EqualsValue or SyntaxKind.SimpleAssignmentStatement or SyntaxKind.ReturnStatement;

    private protected override bool IsInsecureEnvironmentVariableRetrieval(InvocationExpressionSyntax invocation, KnownType type, string methodName, SemanticModel model) =>
        invocation.IsMethodInvocation(type, methodName, model)
        && invocation.ArgumentList?.Arguments.First() is var firstArgument
        && InsecureEnvironmentVariables.Any(x => x.Equals(firstArgument.GetExpression().StringValue(model), StringComparison.OrdinalIgnoreCase));
}
