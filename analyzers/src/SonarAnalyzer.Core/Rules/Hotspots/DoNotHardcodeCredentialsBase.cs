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

using System.Security;
using System.Text.RegularExpressions;
using SonarAnalyzer.CFG.Extensions;
using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.Core.Rules;

public abstract class DoNotHardcodeCredentialsBase<TSyntaxKind> : DoNotHardcodeBase<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string MessageFormat = "{0}";
    private const string MessageHardcodedPassword = "Please review this hard-coded password.";
    private const string MessageFormatCredential = @"""{0}"" detected here, make sure this is not a hard-coded credential.";
    private const string MessageUriUserInfo = "Review this hard-coded URI, which may contain a credential.";
    private const string DefaultCredentialWords = "password, passwd, pwd, passphrase";

    private static readonly Regex UriUserInfoPattern = CreateUriUserInfoPattern();

    protected abstract void InitializeActions(SonarParametrizedAnalysisContext context);
    protected abstract bool IsSecureStringAppendCharFromConstant(SyntaxNode argumentNode, SemanticModel model);

    [RuleParameter("credentialWords", PropertyType.String, "Comma separated list of words identifying potential credentials", DefaultCredentialWords)]
    public string CredentialWords { get => FilterWords; set => FilterWords = value; }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);
    protected override string DiagnosticId => "S2068";

    protected DoNotHardcodeCredentialsBase(IAnalyzerConfiguration configuration) : base(configuration)
    {
        rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);
        CredentialWords = DefaultCredentialWords;   // Property will initialize multiple state variables
    }

    protected override void ExtractKeyWords(string value)
    {
        keyWords = value;
        splitKeyWords = SplitKeyWordsByComma(keyWords);
        var credentialWordsPattern = splitKeyWords.Select(Regex.Escape).JoinStr("|");
        keyWordPattern = new Regex($@"(?<credential>{credentialWordsPattern})\s*=\s*(?<suffix>.+)$", RegexOptions.IgnoreCase, RegexTimeout);
    }

    protected sealed override void Initialize(SonarParametrizedAnalysisContext context)
    {
        var input = new TrackerInput(context, configuration, rule);

        var oc = Language.Tracker.ObjectCreation;
        oc.Track(input, [MessageHardcodedPassword],
            oc.MatchConstructor(KnownType.System_Net_NetworkCredential),
            oc.ArgumentAtIndexIs(1, KnownType.System_String),
            oc.ArgumentAtIndexIsConst(1));

        oc.Track(input, [MessageHardcodedPassword],
            oc.MatchConstructor(KnownType.System_Security_Cryptography_PasswordDeriveBytes),
            oc.ArgumentAtIndexIs(0, KnownType.System_String),
            oc.ArgumentAtIndexIsConst(0));

        var pa = Language.Tracker.PropertyAccess;
        pa.Track(input, [MessageHardcodedPassword],
            pa.MatchSetter(),
            pa.AssignedValueIsConstant(),
            pa.MatchProperty(new MemberDescriptor(KnownType.System_Net_NetworkCredential, "Password")));

        var inv = Language.Tracker.Invocation;
        inv.Track(input, [MessageHardcodedPassword],
            inv.MatchMethod(new MemberDescriptor(KnownType.System_Security_SecureString, nameof(SecureString.AppendChar))),
            inv.ArgumentAtIndexIs(0, IsSecureStringAppendCharFromConstant));

        InitializeActions(context);
        context.RegisterCompilationAction(CheckWebConfig);
        context.RegisterCompilationAction(CheckAppSettings);
        context.RegisterCompilationAction(CheckLaunchSettings);
    }

    protected override string FindIssue(string variableName, string variableValue)
    {
        if (string.IsNullOrWhiteSpace(variableValue))
        {
            return null;
        }
        else if (FindKeyWords(variableName, variableValue) is { } bannedWords and not "")
        {
            return string.Format(MessageFormatCredential, bannedWords);
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

    private string FindKeyWords(string variableName, string variableValue)
    {
        var credentialWordsFound = variableName
            .SplitCamelCaseToWords()
            .Intersect(splitKeyWords)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (credentialWordsFound.Any(x => variableValue.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) >= 0))
        {
            // See https://github.com/SonarSource/sonar-dotnet/issues/2868
            return null;
        }

        if (keyWordPattern.SafeMatch(variableValue) is { Success: true } match && !IsValidKeyword(match.Groups["suffix"].Value))
        {
            credentialWordsFound.Add(match.Groups["credential"].Value);
        }

        // Rule was initially implemented with everything lower (which is wrong) so we have to force lower before reporting to avoid new issues to appear on SQ/SC.
        return credentialWordsFound.Select(x => x.ToLowerInvariant()).JoinAnd();
    }

    private static Regex CreateUriUserInfoPattern()
    {
        const string Rfc3986_Unreserved = "-._~";  // Numbers and letters are embedded in regex itself without escaping
        const string Rfc3986_Pct = "%";
        const string Rfc3986_SubDelims = "!$&'()*+,;=";
        const string UriPasswordSpecialCharacters = Rfc3986_Unreserved + Rfc3986_Pct + Rfc3986_SubDelims;
        // See https://tools.ietf.org/html/rfc3986 Userinfo can contain groups: unreserved | pct-encoded | sub-delims
        var loginGroup = CreateUserInfoGroup("Login");
        var passwordGroup = CreateUserInfoGroup("Password", ":");   // Additional ":" to capture passwords containing it
        return new Regex(@$"\w+:\/\/{loginGroup}:{passwordGroup}@", RegexOptions.Compiled, RegexTimeout);

        static string CreateUserInfoGroup(string name, string additionalCharacters = null) =>
            $@"(?<{name}>[\w\d{Regex.Escape(UriPasswordSpecialCharacters)}{additionalCharacters}]+)";
    }

    private static bool ContainsUriUserInfo(string variableValue)
    {
        var match = UriUserInfoPattern.SafeMatch(variableValue);
        return match.Success
            && match.Groups["Password"].Value is { } password
            && !string.Equals(match.Groups["Login"].Value, password, StringComparison.OrdinalIgnoreCase)
            && password != KeywordSeparator.ToString()
            && !ValidKeywordPattern.SafeIsMatch(password);
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
                if (ShouldHandle(declarator, context.Model)
                    && GetAssignedValue(declarator, context.Model) is { } variableValue
                    && analyzer.FindIssue(GetVariableName(declarator), variableValue) is { } message)
                {
                    context.ReportIssue(analyzer.rule, declarator, message);
                }
            };
    }
}
