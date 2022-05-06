/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules;

namespace SonarAnalyzer.Utilities
{
    internal static class RuleFinder
    {
        public static IEnumerable<Assembly> PackagedRuleAssemblies { get; } = new[]
            {
                Assembly.Load(typeof(Rules.CSharp.FlagsEnumZeroMember).Assembly.GetName()),
                Assembly.Load(typeof(Rules.VisualBasic.FlagsEnumZeroMember).Assembly.GetName()),
                Assembly.Load(typeof(Rules.Common.DoNotInstantiateSharedClassesBase).Assembly.GetName())
            };

        public static IEnumerable<Type> RuleAnalyzerTypes { get; } // Rule-only, without utility analyzers
        public static IEnumerable<Type> UtilityAnalyzerTypes { get; }

        static RuleFinder()
        {
            var all = PackagedRuleAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(x => x.IsSubclassOf(typeof(DiagnosticAnalyzer)) && x.GetCustomAttributes<DiagnosticAnalyzerAttribute>().Any())
                .ToArray();

            RuleAnalyzerTypes = all.Where(x => !typeof(UtilityAnalyzerBase).IsAssignableFrom(x)).ToList();
            UtilityAnalyzerTypes = all.Where(x => typeof(UtilityAnalyzerBase).IsAssignableFrom(x)).ToList();
        }

        public static IEnumerable<Type> GetAnalyzerTypes(AnalyzerLanguage language) =>
            RuleAnalyzerTypes.Where(type => TargetLanguage(type).IsAlso(language));

        public static IEnumerable<DiagnosticAnalyzer> GetAnalyzers(AnalyzerLanguage language)
        {
            var analyzerTypes = PackagedRuleAssemblies.SelectMany(assembly => assembly.GetTypes())
                                                      .Where(type => !type.IsAbstract && typeof(SonarDiagnosticAnalyzer).IsAssignableFrom(type) && TargetLanguage(type).IsAlso(language));
            foreach (var analyzerType in analyzerTypes)
            {
                yield return typeof(HotspotDiagnosticAnalyzer).IsAssignableFrom(analyzerType) && analyzerType.GetConstructor(new[] { typeof(IAnalyzerConfiguration) }) != null
                    ? (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType, AnalyzerConfiguration.AlwaysEnabled)
                    : (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType);
            }
        }

        public static IEnumerable<Type> GetParameterlessAnalyzerTypes(AnalyzerLanguage language) =>
            RuleAnalyzerTypes.Where(type => !IsParameterized(type) && TargetLanguage(type).IsAlso(language));

        public static bool IsParameterized(Type analyzerType) =>
            analyzerType.GetProperties().Any(p => p.GetCustomAttributes<RuleParameterAttribute>().Any());

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
