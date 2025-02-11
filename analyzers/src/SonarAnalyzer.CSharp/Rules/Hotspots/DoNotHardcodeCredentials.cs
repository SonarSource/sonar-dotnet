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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotHardcodeCredentials : DoNotHardcodeCredentialsBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        public DoNotHardcodeCredentials() : this(AnalyzerConfiguration.Hotspot) { }

        internal /*for testing*/ DoNotHardcodeCredentials(IAnalyzerConfiguration configuration) : base(configuration) { }

        protected override void InitializeActions(SonarParametrizedAnalysisContext context) =>
            context.RegisterCompilationStartAction(
                c =>
                {
                    if (!IsEnabled(c.Options))
                    {
                        return;
                    }

                    c.RegisterNodeAction(
                        new VariableDeclarationBannedWordsFinder(this).AnalysisAction(),
                        SyntaxKind.VariableDeclarator);

                    c.RegisterNodeAction(
                        new AssignmentExpressionBannedWordsFinder(this).AnalysisAction(),
                        SyntaxKind.SimpleAssignmentExpression);

                    c.RegisterNodeAction(
                        new StringLiteralBannedWordsFinder(this).AnalysisAction(),
                        SyntaxKind.StringLiteralExpression);

                    c.RegisterNodeAction(
                        new AddExpressionBannedWordsFinder(this).AnalysisAction(),
                        SyntaxKind.AddExpression);

                    c.RegisterNodeAction(
                        new InterpolatedStringBannedWordsFinder(this).AnalysisAction(),
                        SyntaxKind.InterpolatedStringExpression);

                    c.RegisterNodeAction(
                        new InvocationBannedWordsFinder(this).AnalysisAction(),
                        SyntaxKind.InvocationExpression);
                });

        protected override bool IsSecureStringAppendCharFromConstant(SyntaxNode argumentNode, SemanticModel model) =>
            argumentNode is ArgumentSyntax { Expression: { } argumentExpression }
            && argumentExpression switch
            {
                ElementAccessExpressionSyntax { Expression: { } accessed } => accessed.FindConstantValue(model) is string, // AppendChar("AP@ssw0rd"[i])
                LiteralExpressionSyntax { RawKind: (int)SyntaxKind.CharacterLiteralExpression } => true, // AppendChar('P')
                IdentifierNameSyntax identifier when model.GetSymbolInfo(identifier) is { Symbol: ILocalSymbol { } local } // foreach (var c in someConstString) AppendChar(c)
                    && local.DeclaringSyntaxReferences.Length == 1
                    && local.DeclaringSyntaxReferences[0].GetSyntax() is ForEachStatementSyntax { Expression: { } forEachExpression }
                    && forEachExpression.FindConstantValue(model) is string => true,
                _ => false,
            };

        private sealed class VariableDeclarationBannedWordsFinder : CredentialWordsFinderBase<VariableDeclaratorSyntax>
        {
            public VariableDeclarationBannedWordsFinder(DoNotHardcodeCredentials analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(VariableDeclaratorSyntax syntaxNode, SemanticModel semanticModel) =>
                FindStringLiteralInVariableDeclaration(syntaxNode.Initializer.Value)?.StringValue(semanticModel);

            protected override string GetVariableName(VariableDeclaratorSyntax syntaxNode) =>
                syntaxNode.Identifier.ValueText;

            protected override bool ShouldHandle(VariableDeclaratorSyntax syntaxNode, SemanticModel semanticModel) =>
                FindStringLiteralInVariableDeclaration(syntaxNode.Initializer?.Value) is { } literalExpression
                && (syntaxNode.IsDeclarationKnownType(KnownType.System_String, semanticModel)
                    || syntaxNode.IsDeclarationKnownType(KnownType.System_ReadOnlySpan_T, semanticModel) // "utf8"u8
                    || syntaxNode.IsDeclarationKnownType(KnownType.System_Byte_Array, semanticModel));   // "utf8"u8.ToArray()

            private static LiteralExpressionSyntax FindStringLiteralInVariableDeclaration(ExpressionSyntax expression) =>
                expression switch
                {
                    LiteralExpressionSyntax literal => literal,
                    InvocationExpressionSyntax
                    {
                        Expression: MemberAccessExpressionSyntax
                        {
                            Expression: LiteralExpressionSyntax { RawKind: (int)SyntaxKindEx.Utf8StringLiteralExpression } literal
                        }
                    } invocation when invocation.NameIs("ToArray") => literal, // "utf8"u8.ToArray()
                    _ => null
                };
        }

        private sealed class AssignmentExpressionBannedWordsFinder : CredentialWordsFinderBase<AssignmentExpressionSyntax>
        {
            public AssignmentExpressionBannedWordsFinder(DoNotHardcodeCredentials analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(AssignmentExpressionSyntax syntaxNode, SemanticModel semanticModel) =>
                syntaxNode.Right.StringValue(semanticModel);

            protected override string GetVariableName(AssignmentExpressionSyntax syntaxNode) =>
                (syntaxNode.Left as IdentifierNameSyntax)?.Identifier.ValueText;

            protected override bool ShouldHandle(AssignmentExpressionSyntax syntaxNode, SemanticModel semanticModel) =>
                syntaxNode.IsKind(SyntaxKind.SimpleAssignmentExpression)
                && syntaxNode.Left.IsKnownType(KnownType.System_String, semanticModel)
                && syntaxNode.Right.IsKind(SyntaxKind.StringLiteralExpression);
        }

        /// <summary>
        /// This finder checks all string literal in the code, except VariableDeclarator, SimpleAssignmentExpression and string.Format invocation.
        /// These two have their own finders with precise logic and variable name checking.
        /// This class inspects all other standalone string literals for values considered as hardcoded passwords (in connection strings)
        /// based on same rules as in VariableDeclarationBannedWordsFinder and AssignmentExpressionBannedWordsFinder.
        /// </summary>
        private sealed class StringLiteralBannedWordsFinder : CredentialWordsFinderBase<LiteralExpressionSyntax>
        {
            public StringLiteralBannedWordsFinder(DoNotHardcodeCredentials analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(LiteralExpressionSyntax syntaxNode, SemanticModel semanticModel) =>
                syntaxNode.StringValue(semanticModel);

            // We don't have a variable for cases that this finder should handle.  Cases with variable name are
            // handled by VariableDeclarationBannedWordsFinder and AssignmentExpressionBannedWordsFinder
            // Returning null is safe here, it will not be considered as a value.
            protected override string GetVariableName(LiteralExpressionSyntax syntaxNode) =>
                null;

            protected override bool ShouldHandle(LiteralExpressionSyntax syntaxNode, SemanticModel semanticModel) =>
                syntaxNode.IsKind(SyntaxKind.StringLiteralExpression) && ShouldHandle(syntaxNode.GetTopMostContainingMethod(), syntaxNode, semanticModel);

            private static bool ShouldHandle(SyntaxNode method, SyntaxNode current, SemanticModel semanticModel)
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
                        case SyntaxKind.AddExpression: // String concatenation is not supported by other finders
                            return true;

                        // Handle all arguments except those inside string.Format. InvocationBannedWordsFinder takes care of them.
                        case SyntaxKind.Argument:
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

        private sealed class AddExpressionBannedWordsFinder : CredentialWordsFinderBase<BinaryExpressionSyntax>
        {
            public AddExpressionBannedWordsFinder(DoNotHardcodeCredentials analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(BinaryExpressionSyntax syntaxNode, SemanticModel semanticModel)
            {
                var left = syntaxNode.Left is BinaryExpressionSyntax precedingAdd && precedingAdd.IsKind(SyntaxKind.AddExpression) ? precedingAdd.Right : syntaxNode.Left;
                return left.FindStringConstant(semanticModel) is { } leftString
                    && syntaxNode.Right.FindStringConstant(semanticModel) is { } rightString
                    ? leftString + rightString
                    : null;
            }

            protected override string GetVariableName(BinaryExpressionSyntax syntaxNode) => null;

            protected override bool ShouldHandle(BinaryExpressionSyntax syntaxNode, SemanticModel semanticModel) => true;
        }

        private sealed class InterpolatedStringBannedWordsFinder : CredentialWordsFinderBase<InterpolatedStringExpressionSyntax>
        {
            public InterpolatedStringBannedWordsFinder(DoNotHardcodeCredentials analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(InterpolatedStringExpressionSyntax syntaxNode, SemanticModel semanticModel) =>
                syntaxNode.Contents.JoinStr(null, x => x switch
                {
                    InterpolationSyntax interpolation => interpolation.Expression.FindStringConstant(semanticModel),
                    InterpolatedStringTextSyntax text => text.TextToken.ToString(),
                    _ => null
                } ?? KeywordSeparator.ToString()); // Unknown elements resolved to separator to terminate the keyword-value sequence

            protected override string GetVariableName(InterpolatedStringExpressionSyntax syntaxNode) => null;

            protected override bool ShouldHandle(InterpolatedStringExpressionSyntax syntaxNode, SemanticModel semanticModel) => true;
        }

        private sealed class InvocationBannedWordsFinder : CredentialWordsFinderBase<InvocationExpressionSyntax>
        {
            public InvocationBannedWordsFinder(DoNotHardcodeCredentials analyzer) : base(analyzer) { }

            protected override string GetAssignedValue(InvocationExpressionSyntax syntaxNode, SemanticModel semanticModel)
            {
                var allArgs = syntaxNode.ArgumentList.Arguments.Select(x => x.Expression.FindStringConstant(semanticModel) ?? KeywordSeparator.ToString());
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
