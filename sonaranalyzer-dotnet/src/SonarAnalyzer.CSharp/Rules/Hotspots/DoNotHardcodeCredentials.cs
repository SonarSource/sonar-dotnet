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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DoNotHardcodeCredentials : DoNotHardcodeCredentialsBase<SyntaxKind>
    {
        public DoNotHardcodeCredentials() : this(AnalyzerConfiguration.Hotspot) { }

        internal /*for testing*/ DoNotHardcodeCredentials(IAnalyzerConfiguration analyzerConfiguration)
            : base(RspecStrings.ResourceManager, analyzerConfiguration)
        {
            ObjectCreationTracker = new CSharpObjectCreationTracker(analyzerConfiguration, rule);
            PropertyAccessTracker = new CSharpPropertyAccessTracker(analyzerConfiguration, rule);
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
                        SyntaxKind.SimpleAssignmentExpression);

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
                syntaxNode.Identifier.ValueText;

            protected override bool IsAssignedWithStringLiteral(VariableDeclaratorSyntax syntaxNode, SemanticModel semanticModel) =>
                syntaxNode.Initializer?.Value is LiteralExpressionSyntax literalExpression &&
                literalExpression.IsKind(SyntaxKind.StringLiteralExpression) &&
                syntaxNode.IsDeclarationKnownType(KnownType.System_String, semanticModel);
        }

        private class AssignmentExpressionBannedWordsFinder : CredentialWordsFinderBase<AssignmentExpressionSyntax>
        {
            public AssignmentExpressionBannedWordsFinder(DoNotHardcodeCredentialsBase<SyntaxKind> analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(AssignmentExpressionSyntax syntaxNode) =>
                syntaxNode.Right.GetStringValue();

            protected override string GetVariableName(AssignmentExpressionSyntax syntaxNode) =>
                (syntaxNode.Left as IdentifierNameSyntax)?.Identifier.ValueText;

            protected override bool IsAssignedWithStringLiteral(AssignmentExpressionSyntax syntaxNode, SemanticModel semanticModel) =>
                syntaxNode.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
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

            private static bool ShouldHandle(SyntaxNode method, SyntaxNode current)
            {
                // We don't want to handle VariableDeclarator and SimpleAssignmentExpression,
                // they are implemented by other finders with better and more precise logic.
                while (current != null && current != method)
                {
                    switch (current.Kind())
                    {
                        case SyntaxKind.VariableDeclarator:
                        case SyntaxKind.SimpleAssignmentExpression:
                            return false;

                        // Direct return from nested syntaxes that must be handled by this finder
                        // before search reaches top level VariableDeclarator or SimpleAssignmentExpression.
                        case SyntaxKind.InvocationExpression:
                        case SyntaxKind.Argument:
                        case SyntaxKind.AddExpression: // String concatenation is not supported by other finders
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
