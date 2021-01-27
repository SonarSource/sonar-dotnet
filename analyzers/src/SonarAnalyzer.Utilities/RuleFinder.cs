/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
        public static IEnumerable<Assembly> PackagedRuleAssemblies { get; } = new[]
            {
                Assembly.Load(typeof(Rules.CSharp.FlagsEnumZeroMember).Assembly.GetName()),
                Assembly.Load(typeof(Rules.VisualBasic.FlagsEnumZeroMember).Assembly.GetName()),
                Assembly.Load(typeof(Rules.Common.FlagsEnumZeroMemberBase).Assembly.GetName())
            };

        internal IEnumerable<Type> AllAnalyzerTypes { get; }

        public RuleFinder() =>
            AllAnalyzerTypes = PackagedRuleAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(DiagnosticAnalyzer)) || typeof(IRuleFactory).IsAssignableFrom(t))
                .Where(t => t.GetCustomAttributes<RuleAttribute>().Any())
                .ToList();

        public IEnumerable<Type> GetAnalyzerTypes(AnalyzerLanguage language) =>
            AllAnalyzerTypes.Where(type => GetTargetLanguages(type).IsAlso(language));

        internal static IEnumerable<SonarDiagnosticAnalyzer> GetAnalyzers(AnalyzerLanguage language) =>
            PackagedRuleAssemblies.SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && typeof(SonarDiagnosticAnalyzer).IsAssignableFrom(type) && GetTargetLanguages(type).IsAlso(language))
                .Select(type => (SonarDiagnosticAnalyzer)Activator.CreateInstance(type));

        internal IEnumerable<Type> GetParameterlessAnalyzerTypes(AnalyzerLanguage language) =>
            AllAnalyzerTypes.Where(type => !IsParameterized(type) && GetTargetLanguages(type).IsAlso(language));

        internal static bool IsParameterized(Type analyzerType) =>
            analyzerType.GetProperties().Any(p => p.GetCustomAttributes<RuleParameterAttribute>().Any());

        internal static AnalyzerLanguage GetTargetLanguages(MemberInfo analyzerType) =>
            GetLanguages(analyzerType)
            .Aggregate(AnalyzerLanguage.None, (current, lang) => lang switch
                {
                    LanguageNames.CSharp => current.AddLanguage(AnalyzerLanguage.CSharp),
                    LanguageNames.VisualBasic => current.AddLanguage(AnalyzerLanguage.VisualBasic),
                    _ => current
                });

        private static IEnumerable<string> GetLanguages(MemberInfo analyzerType) =>
            analyzerType.GetCustomAttributes<DiagnosticAnalyzerAttribute>().FirstOrDefault()?.Languages
            ?? analyzerType.GetCustomAttributes<RuleAttribute>().FirstOrDefault()?.Languages
            ?? throw new NotSupportedException($"Can not find any language for the given type {analyzerType.Name}!");
    }
}
