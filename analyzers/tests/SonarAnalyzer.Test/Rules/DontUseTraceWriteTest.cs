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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class DontUseTraceWriteTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<DontUseTraceWrite>();

    [TestMethod]
    public void DontUseTraceWrite_CS() =>
        builder.AddPaths("DontUseTraceWrite.cs").Verify();

    [TestMethod]
    public void DontUseTraceWrite_CustomTraceClass_CS() =>
        builder.AddSnippet("""
            public class Program
            {
                public void Method(string arg)
                {
                    Trace.Write("Message");                 // Compliant - the method is not from the System.Diagnostics.Trace class
                    Trace.Write("Message: {0}", arg);
                    Trace.WriteLine("Message");
                    Trace.WriteLine("Message: {0}", arg);
                }
            }

            public class Trace
            {
                public static void Write(string message) { }
                public static void Write(string message, params object[] args) { }
                public static void WriteLine(string message) { }
                public static void WriteLine(string message, params object[] args) { }
            }
            """).Verify();
}
