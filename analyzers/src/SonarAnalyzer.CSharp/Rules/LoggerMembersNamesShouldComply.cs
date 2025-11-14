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

using System.Text.RegularExpressions;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LoggerMembersNamesShouldComply : ParametrizedDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6669";
    private const string MessageFormat = "Rename this {0} '{1}' to match the regular expression '{2}'.";
    private const string DefaultFormat = "^_?[Ll]og(ger)?$"; // unused unless the user changes the regex

    private static readonly ImmutableHashSet<string> DefaultAllowedNames = ImmutableHashSet.Create(
        "log",
        "Log",
        "_log",
        "_Log",
        "logger",
        "Logger",
        "_logger",
        "_Logger",
        "instance",
        "Instance"); // "Instance" is a common name for singleton pattern

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat, isEnabledByDefault: false);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    [RuleParameter("format", PropertyType.RegularExpression, "Regular expression used to check the field or property names against", DefaultFormat)]
    public string Format { get; set; } = DefaultFormat;

    private bool UsesDefaultFormat => Format == DefaultFormat;

    private Regex NameRegex { get; set; }

    private static readonly ImmutableArray<KnownType> Loggers = ImmutableArray.Create(
        KnownType.Microsoft_Extensions_Logging_ILogger,
        KnownType.Microsoft_Extensions_Logging_ILogger_TCategoryName,
        KnownType.Serilog_ILogger,
        KnownType.NLog_ILogger,
        KnownType.NLog_ILoggerBase,
        KnownType.NLog_Logger,
        KnownType.log4net_ILog,
        KnownType.log4net_Core_ILogger,
        KnownType.Castle_Core_Logging_ILogger);

    private static readonly KnownAssembly[] Assemblies =
    [
        KnownAssembly.MicrosoftExtensionsLoggingAbstractions,
        KnownAssembly.Serilog,
        KnownAssembly.NLog,
        KnownAssembly.Log4Net,
        KnownAssembly.CastleCore
    ];

    protected override void Initialize(SonarParametrizedAnalysisContext context) =>
        context.RegisterCompilationStartAction(cc =>
        {
            if (cc.Compilation.ReferencesAny(Assemblies))
            {
                NameRegex = UsesDefaultFormat ? null : new(Format, RegexOptions.Compiled, Constants.DefaultRegexTimeout);

                cc.RegisterNodeAction(c =>
                {
                    foreach (var memberData in Declarations(c.Node))
                    {
                        if (!MatchesFormat(memberData.Name)
                            && c.Model.GetDeclaredSymbol(memberData.Member).GetSymbolType() is { } type
                            && type.DerivesOrImplementsAny(Loggers))
                        {
                            c.ReportIssue(Rule, memberData.Location, memberData.MemberType, memberData.Name, Format);
                        }
                    }
                },
                SyntaxKind.FieldDeclaration,
                SyntaxKind.PropertyDeclaration);
            }
        });

    private bool MatchesFormat(string name) =>
        UsesDefaultFormat
        ? DefaultAllowedNames.Contains(name) // for performance, if the user doesn't change the regex, we can use a hashtable lookup
        : NameRegex.SafeIsMatch(name);

    private static IEnumerable<MemberData> Declarations(SyntaxNode node)
    {
        if (node is FieldDeclarationSyntax field)
        {
            // can be multiple variables in a single declaration
            foreach (var variable in field.Declaration.Variables)
            {
                yield return new(variable, variable.Identifier.GetLocation(), variable.Identifier.ValueText, false);
            }
        }
        else if (node is PropertyDeclarationSyntax property)
        {
            yield return new(property, property.Identifier.GetLocation(), property.Identifier.ValueText, true);
        }
    }

    private readonly record struct MemberData(SyntaxNode Member, Location Location, string Name, bool IsProperty)
    {
        public readonly string MemberType => IsProperty ? "property" : "field";
    }
}
