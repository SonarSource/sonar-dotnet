/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.TestFramework.Common;

public static class TestFiles
{
    public static string ToUnixLineEndings(this string value) =>
        value.Replace(TestConstants.WindowsLineEnding, TestConstants.UnixLineEnding);

    public static string TestPath(TestContext context, string fileName)
    {
        var root = Path.Combine(context.TestRunDirectory, context.FullyQualifiedTestClassName.Replace("SonarAnalyzer.Test.", null));
        var directoryName = root.Length + context.TestName.Length + fileName.Length > 250   // 260 can throw PathTooLongException
            ? $"TooLongTestName.{RootSubdirectoryCount()}"
            : context.TestName;
        var path = Path.Combine(root, directoryName, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        return path;

        int RootSubdirectoryCount() =>
            Directory.Exists(root) ? Directory.GetDirectories(root).Length : 0;
    }

    public static string WriteFile(TestContext context, string fileName, string content = null)
    {
        var path = TestPath(context, fileName);
        File.WriteAllText(path, content);
        return path;
    }

    public static string GetRelativePath(string relativeTo, string path)
    {
        var itemPath = Path.GetFullPath(path);
        var isDirectory = path.EndsWith(Path.DirectorySeparatorChar.ToString());
        var p1 = itemPath.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);
        var p2 = relativeTo.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);

        var i = 0;
        while (i < p1.Length
            && i < p2.Length
            && string.Equals(p1[i], p2[i], StringComparison.OrdinalIgnoreCase))
        {
            i++;
        }

        if (i == 0)
        {
            return itemPath;
        }
        var relativePath = Path.Combine(Enumerable.Repeat("..", p2.Length - i).Concat(p1.Skip(i).Take(p1.Length - i)).ToArray());
        if (isDirectory && p1.Length >= p2.Length)
        {
            relativePath += Path.DirectorySeparatorChar;
        }
        return relativePath;
    }
}
