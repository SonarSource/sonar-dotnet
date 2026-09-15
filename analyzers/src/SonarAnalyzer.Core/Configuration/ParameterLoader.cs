/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

namespace SonarAnalyzer.Core.Configuration;

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

        var ids = new HashSet<string>(parameteredAnalyzer.SupportedDiagnostics.Select(x => x.Id));
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

    internal static bool TryConvertToParameterType(string parameter, PropertyType type, out object result)
    {
        result = parameter is null ? null : ConvertToParameterType(parameter, type);
        return result is not null;
    }

    // Returns null when the user-provided value cannot be parsed, so that the property keeps its default value.
    // An unhandled PropertyType is a bug on our side, so it throws instead of silently ignoring the parameter (NET-4546).
    private static object ConvertToParameterType(string parameter, PropertyType type) =>
        type switch
        {
            PropertyType.String or PropertyType.Text or PropertyType.RegularExpression => parameter,
            PropertyType.Integer => int.TryParse(parameter, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedInt) ? parsedInt : null,
            PropertyType.Float => double.TryParse(parameter, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedDouble) ? parsedDouble : null,
            PropertyType.Boolean => bool.TryParse(parameter, out var parsedBool) ? parsedBool : null,
            _ => throw new UnexpectedValueException(nameof(type), type)
        };
}
