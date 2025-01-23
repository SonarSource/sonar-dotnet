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

using System.Text.RegularExpressions;
using SonarAnalyzer.CFG.Extensions;

namespace SonarAnalyzer.Rules
{
    public abstract class UriShouldNotBeHardcodedBase<TSyntaxKind, TLiteralExpressionSyntax, TArgumentSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
        where TLiteralExpressionSyntax : SyntaxNode
        where TArgumentSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S1075";

        protected const string AbsoluteUriMessage = "Refactor your code not to use hardcoded absolute paths or URIs.";
        protected const string PathDelimiterMessage = "Remove this hardcoded path-delimiter.";

        // Simplified implementation of specification listed on
        // https://en.wikipedia.org/wiki/Uniform_Resource_Identifier
        private const string UriScheme = "^[a-zA-Z][a-zA-Z\\+\\.\\-]+://.+";

        private const string AbsoluteDiskUri = @"^[A-Za-z]:(/|\\)";
        private const string AbsoluteMappedDiskUri = @"^\\\\\w[ \w\.]*";

        protected static readonly Regex UriRegex = new($"{UriScheme}|{AbsoluteDiskUri}|{AbsoluteMappedDiskUri}", RegexOptions.Compiled, RegexConstants.DefaultTimeout);
        protected static readonly Regex PathDelimiterRegex = new(@"^(\\|/)$", RegexOptions.Compiled, RegexConstants.DefaultTimeout);

        protected static readonly ISet<string> CheckedVariableNames =
            new HashSet<string>
            {
                "FILE",
                "FILES",
                "PATH",
                "PATHS",
                "URI",
                "URIS",
                "URL",
                "URLS",
                "URN",
                "URNS",
                "STREAM",
                "STREAMS"
            };

        protected override string MessageFormat => "{0}";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
        protected abstract TSyntaxKind[] StringConcatenateExpressions { get; }
        protected abstract TSyntaxKind[] InvocationOrObjectCreationKind { get; }

        protected abstract SyntaxNode GetRelevantAncestor(SyntaxNode node);

        protected UriShouldNotBeHardcodedBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                GeneratedCodeRecognizer,
                c =>
                {
                    if (UriRegex.SafeIsMatch(Language.Syntax.LiteralText(c.Node)) && IsInCheckedContext(c.Node, c.Model))
                    {
                        c.ReportIssue(SupportedDiagnostics[0], c.Node, AbsoluteUriMessage);
                    }
                },
                Language.SyntaxKind.StringLiteralExpressions);

            context.RegisterNodeAction(
                GeneratedCodeRecognizer,
                c =>
                {
                    var isInCheckedContext = new Lazy<bool>(() => IsInCheckedContext(c.Node, c.Model));

                    var leftNode = Language.Syntax.BinaryExpressionLeft(c.Node);
                    if (IsPathDelimiter(leftNode) && isInCheckedContext.Value)
                    {
                        c.ReportIssue(SupportedDiagnostics[0], leftNode, PathDelimiterMessage);
                    }

                    var rightNode = Language.Syntax.BinaryExpressionRight(c.Node);
                    if (IsPathDelimiter(rightNode) && isInCheckedContext.Value)
                    {
                        c.ReportIssue(SupportedDiagnostics[0], rightNode, PathDelimiterMessage);
                    }
                },
                StringConcatenateExpressions);
        }

        private bool IsInCheckedContext(SyntaxNode expression, SemanticModel model)
        {
            var argument = expression.FirstAncestorOrSelf<TArgumentSyntax>();
            if (argument != null)
            {
                var argumentIndex = Language.Syntax.ArgumentIndex(argument);
                if (argumentIndex is null or < 0)
                {
                    return false;
                }

                var constructorOrMethod = argument.Ancestors().FirstOrDefault(x => Language.Syntax.IsAnyKind(x, InvocationOrObjectCreationKind));
                var methodSymbol = constructorOrMethod != null
                    ? model.GetSymbolInfo(constructorOrMethod).Symbol as IMethodSymbol
                    : null;

                return methodSymbol != null
                       && argumentIndex.Value < methodSymbol.Parameters.Length
                       && methodSymbol.Parameters[argumentIndex.Value].Name.SplitCamelCaseToWords().Any(CheckedVariableNames.Contains);
            }

            return GetRelevantAncestor(expression) is { } relevantAncestor && Language.GetName(relevantAncestor).SplitCamelCaseToWords().Any(CheckedVariableNames.Contains);
        }

        private bool IsPathDelimiter(SyntaxNode expression) =>
            expression is TLiteralExpressionSyntax && PathDelimiterRegex.SafeIsMatch(Language.Syntax.LiteralText(expression));
    }
}
