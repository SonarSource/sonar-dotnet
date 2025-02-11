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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class InsecureEncryptionAlgorithm : InsecureEncryptionAlgorithmBase<SyntaxKind, InvocationExpressionSyntax, ArgumentListSyntax, ArgumentSyntax>
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override ArgumentListSyntax ArgumentList(InvocationExpressionSyntax invocationExpression) =>
            invocationExpression.ArgumentList;

        protected override SeparatedSyntaxList<ArgumentSyntax> Arguments(ArgumentListSyntax argumentList) =>
            argumentList.Arguments;

        protected override bool IsStringLiteralArgument(ArgumentSyntax argument) =>
            argument.Expression.IsKind(SyntaxKind.StringLiteralExpression);

        protected override SyntaxNode Expression(ArgumentSyntax argument) =>
            argument.Expression;

        protected override Location Location(SyntaxNode objectCreation) =>
            objectCreation is ObjectCreationExpressionSyntax objectCreationExpression
                ? objectCreationExpression.Type.GetLocation()
                : objectCreation.GetLocation();
    }
}
