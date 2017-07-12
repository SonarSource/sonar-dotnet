/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
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
    public sealed class UriShouldNotBeHardcoded : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1075";
        private const string MessageFormat = "{0}";
        private const string AbsoluteUriMessage = "Refactor your code to get this URI from a customizable parameter.";
        private const string PathDelimiterMessage = "Remove this hard-coded path-delimiter.";

        // Simplified implementation of specification listed on
        // https://en.wikipedia.org/wiki/Uniform_Resource_Identifier
        private const string UriScheme = "^[a-zA-Z][a-zA-Z\\+\\.\\-]+://.+";
        private const string AbsoluteDiskUri = @"^[A-Za-z]:(/|\\)";
        private const string AbsoluteMappedDiskUri = @"^\\\\\w[ \w\.]*";
        private const string AbsoluteUnixUri = @"^(~\\|~/|/)\w";

        private static readonly Regex UriRegex =
            new Regex($"{UriScheme}|{AbsoluteDiskUri}|{AbsoluteMappedDiskUri}|{AbsoluteUnixUri}",
                RegexOptions.Compiled);

        private static readonly Regex PathDelimiterRegex = new Regex(@"^(\\|/)$", RegexOptions.Compiled);

        private static readonly ISet<string> checkedVariableNames = new HashSet<string>
        {
            "file",
            "path",
            "uri",
            "url",
            "urn",
            "stream"
        };

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var stringLiteral = (LiteralExpressionSyntax)c.Node;
                    if (IsInCheckedContext(stringLiteral, c.SemanticModel) &&
                        UriRegex.IsMatch(stringLiteral.Token.ValueText))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule, stringLiteral.GetLocation(), AbsoluteUriMessage));
                    }
                }, SyntaxKind.StringLiteralExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var addExpression = (BinaryExpressionSyntax)c.Node;
                    if (!IsInCheckedContext(addExpression, c.SemanticModel))
                    {
                        return;
                    }

                    if (IsPathDelimiter(addExpression.Left))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule, addExpression.Left.GetLocation(),
                            PathDelimiterMessage));
                    }

                    if (IsPathDelimiter(addExpression.Right))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule, addExpression.Right.GetLocation(),
                            PathDelimiterMessage));
                    }
                }, SyntaxKind.AddExpression);
        }

        private static bool IsInCheckedContext(ExpressionSyntax expression, SemanticModel model)
        {
            var argument = expression.FirstAncestorOrSelf<ArgumentSyntax>();
            if (argument != null)
            {
                var argumentIndex = (argument.Parent as ArgumentListSyntax)?.Arguments.IndexOf(argument);
                if (argumentIndex == null ||
                    argumentIndex < 0)
                {
                    return false;
                }

                var constructorOrMethod = argument.Ancestors()
                    .FirstOrDefault(ancestor => ancestor.IsKind(SyntaxKind.InvocationExpression) ||
                                                ancestor.IsKind(SyntaxKind.ObjectCreationExpression));
                var methodSymbol = constructorOrMethod != null
                    ? model.GetSymbolInfo(constructorOrMethod).Symbol as IMethodSymbol
                    : null;

                return (methodSymbol != null && argumentIndex.Value < methodSymbol.Parameters.Length)
                    ? methodSymbol.Parameters[argumentIndex.Value].Name.SplitCamelCaseToWords()
                        .Any(name => checkedVariableNames.Contains(name))
                    : false;
            }

            var variableDeclarator = expression.FirstAncestorOrSelf<VariableDeclaratorSyntax>();

            return variableDeclarator != null
                ? variableDeclarator.Identifier.ValueText
                    .SplitCamelCaseToWords()
                    .Any(name => checkedVariableNames.Contains(name))
                : false;
        }

        private static bool IsPathDelimiter(ExpressionSyntax expression)
        {
            return expression.IsKind(SyntaxKind.StringLiteralExpression) &&
                PathDelimiterRegex.IsMatch(((LiteralExpressionSyntax)expression).Token.ValueText);
        }
    }
}