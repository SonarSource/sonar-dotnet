/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
public sealed class ClassName : ParametrizedDiagnosticAnalyzer
{
    private const string DiagnosticId = "S101";
    private const string MessageFormat = "Rename this class to match the regular expression: '{0}'.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat, false);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    [RuleParameter("format", PropertyType.String, "Regular expression used to check the class names against.", NamingPatterns.PascalCasingPattern)]
    public string Pattern { get; set; } = NamingPatterns.PascalCasingPattern;

    protected override void Initialize(SonarParametrizedAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var declaration = (ClassStatementSyntax)c.Node;
                if (!NamingPatterns.IsRegexMatch(declaration.Identifier.ValueText, Pattern))
                {
                    c.ReportIssue(Rule, declaration.Identifier, Pattern);
                }
            },
            SyntaxKind.ClassStatement);
}
