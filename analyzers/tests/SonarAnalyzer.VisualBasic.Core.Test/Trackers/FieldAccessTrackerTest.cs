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

using SonarAnalyzer.Helpers;
using SonarAnalyzer.VisualBasic.Core.Trackers;

namespace SonarAnalyzer.VisualBasic.Core.Test.Trackers;

[TestClass]
public class FieldAccessTrackerTest
{
    [TestMethod]
    public void MatchSet_VB()
    {
        var tracker = new VisualBasicFieldAccessTracker();
        var context = CreateContext("AssignConst");
        tracker.MatchSet()(context).Should().BeTrue();

        context = CreateContext("Read");
        tracker.MatchSet()(context).Should().BeFalse();
    }

    [TestMethod]
    public void AssignedValueIsConstant_VB()
    {
        var tracker = new VisualBasicFieldAccessTracker();
        var context = CreateContext("AssignConst");
        tracker.AssignedValueIsConstant()(context).Should().BeTrue();

        context = CreateContext("AssignVariable");
        tracker.AssignedValueIsConstant()(context).Should().BeFalse();

        context = CreateContext("InvocationArg");
        tracker.AssignedValueIsConstant()(context).Should().BeFalse();
    }

    private static FieldAccessContext CreateContext(string fieldName)
    {
        const string code = """
            Public Class Sample
                Private AssignConst As Integer
                Private AssignVariable As Integer
                Private Read As Integer
                Private InvocationArg As Integer

                Public Sub Usage()
                    Dim X As Integer = Read
                    AssignConst = 42
                    AssignVariable = X
                    Method(InvocationArg)
                End Sub

                Private Sub Method(Arg As Integer)
                End Sub
            End Class
            """;
        var compiler = new SnippetCompiler(code, false, AnalyzerLanguage.VisualBasic);
        var node = compiler.GetNodes<IdentifierNameSyntax>().First(x => x.ToString() == fieldName);
        return new FieldAccessContext(compiler.CreateAnalysisContext(node), fieldName);
    }
}
