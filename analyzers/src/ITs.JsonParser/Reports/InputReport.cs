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

using ITs.JsonParser.Json;

namespace ITs.JsonParser.Reports;

/// <summary>
/// SARIF report generated during the build process.
/// </summary>
public class InputReport
{
    public string Project { get; }
    public string Assembly { get; }
    public string Tfm { get; }
    public SarifReport Sarif { get; }

    public InputReport(string path, string project, JsonSerializerOptions options)
    {
        Project = project;
        Console.WriteLine($"Processing {path}...");
        // .../project/assembly{-TFM}?.json
        var fileName = Path.GetFileNameWithoutExtension(path);
        var index = fileName.LastIndexOf('-');
        (Assembly, Tfm) = index >= 0
            ? (fileName.Substring(0, index), fileName.Substring(index + 1))
            : (fileName, null);
        Sarif = JsonSerializer.Deserialize<SarifReport>(File.ReadAllText(path), options);
        ConsoleHelper.WriteLineColor($"Successfully parsed {this}", ConsoleColor.Green);
    }

    public override string ToString() =>
        $"{Project}/{Assembly} [{Tfm}]";
}
