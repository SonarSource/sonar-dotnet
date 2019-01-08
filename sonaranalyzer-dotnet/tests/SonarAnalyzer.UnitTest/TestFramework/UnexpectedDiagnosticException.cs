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

using System;
using System.IO;
using System.Resources;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    /// <summary>
    /// This exception class is used in unit tests that analyze some code to report a violation.
    /// It differs from classical unit test failure reporting in that it injects in its call
    /// stack a location that correspond to the source file that is being analyzed (when analysing
    /// inline code snippets, this location will be incorrect).
    /// As a result, in the UI, we can just click on this line to directly open the right file at
    /// the right place to see the actual issue.
    /// </summary>
    ///
    public class UnexpectedDiagnosticException : Exception
    {
        private static readonly ResourceManager mscorlibResources = new ResourceManager("mscorlib", typeof(object).Assembly);
        private static readonly string at = mscorlibResources.GetString("Word_At");
        private static readonly string stackTraceFormat = mscorlibResources.GetString("StackTrace_InFileLineNumber");

        private readonly string additionalLine;

        public UnexpectedDiagnosticException(Location location, string message) : base(message)
        {
            var locationPath = location.GetLineSpan().Path;
            if (string.IsNullOrEmpty(locationPath))
            {
                return;
            }

            var testCasePath = GetTestCasePath(locationPath);
            var testCaseLine = location.GetLineNumberToReport();

            additionalLine = $"{at} Test Case {GetStackTraceLocation(testCasePath, testCaseLine)}" + Environment.NewLine;
        }

        public override string StackTrace =>
            additionalLine + base.StackTrace;

        private static string GetStackTraceLocation(string testCasePath, int testCaseLine) =>
            string.Format(stackTraceFormat, testCasePath, testCaseLine);

        private static string GetTestCasePath(string path) =>
            Path.GetFullPath(Path.Combine(@"..\..\..\TestCases", path));
    }
}
