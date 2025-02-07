/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Core.Common;

[AttributeUsage(AttributeTargets.Property)]
public sealed class RuleParameterAttribute : Attribute
{
    public string Key { get; }
    public string Description { get; }
    public PropertyType Type { get; }
    public string DefaultValue { get; }

    public RuleParameterAttribute(string key, PropertyType type, string description, string defaultValue)
    {
        Key = key;
        Description = description;
        Type = type;
        DefaultValue = defaultValue;
    }

    public RuleParameterAttribute(string key, PropertyType type, string description, int defaultValue)
        : this(key, type, description, defaultValue.ToString(CultureInfo.InvariantCulture))
    {
    }

    public RuleParameterAttribute(string key, PropertyType type, string description, double defaultValue)
        : this(key, type, description, defaultValue.ToString(CultureInfo.InvariantCulture))
    {
    }

    public RuleParameterAttribute(string key, PropertyType type, string description)
        : this(key, type, description, null)
    {
    }

    public RuleParameterAttribute(string key, PropertyType type)
        : this(key, type, null, null)
    {
    }
}
