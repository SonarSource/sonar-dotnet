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

namespace SonarAnalyzer.Helpers
{
    public class FilesToAnalyzeRetriever : IFilesToAnalyzeRetriever
    {
        public IEnumerable<string> RetrieveFilesToAnalyze(string filePath)
        {
            try
            {
                return File.ReadAllLines(filePath);
            }
            catch (Exception ex) when (ex is IOException || ex is ArgumentNullException || ex is ArgumentException)
            {
                // cannot log exception
                return Enumerable.Empty<string>();
            }
        }
    }
}
