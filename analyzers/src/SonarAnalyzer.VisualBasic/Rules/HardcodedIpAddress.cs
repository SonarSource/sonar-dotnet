/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class HardcodedIpAddress : HardcodedIpAddressBase<SyntaxKind, LiteralExpressionSyntax>
    {
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } = VisualBasicGeneratedCodeRecognizer.Instance;

        protected override SyntaxKind SyntaxKind { get; } = SyntaxKind.StringLiteralExpression;

        public HardcodedIpAddress()
            : this(AnalyzerConfiguration.Hotspot)
        {
        }

        public HardcodedIpAddress(IAnalyzerConfiguration analyzerConfiguration)
            : base(analyzerConfiguration, RspecStrings.ResourceManager)
        {
        }

        protected override string GetValueText(LiteralExpressionSyntax literalExpression) =>
            literalExpression.Token.ValueText;

        protected override bool HasAttributes(LiteralExpressionSyntax literalExpression) =>
            literalExpression.Ancestors().AnyOfKind(SyntaxKind.Attribute);

        protected override string GetAssignedVariableName(LiteralExpressionSyntax stringLiteral) =>
            stringLiteral.FirstAncestorOrSelf<SyntaxNode>(IsVariableIdentifier)?.ToString();

        private static bool IsVariableIdentifier(SyntaxNode syntaxNode) =>
            syntaxNode is StatementSyntax
            || syntaxNode is VariableDeclaratorSyntax
            || syntaxNode is ParameterSyntax;
    }
}
