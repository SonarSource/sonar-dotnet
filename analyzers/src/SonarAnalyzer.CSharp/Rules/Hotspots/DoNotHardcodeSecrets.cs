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
using SonarAnalyzer.Core.Common;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotHardcodeSecrets : DoNotHardcodeBase<SyntaxKind>
{
    private const string MessageFormat = "\"{0}\" detected here, make sure this is not a hard-coded secret.";

    private const string DefaultSecretWords = """api[_\-]?key, auth, credential, secret, token""";
    private const double DefaultRandomnessSensibility = 3;
    private const double LanuageScoreIncrement = 0.3;
    private const string EqualsName = nameof(string.Equals);

    // https://docs.gitguardian.com/secrets-detection/secrets-detection-engine/detectors/generics/generic_high_entropy_secret#:~:text=Follow%20this%20regular%20expression
    private static readonly Regex ValidationPattern = new(@"^[a-zA-Z0-9_.+/~$-]([a-zA-Z0-9_.+\/=~$-]|\\(?![ntr""])){14,1022}[a-zA-Z0-9_.+/=~$-]$", RegexOptions.None, RegexConstants.DefaultTimeout);
    private static readonly Regex BanList = new(@"public[_.-]?key|document_?key|client[_.-]?id|localhost|127\.0\.0\.1|test|xsrf|csrf", RegexOptions.IgnoreCase, RegexConstants.DefaultTimeout);

    private Regex keyWordInVariablePattern;

    [RuleParameter("secretWords", PropertyType.String, "Comma separated list of words identifying potential secret", DefaultSecretWords)]
    public string SecretWords { get => FilterWords; set => FilterWords = value; }

    [RuleParameter("randomnessSensibility", PropertyType.Float, "Allows to tune the Randomness Sensibility (from 0 to 10)", DefaultRandomnessSensibility)]
    public double RandomnessSensibility { get; set; } = DefaultRandomnessSensibility;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
    protected override string DiagnosticId => "S6418";

    private double MaxLanguageScore => (10 - RandomnessSensibility) * LanuageScoreIncrement;

    public DoNotHardcodeSecrets() : this(AnalyzerConfiguration.Hotspot) { }

    public DoNotHardcodeSecrets(IAnalyzerConfiguration configuration) : base(configuration)
    {
        rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        SecretWords = DefaultSecretWords;
    }

    protected override void Initialize(SonarParametrizedAnalysisContext context)
    {
        context.RegisterCompilationStartAction(
        c =>
        {
            if (!IsEnabled(c.Options))
            {
                return;
            }

            c.RegisterNodeAction(c =>
            {
                var node = c.Node;
                if (FindIdentifier(node) is { } identifier
                    && FindRightHandSide(node) is { } rhs
                    && rhs.FindStringConstant(c.SemanticModel) is { } secret)
                {
                    if (ShouldRaiseBinary(identifier.ValueText, secret))
                    {
                        c.ReportIssue(rule, rhs, identifier.ValueText);
                    }
                    else if (ShouldRaiseLiteral(secret, out var message))
                    {
                        c.ReportIssue(rule, rhs, message);
                    }
                }
            },
            SyntaxKind.AddAssignmentExpression,
            SyntaxKind.SimpleAssignmentExpression,
            SyntaxKind.VariableDeclarator,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.GetAccessorDeclaration,
            SyntaxKind.SetAccessorDeclaration,
            SyntaxKind.EqualsExpression);

            c.RegisterNodeAction(c =>
            {
                var invocationExpression = (InvocationExpressionSyntax)c.Node;

                if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression
                    && memberAccessExpression.Name.Identifier.ValueText == EqualsName
                    && invocationExpression.TryGetFirstArgument(out var firstArgument)
                    && memberAccessExpression.IsMemberAccessOnKnownType(EqualsName, KnownType.System_String, c.SemanticModel)
                    && GetIdentifierAndValue(memberAccessExpression.Expression, firstArgument, out var identifier, out var value)
                    && value.FindStringConstant(c.SemanticModel) is { } secret
                    && ShouldRaiseBinary(identifier.Value.ValueText, secret))
                {
                    c.ReportIssue(rule, memberAccessExpression, identifier.Value.ValueText);
                }
            },
            SyntaxKind.InvocationExpression);
        });

        context.RegisterCompilationAction(CheckWebConfig);
        context.RegisterCompilationAction(CheckAppSettings);
    }

    protected override void ExtractKeyWords(string value)
    {
        keyWords = value;
        splitKeyWords = SplitKeyWordsByComma(keyWords);
        keyWordPattern = new Regex(splitKeyWords.JoinStr("|"), RegexOptions.IgnoreCase, RegexTimeout);
        keyWordInVariablePattern = new Regex($@"(?<secret>\b\w*?({keyWordPattern}))\s*[:=]\s*(?<suffix>[^"";$]+)", RegexOptions.IgnoreCase, RegexTimeout);
    }

    protected override bool ShouldRaise(string variableName, string variableValue, out string message)
    {
        if (ShouldRaiseBinary(variableName, variableValue))
        {
            message = variableName;
            return true;
        }
        else if (ShouldRaiseLiteral(variableValue, out message))
        {
            return true;
        }
        return false;
    }

    protected override IEnumerable<string> FindKeyWords(string variableName, string variableValue) =>
        [];

    private bool ShouldRaiseLiteral(string secret, out string message)
    {
        var variableMatch = keyWordInVariablePattern.SafeMatches(secret);
        var keyWordsFound = new List<string>();

        foreach (Match match in variableMatch)
        {
            if (match.Success && !IsValidKeyword(match.Groups["suffix"].Value)
                && ShouldRaiseBinary(match.Groups["secret"].Value, match.Groups["suffix"].Value))
            {
                keyWordsFound.Add(match.Groups["secret"].Value);
            }
        }
        message = keyWordsFound.JoinAnd();
        return keyWordsFound.Count > 0;
    }

    private bool ShouldRaiseBinary(string left, string right) =>
        !string.IsNullOrEmpty(left)
            && keyWordPattern.SafeMatch(left) is { Success: true, Value: var keyWord }
            && !BanList.SafeIsMatch(left)
            && right.IndexOf(keyWord, StringComparison.InvariantCultureIgnoreCase) < 0
            && IsToken(right);

    private static SyntaxToken? FindIdentifier(SyntaxNode node) =>
        node switch
        {
            AccessorDeclarationSyntax accessorDeclaration => accessorDeclaration.Parent.Parent.GetIdentifier(),
            AssignmentExpressionSyntax assignmentExpression => assignmentExpression.Left.GetIdentifier(),
            BinaryExpressionSyntax binaryExpression => GetBinaryExpressionIdentifier(binaryExpression),
            _ => node.GetIdentifier()
        };

    private static SyntaxToken? GetBinaryExpressionIdentifier(BinaryExpressionSyntax node) =>
        node switch
        {
            { Left: IdentifierNameSyntax identifierLeft } => identifierLeft.Identifier,
            { Right: IdentifierNameSyntax identifierRight } => identifierRight.Identifier,
            _ => null
        };

    private static SyntaxNode? GetBinaryExpressionValue(BinaryExpressionSyntax node) =>
        node switch
        {
            { Left: IdentifierNameSyntax } => node.Right,
            { Right: IdentifierNameSyntax } => node.Left,
            _ => null
        };

    private static SyntaxNode FindRightHandSide(SyntaxNode node) =>
        node switch
        {
            AssignmentExpressionSyntax assignmentExpression => assignmentExpression.Right,
            VariableDeclaratorSyntax variableDeclarator => variableDeclarator.Initializer?.Value,
            PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Initializer?.Value,
            AccessorDeclarationSyntax accessorDeclaration => accessorDeclaration.ExpressionBody()?.Expression,
            BinaryExpressionSyntax binaryExpression => GetBinaryExpressionValue(binaryExpression),
            _ => null
        };

    private static bool GetIdentifierAndValue(ExpressionSyntax expression, ArgumentSyntax argument, out SyntaxToken? identifier, out LiteralExpressionSyntax value)
    {
        identifier = null;
        value = null;
        switch (expression)
        {
            case MemberAccessExpressionSyntax:
            case IdentifierNameSyntax:
            case InvocationExpressionSyntax:
                identifier = expression.GetIdentifier();
                value = argument.Expression as LiteralExpressionSyntax;
                break;
            case LiteralExpressionSyntax:
                identifier = argument.Expression.GetIdentifier();
                value = expression as LiteralExpressionSyntax;
                break;
        }
        return identifier.HasValue && value is not null;
    }

    private bool IsToken(string value) =>
        ShannonEntropy.Calculate(value) > RandomnessSensibility
        && ValidationPattern.SafeIsMatch(value)
        && MaxLanguageScore > NaturalLanguageDetector.HumanLanguageScore(value);
}
