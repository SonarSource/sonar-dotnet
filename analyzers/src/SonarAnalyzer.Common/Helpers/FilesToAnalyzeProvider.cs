/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SonarAnalyzer.Helpers
{
    public class FilesToAnalyzeProvider
    {
        private readonly Lazy<IEnumerable<string>> allFiles;

        public FilesToAnalyzeProvider(string filePath)
        {
            allFiles = new Lazy<IEnumerable<string>>(() => RetrieveFilesToAnalyze(filePath));
        }

        public IEnumerable<string> FindFiles(string fileName) =>
            allFiles.Value.Where(x => Path.GetFileName(x).Equals(fileName, StringComparison.OrdinalIgnoreCase));

        public IEnumerable<string> FindFiles(Regex fullPathRegex) =>
            allFiles.Value.Where(x => fullPathRegex.IsMatch(x));

        private static IEnumerable<string> RetrieveFilesToAnalyze(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return Enumerable.Empty<string>();
            }

            try
            {
                return File.ReadAllLines(filePath);
            }
            catch
            {
                // cannot log exception
                return Enumerable.Empty<string>();
            }
        }
    }
}
