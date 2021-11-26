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

using System.Linq;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution
{
    internal class SETestContext
    {
        public readonly CollectorTestCheck Collector = new();
        private readonly RoslynSymbolicExecution se;

        public SETestContext(string code, bool isCSharp, SymbolicExecutionCheck[] additionalChecks)
        {
            var cfg = TestHelper.CompileCfg(code, isCSharp);
            se = new RoslynSymbolicExecution(cfg, additionalChecks.Concat(new[] { Collector }).ToArray());
            se.Execute();
        }

        public static SETestContext CreateCS(string methodBody, params SymbolicExecutionCheck[] additionalChecks) =>
            CreateCS(methodBody, null, additionalChecks);

        public static SETestContext CreateCS(string methodBody, string additionalParameters, params SymbolicExecutionCheck[] additionalChecks)
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
            return new SETestContext(code, true, additionalChecks);
        }

        public static SETestContext CreateVB(string methodBody, params SymbolicExecutionCheck[] additionalChecks) =>
            CreateVB(methodBody, null, additionalChecks);

        public static SETestContext CreateVB(string methodBody, string additionalParameters, params SymbolicExecutionCheck[] additionalChecks)
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
            return new SETestContext(code, false, additionalChecks);
        }
    }
}
