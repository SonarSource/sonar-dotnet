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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UriShouldNotBeHardcoded : UriShouldNotBeHardcodedBase<SyntaxKind, LiteralExpressionSyntax, ArgumentSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
    protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => CSharpGeneratedCodeRecognizer.Instance;
    protected override SyntaxKind[] StringConcatenateExpressions => [SyntaxKind.AddExpression];
    protected override SyntaxKind[] InvocationOrObjectCreationKind => [SyntaxKind.InvocationExpression, SyntaxKind.ObjectCreationExpression];

    protected override SyntaxNode GetRelevantAncestor(SyntaxNode node) =>
        node switch
        {
            _ when node.FirstAncestorOrSelf<AssignmentExpressionSyntax>() is { } propertyAssignment => propertyAssignment.Left,
            _ when node.FirstAncestorOrSelf<ParameterSyntax>() is { } parameterSyntax => parameterSyntax,
            _ when node.FirstAncestorOrSelf<VariableDeclaratorSyntax>() is { } variableDeclaratorSyntax => variableDeclaratorSyntax,
            _ => null
        };
}
