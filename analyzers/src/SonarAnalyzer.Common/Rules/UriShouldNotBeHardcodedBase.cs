/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using System.Text.RegularExpressions;

namespace SonarAnalyzer.Rules
{
    public abstract class UriShouldNotBeHardcodedBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S1075";
        protected const string MessageFormat = "{0}";

        protected const string AbsoluteUriMessage = "Refactor your code not to use hardcoded absolute paths or URIs.";
        protected const string PathDelimiterMessage = "Remove this hardcoded path-delimiter.";

        // Simplified implementation of specification listed on
        // https://en.wikipedia.org/wiki/Uniform_Resource_Identifier
        private const string UriScheme = "^[a-zA-Z][a-zA-Z\\+\\.\\-]+://.+";

        private const string AbsoluteDiskUri = @"^[A-Za-z]:(/|\\)";
        private const string AbsoluteMappedDiskUri = @"^\\\\\w[ \w\.]*";

        protected static readonly Regex UriRegex = new($"{UriScheme}|{AbsoluteDiskUri}|{AbsoluteMappedDiskUri}", RegexOptions.Compiled);

        protected static readonly Regex PathDelimiterRegex = new(@"^(\\|/)$", RegexOptions.Compiled);

        protected static readonly ISet<string> CheckedVariableNames =
            new HashSet<string>
            {
                "FILE",
                "PATH",
                "URI",
                "URL",
                "URN",
                "STREAM"
            };
    }

    public abstract class UriShouldNotBeHardcodedBase<TExpressionSyntax,
        TLiteralExpressionSyntax, TLanguageKindEnum, TBinaryExpressionSyntax, TArgumentSyntax, TVariableDeclaratorSyntax>
        : UriShouldNotBeHardcodedBase
        where TExpressionSyntax : SyntaxNode
        where TLiteralExpressionSyntax : TExpressionSyntax
        where TBinaryExpressionSyntax : TExpressionSyntax
        where TArgumentSyntax : SyntaxNode
        where TVariableDeclaratorSyntax : SyntaxNode
        where TLanguageKindEnum : struct
    {
        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
        protected abstract TLanguageKindEnum StringLiteralSyntaxKind { get; }
        protected abstract TLanguageKindEnum[] StringConcatenateExpressions { get; }

        protected abstract string GetLiteralText(TLiteralExpressionSyntax literalExpression);

        protected abstract string GetDeclaratorIdentifierName(TVariableDeclaratorSyntax declarator);

        protected abstract TExpressionSyntax GetLeftNode(TBinaryExpressionSyntax binaryExpression);

        protected abstract TExpressionSyntax GetRightNode(TBinaryExpressionSyntax binaryExpression);

        protected abstract bool IsInvocationOrObjectCreation(SyntaxNode node);

        protected abstract int? GetArgumentIndex(TArgumentSyntax argument);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                GeneratedCodeRecognizer,
                c =>
                {
                    var stringLiteral = (TLiteralExpressionSyntax)c.Node;
                    if (UriRegex.IsMatch(GetLiteralText(stringLiteral))
                        && IsInCheckedContext(stringLiteral, c.SemanticModel))
                    {
                        c.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], stringLiteral.GetLocation(), AbsoluteUriMessage));
                    }
                },
                StringLiteralSyntaxKind);

            context.RegisterNodeAction(
                GeneratedCodeRecognizer,
                c =>
                {
                    var addExpression = (TBinaryExpressionSyntax)c.Node;
                    var isInCheckedContext = new Lazy<bool>(() => IsInCheckedContext(addExpression, c.SemanticModel));

                    var leftNode = GetLeftNode(addExpression);
                    if (IsPathDelimiter(leftNode) && isInCheckedContext.Value)
                    {
                        c.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], leftNode.GetLocation(), PathDelimiterMessage));
                    }

                    var rightNode = GetRightNode(addExpression);
                    if (IsPathDelimiter(rightNode) && isInCheckedContext.Value)
                    {
                        c.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], rightNode.GetLocation(), PathDelimiterMessage));
                    }
                },
                StringConcatenateExpressions);
        }

        private bool IsInCheckedContext(TExpressionSyntax expression, SemanticModel model)
        {
            var argument = expression.FirstAncestorOrSelf<TArgumentSyntax>();
            if (argument != null)
            {
                var argumentIndex = GetArgumentIndex(argument);
                if (argumentIndex is null or < 0)
                {
                    return false;
                }

                var constructorOrMethod = argument.Ancestors().FirstOrDefault(IsInvocationOrObjectCreation);
                var methodSymbol = constructorOrMethod != null
                    ? model.GetSymbolInfo(constructorOrMethod).Symbol as IMethodSymbol
                    : null;

                return methodSymbol != null
                       && argumentIndex.Value < methodSymbol.Parameters.Length
                       && methodSymbol.Parameters[argumentIndex.Value].Name.SplitCamelCaseToWords().Any(CheckedVariableNames.Contains);
            }

            var variableDeclarator = expression.FirstAncestorOrSelf<TVariableDeclaratorSyntax>();
            return variableDeclarator != null
                   && GetDeclaratorIdentifierName(variableDeclarator)
                        .SplitCamelCaseToWords()
                        .Any(name => CheckedVariableNames.Contains(name));
        }

        private bool IsPathDelimiter(TExpressionSyntax expression) =>
            GetLiteralText(expression as TLiteralExpressionSyntax) is { } text
            && PathDelimiterRegex.IsMatch(text);
    }
}
