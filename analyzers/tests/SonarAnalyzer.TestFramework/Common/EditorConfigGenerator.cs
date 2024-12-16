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

using System.Text;

namespace SonarAnalyzer.TestFramework.Common;

/*
 * This class has copy-pasted logic similar to EditorConfigGenerator in Autoscan .Net.
 * See https://github.com/SonarSource/sonar-dotnet-autoscan/blob/master/AutoScan.NET/Build/EditorConfigGenerator.cs
*/
public class EditorConfigGenerator
{
    private readonly string rootPath;

    public EditorConfigGenerator(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentNullException(nameof(rootPath));
        }
        this.rootPath = rootPath;
    }

    public string Generate(IEnumerable<string> razorFiles) =>
        razorFiles.Select(x => x.Replace('\\', '/')).Select(ToConfigLine).Prepend("is_global = true").JoinStr(Environment.NewLine);

    private string ToConfigLine(string file) =>
        $"""
        [{file}]
        build_metadata.AdditionalFiles.TargetPath = {Convert.ToBase64String(Encoding.UTF8.GetBytes(TestHelper.GetRelativePath(rootPath, file)))}
        """;
}
