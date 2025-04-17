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

using SonarAnalyzer.Core.RegularExpressions;

namespace SonarAnalyzer.Core.Rules;

public abstract class EnumNameShouldFollowRegexBase<TSyntaxKind> : ParametrizedDiagnosticAnalyzer
    where TSyntaxKind : struct
{
    protected const string DiagnosticId = "S2342";
    private const string MessageFormat = "Rename the enumeration '{0}' to match the regular expression: '{1}'.";
    private const string DefaultEnumNamePattern = NamingPatterns.PascalCasingPattern;
    private const string DefaultFlagsEnumNamePattern = "^" + NamingPatterns.PascalCasingInternalPattern + "s$";

    private readonly DiagnosticDescriptor rule;

    protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

    [RuleParameter("format", PropertyType.String, "Regular expression used to check the enumeration type names against.", DefaultEnumNamePattern)]
    public string EnumNamePattern { get; set; } = DefaultEnumNamePattern;

    [RuleParameter("flagsAttributeFormat", PropertyType.String, "Regular expression used to check the flags enumeration type names against.", DefaultFlagsEnumNamePattern)]
    public string FlagsEnumNamePattern { get; set; } = DefaultFlagsEnumNamePattern;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

    protected EnumNameShouldFollowRegexBase() =>
        rule = Language.CreateDescriptor(DiagnosticId, MessageFormat, isEnabledByDefault: false);

    protected sealed override void Initialize(SonarParametrizedAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
            {
                var pattern = c.Node.HasFlagsAttribute(c.Model) ? FlagsEnumNamePattern : EnumNamePattern;
                if (Language.Syntax.NodeIdentifier(c.Node) is { } identifier && !NamingPatterns.IsRegexMatch(identifier.ValueText, pattern, true))
                {
                    c.ReportIssue(SupportedDiagnostics[0], identifier, identifier.ValueText, pattern);
                }
            },
            Language.SyntaxKind.EnumDeclaration);
}
