/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
using SonarAnalyzer.Core.Common;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotHardcodeSecrets : DoNotHardcodeBase<SyntaxKind>
{
    private const string MessageFormat = "\"{0}\" detected here, make sure this is not a hard-coded secret.";

    private const string DefaultSecretWords = """api[_\-]?key, auth, credential, secret, token""";
    private const double DefaultRandomnessSensibility = 3;
    private const double LanuageScoreIncrement = 0.3;

    // https://docs.gitguardian.com/secrets-detection/secrets-detection-engine/detectors/generics/generic_high_entropy_secret#:~:text=Follow%20this%20regular%20expression
    private static readonly Regex ValidationPattern = new(@"^[a-zA-Z0-9_.+/~$-]([a-zA-Z0-9_.+\/=~$-]|\\(?![ntr""])){14,1022}[a-zA-Z0-9_.+/=~$-]$", RegexOptions.None, RegexConstants.DefaultTimeout);

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
                    && rhs.FindStringConstant(c.SemanticModel) is { } secret
                    && IsToken(secret)
                    && ShouldRaise(identifier.ValueText, secret, out string message))
                {
                    c.ReportIssue(rule, rhs, message);
                }
            },
        SyntaxKind.AddAssignmentExpression,
        SyntaxKind.SimpleAssignmentExpression,
        SyntaxKind.VariableDeclarator,
        SyntaxKind.PropertyDeclaration,
        SyntaxKind.GetAccessorDeclaration,
        SyntaxKind.SetAccessorDeclaration);
        });

        context.RegisterCompilationAction(CheckWebConfig);
        context.RegisterCompilationAction(CheckAppSettings);
    }

    protected override void ExtractKeyWords(string value)
    {
        keyWords = value;
        splitKeyWords = SplitKeyWordsByComma(keyWords);
        keyWordPattern = new Regex(splitKeyWords.JoinStr("|"), RegexOptions.IgnoreCase, RegexTimeout);
        keyWordInVariablePattern = new Regex($@"(?<secret>{keyWordPattern})\s*[:=]\s*(?<suffix>.+)$", RegexOptions.IgnoreCase, RegexTimeout);
    }

    protected override bool ShouldRaise(string variableName, string variableValue, out string message)
    {
        message = string.Empty;
        if (string.IsNullOrWhiteSpace(variableValue))
        {
            return false;
        }
        if (FindKeyWords(variableName, variableValue) is var bannedWords && bannedWords.Any())
        {
            message = bannedWords.JoinAnd();
            return true;
        }
        return false;
    }

    protected override IEnumerable<string> FindKeyWords(string variableName, string variableValue)
    {
        var secretWordsFound = new HashSet<string>();
        if (!string.IsNullOrEmpty(variableName)
            && keyWordPattern.SafeMatch(variableName) is { } match
            && match.Success)
        {
                secretWordsFound.Add(match.Value);
        }

        if (secretWordsFound.Any(x => variableValue.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) >= 0))
        {
            // See https://github.com/SonarSource/sonar-dotnet/issues/2868
            return [];
        }

        var variableMatch = keyWordInVariablePattern.SafeMatch(variableValue);
        if (variableMatch.Success && !IsValidKeyword(variableMatch.Groups["suffix"].Value))
        {
            secretWordsFound.Add(variableMatch.Groups["secret"].Value);
        }

        return secretWordsFound;
    }

    private static SyntaxToken? FindIdentifier(SyntaxNode node) =>
        node switch
        {
            AccessorDeclarationSyntax accessorDeclaration => accessorDeclaration.Parent.Parent.GetIdentifier(),
            AssignmentExpressionSyntax assignmentExpression => assignmentExpression.Left.GetIdentifier(),
            _ => node.GetIdentifier()
        };

    private static SyntaxNode FindRightHandSide(SyntaxNode node) =>
        node switch
        {
            AssignmentExpressionSyntax assignmentExpression => assignmentExpression.Right,
            VariableDeclaratorSyntax variableDeclarator => variableDeclarator.Initializer?.Value,
            PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Initializer?.Value,
            AccessorDeclarationSyntax accessorDeclaration => accessorDeclaration.ExpressionBody()?.Expression,
            _ => null
        };

    private bool IsToken(string value) =>
        ShannonEntropy.Calculate(value) > RandomnessSensibility
        && ValidationPattern.SafeIsMatch(value)
        && MaxLanguageScore > NaturalLanguageDetector.HumanLanguageScore(value);
}
