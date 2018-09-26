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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class DoNotHardcodeCredentials : DoNotHardcodeCredentialsBase
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

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

        private class VariableDeclarationBannedWordsFinder : BannedWordsFinderBase<VariableDeclaratorSyntax>
        {
            public VariableDeclarationBannedWordsFinder(DoNotHardcodeCredentialsBase analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(VariableDeclaratorSyntax declarator) =>
                declarator.Initializer?.Value.ToString();

            protected override string GetVariableName(VariableDeclaratorSyntax declarator) =>
                declarator.Names[0].Identifier.ValueText; // We already tested the count in IsAssignedWithStringLiteral

            protected override bool IsAssignedWithStringLiteral(VariableDeclaratorSyntax declarator,
                SemanticModel semanticModel) =>
                declarator.Names.Count == 1 &&
                (declarator.Initializer?.Value is LiteralExpressionSyntax literalExpression) &&
                literalExpression.IsKind(SyntaxKind.StringLiteralExpression) &&
                declarator.Names[0].IsDeclarationKnownType(KnownType.System_String, semanticModel);
        }

        private class AssignmentExpressionBannedWordsFinder : BannedWordsFinderBase<AssignmentStatementSyntax>
        {
            public AssignmentExpressionBannedWordsFinder(DoNotHardcodeCredentialsBase analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(AssignmentStatementSyntax assignment) =>
                (assignment.Right as LiteralExpressionSyntax)?.Token.ValueText;

            protected override string GetVariableName(AssignmentStatementSyntax assignment) =>
                (assignment.Left as IdentifierNameSyntax)?.Identifier.ValueText;

            protected override bool IsAssignedWithStringLiteral(AssignmentStatementSyntax assignment,
                SemanticModel semanticModel) =>
                assignment.IsKind(SyntaxKind.SimpleAssignmentStatement) &&
                assignment.Left.IsKnownType(KnownType.System_String, semanticModel) &&
                assignment.Right.IsKind(SyntaxKind.StringLiteralExpression);
        }
    }
}
