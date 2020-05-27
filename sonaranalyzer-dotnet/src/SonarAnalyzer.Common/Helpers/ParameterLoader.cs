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

        /**
         * At each compilation, parse the configuration file and set the rule parameters on
         *
         * There is no caching mechanism because inside the IDE, the configuration file can change when the user:
         * - changes something inside the configuration file
         * - loads a different solution in the IDE
         *
         * If caching needs to be done in the future, it should take into account:
         * - diffing the contents of the configuration file
         * - associating the file with a unique identifier for the build project
         */
        internal static void SetParameterValues(ParameterLoadingDiagnosticAnalyzer parameteredAnalyzer,
            AnalyzerOptions options)
        {
            var additionalFile = options.AdditionalFiles.FirstOrDefault(f => IsSonarLintXml(f.Path));

            if (additionalFile == null)
            {
                return;
            }

            var parameters = ParseParameters(additionalFile.Path);
            if (parameters.IsEmpty)
            {
                return;
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

                if (TryConvertToParameterType(parameterValue?.ParameterValue, propertyParameterPair.Descriptor.Type, out var value))
                {
                    propertyParameterPair.Property.SetValue(parameteredAnalyzer, value);
                }
            }
        }

        internal static bool IsSonarLintXml(string path) => ConfigurationFilePathMatchesExpected(path, ParameterConfigurationFileName);

        internal static bool ConfigurationFilePathMatchesExpected(string path, string fileName) =>
            new FileInfo(path).Name.Equals(fileName, StringComparison.OrdinalIgnoreCase);

        private static ImmutableList<RuleParameterValues> ParseParameters(string path)
        {
            try
            {
                using var xtr = XmlReader.Create(path);
                var xml = XDocument.Load(xtr);
                return ParseParameters(xml);
            }
            catch (Exception ex) when (ex is IOException || ex is XmlException)
            {
                // cannot log exception
                return ImmutableList.Create<RuleParameterValues>();
            }
        }

        private static ImmutableList<RuleParameterValues> ParseParameters(XContainer xml)
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

        private static bool TryConvertToParameterType(string parameter, PropertyType type, out object result)
        {
            if (parameter == null)
            {
                result = null;
                return false;
            }
            switch (type)
            {
                case PropertyType.Text:
                case PropertyType.String:
                    result = parameter;
                    return true;

                case PropertyType.Integer when int.TryParse(parameter, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedInt):
                    result = parsedInt;
                    return true;

                case PropertyType.Boolean when bool.TryParse(parameter, out var parsedBool):
                    result = parsedBool;
                    return true;

                default:
                    result = null;
                    return false;
            }
        }

        private sealed class RuleParameterValues
        {
            public string RuleId { get; set; }

            public List<RuleParameterValue> ParameterValues { get; } = new List<RuleParameterValue>();
        }

        private sealed class RuleParameterValue
        {
            public string ParameterKey { get; set; }

            public string ParameterValue { get; set; }
        }
    }
}
