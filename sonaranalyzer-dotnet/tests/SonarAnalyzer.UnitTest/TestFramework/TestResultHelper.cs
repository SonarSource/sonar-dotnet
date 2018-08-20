/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest
{
    public class TestResultHelper
    {
        /// <summary>
        /// Returns the date of the previous test run. If no other test runs were found returns DateTime.MinValue.
        /// </summary>
        public static DateTime GetPreviousRunDate(TestContext context) =>
            GetPreviousRunDate(
                Directory.GetDirectories(Path.GetDirectoryName(context.TestRunDirectory)));

        public static DateTime GetPreviousRunDate(IEnumerable<string> directoryNames)
        {
            // Directory names are alphabetically comparable, skip the latest result
            var previousTestRunDir = directoryNames
                .OrderByDescending(name => name)
                .Skip(1)
                .FirstOrDefault();

            // Coalesce to avoid NRE in case there are less than 2 directories
            return ParseDate(previousTestRunDir ?? string.Empty);
        }

        /// <summary>
        /// Returns parsed DateTime from MSBuild test run directory name. If cannot parse, returns DateTime.MinValue.
        /// </summary>
        public static DateTime ParseDate(string testRunDirectory)
        {
            // The format is "Deploy_Valeri Hristov 2018-08-17 14_52_41"
            var match = Regex.Match(testRunDirectory, "(?<year>\\d{4})-(?<month>\\d{2})-(?<day>\\d{2}) \\d{2}_\\d{2}_\\d{2}");
            if (match.Success &&
                match.Groups.Count == 4 &&
                int.TryParse(match.Groups["year"].Value, out var year) &&
                int.TryParse(match.Groups["month"].Value, out var month) &&
                int.TryParse(match.Groups["day"].Value, out var day))
            {
                return new DateTime(year, month, day);
            }
            return DateTime.MinValue;
        }
    }
}
