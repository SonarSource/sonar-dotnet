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

using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

public partial class RoslynSymbolicExecutionTest
{
    [DataTestMethod]
    [DataRow("System.Diagnostics.CodeAnalysis.DoesNotReturn")]
    [DataRow("System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute")]
    [DataRow("OtherNamespace.DoesNotReturn")]
    [DataRow("OtherNamespace.DoesNotReturnAttribute")]
    [DataRow("JetBrains.Annotations.TerminatesProgram")]
    [DataRow("JetBrains.Annotations.TerminatesProgramAttribute")]
    [DataRow("OtherNamespace.TerminatesProgram")]
    [DataRow("OtherNamespace.TerminatesProgramAttribute")]
    public void Invocation_ThrowHelper_Attributes(string throwHelperAttribute)
    {
        var code = $@"
using System;
using System.Diagnostics;

public class Sample
{{
    public void Main()
    {{
        Tag(""Before"");
        ThrowHelper();
        Tag(""Unreachable"");
    }}

    [{throwHelperAttribute}]
    public void ThrowHelper()
    {{
        // No implementation. The attribute should drive the analysis.
    }}

    static void Tag(string name) {{ }}
}}

namespace JetBrains.Annotations
{{
    public sealed class TerminatesProgramAttribute : Attribute {{ }}
}}
namespace OtherNamespace
{{
    public sealed class TerminatesProgramAttribute : Attribute {{ }}
    public sealed class DoesNotReturnAttribute : Attribute {{ }}
}}";
#if NETFRAMEWORK
        code += @"
namespace System.Diagnostics.CodeAnalysis
{
    public sealed class DoesNotReturnAttribute : Attribute { }
}";
#endif
        var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
        validator.ValidateTagOrder("Before");
        validator.ValidateExitReachCount(0);
        validator.ValidateExecutionCompleted();
    }

    [DataTestMethod]
    [DataRow("DoesTerminatesProgramAttribute")]
    [DataRow("TerminatesMyProgramAttribute")]
    [DataRow("TerminatesProgramAttributeS")]
    [DataRow("DoesNotReturnEver")]
    [DataRow("ItDoesNotReturn")]
    public void Invocation_ThrowHelper_OtherAttributes_NotSupported(string attributeName)
    {
        var code = $@"
using System;
using System.Diagnostics;

public class Sample
{{
    public void Main()
    {{
        Tag(""Before"");
        ThrowHelper();
        Tag(""After"");
    }}

    [{attributeName}]
    public void ThrowHelper()
    {{
        // No implementation. The attribute should drive the analysis.
    }}

    static void Tag(string name) {{ }}
}}

public sealed class {attributeName}: Attribute {{ }}";
        var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
        validator.ValidateTagOrder("Before", "After");
        validator.ValidateExitReachCount(1);
        validator.ValidateExecutionCompleted();
    }

#if NET

    [TestMethod]
    public void Invocation_NotNullWhen_Null()
    {
        const string code = @"
this.ObjectField = null;
string byteString = null;
var success = byte.TryParse(byteString, out var result); // bool TryParse([NotNullWhen(true)] string? s, out byte result)
Tag(""ByteString"", byteString);
Tag(""Success"", success);
Tag(""Result"", result);
Tag(""ObjectField"", ObjectField);";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("ByteString").Should().HaveOnlyConstraint(ObjectConstraint.Null);
        validator.TagValue("Success").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False);
        validator.TagValue("Result").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("ObjectField").Should().HaveOnlyConstraint(ObjectConstraint.Null);
    }

    [TestMethod]
    public void Invocation_NotNullWhen_NotNull()
    {
        const string code = @"
this.ObjectField = null;
string byteString = ""42"";
var success = byte.TryParse(byteString, out var result); // bool TryParse([NotNullWhen(true)] string? s, out byte result)
Tag(""ByteString"", byteString);
Tag(""Success"", success);
Tag(""Result"", result);
Tag(""ObjectField"", ObjectField);";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("ByteString").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Success").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Result").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("ObjectField").Should().HaveOnlyConstraint(ObjectConstraint.Null);
    }

    [TestMethod]
    public void Invocation_NotNullWhen_Unknown()
    {
        const string code = @"
this.ObjectField = null;
string byteString = Unknown<string>();
var success = byte.TryParse(byteString, out var result);
Tag(""End"", null);";
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagStates("End").Should().SatisfyRespectively(
            x =>
            {
                x[validator.Symbol("ObjectField")].Should().HaveOnlyConstraint(ObjectConstraint.Null);
                x[validator.Symbol("byteString")].Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
                x[validator.Symbol("success")].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True);
                x[validator.Symbol("result")].Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
            },
            x =>
            {
                x[validator.Symbol("ObjectField")].Should().HaveOnlyConstraint(ObjectConstraint.Null);
                x[validator.Symbol("byteString")].Should().HaveNoConstraints();
                x[validator.Symbol("success")].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False);
                x[validator.Symbol("result")].Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
            });
    }

    [TestMethod]
    public void Invocation_NotNullWhen_Unknown_InstanceMethodResetsFieldConstraints()
    {
        const string code = """
            private object ObjectField;
            public void Test()
            {
                this.ObjectField = null;
                string byteString = Unknown<string>();
                var success = TryParse(byteString, out var result);
                Tag("End", null);
            }
            public bool TryParse([System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] string s, out object o) { o = null; return true; }
            """;
        var validator = SETestContext.CreateCSMethod(code).Validator;
        validator.TagStates("End").Should().SatisfyRespectively(
            x =>
            {
                x[validator.Symbol("ObjectField")].Should().HaveNoConstraints();
                x[validator.Symbol("byteString")].Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
                x[validator.Symbol("success")].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True);
                x[validator.Symbol("result")].Should().HaveNoConstraints();
            },
            x =>
            {
                x[validator.Symbol("ObjectField")].Should().HaveNoConstraints();
                x[validator.Symbol("byteString")].Should().HaveNoConstraints();
                x[validator.Symbol("success")].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.False);
                x[validator.Symbol("result")].Should().HaveNoConstraints();
            });
    }

    [TestMethod]
    public void Invocation_NotNullWhen_TwoParametersWithAttribute_Unknown()
    {
        const string code = @"
public void Main()
{
var first = Unknown<object>();
var second = Unknown<object>();
if(CustomValidator(first, second))
{
    Tag(""First"", first);
    Tag(""Second"", second);
}
}

public bool CustomValidator([System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] object first, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] object second) => true;";
        var validator = SETestContext.CreateCSMethod(code).Validator;
        validator.TagValue("First").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Second").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void Invocation_NotNullWhen_TwoParametersWithAttribute_ContradictingValues()
    {
        const string code = @"
public void Main()
{
var first = Unknown<object>();
object second = null;
if(CustomValidator(first, second))
{
    Tag(""First"", first);
    Tag(""Second"", second);
}
}

public bool CustomValidator([System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] object first, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] object second) => true;";
        var validator = SETestContext.CreateCSMethod(code).Validator;
        validator.TagValue("First").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);   // This path should be unreachable instead
        validator.TagValue("Second").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void Invocation_NotNullWhen_TwoParametersWithAttribute_UntrackedSymbol()
    {
        const string code = @"
private object field;

public void Main(Sample untracked)
{
var first = Unknown<object>();
untracked.field = null;
if(CustomValidator(first, untracked.field))
{
    Tag(""First"", first);
    Tag(""Second"", untracked.field);
}
}

public bool CustomValidator([System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] object first, [System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] object second) => true;";
        var validator = SETestContext.CreateCSMethod(code).Validator;
        validator.TagValue("First").Should().HaveOnlyConstraint(ObjectConstraint.NotNull);
        validator.TagValue("Second").Should().BeNull();  // We didn't learn anything. And we continued
    }

    [DataTestMethod]
    [DataRow("null", "false")]
    [DataRow("new object()", "true")]
    public void Invocation_DoesNotReturnIf_Continues(string value, string stopsIf)
    {
        var code = $@"
public void Main()
{{
Tag(""Begin"");
object value = {value};
CustomValidator(""Irrelevant"", value == null);
Tag(""End"");
}}

public void CustomValidator(object irrelevant, [System.Diagnostics.CodeAnalysis.DoesNotReturnIf({stopsIf})] bool condition) {{ }}";
        var validator = SETestContext.CreateCSMethod(code).Validator;
        validator.ValidateTagOrder("Begin", "End");
        validator.ValidateExitReachCount(1);
        validator.ValidateExecutionCompleted();
    }

    [DataTestMethod]
    [DataRow("null", "true")]
    [DataRow("new object()", "false")]
    public void Invocation_DoesNotReturnIf_Stops(string value, string stopsIf)
    {
        var code = $@"
public void Main()
{{
Tag(""Begin"");
object value = {value};
CustomValidator(""Irrelevant"", value == null);
Tag(""Unreachable"");
}}

public void CustomValidator(object irrelevant, [System.Diagnostics.CodeAnalysis.DoesNotReturnIf({stopsIf})] bool condition) {{ }}";
        var validator = SETestContext.CreateCSMethod(code).Validator;
        validator.ValidateTagOrder("Begin");
        validator.ValidateExitReachCount(0);
        validator.ValidateExecutionCompleted();
    }

    [DataTestMethod]
    [DataRow("true", "false")]
    [DataRow("!false", "false")]
    [DataRow("false", "true")]
    [DataRow("!true", "true")]
    public void Invocation_DoesNotReturnIf_BoolSymbol_Continues(string value, string stopsIf)
    {
        var code = $@"
public void Main()
{{
Tag(""Begin"");
CustomValidator(""Irrelevant"", {value});
Tag(""End"");
}}

public void CustomValidator(object irrelevant, [System.Diagnostics.CodeAnalysis.DoesNotReturnIf({stopsIf})] bool condition) {{ }}";
        var validator = SETestContext.CreateCSMethod(code).Validator;
        validator.ValidateTagOrder("Begin", "End");
        validator.ValidateExitReachCount(1);
        validator.ValidateExecutionCompleted();
    }

    [DataTestMethod]
    [DataRow("true", "true")]
    [DataRow("!!true", "true")]
    [DataRow("false", "false")]
    [DataRow("!!false", "false")]
    public void Invocation_DoesNotReturnIf_BoolSymbol_Stops(string value, string stopsIf)
    {
        var code = $@"
public void Main()
{{
Tag(""Begin"");
CustomValidator(""Irrelevant"", {value});
Tag(""Unreachable"");
}}

public void CustomValidator(object irrelevant, [System.Diagnostics.CodeAnalysis.DoesNotReturnIf({stopsIf})] bool condition) {{ }}";
        var validator = SETestContext.CreateCSMethod(code).Validator;
        validator.ValidateTagOrder("Begin");
        validator.ValidateExitReachCount(0);
        validator.ValidateExecutionCompleted();
    }

#endif

}
