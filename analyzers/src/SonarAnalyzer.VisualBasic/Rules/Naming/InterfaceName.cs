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

namespace SonarAnalyzer.VisualBasic.Rules;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class InterfaceName : ParametrizedDiagnosticAnalyzer
{
    private const string DiagnosticId = "S114";
    private const string MessageFormat = "Rename this interface to match the regular expression: '{0}'.";
    private const string DefaultPattern = "^I" + NamingPatterns.PascalCasingInternalPattern + "$";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat, false);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    [RuleParameter("format", PropertyType.String, "Regular expression used to check the interface names against.", DefaultPattern)]
    public string Pattern { get; set; } = DefaultPattern;

    protected override void Initialize(SonarParametrizedAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var declaration = (InterfaceStatementSyntax)c.Node;
                if (!NamingPatterns.IsRegexMatch(declaration.Identifier.ValueText, Pattern))
                {
                    c.ReportIssue(Rule, declaration.Identifier, Pattern);
                }
            },
            SyntaxKind.InterfaceStatement);
}
