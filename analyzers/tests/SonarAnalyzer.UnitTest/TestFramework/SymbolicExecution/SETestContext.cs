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

using SonarAnalyzer.Common;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution
{
    internal class SETestContext
    {
        public readonly ValidatorTestCheck Validator;

        public SETestContext(string code,
                             AnalyzerLanguage language,
                             SymbolicCheck[] additionalChecks,
                             string localFunctionName = null,
                             string anonymousFunctionFragment = null,
                             OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
        {
            var cfg = TestHelper.CompileCfg(code, language, false, localFunctionName, anonymousFunctionFragment, outputKind);
            Validator = new ValidatorTestCheck(cfg);
            var se = new RoslynSymbolicExecution(cfg, additionalChecks.Concat(new[] { Validator }).ToArray(), default);
            se.Execute();
        }

        public static SETestContext CreateCS(string methodBody, params SymbolicCheck[] additionalChecks) =>
            CreateCS(methodBody, null, null, additionalChecks);

        public static SETestContext CreateCS(string methodBody, string additionalParameters, params SymbolicCheck[] additionalChecks) =>
            CreateCS(methodBody, additionalParameters, null, additionalChecks);

        public static SETestContext CreateCS(string methodBody, string additionalParameters, string localFunctionName, params SymbolicCheck[] additionalChecks) =>
            new(ClassCodeCS(methodBody, additionalParameters), AnalyzerLanguage.CSharp, additionalChecks, localFunctionName);

        public static SETestContext CreateCSLambda(string methodBody, string lambdaFragment, params SymbolicCheck[] additionalChecks) =>
            new(ClassCodeCS(methodBody, null), AnalyzerLanguage.CSharp, additionalChecks, null, lambdaFragment);

        public static SETestContext CreateCSMethod(string method, params SymbolicCheck[] additionalChecks) =>
            new($@"
using System;

public class Sample
{{
    {method}

    private static void Tag(string name, object arg) {{ }}
    private static T Unknown<T>() => default;
}}

public class Deconstructable
{{
    public void Deconstruct(out int A, out int B) {{ A = 1; B = 2; }}
}}", AnalyzerLanguage.CSharp, additionalChecks);

        public static SETestContext CreateVB(string methodBody, params SymbolicCheck[] additionalChecks) =>
            CreateVB(methodBody, null, additionalChecks);

        public static SETestContext CreateVB(string methodBody, string additionalParameters, params SymbolicCheck[] additionalChecks)
        {
            var code = $@"
Public Class Sample

    Private Readonly Property Condition As Boolean = Environment.ProcessorCount = 42    ' Something that cannot have constraint
    Private FieldArray() As Integer

    Public Sub Main(BoolParameter As Boolean{additionalParameters})
        {methodBody}
    End Sub

    Private Shared Sub Tag(Name As String, Optional Arg As Object = Nothing)
    End Sub

End Class";
            return new(code, AnalyzerLanguage.VisualBasic, additionalChecks);
        }

        public static SETestContext CreateVBMethod(string method, params SymbolicCheck[] additionalChecks) =>
            new($@"
Public Class Sample

    {method}

    Private Shared Sub Tag(Name As String, Optional Arg As Object = Nothing)
    End Sub

End Class", AnalyzerLanguage.VisualBasic, additionalChecks);

        private static string ClassCodeCS(string methodBody, string additionalParameters) =>
            $@"
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public unsafe class Sample
{{
    public object ObjectField;
    public static int StaticField;
    public static object StaticObjectField;
    public static int StaticProperty {{ get; set; }}
    public static event EventHandler StaticEvent;
    public event EventHandler Event;
    public int Property {{ get; set; }}
    public Sample SampleProperty {{ get; set; }}
    public NotImplementedException PropertyException {{ get; set; }}
    public int this[int index] {{get => 42; set {{ }} }}
    private int field;
    private NotImplementedException fieldException;

    private bool Condition => Environment.ProcessorCount == 42;  // Something that cannot have constraint

    public Sample(){{ }}
    public Sample(int i){{ }}

    public void Main(bool boolParameter{additionalParameters})
    {{
        {methodBody}
    }}

    public NotImplementedException CreateException() => new NotImplementedException();
    public void InstanceMethod(object parameter = null) {{ }}
    public static void StaticMethod() {{ }}

    private void Tag(string name) {{ }}
    private static void Tag(string name, object arg = null) {{ }}
    private static T Unknown<T>() => default;
}}

public class Person : PersonBase
{{
    public static string StaticProperty {{ get; set; }}
    public string Field;
    public event EventHandler Event;
    public string Method() => null;
    public static void StaticMethod() {{ }}
}}

public class PersonBase
{{
}}";
    }
}
