/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

#if NETFRAMEWORK
using System.Resources;
#endif
using System.IO;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    /// <summary>
    /// This exception class is used in unit tests that analyze some code to report a violation.
    /// It differs from classical unit test failure reporting in that it injects in its call
    /// stack a location that correspond to the source file that is being analyzed (when analyzing
    /// inline code snippets, this location will be incorrect).
    /// As a result, in the UI, we can just click on this line to directly open the right file at
    /// the right place to see the actual issue.
    /// </summary>
    public abstract class TestfileDiagnosticException : Exception
    {
        private readonly string additionalLine;

        protected TestfileDiagnosticException(Location location, string message) : this(location.GetLineSpan().Path, location.GetLineNumberToReport(), message)
        {
        }

        protected TestfileDiagnosticException(string filePath, int lineNumber, string message) : base(message)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }
            var testCasePath = GetTestCasePath(filePath);

#if NETFRAMEWORK
            additionalLine = NetFrameworkFormatter.GetAdditionalLine(testCasePath, lineNumber) + Environment.NewLine;
#else
            additionalLine = NetCoreFormatter.GetAdditionalLine(testCasePath, lineNumber) + Environment.NewLine;
#endif
        }

        public override string StackTrace =>
            additionalLine + base.StackTrace;

        private static string GetTestCasePath(string path) =>
            Path.GetFullPath(Path.Combine(@"..\..\..\TestCases", path));

#if NETFRAMEWORK
        private static class NetFrameworkFormatter
        {
            private static readonly ResourceManager MscorlibResources = new ResourceManager("mscorlib", typeof(object).Assembly);
            private static readonly string At = MscorlibResources.GetString("Word_At");
            private static readonly string StackTraceFormat = MscorlibResources.GetString("StackTrace_InFileLineNumber");

            internal static string GetAdditionalLine(string path, int line) =>
                $"{At} Test Case {GetStackTraceLocation(path, line)}";

            private static string GetStackTraceLocation(string testCasePath, int testCaseLine) =>
                string.Format(StackTraceFormat, testCasePath, testCaseLine);
        }
#else
        private static class NetCoreFormatter
        {
            internal static string GetAdditionalLine(string path, int line) =>
                $"at Test Case in {path}:line {line}";
        }
#endif
    }

    /// <inheritdoc/>
    public sealed class UnexpectedDiagnosticException : TestfileDiagnosticException
    {
        public UnexpectedDiagnosticException(Location location, string message) : base(location, message) { }
    }

    /// <inheritdoc/>
    public sealed class MissingDiagnosticException : TestfileDiagnosticException
    {
        public MissingDiagnosticException(string filePath, int lineNumber, string message) : base(filePath, lineNumber, message) { }
    }
}
