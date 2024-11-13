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

namespace SonarAnalyzer.Rules;

public abstract class DoNotHardcodeBase<TSyntaxKind> : ParametrizedDiagnosticAnalyzer where TSyntaxKind : struct
{
    protected const char KeywordSeparator = ';';

    protected static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(250);
    protected static readonly Regex ValidKeywordPattern = new(@"^(\?|:\w+|\{\d+[^}]*\}|""|')$", RegexOptions.IgnoreCase | RegexOptions.Compiled, RegexTimeout);

    protected readonly IAnalyzerConfiguration configuration;
    protected string keyWords;
    protected ImmutableList<string> splitKeyWords;
    protected Regex keyWordPattern;

    protected abstract void ExtractKeyWords(string value);
    protected abstract IEnumerable<string> FindKeyWords(string variableName, string variableValue);
    protected abstract bool ShouldRaise(string variableName, string variableValue, out string message);

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
}
