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
