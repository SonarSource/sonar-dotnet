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

using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SonarAnalyzer.Core.Json;
using SonarAnalyzer.Core.Json.Parsing;

namespace SonarAnalyzer.Core.Rules;

public abstract class DoNotHardcodeBase<TSyntaxKind> : ParametrizedDiagnosticAnalyzer where TSyntaxKind : struct
{
    protected const char KeywordSeparator = ';';

    protected static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(250);
    protected static readonly Regex ValidKeywordPattern = new(@"^(\?|:\w+|\{\d+[^}]*\}|""|')$", RegexOptions.IgnoreCase | RegexOptions.Compiled, RegexTimeout);

    protected readonly IAnalyzerConfiguration configuration;
    protected string keyWords;
    protected DiagnosticDescriptor rule;
    protected ImmutableList<string> splitKeyWords;
    protected Regex keyWordPattern;

    protected abstract void ExtractKeyWords(string value);
    protected abstract string FindIssue(string variableName, string variableValue);

    protected abstract string DiagnosticId { get; }
    protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

    public string FilterWords
    {
        get => keyWords;
        set => ExtractKeyWords(value);
    }

    protected DoNotHardcodeBase(IAnalyzerConfiguration configuration)
    {
        this.configuration = configuration;
    }

    protected bool IsEnabled(AnalyzerOptions options)
    {
        configuration.Initialize(options);
        return configuration.IsEnabled(DiagnosticId);
    }

    protected static ImmutableList<string> SplitKeyWordsByComma(string keyWords) =>
        keyWords.ToUpperInvariant()
            .Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => x.Length != 0)
            .ToImmutableList();

    protected static bool IsValidKeyword(string suffix)
    {
        var candidateKeyword = suffix.Split(KeywordSeparator)[0].Trim();
        return string.IsNullOrWhiteSpace(candidateKeyword) || ValidKeywordPattern.SafeIsMatch(candidateKeyword);
    }

    protected void CheckWebConfig(SonarCompilationReportingContext context)
    {
        if (!IsEnabled(context.Options))
        {
            return;
        }

        foreach (var path in context.WebConfigFiles())
        {
            if (File.ReadAllText(path).ParseXDocument() is { } doc)
            {
                CheckWebConfig(context, path, doc.Descendants());
            }
        }
    }

    protected void CheckAppSettings(SonarCompilationReportingContext context) =>
        CheckJsonSettings(context, context.AppSettingsFiles());

    protected void CheckLaunchSettings(SonarCompilationReportingContext context) =>
        CheckJsonSettings(context, context.LaunchSettingsFiles());

    private void CheckJsonSettings(SonarCompilationReportingContext context, IEnumerable<string> jsonFiles)
    {
        if (!IsEnabled(context.Options))
        {
            return;
        }

        foreach (var path in jsonFiles)
        {
            if (JsonNode.FromString(File.ReadAllText(path)) is { } json)
            {
                var walker = new CredentialWordsJsonWalker(this, context, path);
                walker.Visit(json);
            }
        }
    }

    private void CheckWebConfig(SonarCompilationReportingContext context, string path, IEnumerable<XElement> elements)
    {
        foreach (var element in elements)
        {
            if (!element.HasElements && FindIssue(element.Name.LocalName, element.Value) is { } message && element.CreateLocation(path) is { } elementLocation)
            {
                context.ReportIssue(Language.GeneratedCodeRecognizer, rule, elementLocation, message);
            }
            foreach (var attribute in element.Attributes())
            {
                if (FindIssue(attribute.Name.LocalName, attribute.Value) is { } attributeMessage && attribute.CreateLocation(path) is { } attributeLocation)
                {
                    context.ReportIssue(Language.GeneratedCodeRecognizer, rule, attributeLocation, attributeMessage);
                }
            }
        }
    }

    private sealed class CredentialWordsJsonWalker : JsonWalker
    {
        private readonly DoNotHardcodeBase<TSyntaxKind> analyzer;
        private readonly SonarCompilationReportingContext context;
        private readonly string path;

        public CredentialWordsJsonWalker(DoNotHardcodeBase<TSyntaxKind> analyzer, SonarCompilationReportingContext context, string path)
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
            if (value.Value is string str && analyzer.FindIssue(key, str) is { } message)
            {
                context.ReportIssue(analyzer.Language.GeneratedCodeRecognizer, analyzer.rule, value.ToLocation(path), message);
            }
        }
    }
}
