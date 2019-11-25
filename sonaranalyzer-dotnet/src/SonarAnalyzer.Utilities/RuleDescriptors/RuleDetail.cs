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

using System.Collections.Generic;

namespace SonarAnalyzer.RuleDescriptors
{
    public class RuleDetail
    {
        public RuleDetail()
        {
            Tags = new List<string>();
            Parameters = new List<RuleParameter>();
            CodeFixTitles = new List<string>();
        }

        public string Key { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Severity { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; private set; }
        public List<RuleParameter> Parameters { get; private set; }
        public bool IsActivatedByDefault { get; set; }
        public List<string> CodeFixTitles { get; private set; }
        public string Remediation { get; set; }
        public string RemediationCost { get; set; }
    }
}