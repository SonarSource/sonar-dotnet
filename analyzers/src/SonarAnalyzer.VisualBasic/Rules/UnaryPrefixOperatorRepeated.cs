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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class UnaryPrefixOperatorRepeated :
        UnaryPrefixOperatorRepeatedBase<SyntaxKind, UnaryExpressionSyntax>
    {
        protected override ISet<SyntaxKind> SyntaxKinds { get; } = new HashSet<SyntaxKind>
        {
            SyntaxKind.NotExpression
        };

        protected override DiagnosticDescriptor Rule { get; } =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } =
            VisualBasicGeneratedCodeRecognizer.Instance;

        protected override SyntaxNode GetOperand(UnaryExpressionSyntax unarySyntax) =>
             unarySyntax.Operand;

        protected override SyntaxToken GetOperatorToken(UnaryExpressionSyntax unarySyntax) =>
            unarySyntax.OperatorToken;

        protected override bool SameOperators(UnaryExpressionSyntax expression1, UnaryExpressionSyntax expression2) =>
            expression1.OperatorToken.IsKind(expression2.OperatorToken.Kind());
    }
}
