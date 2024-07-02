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

using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

public partial class RoslynSymbolicExecutionTest
{
    [TestMethod]
    public void ExceptionCandidate_ArrayElementReference()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    var x = c[42];
}
catch
{
    tag = ""InCatch"";
}
tag = ""AfterCatch"";";
        var validator = SETestContext.CreateCS(code, "char[] c").Validator;
        validator.ValidateContainsOperation(OperationKindEx.ArrayElementReference);
        validator.ValidateTagOrder("BeforeTry", "InTry", "InCatch", "AfterCatch");
        validator.TagStates("InCatch").Should().HaveCount(1).And.ContainSingle(x => HasExceptionOfType(x, "IndexOutOfRangeException"));
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [TestMethod]
    public void ExceptionCandidate_ObjectCreation()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    var x = new String(c, 0, 0); // Can throw ArgumentNullException
}
catch
{
    tag = ""InCatch"";
}
tag = ""AfterCatch"";";
        var validator = SETestContext.CreateCS(code, "char[] c").Validator;
        validator.ValidateContainsOperation(OperationKindEx.ObjectCreation);
        validator.ValidateTagOrder("BeforeTry", "InTry", "InCatch", "AfterCatch");
        validator.TagStates("InCatch").Should().ContainSingle(x => HasUnknownException(x));
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [TestMethod]
    public void ExceptionCandidate_ConversionOperation_Explicit_Narrowing()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    int x = (int)p;
}
catch
{
    tag = ""InCatch"";
}
tag = ""AfterCatch"";";
        var validator = SETestContext.CreateCS(code, "long p").Validator;
        validator.ValidateContainsOperation(OperationKindEx.Conversion);
        validator.ValidateTagOrder("BeforeTry", "InTry", "InCatch", "AfterCatch");
        validator.TagStates("InCatch").Should().ContainSingle(x => HasExceptionOfType(x, "InvalidCastException"));
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [TestMethod]
    public void ExceptionCandidate_ConversionOperation_Explicit_Widening()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    var x = (PersonBase)p;
}
catch
{
    tag = ""UnreachableCatch"";
}
tag = ""AfterCatch"";";
        var validator = SETestContext.CreateCS(code, "Person p").Validator;
        validator.ValidateContainsOperation(OperationKindEx.Conversion);
        validator.ValidateTagOrder("BeforeTry", "InTry", "AfterCatch");
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [TestMethod]
    public void ExceptionCandidate_ConversionOperation_Implicit()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    long x = p;
}
catch
{
    tag = ""UnreachableCatch"";
}
tag = ""AfterCatch"";";
        var validator = SETestContext.CreateCS(code, "int p").Validator;
        validator.ValidateContainsOperation(OperationKindEx.Conversion);
        validator.ValidateTagOrder("BeforeTry", "InTry", "AfterCatch");
        validator.TagStates("AfterCatch").Should().ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void ExceptionCandidate_DynamicIndexerAccess()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    int y = arg[0];
}
catch
{
    tag = ""InCatch"";
}
tag = ""AfterCatch"";";
        var validator = SETestContext.CreateCS(code, "dynamic arg").Validator;
        validator.ValidateContainsOperation(OperationKindEx.DynamicIndexerAccess);
        validator.ValidateTagOrder("BeforeTry", "InTry", "InCatch", "AfterCatch");
        validator.TagStates("InCatch").Should().ContainSingle(x => HasExceptionOfType(x, "IndexOutOfRangeException"));
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [TestMethod]
    public void ExceptionCandidate_DynamicInvocation()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    int y = arg.Invocation();
}
catch
{
    tag = ""InCatch"";
}
tag = ""AfterCatch"";";
        var validator = SETestContext.CreateCS(code, "dynamic arg").Validator;
        validator.ValidateContainsOperation(OperationKindEx.DynamicInvocation);
        validator.ValidateTagOrder("BeforeTry", "InTry", "InCatch", "AfterCatch");
        validator.TagStates("InCatch").Should().ContainSingle(x => HasUnknownException(x));
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [TestMethod]
    public void ExceptionCandidate_DynamicMemberReference()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    int y = arg.Property;
}
catch
{
    tag = ""InCatch"";
}
tag = ""AfterCatch"";";
        var validator = SETestContext.CreateCS(code, "dynamic arg").Validator;
        validator.ValidateContainsOperation(OperationKindEx.DynamicMemberReference);
        validator.ValidateTagOrder("BeforeTry", "InTry", "InCatch", "AfterCatch");
        validator.TagStates("InCatch").Should().ContainSingle(x => HasUnknownException(x));
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [TestMethod]
    public void ExceptionCandidate_DynamicObjectCreation()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    var x = new Sample(d);
}
catch
{
    tag = ""InCatch"";
}
tag = ""AfterCatch"";";
        var validator = SETestContext.CreateCS(code, "dynamic d").Validator;
        validator.ValidateContainsOperation(OperationKindEx.DynamicObjectCreation);
        validator.ValidateTagOrder("BeforeTry", "InTry", "InCatch", "AfterCatch");
        validator.TagStates("InCatch").Should().ContainSingle(x => HasUnknownException(x));
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [TestMethod]
    public void ExceptionCandidate_EventReference_Instance()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    p.Event += (sender, args) => { };
}
catch
{
    tag = ""InCatch"";
}
tag = ""AfterCatch"";";
        var validator = SETestContext.CreateCS(code, "Person p").Validator;
        validator.ValidateContainsOperation(OperationKindEx.EventReference);
        validator.ValidateTagOrder("BeforeTry", "InTry", "InCatch", "AfterCatch");
        validator.TagStates("InCatch").Should().ContainSingle(x => HasExceptionOfType(x, "NullReferenceException"));
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [TestMethod]
    public void ExceptionCandidate_EventReference_Static()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    StaticEvent += (sender, args) => { };
}
catch
{
    tag = ""UnreachableCatch"";
}
tag = ""AfterCatch"";";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKindEx.EventReference);
        validator.ValidateTagOrder("BeforeTry", "InTry", "AfterCatch");
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [DataTestMethod]
    [DataRow("this.Event")]
    [DataRow("Event")]
    public void ExceptionCandidate_EventReference_This(string eventReference)
    {
        var code = $@"
var tag = ""BeforeTry"";
try
{{
    tag = ""InTry"";
    {eventReference} += (sender, args) => {{ }};
}}
catch
{{
    tag = ""UnreachableCatch"";
}}
tag = ""AfterCatch"";";
        var validator = SETestContext.CreateCS(code, "Person p").Validator;
        validator.ValidateContainsOperation(OperationKindEx.EventReference);
        validator.ValidateTagOrder("BeforeTry", "InTry", "AfterCatch");
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [TestMethod]
    public void ExceptionCandidate_FieldReference_Instance()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    var x = p.Field;
}
catch
{
    tag = ""InCatch"";
}
tag = ""AfterCatch"";";
        var validator = SETestContext.CreateCS(code, "Person p").Validator;
        validator.ValidateContainsOperation(OperationKindEx.FieldReference);
        validator.ValidateTagOrder("BeforeTry", "InTry", "InCatch", "AfterCatch");
        validator.TagStates("InCatch").Should().ContainSingle(x => HasExceptionOfType(x, "NullReferenceException"));
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [DataTestMethod]
    [DataRow("this.field")]
    [DataRow("field")]
    public void ExceptionCandidate_FieldReference_This(string field)
    {
        var code = $@"
var tag = ""BeforeTry"";
try
{{
    tag = ""InTry"";
    var x = {field};
}}
catch
{{
    tag = ""UnreachableCatch"";
}}
tag = ""AfterCatch"";";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKindEx.FieldReference);
        validator.ValidateTagOrder("BeforeTry", "InTry", "AfterCatch");
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [TestMethod]
    public void ExceptionCandidate_FieldReference_Static()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    var x = DateTime.MaxValue;
}
catch
{
    tag = ""UnreachableCatch"";
}
tag = ""AfterCatch"";";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKindEx.FieldReference);
        validator.ValidateTagOrder("BeforeTry", "InTry", "AfterCatch");
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [TestMethod]
    public void ExceptionCandidate_ForEachLoop()
    {
        const string code = """
            var tag = "BeforeTry";
            try
            {
                tag = "InTry";
                foreach (string element in collection) // can throw if the collection is null or if it gets modified
                {
                }
            }
            catch
            {
                tag = "InCatch";
            }
            tag = "AfterCatch";
            """;
        var validator = SETestContext.CreateCS(code, "IEnumerable<string> collection").Validator;
        validator.ValidateTagOrder("BeforeTry", "InTry", "InCatch", "AfterCatch", "InCatch", "InCatch", "AfterCatch", "AfterCatch");
        // IForEachLoopOperation is not generated. It doesn't seem to be used.
        // In the case of foreach, there are implicit method calls that in the current implementation can throw:
        // - IEnumerable<>.GetEnumerator()
        // - System.Collections.IEnumerator.MoveNext()
        // - System.IDisposable.Dispose()
        validator.TagStates("InCatch").Should().HaveCount(3).And.OnlyContain(x => HasUnknownException(x));
        validator.ExitStates.Should().HaveCount(3).And.OnlyContain(x => HasNoException(x));
    }

    [TestMethod]
    public void ExceptionCandidate_FunctionPointerInvocation()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    ptr(tag);
}
catch
{
    tag = ""UnreachableCatch"";
}
tag = ""AfterCatch"";";

        // IFunctionPointerInvocationOperation is not generated. It doesn't seem used.
        var validator = SETestContext.CreateCS(code, "delegate*<string, void> ptr").Validator;

        validator.ValidateTagOrder("BeforeTry", "InTry", "AfterCatch");
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [TestMethod]
    public void ExceptionCandidate_PropertyReference()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    var x = arg.Length;
}
catch
{
    tag = ""InCatch"";
}
tag = ""AfterCatch"";";

        var validator = SETestContext.CreateCS(code, "string arg").Validator;
        validator.ValidateContainsOperation(OperationKindEx.PropertyReference);
        validator.ValidateTagOrder("BeforeTry", "InTry", "InCatch", "AfterCatch");
        validator.TagStates("InCatch").Should().ContainSingle(x => HasExceptionOfType(x, "NullReferenceException"));
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [TestMethod]
    public void ExceptionCandidate_PropertyReference_Static()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    var x = Person.StaticProperty;
}
catch
{
    tag = ""UnreachableCatch"";
}
tag = ""AfterCatch"";";

        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKindEx.PropertyReference);
        validator.ValidateTagOrder("BeforeTry", "InTry", "AfterCatch");
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [DataTestMethod]
    [DataRow("this.Property")]
    [DataRow("Property")]
    public void ExceptionCandidate_PropertyReference_This(string property)
    {
        var code = $@"
var tag = ""BeforeTry"";
try
{{
    tag = ""InTry"";
    var x = {property};
}}
catch
{{
    tag = ""UnreachableCatch"";
}}
tag = ""AfterCatch"";";

        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKindEx.PropertyReference);
        validator.ValidateTagOrder("BeforeTry", "InTry", "AfterCatch");
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [TestMethod]
    public void ExceptionCandidate_MethodReference()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    var x = p.Method;
}
catch
{
    tag = ""InCatch"";
}
tag = ""AfterCatch"";";

        var validator = SETestContext.CreateCS(code, "Person p").Validator;
        validator.ValidateContainsOperation(OperationKindEx.MethodReference);
        validator.ValidateTagOrder("BeforeTry", "InTry", "InCatch", "AfterCatch");
        validator.TagStates("InCatch").Should().ContainSingle(x => HasExceptionOfType(x, "NullReferenceException"));
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [DataTestMethod]
    [DataRow("this.ToString")]
    [DataRow("ToString")]
    public void ExceptionCandidate_MethodReference_This(string method)
    {
        var code = $@"
var tag = ""BeforeTry"";
try
{{
    tag = ""InTry"";
    var x = {method};
}}
catch
{{
    tag = ""UnreachableCatch"";
}}
tag = ""AfterCatch"";";

        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKindEx.MethodReference);
        validator.ValidateTagOrder("BeforeTry", "InTry", "AfterCatch");
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [TestMethod]
    public void ExceptionCandidate_MethodReference_Static()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    var x = Person.StaticMethod;
}
catch
{
    tag = ""UnreachableCatch"";
}
tag = ""AfterCatch"";";

        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateContainsOperation(OperationKindEx.MethodReference);
        validator.ValidateTagOrder("BeforeTry", "InTry", "AfterCatch");
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [TestMethod]
    public void ExceptionCandidate_End()
    {
        const string code = @"
Module Program
    Sub Main(args As String())
        Dim tag = ""BeforeTry""
        Try
            tag = ""InTry""
            END
        Catch
            tag = ""InCatch""
        End Try
        tag = ""UnreachableAfterCatch""
    End Sub
End Module";

        var validator = new SETestContext(code, AnalyzerLanguage.VisualBasic, Array.Empty<SymbolicCheck>(), outputKind: OutputKind.ConsoleApplication).Validator;

        // End operation is not part of the CFG
        validator.ValidateTagOrder("BeforeTry", "InTry");
        validator.ExitStates.Should().HaveCount(0);
    }

#if NET

    [TestMethod]
    public void ExceptionCandidate_Index_Array()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
tag = ""InTry"";
_ = array[^0];
}
catch
{
tag = ""InCatch"";
}
tag = ""AfterCatch"";";

        var validator = SETestContext.CreateCS(code, "int[] array").Validator;
        validator.ValidateContainsOperation(OperationKindEx.MethodReference);
        validator.ValidateTagOrder("BeforeTry", "InTry", "InCatch", "AfterCatch");
        validator.TagStates("InCatch").Should().ContainSingle(x => HasExceptionOfType(x, "IndexOutOfRangeException"));
        validator.ExitStates.Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void ExceptionCandidate_Index_This()
    {
        const string code = @"
using System;

class Sample
{
public int this[Index i] => 0;

public void Method()
{
    var tag = ""BeforeTry"";
    try
    {
        tag = ""InTry"";
        _ = this[^0];
    }
    catch
    {
        tag = ""UnreachableInCatch"";
    }
    tag = ""AfterCatch"";
}
}";

        var validator = CreateCSharpValidator(code);
        // IImplicitIndexerReferenceOperation is not generated in the current version of Roslyn. It will be generated in 4.4.0.
        validator.ValidateTagOrder("BeforeTry", "InTry", "AfterCatch");
        validator.ValidateHasSingleExitStateAndNoException();
    }

    [TestMethod]
    public void ExceptionCandidate_Range_Array()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
tag = ""InTry"";
_ = array[0..];
}
catch
{
tag = ""InCatch"";
}
tag = ""AfterCatch"";";

        var validator = SETestContext.CreateCS(code, "int[] array").Validator;
        validator.ValidateContainsOperation(OperationKindEx.MethodReference);
        validator.ValidateTagOrder("BeforeTry", "InTry", "InCatch", "AfterCatch");
        validator.TagStates("InCatch").Should().ContainSingle(x => HasExceptionOfType(x, "ArgumentOutOfRangeException"));
        validator.ExitStates.Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void ExceptionCandidate_Range_This()
    {
        const string code = @"
using System;

class Sample
{
public int this[Range r] => 0;

public void Method()
{
    var tag = ""BeforeTry"";
    try
    {
        tag = ""InTry"";
        _ = this[0..];
    }
    catch
    {
        tag = ""InCatch"";
    }
    tag = ""AfterCatch"";
}
}";

        var validator = CreateCSharpValidator(code);
        validator.ValidateContainsOperation(OperationKindEx.Range);
        // IImplicitIndexerReferenceOperation is not generated in the current version of Roslyn. It will be generated in 4.4.0.
        validator.ValidateTagOrder("BeforeTry", "InTry", "AfterCatch");
        validator.ValidateHasSingleExitStateAndNoException();
    }

    private static ValidatorTestCheck CreateCSharpValidator(string code) =>
        new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;

#endif

    [TestMethod]
    public void ExceptionCandidate_Excluded_DoNotThrow()
    {
        const string code = @"
var tag = ""BeforeTry"";
try
{
    tag = ""InTry"";
    System.Threading.Monitor.Exit(tag);    // This invocation would normally throw, but it is excluded from throwing
}
catch
{
    tag = ""UnreachableInCatch"";
}
tag = ""AfterCatch"";
";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.ValidateTagOrder("BeforeTry", "InTry", "AfterCatch");
        validator.ExitStates.Should().HaveCount(1).And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void ExceptionCandidate_DivisionByZero()
    {
        const string code = """
            var tag = "BeforeTry";
            try
            {
                tag = "InTry";
                _ = 42 / canBeZero;
            }
            catch
            {
                tag = "InCatch";
            }
            tag = "AfterCatch";

            """;
        var validator = SETestContext.CreateCS(code, "int canBeZero").Validator;
        validator.ValidateContainsOperation(OperationKindEx.Binary);
        validator.ValidateTagOrder("BeforeTry", "InTry", "InCatch", "AfterCatch");
        validator.TagStates("InCatch").Should().ContainSingle(x => HasExceptionOfType(x, "DivideByZeroException"));
        validator.ExitStates.Should().ContainSingle().And.ContainSingle(x => HasNoException(x));
    }

    [TestMethod]
    public void ExceptionCandidate_DivisionByZero_Remainder()
    {
        const string code = """
            var tag = "BeforeTry";
            try
            {
                tag = "InTry";
                _ = 42 % canBeZero;
            }
            catch
            {
                tag = "InCatch";
            }
            tag = "AfterCatch";

            """;
        var validator = SETestContext.CreateCS(code, "int canBeZero").Validator;
        validator.ValidateContainsOperation(OperationKindEx.Binary);
        validator.ValidateTagOrder("BeforeTry", "InTry", "InCatch", "AfterCatch");
        validator.TagStates("InCatch").Should().ContainSingle(x => HasExceptionOfType(x, "DivideByZeroException"));
        validator.ExitStates.Should().ContainSingle().And.ContainSingle(x => HasNoException(x));
    }
}
