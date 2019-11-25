/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.RuleDescriptors;

namespace SonarAnalyzer.Utilities
{
    public static class RuleDetailBuilder
    {
        private const string RuleDescriptionPathPattern = "SonarAnalyzer.Rules.Description.{0}.html";
        internal const string CodeFixProviderSuffix = "CodeFixProvider";

        private static readonly HashSet<string> BackwardsCompatibleTypes = new HashSet<string>
        {
            "BUG",
            "CODE_SMELL",
            "VULNERABILITY",
        };

        private static readonly Assembly SonarAnalyzerUtilitiesAssembly = typeof(RuleDetailBuilder).Assembly;

        public static IEnumerable<RuleDetail> GetAllRuleDetails(AnalyzerLanguage language)
        {
            return new RuleFinder()
                .GetAnalyzerTypes(language)
                .SelectMany(t => GetRuleDetailFromRuleAttributes(t, language));
        }

        public static IEnumerable<RuleDetail> GetParameterlessRuleDetails(AnalyzerLanguage language)
        {
            return new RuleFinder()
                .GetParameterlessAnalyzerTypes(language)
                .SelectMany(t => GetRuleDetailFromRuleAttributes(t, language));
        }

        private static IEnumerable<RuleDetail> GetRuleDetailFromRuleAttributes(Type analyzerType,
            AnalyzerLanguage language)
        {
            return analyzerType.GetCustomAttributes<RuleAttribute>()
                .Select(ruleAttribute => GetRuleDetail(ruleAttribute, analyzerType, language));
        }

        private static RuleDetail GetRuleDetail(RuleAttribute rule, Type analyzerType, AnalyzerLanguage language)
        {
            var resources = new ResourceManager("SonarAnalyzer.RspecStrings", analyzerType.Assembly);

            var ruleDetail = new RuleDetail
            {
                Key = rule.Key,
                Type = GetBackwardsCompatibleType(resources.GetString($"{rule.Key}_Type")),
                Title = resources.GetString($"{rule.Key}_Title"),
                Severity = resources.GetString($"{rule.Key}_Severity"),
                IsActivatedByDefault = bool.Parse(resources.GetString($"{rule.Key}_IsActivatedByDefault")),
                Description = GetResourceHtml(rule, language),
                Remediation = ToSonarQubeRemediationFunction(resources.GetString($"{rule.Key}_Remediation")),
                RemediationCost = resources.GetString($"{rule.Key}_RemediationCost")
            };

            ruleDetail.Tags.AddRange(resources.GetString($"{rule.Key}_Tags").Split(new[] { ',' },
                StringSplitOptions.RemoveEmptyEntries));

            GetParameters(analyzerType, ruleDetail);
            GetCodeFixNames(analyzerType, ruleDetail);

            return ruleDetail;
        }

        private static Type GetCodeFixProviderType(Type analyzerType)
        {
            var typeName = analyzerType.FullName + CodeFixProviderSuffix;
            return analyzerType.Assembly.GetType(typeName);
        }

        private static string ToSonarQubeRemediationFunction(string remediation)
        {
            if (remediation == null)
            {
                return null;
            }

            if (remediation == "Constant/Issue")
            {
                return "CONSTANT_ISSUE";
            }

            return null;
        }

        private static void GetCodeFixNames(Type analyzerType, RuleDetail ruleDetail)
        {
            var codeFixProvider = GetCodeFixProviderType(analyzerType);
            if (codeFixProvider == null)
            {
                return;
            }

            var titles = GetCodeFixTitles(codeFixProvider);

            ruleDetail.CodeFixTitles.AddRange(titles);
        }

        public static IEnumerable<string> GetCodeFixTitles(Type codeFixProvider)
        {
            return GetCodeFixProvidersWithBase(codeFixProvider)
                .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                .Where(field =>
                    field.Name.StartsWith("Title", StringComparison.Ordinal) &&
                    field.FieldType == typeof(string))
                .Select(field => (string)field.GetRawConstantValue());
        }

        private static IEnumerable<Type> GetCodeFixProvidersWithBase(Type codeFixProvider)
        {
            yield return codeFixProvider;

            var baseClass = codeFixProvider.BaseType;
            while (baseClass != null && baseClass != typeof(SonarCodeFixProvider))
            {
                yield return baseClass;
                baseClass = baseClass.BaseType;
            }
        }

        private static void GetParameters(Type analyzerType, RuleDetail ruleDetail)
        {
            var parameters = analyzerType.GetProperties()
                .Select(p => p.GetCustomAttributes<RuleParameterAttribute>().SingleOrDefault());

            foreach (var ruleParameter in parameters
                .WhereNotNull())
            {
                ruleDetail.Parameters.Add(
                    new RuleParameter
                    {
                        DefaultValue = ruleParameter.DefaultValue,
                        Description = ruleParameter.Description,
                        Key = ruleParameter.Key,
                        Type = ruleParameter.Type.ToSonarQubeString()
                    });
            }
        }

        private static string GetResourceHtml(RuleAttribute rule, AnalyzerLanguage language)
        {
            var resources = SonarAnalyzerUtilitiesAssembly.GetManifestResourceNames();
            var resource = GetResource(resources, rule.Key, language);
            if (resource == null)
            {
                throw new InvalidDataException($"Could not locate resource for rule {rule.Key}");
            }

            using (var stream = SonarAnalyzerUtilitiesAssembly.GetManifestResourceStream(resource))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static string GetResource(IEnumerable<string> resources, string key, AnalyzerLanguage language)
        {
            if (language == AnalyzerLanguage.CSharp)
            {
                return resources.FirstOrDefault(r =>
                    r.EndsWith(string.Format(CultureInfo.InvariantCulture, RuleDescriptionPathPattern, key),
                        StringComparison.OrdinalIgnoreCase) ||
                    r.EndsWith(string.Format(CultureInfo.InvariantCulture, RuleDescriptionPathPattern, key + "_cs"),
                        StringComparison.OrdinalIgnoreCase));
            }
            if (language == AnalyzerLanguage.VisualBasic)
            {
                return resources.FirstOrDefault(r =>
                    r.EndsWith(string.Format(CultureInfo.InvariantCulture, RuleDescriptionPathPattern, key),
                        StringComparison.OrdinalIgnoreCase) ||
                    r.EndsWith(string.Format(CultureInfo.InvariantCulture, RuleDescriptionPathPattern, key + "_vb"),
                        StringComparison.OrdinalIgnoreCase));
            }

            throw new ArgumentException("Language needs to be either C# or VB.NET", nameof(language));
        }

        // SonarQube before 7.3 supports only 3 types of issues: BUG, CODE_SMELL and VULNERABILITY.
        // This method returns backwards compatible issue type. The type should be adjusted in
        // AbstractRulesDefinition.
        private static string GetBackwardsCompatibleType(string type) =>
            BackwardsCompatibleTypes.Contains(type)
                ? type
                : "VULNERABILITY";
    }
}
