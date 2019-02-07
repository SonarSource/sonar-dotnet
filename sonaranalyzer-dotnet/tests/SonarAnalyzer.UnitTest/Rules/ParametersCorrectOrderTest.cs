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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ParametersCorrectOrderTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void ParametersCorrectOrder_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\ParametersCorrectOrder.cs",
                new CS.ParametersCorrectOrder());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void ParametersCorrectOrder_InvalidCode_CS()
        {
            Verifier.VerifyCSharpAnalyzer(@"
public class Foo
{
    public void Bar()
    {
        new Foo
        new ()
        new System. ()
    }
}", new CS.ParametersCorrectOrder(), checkMode: CompilationErrorBehavior.Ignore);
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void ParametersCorrectOrder_VB()
        {
            Verifier.VerifyAnalyzer(@"TestCases\ParametersCorrectOrder.vb",
                new VB.ParametersCorrectOrder());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void ParametersCorrectOrder_InvalidCode_VB()
        {
            Verifier.VerifyVisualBasicAnalyzer(@"
Public Class Foo
    Public Sub Bar()
        Dim x = New ()
        Dim y = New System. ()
    End Sub
End Class", new VB.ParametersCorrectOrder(), checkMode: CompilationErrorBehavior.Ignore);
        }
    }
}

