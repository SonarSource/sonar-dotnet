/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using System.Globalization;
using System.Reflection;

namespace SonarAnalyzer.Helpers;

internal static class ParameterLoader
{
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
    internal static void SetParameterValues(ParametrizedDiagnosticAnalyzer parameteredAnalyzer, SonarLintXmlReader sonarLintXml)
    {
        if (!sonarLintXml.ParametrizedRules.Any())
        {
            return;
        }

        var propertyParameterPairs = parameteredAnalyzer.GetType()
            .GetRuntimeProperties()
            .Select(x => new { Property = x, Descriptor = x.GetCustomAttributes<RuleParameterAttribute>().SingleOrDefault() })
            .Where(x => x.Descriptor is not null);

        var ids = new HashSet<string>(parameteredAnalyzer.SupportedDiagnostics.Select(diagnostic => diagnostic.Id));
        foreach (var propertyParameterPair in propertyParameterPairs)
        {
            var parameter = sonarLintXml.ParametrizedRules.FirstOrDefault(x => ids.Contains(x.Key));
            var parameterValue = parameter?.Parameters.FirstOrDefault(x => x.Key == propertyParameterPair.Descriptor.Key);
            if (TryConvertToParameterType(parameterValue?.Value, propertyParameterPair.Descriptor.Type, out var value))
            {
                propertyParameterPair.Property.SetValue(parameteredAnalyzer, value);
            }
        }
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
}
