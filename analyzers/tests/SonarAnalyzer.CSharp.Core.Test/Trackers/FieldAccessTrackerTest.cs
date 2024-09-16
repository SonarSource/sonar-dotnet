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

using SonarAnalyzer.Core.Trackers;
using SonarAnalyzer.CSharp.Core.Trackers;

namespace SonarAnalyzer.CSharp.Core.Test.Trackers
{
    [TestClass]
    public class FieldAccessTrackerTest
    {
        [TestMethod]
        public void MatchSet_CS()
        {
            var tracker = new CSharpFieldAccessTracker();
            var context = CreateContext("assignConst");
            tracker.MatchSet()(context).Should().BeTrue();

            context = CreateContext("read");
            tracker.MatchSet()(context).Should().BeFalse();
        }

        [TestMethod]
        public void AssignedValueIsConstant_CS()
        {
            var tracker = new CSharpFieldAccessTracker();
            var context = CreateContext("assignConst");
            tracker.AssignedValueIsConstant()(context).Should().BeTrue();

            context = CreateContext("assignVariable");
            tracker.AssignedValueIsConstant()(context).Should().BeFalse();

            context = CreateContext("invocationArg");
            tracker.AssignedValueIsConstant()(context).Should().BeFalse();
        }

        private static FieldAccessContext CreateContext(string fieldName)
        {
            const string code = """
                public class Sample
                {
                    private int assignConst;
                    private int assignVariable;
                    private int read;
                    private int invocationArg;

                    private void Usage()
                    {
                        var x = read;
                        assignConst = 42;
                        assignVariable = x;
                        Method(invocationArg);
                    }

                    private void Method(int arg) { }
                }
                """;
            var testCode = new SnippetCompiler(code, false, AnalyzerLanguage.CSharp);
            var node = testCode.GetNodes<IdentifierNameSyntax>().First(x => x.ToString() == fieldName);
            return new FieldAccessContext(testCode.CreateAnalysisContext(node), fieldName);
        }
    }
}
