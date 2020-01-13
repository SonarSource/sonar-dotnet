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
using System.Globalization;

namespace SonarAnalyzer.Common
{
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

        public RuleParameterAttribute(string key, PropertyType type, string description)
            : this(key, type, description, null)
        {
        }

        public RuleParameterAttribute(string key, PropertyType type)
            : this(key, type, null, null)
        {
        }
    }
}
