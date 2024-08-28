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

namespace ITs.JsonParser.Json;

// Format of JSONs in "output/"
public class SarifReport
{
    public SarifRun[] Runs { get; set; }

    public SarifIssue[] AllIssues() => Runs.SelectMany(x => x.Results).ToArray();
}

public class SarifRun
{
    public SarifIssue[] Results { get; set; }
}

public class SarifIssue
{
    public string RuleId { get; set; }
    public string Message { get; set; }
    public SarifLocation[] Locations { get; set; }

    public SarifLocationFile Location => Locations is { Length: > 0 } ? Locations[0].ResultFile : null;

    public string NormalizedUri()
    {
        if (Location is null)
        {
            return null;
        }
        else
        {
            // analyzers/its/Projects/<Project>/...
            var filePath = Location.Uri.Substring(Location.Uri.IndexOf("analyzers/its"));
            // ...#L1-L2
            var suffix = Location.Region.StartLine == Location.Region.EndLine
                ? $"#L{Location.Region.StartLine}"
                : $"#L{Location.Region.StartLine}-L{Location.Region.EndLine}";
            return $"https://github.com/SonarSource/sonar-dotnet-enterprise/blob/master/private/{filePath}{suffix}";
        }
    }
}

public class SarifLocation
{
    public SarifLocationFile ResultFile { get; set; }
}

public class SarifLocationFile
{
    public string Uri { get; set; }
    public SarifRegion Region { get; set; }
}

public class SarifRegion
{
    public int StartLine { get; set; }
    public int StartColumn { get; set; }
    public int EndLine { get; set; }
    public int EndColumn { get; set; }
}
