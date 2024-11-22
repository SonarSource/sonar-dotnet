/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
    public sealed class MethodsShouldNotHaveTooManyLines
        : MethodsShouldNotHaveTooManyLinesBase<SyntaxKind, MethodBlockBaseSyntax>
    {
        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer =>
            VisualBasicGeneratedCodeRecognizer.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } =
            new[]
            {
                SyntaxKind.ConstructorBlock,
                SyntaxKind.SubBlock,
                SyntaxKind.FunctionBlock
            };

        protected override string MethodKeyword { get; } = "procedures";

        protected override IEnumerable<SyntaxToken> GetMethodTokens(MethodBlockBaseSyntax baseMethodDeclaration) =>
            baseMethodDeclaration.Statements.SelectMany(s => s.DescendantTokens());

        protected override SyntaxToken? GetMethodIdentifierToken(MethodBlockBaseSyntax baseMethodDeclaration) =>
            baseMethodDeclaration.GetIdentifierOrDefault();

        protected override string GetMethodKindAndName(SyntaxToken identifierToken)
        {
            var declaration = identifierToken.Parent;
            if (declaration.IsKind(SyntaxKind.SubNewStatement))
            {
                return $"constructor";
            }

            var identifierName = identifierToken.ValueText;
            if (string.IsNullOrEmpty(identifierName))
            {
                return "procedure";
            }

            if (declaration.IsKind(SyntaxKind.FunctionStatement))
            {
                return $"function '{identifierName}'";
            }

            if (declaration.IsKind(SyntaxKind.SubStatement))
            {
                return identifierName == "Finalize"
                    ? "finalizer"
                    : $"method '{identifierName}'";
            }

            return "procedure";
        }
    }
}

