/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class DoNotHardcodeCredentials : DoNotHardcodeCredentialsBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        public DoNotHardcodeCredentials() : this(AnalyzerConfiguration.Hotspot) { }

        internal /*for testing*/ DoNotHardcodeCredentials(IAnalyzerConfiguration configuration) : base(configuration) { }

        protected override void InitializeActions(ParameterLoadingAnalysisContext context) =>
            context.RegisterCompilationStartAction(
                c =>
                {
                    if (!IsEnabled(c.Options))
                    {
                        return;
                    }

                    c.RegisterSyntaxNodeActionInNonGenerated(
                        new VariableDeclarationBannedWordsFinder(this).AnalysisAction(),
                        SyntaxKind.VariableDeclarator);

                    c.RegisterSyntaxNodeActionInNonGenerated(
                        new AssignmentExpressionBannedWordsFinder(this).AnalysisAction(),
                        SyntaxKind.SimpleAssignmentStatement);

                    c.RegisterSyntaxNodeActionInNonGenerated(
                        new StringLiteralBannedWordsFinder(this).AnalysisAction(),
                        SyntaxKind.StringLiteralExpression);

                    c.RegisterSyntaxNodeActionInNonGenerated(
                        new AddExpressionBannedWordsFinder(this).AnalysisAction(),
                        SyntaxKind.ConcatenateExpression,
                        SyntaxKind.AddExpression);

                    c.RegisterSyntaxNodeActionInNonGenerated(
                        new InterpolatedStringBannedWordsFinder(this).AnalysisAction(),
                        SyntaxKind.InterpolatedStringExpression);

                    c.RegisterSyntaxNodeActionInNonGenerated(
                        new InvocationBannedWordsFinder(this).AnalysisAction(),
                        SyntaxKind.InvocationExpression);
                });

        private class VariableDeclarationBannedWordsFinder : CredentialWordsFinderBase<VariableDeclaratorSyntax>
        {
            public VariableDeclarationBannedWordsFinder(DoNotHardcodeCredentialsBase<SyntaxKind> analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(VariableDeclaratorSyntax syntaxNode, SemanticModel semanticModel) =>
                syntaxNode.Initializer?.Value.GetStringValue();

            protected override string GetVariableName(VariableDeclaratorSyntax syntaxNode) =>
                syntaxNode.Names[0].Identifier.ValueText; // We already tested the count in IsAssignedWithStringLiteral

            protected override bool ShouldHandle(VariableDeclaratorSyntax syntaxNode, SemanticModel semanticModel) =>
                syntaxNode.Names.Count == 1
                && syntaxNode.Initializer?.Value is LiteralExpressionSyntax literalExpression
                && literalExpression.IsKind(SyntaxKind.StringLiteralExpression)
                && syntaxNode.Names[0].IsDeclarationKnownType(KnownType.System_String, semanticModel);
        }

        private class AssignmentExpressionBannedWordsFinder : CredentialWordsFinderBase<AssignmentStatementSyntax>
        {
            public AssignmentExpressionBannedWordsFinder(DoNotHardcodeCredentialsBase<SyntaxKind> analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(AssignmentStatementSyntax syntaxNode, SemanticModel semanticModel) =>
                syntaxNode.Right.GetStringValue();

            protected override string GetVariableName(AssignmentStatementSyntax syntaxNode) =>
                (syntaxNode.Left as IdentifierNameSyntax)?.Identifier.ValueText;

            protected override bool ShouldHandle(AssignmentStatementSyntax syntaxNode, SemanticModel semanticModel) =>
                syntaxNode.IsKind(SyntaxKind.SimpleAssignmentStatement)
                && syntaxNode.Left.IsKnownType(KnownType.System_String, semanticModel)
                && syntaxNode.Right.IsKind(SyntaxKind.StringLiteralExpression);
        }

        /// <summary>
        /// This finder checks all string literal in the code, except VariableDeclarator, SimpleAssignmentExpression and String.Format invocation.
        /// These two have their own finders with precise logic and variable name checking.
        /// This class inspects all other standalone string literals for values considered as hardcoded passwords (in connection strings)
        /// based on same rules as in VariableDeclarationBannedWordsFinder and AssignmentExpressionBannedWordsFinder.
        /// </summary>
        private class StringLiteralBannedWordsFinder : CredentialWordsFinderBase<LiteralExpressionSyntax>
        {
            public StringLiteralBannedWordsFinder(DoNotHardcodeCredentialsBase<SyntaxKind> analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(LiteralExpressionSyntax syntaxNode, SemanticModel semanticModel) =>
                syntaxNode.GetStringValue();

            // We don't have a variable for cases that this finder should handle.  Cases with variable name are
            // handled by VariableDeclarationBannedWordsFinder and AssignmentExpressionBannedWordsFinder
            // Returning null is safe here, it will not be considered as a value.
            protected override string GetVariableName(LiteralExpressionSyntax syntaxNode) =>
                null;

            protected override bool ShouldHandle(LiteralExpressionSyntax syntaxNode, SemanticModel semanticModel) =>
                syntaxNode.IsKind(SyntaxKind.StringLiteralExpression) && ShouldHandle(syntaxNode.GetTopMostContainingMethod(), syntaxNode, semanticModel);

            // We don't want to handle VariableDeclarator and SimpleAssignmentExpression,
            // they are implemented by other finders with better and more precise logic.
            private static bool ShouldHandle(SyntaxNode method, SyntaxNode current, SemanticModel semanticModel)
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
                        case SyntaxKind.AddExpression: // String concatenation is not supported by other finders
                        case SyntaxKind.ConcatenateExpression:
                            return true;

                        // Handle all arguments except those inside string.Format. InvocationBannedWordsFinder takes care of them.
                        case SyntaxKind.SimpleArgument:
                            return !(current.Parent.Parent is InvocationExpressionSyntax invocation && invocation.IsMethodInvocation(KnownType.System_String, "Format", semanticModel));

                        default:
                            current = current.Parent;
                            break;
                    }
                }
                // We want to handle all other literals (property initializers, return statement and return values from lambdas, arrow functions, ...)
                return true;
            }
        }

        private class AddExpressionBannedWordsFinder : CredentialWordsFinderBase<BinaryExpressionSyntax>
        {
            public AddExpressionBannedWordsFinder(DoNotHardcodeCredentialsBase<SyntaxKind> analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(BinaryExpressionSyntax syntaxNode, SemanticModel semanticModel)
            {
                var left = syntaxNode.Left is BinaryExpressionSyntax precedingConcat && precedingConcat.IsAnyKind(SyntaxKind.ConcatenateExpression, SyntaxKind.AddExpression)
                    ? precedingConcat.Right
                    : syntaxNode.Left;
                return left.FindStringConstant(semanticModel) is { } leftString
                    && syntaxNode.Right.FindStringConstant(semanticModel) is { } rightString
                    ? leftString + rightString
                    : null;
            }

            protected override string GetVariableName(BinaryExpressionSyntax syntaxNode) => null;

            protected override bool ShouldHandle(BinaryExpressionSyntax syntaxNode, SemanticModel semanticModel) => true;
        }

        private class InterpolatedStringBannedWordsFinder : CredentialWordsFinderBase<InterpolatedStringExpressionSyntax>
        {
            public InterpolatedStringBannedWordsFinder(DoNotHardcodeCredentialsBase<SyntaxKind> analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(InterpolatedStringExpressionSyntax syntaxNode, SemanticModel semanticModel) =>
                syntaxNode.Contents.JoinStr(null, x => x switch
                {
                    InterpolationSyntax interpolation => interpolation.Expression.FindStringConstant(semanticModel),
                    InterpolatedStringTextSyntax text => text.TextToken.ToString(),
                    _ => null
                } ?? CredentialSeparator.ToString()); // Unknown elements resolved to separator to terminate the keyword-value sequence

            protected override string GetVariableName(InterpolatedStringExpressionSyntax syntaxNode) => null;

            protected override bool ShouldHandle(InterpolatedStringExpressionSyntax syntaxNode, SemanticModel semanticModel) => true;
        }

        private class InvocationBannedWordsFinder : CredentialWordsFinderBase<InvocationExpressionSyntax>
        {
            public InvocationBannedWordsFinder(DoNotHardcodeCredentials analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(InvocationExpressionSyntax syntaxNode, SemanticModel semanticModel)
            {
                var allArgs = syntaxNode.ArgumentList.Arguments.Select(x => x.GetExpression().FindStringConstant(semanticModel) ?? CredentialSeparator.ToString());
                try
                {
                    return string.Format(allArgs.First(), allArgs.Skip(1).ToArray());
                }
                catch (FormatException)
                {
                    return null;
                }
            }

            protected override string GetVariableName(InvocationExpressionSyntax syntaxNode) => null;

            protected override bool ShouldHandle(InvocationExpressionSyntax syntaxNode, SemanticModel semanticModel) =>
                syntaxNode.IsMethodInvocation(KnownType.System_String, "Format", semanticModel)
                && semanticModel.GetSymbolInfo(syntaxNode).Symbol is IMethodSymbol symbol
                && symbol.Parameters.First().Type.Is(KnownType.System_String);
        }
    }
}
