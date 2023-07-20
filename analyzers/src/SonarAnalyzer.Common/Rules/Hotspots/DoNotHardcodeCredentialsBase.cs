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

using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SonarAnalyzer.Json;
using SonarAnalyzer.Json.Parsing;

namespace SonarAnalyzer.Rules
{
    public abstract class DoNotHardcodeCredentialsBase<TSyntaxKind> : ParametrizedDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const char CredentialSeparator = ';';
        private const string DiagnosticId = "S2068";
        private const string MessageFormat = "{0}";
        private const string MessageHardcodedPassword = "Please review this hard-coded password.";
        private const string MessageFormatCredential = @"""{0}"" detected here, make sure this is not a hard-coded credential.";
        private const string MessageUriUserInfo = "Review this hard-coded URI, which may contain a credential.";
        private const string DefaultCredentialWords = "password, passwd, pwd, passphrase";

        private readonly IAnalyzerConfiguration configuration;
        private readonly DiagnosticDescriptor rule;
        private readonly Regex validCredentialPattern = new(@"^(\?|:\w+|\{\d+[^}]*\}|""|')$", RegexOptions.IgnoreCase);
        private readonly Regex uriUserInfoPattern;
        private string credentialWords;
        private IEnumerable<string> splitCredentialWords;
        private Regex passwordValuePattern;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
        protected abstract void InitializeActions(SonarParametrizedAnalysisContext context);
        protected abstract bool IsSecureStringAppendCharFromConstant(SyntaxNode argumentNode, SemanticModel model);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        [RuleParameter("credentialWords", PropertyType.String, "Comma separated list of words identifying potential credentials", DefaultCredentialWords)]
        public string CredentialWords
        {
            get => credentialWords;
            set
            {
                credentialWords = value;
                splitCredentialWords = value.ToUpperInvariant()
                    .Split(',')
                    .Select(x => x.Trim())
                    .Where(x => x.Length != 0)
                    .ToList();
                passwordValuePattern = new Regex(string.Format(@"\b(?<credential>{0})\s*[:=]\s*(?<suffix>.+)$",
                    string.Join("|", splitCredentialWords.Select(Regex.Escape))), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        protected DoNotHardcodeCredentialsBase(IAnalyzerConfiguration configuration)
        {
            const string Rfc3986_Unreserved = "-._~";  // Numbers and letters are embedded in regex itself without escaping
            const string Rfc3986_Pct = "%";
            const string Rfc3986_SubDelims = "!$&'()*+,;=";
            const string UriPasswordSpecialCharacters = Rfc3986_Unreserved + Rfc3986_Pct + Rfc3986_SubDelims;
            // See https://tools.ietf.org/html/rfc3986 Userinfo can contain groups: unreserved | pct-encoded | sub-delims
            var loginGroup = CreateUserInfoGroup("Login");
            var passwordGroup = CreateUserInfoGroup("Password", ":");   // Additional ":" to capture passwords containing it
            uriUserInfoPattern = new Regex(@$"\w+:\/\/{loginGroup}:{passwordGroup}@", RegexOptions.Compiled);
            this.configuration = configuration;
            rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);
            CredentialWords = DefaultCredentialWords;   // Property will initialize multiple state variables

            static string CreateUserInfoGroup(string name, string additionalCharacters = null) =>
                $@"(?<{name}>[\w\d{Regex.Escape(UriPasswordSpecialCharacters)}{additionalCharacters}]+)";
        }

        protected sealed override void Initialize(SonarParametrizedAnalysisContext context)
        {
            var input = new TrackerInput(context, configuration, rule);

            var oc = Language.Tracker.ObjectCreation;
            oc.Track(input, new object[] { MessageHardcodedPassword },
                oc.MatchConstructor(KnownType.System_Net_NetworkCredential),
                oc.ArgumentAtIndexIs(1, KnownType.System_String),
                oc.ArgumentAtIndexIsConst(1));

            oc.Track(input, new object[] { MessageHardcodedPassword },
               oc.MatchConstructor(KnownType.System_Security_Cryptography_PasswordDeriveBytes),
               oc.ArgumentAtIndexIs(0, KnownType.System_String),
               oc.ArgumentAtIndexIsConst(0));

            var pa = Language.Tracker.PropertyAccess;
            pa.Track(input, new object[] { MessageHardcodedPassword },
               pa.MatchSetter(),
               pa.AssignedValueIsConstant(),
               pa.MatchProperty(new MemberDescriptor(KnownType.System_Net_NetworkCredential, "Password")));

            var inv = Language.Tracker.Invocation;
            inv.Track(input, new object[] { MessageHardcodedPassword },
                inv.MatchMethod(new MemberDescriptor(KnownType.System_Security_SecureString, nameof(SecureString.AppendChar))),
                inv.ArgumentAtIndexIs(0, IsSecureStringAppendCharFromConstant));

            InitializeActions(context);
            context.RegisterCompilationAction(CheckWebConfig);
            context.RegisterCompilationAction(CheckAppSettings);
        }

        protected bool IsEnabled(AnalyzerOptions options)
        {
            configuration.Initialize(options);
            return configuration.IsEnabled(DiagnosticId);
        }

        private void CheckWebConfig(SonarCompilationReportingContext context)
        {
            foreach (var path in context.WebConfigFiles())
            {
                if (XmlHelper.ParseXDocument(File.ReadAllText(path)) is { } doc)
                {
                    CheckWebConfig(context, path, doc.Descendants());
                }
            }
        }

        private void CheckWebConfig(SonarCompilationReportingContext context, string path, IEnumerable<XElement> elements)
        {
            foreach (var element in elements)
            {
                if (!element.HasElements && IssueMessage(element.Name.LocalName, element.Value) is { } elementMessage && element.CreateLocation(path) is { } elementLocation)
                {
                    context.ReportIssue(Language.GeneratedCodeRecognizer, CreateDiagnostic(rule, elementLocation, elementMessage));
                }
                foreach (var attribute in element.Attributes())
                {
                    if (IssueMessage(attribute.Name.LocalName, attribute.Value) is { } attributeMessage && attribute.CreateLocation(path) is { } attributeLocation)
                    {
                        context.ReportIssue(Language.GeneratedCodeRecognizer, CreateDiagnostic(rule, attributeLocation, attributeMessage));
                    }
                }
            }
        }

        private void CheckAppSettings(SonarCompilationReportingContext context)
        {
            foreach (var path in context.AppSettingsFiles())
            {
                if (JsonNode.FromString(File.ReadAllText(path)) is { } json)
                {
                    var walker = new CredentialWordsJsonWalker(this, context, path);
                    walker.Visit(json);
                }
            }
        }

        private string IssueMessage(string variableName, string variableValue)
        {
            if (string.IsNullOrWhiteSpace(variableValue))
            {
                return null;
            }
            else if (FindCredentialWords(variableName, variableValue) is var bannedWords && bannedWords.Any())
            {
                return string.Format(MessageFormatCredential, bannedWords.JoinAnd());
            }
            else if (ContainsUriUserInfo(variableValue))
            {
                return MessageUriUserInfo;
            }
            else
            {
                return null;
            }
        }

        private IEnumerable<string> FindCredentialWords(string variableName, string variableValue)
        {
            var credentialWordsFound = variableName
                .SplitCamelCaseToWords()
                .Intersect(splitCredentialWords)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (credentialWordsFound.Any(x => variableValue.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) >= 0))
            {
                // See https://github.com/SonarSource/sonar-dotnet/issues/2868
                return Enumerable.Empty<string>();
            }

            var match = passwordValuePattern.Match(variableValue);
            if (match.Success && !IsValidCredential(match.Groups["suffix"].Value))
            {
                credentialWordsFound.Add(match.Groups["credential"].Value);
            }

            // Rule was initially implemented with everything lower (which is wrong) so we have to force lower before reporting to avoid new issues to appear on SQ/SC.
            return credentialWordsFound.Select(x => x.ToLowerInvariant());
        }

        private bool IsValidCredential(string suffix)
        {
            var candidateCredential = suffix.Split(CredentialSeparator).First().Trim();
            return string.IsNullOrWhiteSpace(candidateCredential) || validCredentialPattern.IsMatch(candidateCredential);
        }

        private bool ContainsUriUserInfo(string variableValue)
        {
            var match = uriUserInfoPattern.Match(variableValue);
            return match.Success
                && match.Groups["Password"].Value is { } password
                && !string.Equals(match.Groups["Login"].Value, password, StringComparison.OrdinalIgnoreCase)
                && password != CredentialSeparator.ToString()
                && !validCredentialPattern.IsMatch(password);
        }

        protected abstract class CredentialWordsFinderBase<TSyntaxNode>
             where TSyntaxNode : SyntaxNode
        {
            private readonly DoNotHardcodeCredentialsBase<TSyntaxKind> analyzer;

            protected abstract bool ShouldHandle(TSyntaxNode syntaxNode, SemanticModel semanticModel);
            protected abstract string GetVariableName(TSyntaxNode syntaxNode);
            protected abstract string GetAssignedValue(TSyntaxNode syntaxNode, SemanticModel semanticModel);

            protected CredentialWordsFinderBase(DoNotHardcodeCredentialsBase<TSyntaxKind> analyzer) =>
                this.analyzer = analyzer;

            public Action<SonarSyntaxNodeReportingContext> AnalysisAction() =>
                context =>
                {
                    var declarator = (TSyntaxNode)context.Node;
                    if (ShouldHandle(declarator, context.SemanticModel)
                        && GetAssignedValue(declarator, context.SemanticModel) is { } variableValue
                        && analyzer.IssueMessage(GetVariableName(declarator), variableValue) is { } message)
                    {
                        context.ReportIssue(CreateDiagnostic(analyzer.rule, declarator.GetLocation(), message));
                    }
                };
        }

        private sealed class CredentialWordsJsonWalker : JsonWalker
        {
            private readonly DoNotHardcodeCredentialsBase<TSyntaxKind> analyzer;
            private readonly SonarCompilationReportingContext context;
            private readonly string path;

            public CredentialWordsJsonWalker(DoNotHardcodeCredentialsBase<TSyntaxKind> analyzer, SonarCompilationReportingContext context, string path)
            {
                this.analyzer = analyzer;
                this.context = context;
                this.path = path;
            }

            protected override void VisitObject(string key, JsonNode value)
            {
                if (value.Kind == Kind.Value)
                {
                    CheckKeyValue(key, value);
                }
                else
                {
                    base.VisitObject(key, value);
                }
            }

            protected override void VisitValue(JsonNode node) =>
                CheckKeyValue(null, node);

            private void CheckKeyValue(string key, JsonNode value)
            {
                if (value.Value is string str && analyzer.IssueMessage(key, str) is { } valueMessage)
                {
                    context.ReportIssue(analyzer.Language.GeneratedCodeRecognizer, CreateDiagnostic(analyzer.rule, value.ToLocation(path), valueMessage));
                }
            }
        }
    }
}
