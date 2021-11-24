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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    [TestClass]
    public class RoslynSymbolicExecutionTest
    {
        [TestMethod]
        public void Constructor_Null_Throws()
        {
            Action a = () => new RoslynSymbolicExecution(null);
            a.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("cfg");
        }

        [TestMethod]
        public void SequentialInput_CS()
        {
            var context = CreateContextCS("var a = true; var b = false; b = !b; a = (b);");
            //FIXME: Assert
        }

        [TestMethod]
        public void SequentialInput_VB()
        {
            var context = CreateContextVB("Dim A As Boolean = True, B As Boolean = False : B = Not B : A = (B)");
            //FIXME: Assert
        }

        private Context CreateContextCS(string methodBody, string additionalParameters = null)
        {
            var code = $@"
public class Sample
{{
    public void Main(bool boolParameter{additionalParameters})
    {{
        {methodBody}
    }}

    private string Method(params string[] args) => null;
    private bool IsMethod(params bool[] args) => true;
}}";
            return new Context(code, true);
        }

        private Context CreateContextVB(string methodBody, string additionalParameters = null)
        {
            var code = $@"
Public Class Sample

    Public Sub Main(BoolParameter As Boolean{additionalParameters})
        {methodBody}
    End Sub

    Private Function Method(ParamArray Args() As String) As String
    End Function

    Private Function IsMethod(ParamArray Args() As Boolean) As Boolean
    End Function

End Class";
            return new Context(code, false);
        }

        private class Context
        {
            private readonly RoslynSymbolicExecution se;

            public Context(string code, bool isCSharp)
            {
                var cfg = TestHelper.CompileCfg(code, isCSharp);
                se = new RoslynSymbolicExecution(cfg);
                se.Execute();
            }
        }
    }
}
