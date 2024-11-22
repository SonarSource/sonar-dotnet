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

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using SonarAnalyzer.Common;

namespace RuleDescriptorGenerator;

[ExcludeFromCodeCoverage]
public class RuleParameter
{
    // To speed up the use of reflection in the PropertyValue() method for the 4 properties, we cache the PropertyInfo instances.
    // It's only used in a single-threaded Console application.
    // The RuleParameter constructor is called hundreds of times, so it's worth caching the PropertyInfo instances.
    private static readonly Dictionary<string, PropertyInfo> PropertyInfoCache = [];

    public string Key { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public string DefaultValue { get; set; }

    public RuleParameter() { }

    public RuleParameter(object attribute)
    {
        Key = PropertyValue<string>(attribute, nameof(RuleParameterAttribute.Key));
        Description = PropertyValue<string>(attribute, nameof(RuleParameterAttribute.Description));
        Type = ToServerApi(PropertyValue<int>(attribute, nameof(RuleParameterAttribute.Type)));
        DefaultValue = PropertyValue<string>(attribute, nameof(RuleParameterAttribute.DefaultValue));
    }

    /// <summary>
    /// sonar-plugin-api / RuleParamTypeTest.java contains other possibilities how to serialize enum values with single-select and multi-select:
    /// SINGLE_SELECT_LIST,multiple=false,values="First,Second,Third"
    /// SINGLE_SELECT_LIST,multiple=true,values="First,Second,Third"
    /// </summary>
    private static string ToServerApi(int type) =>
        type switch
        {
            (int)PropertyType.String => "STRING",
            (int)PropertyType.Text => "TEXT",
            (int)PropertyType.Boolean => "BOOLEAN",
            (int)PropertyType.Integer => "INTEGER",
            (int)PropertyType.Float => "FLOAT",
            (int)PropertyType.RegularExpression => "REGULAR_EXPRESSION",  // This will be translated to STRING by RuleParamType.parse() on the sonar-plugin-api side
            _ => throw new UnexpectedValueException(nameof(type), type.ToString())
        };

    // Reflection must be used because of the merged assemblies in the pipeline.
    // RuleDescriptionGenerator will not recognize the RuleParameterAttribute type because it's no longer in the SonarAnalyzer.Common module, as it was during compilation.
    private static T PropertyValue<T>(object target, string propertyName)
    {
        if (!PropertyInfoCache.TryGetValue(propertyName, out var propertyInfo))
        {
            propertyInfo = target.GetType().GetProperty(propertyName);
            PropertyInfoCache[propertyName] = propertyInfo;
        }
        return (T)propertyInfo.GetValue(target, null);
    }
}
