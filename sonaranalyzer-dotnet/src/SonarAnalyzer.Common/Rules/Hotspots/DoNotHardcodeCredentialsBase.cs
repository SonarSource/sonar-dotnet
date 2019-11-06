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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class DoNotHardcodeCredentialsBase<TSyntaxKind> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
    {

        protected const string DiagnosticId = "S2068";
        protected const string MessageFormat = "Make sure hard-coded credential is safe.";

        private const string DefaultCredentialWords = "password, passwd, pwd";

        private string credentialWords;
        private IEnumerable<string> splitCredentialWords;
        private Regex passwordValuePattern;
        private IAnalyzerConfiguration analyzerConfiguration;

        [RuleParameter("credentialWords", PropertyType.String,
            "Comma separated list of words identifying potential credentials", DefaultCredentialWords)]
        public string CredentialWords
        {
            get => this.credentialWords;
            set
            {
                this.credentialWords = value;
                this.splitCredentialWords = value.ToUpperInvariant()
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToList();
                this.passwordValuePattern = new Regex(string.Format(@"\b(?<password>{0})\b[:=]\S",
                    string.Join("|", this.splitCredentialWords)), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        protected ObjectCreationTracker<TSyntaxKind> ObjectCreationTracker { get; set; }

        protected PropertyAccessTracker<TSyntaxKind> PropertyAccessTracker { get; set; }

        protected DoNotHardcodeCredentialsBase(IAnalyzerConfiguration analyzerConfiguration)
        {
            CredentialWords = DefaultCredentialWords;
            this.analyzerConfiguration = analyzerConfiguration;
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            ObjectCreationTracker.Track(context,
                ObjectCreationTracker.MatchConstructor(KnownType.System_Net_NetworkCredential),
                ObjectCreationTracker.ArgumentAtIndexIs(1, KnownType.System_String),
                ObjectCreationTracker.ArgumentAtIndexIsConst(1));

            ObjectCreationTracker.Track(context,
               ObjectCreationTracker.MatchConstructor(KnownType.System_Security_Cryptography_PasswordDeriveBytes),
               ObjectCreationTracker.ArgumentAtIndexIs(0, KnownType.System_String),
               ObjectCreationTracker.ArgumentAtIndexIsConst(0));

            PropertyAccessTracker.Track(context,
               PropertyAccessTracker.MatchSetter(),
               PropertyAccessTracker.AssignedValueIsConstant(),
               PropertyAccessTracker.MatchProperty(
                   new MemberDescriptor(KnownType.System_Net_NetworkCredential, "Password")));
        }

        protected bool IsEnabled(AnalyzerOptions options)
        {
            analyzerConfiguration.Initialize(options);
            return analyzerConfiguration.IsEnabled(DiagnosticId);
        }

        protected abstract class CredentialWordsFinderBase<TSyntaxNode>
             where TSyntaxNode : SyntaxNode
        {
            private readonly DoNotHardcodeCredentialsBase<TSyntaxKind> analyzer;

            protected CredentialWordsFinderBase(DoNotHardcodeCredentialsBase<TSyntaxKind> analyzer)
            {
                this.analyzer = analyzer;
            }

            protected abstract bool IsAssignedWithStringLiteral(TSyntaxNode syntaxNode, SemanticModel semanticModel);

            protected abstract string GetVariableName(TSyntaxNode syntaxNode);

            protected abstract string GetAssignedValue(TSyntaxNode syntaxNode);

            public Action<SyntaxNodeAnalysisContext> GetAnalysisAction(DiagnosticDescriptor rule) =>
                context =>
                {
                    var declarator = (TSyntaxNode)context.Node;

                    if (!IsAssignedWithStringLiteral(declarator, context.SemanticModel))
                    {
                        return;
                    }

                    var variableName = GetVariableName(declarator);
                    var variableValue = GetAssignedValue(declarator);

                    var bannedWords = FindCredentialWords(variableName, variableValue);
                    if (bannedWords.Any())
                    {
                        context.ReportDiagnosticWhenActive(
                            Diagnostic.Create(rule, declarator.GetLocation(), string.Join(", ", bannedWords)));
                    }
                };

            protected IEnumerable<string> FindCredentialWords(string variableName, string variableValue)
            {
                if (string.IsNullOrEmpty(variableValue?.Trim()))
                {
                    return Enumerable.Empty<string>();
                }

                var credentialWordsFound = variableName
                    .SplitCamelCaseToWords()
                    .Intersect(analyzer.splitCredentialWords)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var matches = analyzer.passwordValuePattern.Matches(variableValue);
                foreach (Match match in matches)
                {
                    credentialWordsFound.Add(match.Groups["password"].Value);
                }

                // Rule was initially implemented with everything lower (which is wrong) so we have to force lower
                // before reporting to avoid new issues to appear on SQ/SC.
                return credentialWordsFound.Select(x => x.ToLowerInvariant());
            }
        }
    }
}
