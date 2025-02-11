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

namespace SonarAnalyzer.VisualBasic.Rules
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class HardcodedIpAddress : HardcodedIpAddressBase<SyntaxKind, LiteralExpressionSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        public HardcodedIpAddress() : this(AnalyzerConfiguration.Hotspot) { }

        public HardcodedIpAddress(IAnalyzerConfiguration analyzerConfiguration) : base(analyzerConfiguration) { }

        protected override bool HasAttributes(SyntaxNode literalExpression) =>
            literalExpression.HasAncestor(SyntaxKind.Attribute);

        protected override string GetAssignedVariableName(SyntaxNode stringLiteral) =>
            stringLiteral.FirstAncestorOrSelf<SyntaxNode>(IsVariableIdentifier)?.ToString();

        private static bool IsVariableIdentifier(SyntaxNode syntaxNode) =>
            syntaxNode is StatementSyntax
            || syntaxNode is VariableDeclaratorSyntax
            || syntaxNode is ParameterSyntax;
    }
}
