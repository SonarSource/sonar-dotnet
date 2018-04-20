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

using System;
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
    public sealed class DoNotHardcodeCredentials : ParameterLoadingDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2068";
        private const string MessageFormat = "Remove hard-coded password(s): '{0}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                isEnabledByDefault: false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private IEnumerable<string> splitCredentialWords;
        private string credentialWords;
        private Regex passwordValuePattern;

        private const string DefaultCredentialWords = "password, passwd, pwd";
        [RuleParameter("credentialWords", PropertyType.String, "Comma separated list of words identifying potential credentials",
            DefaultCredentialWords)]
        public string CredentialWords
        {
            get => credentialWords;
            set
            {
                credentialWords = value;
                splitCredentialWords = value.ToLowerInvariant()
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToList();
                passwordValuePattern = new Regex(string.Format(@"\b(?<password>{0})\b[:=]\S",
                    string.Join("|", splitCredentialWords)), RegexOptions.Compiled);
            }
        }

        public DoNotHardcodeCredentials()
        {
            CredentialWords = DefaultCredentialWords;
        }

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(VerifyDeclaration, SyntaxKind.VariableDeclaration);
            context.RegisterSyntaxNodeActionInNonGenerated(VerifyAssignment, SyntaxKind.SimpleAssignmentExpression);
        }

        private void VerifyAssignment(SyntaxNodeAnalysisContext context)
        {
            var assignment = context.Node as AssignmentExpressionSyntax;
            if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) ||
                !assignment.Left.IsKnownType(KnownType.System_String, context.SemanticModel) ||
                !assignment.Right.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return;
            }

            var variableName = (assignment.Left as IdentifierNameSyntax)?.Identifier.ValueText;
            var variableValue = (assignment.Right as LiteralExpressionSyntax)?.Token.ValueText;

            var bannedWords = FindBannedWords(variableName, variableValue);
            if (bannedWords != null)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, assignment.GetLocation(), bannedWords));
            }
        }

        private void VerifyDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = context.Node as VariableDeclarationSyntax;

            var stringTypeDeclarators = declaration.Variables
                .Where(v => v.IsDeclarationKnownType(KnownType.System_String, context.SemanticModel));

            foreach (var variableDeclarator in stringTypeDeclarators)
            {
                var bannedWords = FindBannedWords(variableDeclarator);
                if (bannedWords != null)
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, variableDeclarator.GetLocation(), bannedWords));
                }
            }
        }

        private string FindBannedWords(VariableDeclaratorSyntax variableDeclarator)
        {
            var variableName = variableDeclarator?.Identifier.ValueText;
            var literalExpression = variableDeclarator?.Initializer?.Value as LiteralExpressionSyntax;
            if (literalExpression == null ||
                !literalExpression.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return null;
            }

            return FindBannedWords(variableName, literalExpression.Token.ValueText);
        }

        private string FindBannedWords(string variableName, string variableValue)
        {
            if (string.IsNullOrWhiteSpace(variableValue))
            {
                return null;
            }

            var bannedWordsFound = variableName
                .SplitCamelCaseToWords()
                .Intersect(splitCredentialWords)
                .ToHashSet();

            var matches = passwordValuePattern.Matches(variableValue.ToLowerInvariant());
            foreach (Match match in matches)
            {
                bannedWordsFound.Add(match.Groups["password"].Value);
            }

            return bannedWordsFound.Count > 0
                ? string.Join(", ", bannedWordsFound)
                : null;
        }
    }
}
