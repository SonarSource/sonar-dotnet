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
        TestHelper.ReplaceLineEndings(
            $"""
            [{file}]
            build_metadata.AdditionalFiles.TargetPath = {Convert.ToBase64String(Encoding.UTF8.GetBytes(TestHelper.GetRelativePath(rootPath, file)))}
            """, Environment.NewLine);
}
