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

using System.Reflection;
using Microsoft.CodeAnalysis.CodeFixes;
using SonarAnalyzer.Rules;

namespace SonarAnalyzer.Test.TestFramework
{
    internal static class RuleFinder
    {
        public static IEnumerable<Type> AllAnalyzerTypes { get; }       // Rules and Utility analyzers
        public static IEnumerable<Type> AllTypesWithDiagnosticAnalyzerAttribute { get; }
        public static IEnumerable<Type> RuleAnalyzerTypes { get; }      // Rules-only, without Utility analyzers
        public static IEnumerable<Type> UtilityAnalyzerTypes { get; }
        public static IEnumerable<Type> CodeFixTypes { get; }

        static RuleFinder()
        {
            var allTypes = new[]
            {
                typeof(SonarAnalyzer.Rules.CSharp.FlagsEnumZeroMember),
                typeof(SonarAnalyzer.Rules.VisualBasic.FlagsEnumZeroMember),
                typeof(FlagsEnumZeroMemberBase<int>)
            }
                .SelectMany(x => x.Assembly.GetExportedTypes())
                .ToArray();
            CodeFixTypes = allTypes.Where(x => typeof(CodeFixProvider).IsAssignableFrom(x) && x.GetCustomAttributes<ExportCodeFixProviderAttribute>().Any()).ToArray();
            AllAnalyzerTypes = allTypes.Where(x => x.IsSubclassOf(typeof(DiagnosticAnalyzer)) && x.GetCustomAttributes<DiagnosticAnalyzerAttribute>().Any()).ToArray();
            AllTypesWithDiagnosticAnalyzerAttribute = allTypes.Where(x => x.GetCustomAttributes<DiagnosticAnalyzerAttribute>().Any()).ToArray();
            UtilityAnalyzerTypes = AllAnalyzerTypes.Where(x => typeof(UtilityAnalyzerBase).IsAssignableFrom(x)).ToList();
            RuleAnalyzerTypes = AllAnalyzerTypes.Except(UtilityAnalyzerTypes).ToList();
        }

        public static IEnumerable<Type> GetAnalyzerTypes(AnalyzerLanguage language) =>
            RuleAnalyzerTypes.Where(x => TargetLanguage(x) == language);

        public static IEnumerable<DiagnosticAnalyzer> CreateAnalyzers(AnalyzerLanguage language, bool includeUtilityAnalyzers)
        {
            var types = GetAnalyzerTypes(language);
            if (includeUtilityAnalyzers)
            {
                types = types.Concat(UtilityAnalyzerTypes.Where(x => TargetLanguage(x) == language));
            }
            foreach (var type in types)
            {
                yield return typeof(HotspotDiagnosticAnalyzer).IsAssignableFrom(type) && type.GetConstructor(new[] { typeof(IAnalyzerConfiguration) }) != null
                    ? (DiagnosticAnalyzer)Activator.CreateInstance(type, AnalyzerConfiguration.AlwaysEnabled)
                    : (DiagnosticAnalyzer)Activator.CreateInstance(type);
            }
        }

        public static bool IsParameterized(Type analyzerType) =>
            analyzerType.GetProperties().Any(x => x.GetCustomAttributes<RuleParameterAttribute>().Any());

        private static AnalyzerLanguage TargetLanguage(MemberInfo analyzerType)
        {
            var languages = analyzerType.GetCustomAttributes<DiagnosticAnalyzerAttribute>().SingleOrDefault()?.Languages
                ?? throw new NotSupportedException($"Can not find any language for the given type {analyzerType.Name}!");
            return languages.Length == 1
                ? AnalyzerLanguage.FromName(languages.Single())
                : throw new NotSupportedException($"Analyzer can not have multiple languages: {analyzerType.Name}");
        }
    }
}
