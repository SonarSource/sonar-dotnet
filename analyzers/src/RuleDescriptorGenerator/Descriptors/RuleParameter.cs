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

using System.Diagnostics.CodeAnalysis;
using SonarAnalyzer.Common;

namespace RuleDescriptorGenerator;

[ExcludeFromCodeCoverage]
public class RuleParameter
{
    public string Key { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public string DefaultValue { get; set; }

    public RuleParameter() { }

    public RuleParameter(RuleParameterAttribute attribute)
    {
        Key = attribute.Key;
        Description = attribute.Description;
        Type = ToServerApi(attribute.Type);
        DefaultValue = attribute.DefaultValue;
    }

    /// <summary>
    /// sonar-plugin-api / RuleParamTypeTest.java contains other possibilities how to serialize enum values with single-select and multi-select:
    /// SINGLE_SELECT_LIST,multiple=false,values="First,Second,Third"
    /// SINGLE_SELECT_LIST,multiple=true,values="First,Second,Third"
    /// </summary>
    private static string ToServerApi(PropertyType type) =>
        type switch
        {
            PropertyType.String => "STRING",
            PropertyType.Text => "TEXT",
            PropertyType.Boolean => "BOOLEAN",
            PropertyType.Integer => "INTEGER",
            PropertyType.Float => "FLOAT",
            PropertyType.RegularExpression => "REGULAR_EXPRESSION",  // This will be translated to STRING by RuleParamType.parse() on the sonar-plugin-api side
            _ => throw new UnexpectedValueException(nameof(type), type.ToString())
        };
}
