/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.Core.Rules;

public abstract class DoNotHardcodeSecretsBase<TSyntaxKind> : DoNotHardcodeBase<TSyntaxKind>
    where TSyntaxKind : struct
{
    protected const string MessageFormat = @"""{0}"" detected here, make sure this is not a hard-coded secret.";
    protected const string DefaultSecretWords = @"api[_\-]?key, auth, credential, secret, token";
    protected const double DefaultRandomnessSensibility = 3;
    protected const double LanguageScoreIncrement = 0.3;
    protected const string EqualsName = nameof(string.Equals);

    // https://docs.gitguardian.com/secrets-detection/secrets-detection-engine/detectors/generics/generic_high_entropy_secret#:~:text=Follow%20this%20regular%20expression
    protected static readonly Regex ValidationPattern = new(@"^[a-zA-Z0-9_.+/~$-]([a-zA-Z0-9_.+\/=~$-]|\\(?![ntr""])){14,1022}[a-zA-Z0-9_.+/=~$-]$", RegexOptions.None, Constants.DefaultRegexTimeout);
    protected static readonly Regex BanList = new(@"public[_.-]?key|document_?key|client[_.-]?id|localhost|127\.0\.0\.1|test|xsrf|csrf", RegexOptions.IgnoreCase, Constants.DefaultRegexTimeout);

    protected Regex keyWordInVariablePattern;

    protected abstract void RegisterNodeActions(SonarCompilationStartAnalysisContext context);
    protected abstract SyntaxNode IdentifierRoot(SyntaxNode node);
    protected abstract SyntaxNode RightHandSide(SyntaxNode node);

    [RuleParameter("secretWords", PropertyType.String, "Comma separated list of words identifying potential secret", DefaultSecretWords)]
    public string SecretWords { get => FilterWords; set => FilterWords = value; }

    [RuleParameter("randomnessSensibility", PropertyType.Float, "Allows to tune the Randomness Sensibility (from 0 to 10)", DefaultRandomnessSensibility)]
    public double RandomnessSensibility { get; set; } = DefaultRandomnessSensibility;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);
    protected override string DiagnosticId => "S6418";

    protected double MaxLanguageScore => (10 - RandomnessSensibility) * LanguageScoreIncrement;

    protected DoNotHardcodeSecretsBase(IAnalyzerConfiguration configuration) : base(configuration)
    {
        rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);
        SecretWords = DefaultSecretWords;
    }

    protected override void Initialize(SonarParametrizedAnalysisContext context)
    {
        context.RegisterCompilationStartAction(compilationStartContext =>
            {
                if (IsEnabled(compilationStartContext.Options))
                {
                    RegisterNodeActions(compilationStartContext);
                }
            });

        context.RegisterCompilationAction(CheckWebConfig);
        context.RegisterCompilationAction(CheckAppSettings);
        context.RegisterCompilationAction(CheckLaunchSettings);
    }

    protected override void ExtractKeyWords(string value)
    {
        keyWords = value;
        splitKeyWords = SplitKeyWordsByComma(keyWords);
        keyWordPattern = new Regex(splitKeyWords.JoinStr("|"), RegexOptions.IgnoreCase, RegexTimeout);
        keyWordInVariablePattern = new Regex($@"(?<secret>\b\w*?({keyWordPattern}))\s*[:=]\s*(?<suffix>[^"";$]+)", RegexOptions.IgnoreCase, RegexTimeout);
    }

    protected override string FindIssue(string variableName, string variableValue)
    {
        if (ShouldRaiseBinary(variableName, variableValue))
        {
            return variableName;
        }
        else if (FindLiteralIssue(variableValue) is { } message)
        {
            return message;
        }
        else
        {
            return null;
        }
    }

    protected void ReportIssues(SonarSyntaxNodeReportingContext context)
    {
        var node = context.Node;
        if (Language.Syntax.NodeIdentifier(IdentifierRoot(node)) is { } identifier
            && RightHandSide(node) is { } rhs
            && Language.FindConstantValue(context.Model, rhs) is string secret)
        {
            if (ShouldRaiseBinary(identifier.ValueText, secret))
            {
                context.ReportIssue(rule, rhs, identifier.ValueText);
            }
            else if (FindLiteralIssue(secret) is { } message)
            {
                context.ReportIssue(rule, rhs, message);
            }
        }
    }

    protected void ReportIssuesForEquals(SonarSyntaxNodeReportingContext context, SyntaxNode memberAccessExpression, IdentifierValuePair identifierAndValue)
    {
        if (identifierAndValue is { Identifier: { } identifier, Value: { } value }
            && Language.FindConstantValue(context.Model, value) is string secret
            && ShouldRaiseBinary(identifier.ValueText, secret))
        {
            context.ReportIssue(rule, memberAccessExpression, identifier.ValueText);
        }
    }

    private string FindLiteralIssue(string secret)
    {
        var variableMatch = keyWordInVariablePattern.SafeMatches(secret);
        var keyWordsFound = new List<string>();
        foreach (Match match in variableMatch)
        {
            if (match.Success
                && !IsValidKeyword(match.Groups["suffix"].Value)
                && ShouldRaiseBinary(match.Groups["secret"].Value, match.Groups["suffix"].Value))
            {
                keyWordsFound.Add(match.Groups["secret"].Value);
            }
        }
        return keyWordsFound.Count > 0 ? keyWordsFound.JoinAnd() : null;
    }

    private bool ShouldRaiseBinary(string left, string right) =>
        !string.IsNullOrEmpty(left)
        && keyWordPattern.SafeMatch(left) is { Success: true } keyWord
        && !BanList.SafeIsMatch(left)
        && right.IndexOf(keyWord.Value, StringComparison.InvariantCultureIgnoreCase) < 0
        && IsToken(right);

    private bool IsToken(string value) =>
        ShannonEntropy.Calculate(value) > RandomnessSensibility
        && ValidationPattern.SafeIsMatch(value)
        && MaxLanguageScore > NaturalLanguageDetector.HumanLanguageScore(value);

    protected sealed record IdentifierValuePair(SyntaxToken? Identifier, SyntaxNode Value);
}
