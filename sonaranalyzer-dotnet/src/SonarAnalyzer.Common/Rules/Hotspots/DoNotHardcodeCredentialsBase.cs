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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class DoNotHardcodeCredentialsBase<TSyntaxKind> : ParameterLoadingDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S2068";
        private const string MessageFormat = "{0}";
        private const string MessageFormatCredential = @"""{0}"" detected here, make sure this is not a hard-coded credential.";
        private const string MessageUriUserInfo = "Review this hard-coded URI, which may contain a credential.";
        private const string DefaultCredentialWords = "password, passwd, pwd, passphrase";

        protected readonly DiagnosticDescriptor rule;
        private string credentialWords;
        private IEnumerable<string> splitCredentialWords;
        private Regex passwordValuePattern;
        private readonly IAnalyzerConfiguration analyzerConfiguration;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        [RuleParameter("credentialWords", PropertyType.String, "Comma separated list of words identifying potential credentials", DefaultCredentialWords)]
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

                this.passwordValuePattern = new Regex(string.Format(@"\b(?<credential>{0})\s*[:=]\s*(?<suffix>.+)$",
                    string.Join("|", this.splitCredentialWords.Select(Regex.Escape))), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        protected ObjectCreationTracker<TSyntaxKind> ObjectCreationTracker { get; set; }

        protected PropertyAccessTracker<TSyntaxKind> PropertyAccessTracker { get; set; }

        protected DoNotHardcodeCredentialsBase(System.Resources.ResourceManager rspecResources, IAnalyzerConfiguration analyzerConfiguration)
        {
            this.rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources).WithNotConfigurable();
            CredentialWords = DefaultCredentialWords;
            this.analyzerConfiguration = analyzerConfiguration;
        }

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            var innerContext = context.GetInnerContext();

            ObjectCreationTracker.Track(innerContext,
                ObjectCreationTracker.MatchConstructor(KnownType.System_Net_NetworkCredential),
                ObjectCreationTracker.ArgumentAtIndexIs(1, KnownType.System_String),
                ObjectCreationTracker.ArgumentAtIndexIsConst(1));

            ObjectCreationTracker.Track(innerContext,
               ObjectCreationTracker.MatchConstructor(KnownType.System_Security_Cryptography_PasswordDeriveBytes),
               ObjectCreationTracker.ArgumentAtIndexIs(0, KnownType.System_String),
               ObjectCreationTracker.ArgumentAtIndexIsConst(0));

            PropertyAccessTracker.Track(innerContext,
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
            private readonly Regex validCredentialPattern = new Regex(@"^\?|:\w+|\{\d+[^}]*\}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            private readonly Regex uriUserInfoPattern = new Regex(@"\w+:\/\/(?<Login>[^:]+):(?<Password>[^@]+)@", RegexOptions.Compiled);
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
                        context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, declarator.GetLocation(),
                            string.Format(MessageFormatCredential, bannedWords.JoinStr(", "))));
                    }
                    else if (ContainsUriUserInfo(variableValue))
                    {
                        context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, declarator.GetLocation(),
                            MessageUriUserInfo));
                    }
                };

            private IEnumerable<string> FindCredentialWords(string variableName, string variableValue)
            {
                if (string.IsNullOrEmpty(variableValue?.Trim()))
                {
                    return Enumerable.Empty<string>();
                }

                var credentialWordsFound = (variableName ==null || variableName.Any(x => char.IsLower(x)) ? variableName : variableName.ToLower()) // Prepare "PASSWORD" for SplitCamelCaseToWords()
                    .SplitCamelCaseToWords()
                    .Intersect(analyzer.splitCredentialWords)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (credentialWordsFound.Any(x => variableValue.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) >= 0))
                {
                    // See https://github.com/SonarSource/sonar-dotnet/issues/2868
                    return Enumerable.Empty<string>();
                }

                var match = analyzer.passwordValuePattern.Match(variableValue);
                if (match.Success && !IsValidCredential(match.Groups["suffix"].Value))
                {
                    credentialWordsFound.Add(match.Groups["credential"].Value);
                }

                // Rule was initially implemented with everything lower (which is wrong) so we have to force lower
                // before reporting to avoid new issues to appear on SQ/SC.
                return credentialWordsFound.Select(x => x.ToLowerInvariant());
            }

            private bool IsValidCredential(string suffix)
            {
                var candidateCredential = suffix.Split(';').First().Trim();

                return string.IsNullOrWhiteSpace(candidateCredential) ||
                       this.validCredentialPattern.IsMatch(candidateCredential);
            }

            private bool ContainsUriUserInfo(string variableValue)
            {
                var match = uriUserInfoPattern.Match(variableValue);
                return match.Success && !string.Equals(match.Groups["Login"].Value, match.Groups["Password"].Value, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
