/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DoNotHardcodeCredentials : DoNotHardcodeCredentialsBase
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                isEnabledByDefault: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(rule);

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                new VariableDeclarationBannedWordsFinder(this).GetAnalysisAction(rule),
                SyntaxKind.VariableDeclarator);

            context.RegisterSyntaxNodeActionInNonGenerated(
                new AssignmentExpressionBannedWordsFinder(this).GetAnalysisAction(rule),
                SyntaxKind.SimpleAssignmentExpression);
        }

        private class VariableDeclarationBannedWordsFinder : CredentialWordsFinderBase<VariableDeclaratorSyntax>
        {
            public VariableDeclarationBannedWordsFinder(DoNotHardcodeCredentialsBase analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(VariableDeclaratorSyntax syntaxNode) =>
                syntaxNode.Initializer?.Value.ToString();

            protected override string GetVariableName(VariableDeclaratorSyntax syntaxNode) =>
                syntaxNode.Identifier.ValueText;

            protected override bool IsAssignedWithStringLiteral(VariableDeclaratorSyntax syntaxNode, SemanticModel semanticModel) =>
                (syntaxNode.Initializer?.Value is LiteralExpressionSyntax literalExpression) &&
                literalExpression.IsKind(SyntaxKind.StringLiteralExpression) &&
                syntaxNode.IsDeclarationKnownType(KnownType.System_String, semanticModel);
        }

        private class AssignmentExpressionBannedWordsFinder : CredentialWordsFinderBase<AssignmentExpressionSyntax>
        {
            public AssignmentExpressionBannedWordsFinder(DoNotHardcodeCredentialsBase analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(AssignmentExpressionSyntax syntaxNode) =>
                (syntaxNode.Right as LiteralExpressionSyntax)?.Token.ValueText;

            protected override string GetVariableName(AssignmentExpressionSyntax syntaxNode) =>
                (syntaxNode.Left as IdentifierNameSyntax)?.Identifier.ValueText;

            protected override bool IsAssignedWithStringLiteral(AssignmentExpressionSyntax syntaxNode, SemanticModel semanticModel) =>
                syntaxNode.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
                syntaxNode.Left.IsKnownType(KnownType.System_String, semanticModel) &&
                syntaxNode.Right.IsKind(SyntaxKind.StringLiteralExpression);
        }
    }
}
