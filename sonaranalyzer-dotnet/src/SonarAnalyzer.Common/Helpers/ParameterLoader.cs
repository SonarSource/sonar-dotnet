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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    internal static class ParameterLoader
    {
        private const string ParameterConfigurationFileName = "SonarLint.xml";

        internal static void SetParameterValues(ParameterLoadingDiagnosticAnalyzer parameteredAnalyzer,
            AnalyzerOptions options)
        {
            if (ProcessedAnalyzers.ContainsKey(parameteredAnalyzer))
            {
                return;
            }

            var additionalFile = options.AdditionalFiles
                .FirstOrDefault(f => IsSonarLintXml(f.Path));

            if (additionalFile == null)
            {
                return;
            }

            ImmutableList<RuleParameterValues> parameters;
            using (var xtr = XmlReader.Create(additionalFile.Path))
            {
                var xml = XDocument.Load(xtr);
                parameters = ParseParameters(xml);
            }

            var propertyParameterPairs = parameteredAnalyzer.GetType()
                .GetRuntimeProperties()
                .Select(p => new { Property = p, Descriptor = p.GetCustomAttributes<RuleParameterAttribute>().SingleOrDefault() })
                .Where(p => p.Descriptor != null);

            var ids = new HashSet<string>(parameteredAnalyzer.SupportedDiagnostics.Select(diagnostic => diagnostic.Id));
            foreach (var propertyParameterPair in propertyParameterPairs)
            {
                var parameter = parameters
                    .FirstOrDefault(p => ids.Contains(p.RuleId));

                var parameterValue = parameter?.ParameterValues
                    .FirstOrDefault(pv => pv.ParameterKey == propertyParameterPair.Descriptor.Key);

                if (parameterValue == null)
                {
                    return;
                }

                var value = parameterValue.ParameterValue;
                var convertedValue = ChangeParameterType(value, propertyParameterPair.Descriptor.Type);
                propertyParameterPair.Property.SetValue(parameteredAnalyzer, convertedValue);
            }

            ProcessedAnalyzers.AddOrUpdate(parameteredAnalyzer, 0, (a, b) => b);
        }

        internal static bool IsSonarLintXml(string path)
        {
            return ConfigurationFilePathMatchesExpected(path, ParameterConfigurationFileName);
        }

        internal static bool ConfigurationFilePathMatchesExpected(string path, string fileName)
        {
            return new FileInfo(path).Name.Equals(fileName, StringComparison.OrdinalIgnoreCase);
        }

        private static ImmutableList<RuleParameterValues> ParseParameters(XDocument xml)
        {
            var builder = ImmutableList.CreateBuilder<RuleParameterValues>();
            foreach (var rule in xml.Descendants("Rule").Where(e => e.Elements("Parameters").Any()))
            {
                var analyzerId = rule.Elements("Key").Single().Value;

                var parameterValues = rule
                    .Elements("Parameters").Single()
                    .Elements("Parameter")
                    .Select(e => new RuleParameterValue
                    {
                        ParameterKey = e.Elements("Key").Single().Value,
                        ParameterValue = e.Elements("Value").Single().Value
                    });

                var pvs = new RuleParameterValues
                {
                    RuleId = analyzerId
                };
                pvs.ParameterValues.AddRange(parameterValues);

                builder.Add(pvs);
            }

            return builder.ToImmutable();
        }

        private static readonly ConcurrentDictionary<ParameterLoadingDiagnosticAnalyzer, byte> ProcessedAnalyzers =
            new ConcurrentDictionary<ParameterLoadingDiagnosticAnalyzer, byte>();

        private static object ChangeParameterType(string parameter, PropertyType type)
        {
            switch (type)
            {
                case PropertyType.Text:
                case PropertyType.String:
                    return parameter;

                case PropertyType.Integer:
                    return int.Parse(parameter, NumberStyles.None, CultureInfo.InvariantCulture);

                case PropertyType.Boolean:
                    return bool.Parse(parameter);

                default:
                    throw new NotSupportedException();
            }
        }

        private class RuleParameterValues
        {
            public string RuleId { get; set; }

            public List<RuleParameterValue> ParameterValues { get; } = new List<RuleParameterValue>();
        }

        private class RuleParameterValue
        {
            public string ParameterKey { get; set; }

            public string ParameterValue { get; set; }
        }
    }
}
