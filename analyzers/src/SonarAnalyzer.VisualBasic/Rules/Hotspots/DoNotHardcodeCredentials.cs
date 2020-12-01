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
    public sealed class DoNotHardcodeCredentials : DoNotHardcodeCredentialsBase<SyntaxKind>
    {
        public DoNotHardcodeCredentials() : this(AnalyzerConfiguration.Hotspot) { }

        internal /*for testing*/ DoNotHardcodeCredentials(IAnalyzerConfiguration analyzerConfiguration)
            : base(RspecStrings.ResourceManager, analyzerConfiguration)
        {
            ObjectCreationTracker = new VisualBasicObjectCreationTracker(analyzerConfiguration, rule);
            PropertyAccessTracker = new VisualBasicPropertyAccessTracker(analyzerConfiguration, rule);
        }

        protected override void InitializeActions(ParameterLoadingAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    if (!IsEnabled(c.Options))
                    {
                        return;
                    }

                    c.RegisterSyntaxNodeActionInNonGenerated(
                        new VariableDeclarationBannedWordsFinder(this).GetAnalysisAction(rule),
                        SyntaxKind.VariableDeclarator);

                    c.RegisterSyntaxNodeActionInNonGenerated(
                        new AssignmentExpressionBannedWordsFinder(this).GetAnalysisAction(rule),
                        SyntaxKind.SimpleAssignmentStatement);

                    c.RegisterSyntaxNodeActionInNonGenerated(
                        new StringLiteralBannedWordsFinder(this).GetAnalysisAction(rule),
                        SyntaxKind.StringLiteralExpression);
                });
        }

        private class VariableDeclarationBannedWordsFinder : CredentialWordsFinderBase<VariableDeclaratorSyntax>
        {
            public VariableDeclarationBannedWordsFinder(DoNotHardcodeCredentialsBase<SyntaxKind> analyzer) : base(analyzer) { }

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
            public AssignmentExpressionBannedWordsFinder(DoNotHardcodeCredentialsBase<SyntaxKind> analyzer) : base(analyzer) { }

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

        /// <summary>
        /// This finder checks all string literal in the code, except VariableDeclarator and SimpleAssignmentExpression. These two have their own
        /// finders with precise logic and variable name checking.
        /// This class inspects all other standalone string literals for values considered as hardcoded passwords (in connection strings)
        /// based on same rules as in VariableDeclarationBannedWordsFinder and AssignmentExpressionBannedWordsFinder.
        /// </summary>
        private class StringLiteralBannedWordsFinder : CredentialWordsFinderBase<LiteralExpressionSyntax>
        {
            public StringLiteralBannedWordsFinder(DoNotHardcodeCredentialsBase<SyntaxKind> analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(LiteralExpressionSyntax syntaxNode) =>
                syntaxNode.GetStringValue();

            // We don't have a variable for cases that this finder should handle.  Cases with variable name are
            // handled by VariableDeclarationBannedWordsFinder and AssignmentExpressionBannedWordsFinder
            // Returning null is safe here, it will not be considered as a value.
            protected override string GetVariableName(LiteralExpressionSyntax syntaxNode) =>
                null;

            protected override bool IsAssignedWithStringLiteral(LiteralExpressionSyntax syntaxNode, SemanticModel semanticModel) =>
                syntaxNode.IsKind(SyntaxKind.StringLiteralExpression) && ShouldHandle(syntaxNode.GetTopMostContainingMethod(), syntaxNode);

            // We don't want to handle VariableDeclarator and SimpleAssignmentExpression,
            // they are implemented by other finders with better and more precise logic.
            private static bool ShouldHandle(SyntaxNode method, SyntaxNode current)
            {
                while (current != null && current != method)
                {
                    switch (current.Kind())
                    {
                        case SyntaxKind.VariableDeclarator:
                        case SyntaxKind.SimpleAssignmentStatement:
                            return false;

                        // Direct return from nested syntaxes that must be handled by this finder
                        // before search reaches top level VariableDeclarator or SimpleAssignmentExpression.
                        case SyntaxKind.InvocationExpression:
                        case SyntaxKind.SimpleArgument:
                        case SyntaxKind.AddExpression: // String concatenation is not supported by other finders
                        case SyntaxKind.ConcatenateExpression:
                            return true;

                        default:
                            current = current.Parent;
                            break;
                    }
                }
                // We want to handle all other literals (property initializers, return statement and return values from lambdas, arrow functions, ...)
                return true;
            }
        }
    }
}
