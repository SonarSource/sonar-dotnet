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
public sealed class DoNotHardcodeSecrets : ParametrizedDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6418";
    private const string MessageFormat = "\"{0}\" detected here, make sure this is not a hard-coded secret.";

    private const string DefaultSecretWords = """api[_\-]?key, auth, credential, secret, token""";
    private const double DefaultRandomnessSensibility = 3;

    // https://docs.gitguardian.com/secrets-detection/secrets-detection-engine/detectors/generics/generic_high_entropy_secret#:~:text=Follow%20this%20regular%20expression
    private static readonly Regex ValidationPattern = new(@"^[a-zA-Z0-9_.+/~$-]([a-zA-Z0-9_.+\/=~$-]|\\(?![ntr""])){14,1022}[a-zA-Z0-9_.+/=~$-]$", RegexOptions.None, RegexConstants.DefaultTimeout);

    private readonly IAnalyzerConfiguration configuration;
    private readonly DiagnosticDescriptor rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private string secretWords;
    private Regex secretsPattern;

    [RuleParameter("secretWords", PropertyType.String, "Comma separated list of words identifying potential secret", DefaultSecretWords)]
    public string SecretWords
    {
        get => secretWords;
        set
        {
            secretWords = value;
            secretsPattern = new Regex(SplitCredentialWordsByComma(secretWords).JoinStr("|"), RegexOptions.IgnoreCase, RegexConstants.DefaultTimeout);
        }
    }

    [RuleParameter("randomnessSensibility", PropertyType.Float, "Allows to tune the Randomness Sensibility (from 0 to 10)", DefaultRandomnessSensibility)]
    public double RandomnessSensibility { get; set; } = DefaultRandomnessSensibility;
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

    public DoNotHardcodeSecrets() : this(AnalyzerConfiguration.Hotspot) { }

    public DoNotHardcodeSecrets(IAnalyzerConfiguration configuration)
    {
        SecretWords = DefaultSecretWords;
        this.configuration = configuration;
    }

    protected override void Initialize(SonarParametrizedAnalysisContext context) =>
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
                        && secretsPattern.SafeIsMatch(identifier.ValueText)
                        && FindRightHandSide(node) is { } rhs
                        && rhs.FindStringConstant(c.SemanticModel) is { } secret
                        && IsToken(secret))
                    {
                        c.ReportIssue(rule, rhs, identifier.ValueText);
                    }
                },
            SyntaxKind.AddAssignmentExpression,
            SyntaxKind.SimpleAssignmentExpression,
            SyntaxKind.VariableDeclarator,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.GetAccessorDeclaration,
            SyntaxKind.SetAccessorDeclaration);
            });

    private static ImmutableList<string> SplitCredentialWordsByComma(string credentialWords) =>
        credentialWords.ToUpperInvariant()
        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(x => x.Trim())
        .Where(x => x.Length != 0)
        .ToImmutableList();

    private bool IsEnabled(AnalyzerOptions options)
    {
        configuration.Initialize(options);
        return configuration.IsEnabled(DiagnosticId);
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
        ShannonEntropy.Calculate(value) > RandomnessSensibility && ValidationPattern.SafeIsMatch(value);
}
