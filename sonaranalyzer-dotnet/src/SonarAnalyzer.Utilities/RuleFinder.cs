/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

namespace SonarAnalyzer.Utilities
{
    public class RuleFinder
    {
        private readonly IReadOnlyList<Type> diagnosticAnalyzers;

        public static IEnumerable<Assembly> PackagedRuleAssemblies { get; } =
            new[]
            {
                Assembly.Load(typeof(Rules.CSharp.FlagsEnumZeroMember).Assembly.GetName()),
                Assembly.Load(typeof(Rules.VisualBasic.FlagsEnumZeroMember).Assembly.GetName()),
                Assembly.Load(typeof(Rules.Common.FlagsEnumZeroMemberBase).Assembly.GetName())
            };

        public RuleFinder()
        {
            diagnosticAnalyzers = PackagedRuleAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(DiagnosticAnalyzer)) || typeof(IRuleFactory).IsAssignableFrom(t))
                .Where(t => t.GetCustomAttributes<RuleAttribute>().Any())
                .ToList();
        }

        public static IEnumerable<SonarDiagnosticAnalyzer> GetAnalyzers() =>
            PackagedRuleAssemblies.SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && typeof(SonarDiagnosticAnalyzer).IsAssignableFrom(type))
                .Select(type => (SonarDiagnosticAnalyzer)Activator.CreateInstance(type));

        public IEnumerable<Type> GetParameterlessAnalyzerTypes(AnalyzerLanguage language)
        {
            return this.diagnosticAnalyzers
                .Where(analyzerType => !IsParameterized(analyzerType))
                .Where(type => GetTargetLanguages(type).IsAlso(language));
        }

        public static bool IsParameterized(Type analyzerType)
        {
            return analyzerType.GetProperties()
                .Any(p => p.GetCustomAttributes<RuleParameterAttribute>().Any());
        }

        public IEnumerable<Type> AllAnalyzerTypes => diagnosticAnalyzers;

        public IEnumerable<Type> GetAnalyzerTypes(AnalyzerLanguage language)
        {
            return this.diagnosticAnalyzers
                .Where(type => GetTargetLanguages(type).IsAlso(language));
        }

        public static IEnumerable<Type> GetUtilityAnalyzerTypes(AnalyzerLanguage language)
        {
            return PackagedRuleAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Rules.UtilityAnalyzerBase)))
                .Where(type => GetTargetLanguages(type).IsAlso(language));
        }

        private static AnalyzerLanguage GetTargetLanguages(MemberInfo analyzerType)
        {
            var languages = GetLanguages(analyzerType);

            return languages.Aggregate(AnalyzerLanguage.None, (current, lang) => lang switch
            {
                LanguageNames.CSharp => current.AddLanguage(AnalyzerLanguage.CSharp),
                LanguageNames.VisualBasic => current.AddLanguage(AnalyzerLanguage.VisualBasic),
                _ => current
            });
        }

        private static IEnumerable<string> GetLanguages(MemberInfo analyzerType)
        {
            var diagnosticAttribute = analyzerType.GetCustomAttributes<DiagnosticAnalyzerAttribute>().FirstOrDefault();

            return diagnosticAttribute == null
                ? analyzerType.GetCustomAttributes<RuleAttribute>().FirstOrDefault()?.Languages
                : diagnosticAttribute.Languages;
        }
    }
}
