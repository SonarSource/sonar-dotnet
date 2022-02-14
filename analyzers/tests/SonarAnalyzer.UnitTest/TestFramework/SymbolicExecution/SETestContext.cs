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

using System;
using System.Linq;
using SonarAnalyzer.CFG;
using SonarAnalyzer.Common;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution
{
    internal class SETestContext
    {
        public readonly ValidatorTestCheck Validator = new();

        public SETestContext(string code, AnalyzerLanguage language, SymbolicCheck[] additionalChecks)
        {
            const string Separator = "----------";
            var cfg = TestHelper.CompileCfg(code, language);
            var se = new RoslynSymbolicExecution(cfg, additionalChecks.Concat(new[] { Validator }).ToArray());
            Console.WriteLine(Separator);
            Console.Write(CfgSerializer.Serialize(cfg));
            Console.WriteLine(Separator);
            se.Execute();
        }

        public static SETestContext CreateCS(string methodBody, params SymbolicCheck[] additionalChecks) =>
            CreateCS(methodBody, null, additionalChecks);

        public static SETestContext CreateCS(string methodBody, string additionalParameters, params SymbolicCheck[] additionalChecks)
        {
            var code = $@"
using System.Collections.Generic;

public class Sample
{{
    public static int StaticField;
    public static int StaticProperty {{get; set;}}
    public int Property {{get; set;}}
    private int field;

    public void Main(bool boolParameter{additionalParameters})
    {{
        {methodBody}
    }}

    private void Tag(string name, object arg = null) {{ }}
}}";
            return new(code, AnalyzerLanguage.CSharp, additionalChecks);
        }

        public static SETestContext CreateCSMethod(string method, params SymbolicCheck[] additionalChecks) =>
            new($@"
public class Sample
{{
    {method}
}}", AnalyzerLanguage.CSharp, additionalChecks);

        public static SETestContext CreateVB(string methodBody, params SymbolicCheck[] additionalChecks) =>
            CreateVB(methodBody, null, additionalChecks);

        public static SETestContext CreateVB(string methodBody, string additionalParameters, params SymbolicCheck[] additionalChecks)
        {
            var code = $@"
Public Class Sample

    Public Sub Main(BoolParameter As Boolean{additionalParameters})
        {methodBody}
    End Sub

    Private Sub Tag(Name As String, Optional Arg As Object = Nothing)
    End Sub

End Class";
            return new(code, AnalyzerLanguage.VisualBasic, additionalChecks);
        }
    }
}
