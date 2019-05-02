/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
                SyntaxKind.SimpleAssignmentStatement);
        }

        private class VariableDeclarationBannedWordsFinder : CredentialWordsFinderBase<VariableDeclaratorSyntax>
        {
            public VariableDeclarationBannedWordsFinder(DoNotHardcodeCredentialsBase analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(VariableDeclaratorSyntax syntaxNode) =>
                syntaxNode.Initializer?.Value.GetStringValue();


            protected override string GetVariableName(VariableDeclaratorSyntax syntaxNode) =>
                syntaxNode.Names[0].Identifier.ValueText; // We already tested the count in IsAssignedWithStringLiteral

            protected override bool IsAssignedWithStringLiteral(VariableDeclaratorSyntax syntaxNode,
                SemanticModel semanticModel) =>
                syntaxNode.Names.Count == 1 &&
                (syntaxNode.Initializer?.Value is LiteralExpressionSyntax literalExpression) &&
                literalExpression.IsKind(SyntaxKind.StringLiteralExpression) &&
                syntaxNode.Names[0].IsDeclarationKnownType(KnownType.System_String, semanticModel);
        }

        private class AssignmentExpressionBannedWordsFinder : CredentialWordsFinderBase<AssignmentStatementSyntax>
        {
            public AssignmentExpressionBannedWordsFinder(DoNotHardcodeCredentialsBase analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(AssignmentStatementSyntax syntaxNode) =>
                syntaxNode.Right.GetStringValue();

            protected override string GetVariableName(AssignmentStatementSyntax syntaxNode) =>
                (syntaxNode.Left as IdentifierNameSyntax)?.Identifier.ValueText;

            protected override bool IsAssignedWithStringLiteral(AssignmentStatementSyntax syntaxNode,
                SemanticModel semanticModel) =>
                syntaxNode.IsKind(SyntaxKind.SimpleAssignmentStatement) &&
                syntaxNode.Left.IsKnownType(KnownType.System_String, semanticModel) &&
                syntaxNode.Right.IsKind(SyntaxKind.StringLiteralExpression);
        }
    }
}
