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
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
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
    public void Test()
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
    public void Test()
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
            validator.ValidateTag("ByteString", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("Success", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
            validator.ValidateTag("Result", x => x.Should().BeNull());
            validator.ValidateTag("ObjectField", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
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
            validator.ValidateTag("ByteString", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("Success", x => x.Should().BeNull());
            validator.ValidateTag("Result", x => x.Should().BeNull());
            validator.ValidateTag("ObjectField", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
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
                    x[validator.Symbol("ObjectField")].HasConstraint(ObjectConstraint.Null).Should().BeTrue();
                    x[validator.Symbol("byteString")].HasConstraint(ObjectConstraint.Null).Should().BeTrue();
                    x[validator.Symbol("success")].HasConstraint(BoolConstraint.False).Should().BeTrue();
                    x[validator.Symbol("result")].Should().BeNull();
                },
                x =>
                {
                    x[validator.Symbol("ObjectField")].HasConstraint(ObjectConstraint.Null).Should().BeTrue();
                    x[validator.Symbol("byteString")].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
                    x[validator.Symbol("success")].HasConstraint(BoolConstraint.False).Should().BeTrue();
                    x[validator.Symbol("result")].Should().BeNull();
                },
                x =>
                {
                    x[validator.Symbol("ObjectField")].HasConstraint(ObjectConstraint.Null).Should().BeTrue();
                    x[validator.Symbol("byteString")].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
                    x[validator.Symbol("success")].HasConstraint(BoolConstraint.True).Should().BeTrue();
                    x[validator.Symbol("result")].Should().BeNull();
                });
        }

        [TestMethod]
        public void Invocation_NotNullWhen_Unknown_InstanceMethodResetsFieldConstraints()
        {
            const string code = @"
private object ObjectField;

public void Test()
{
    this.ObjectField = null;
    string byteString = Unknown<string>();
    var success = TryParse(byteString, out var result);
    Tag(""End"", null);
}

public bool TryParse([System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] string s, out object o) { o = null; return true; }";
            var validator = SETestContext.CreateCSMethod(code).Validator;
            validator.TagStates("End").Should().SatisfyRespectively(
                x =>
                {
                    x[validator.Symbol("ObjectField")].HasConstraint<ObjectConstraint>().Should().BeFalse();
                    x[validator.Symbol("byteString")].HasConstraint(ObjectConstraint.Null).Should().BeTrue();
                    x[validator.Symbol("success")].HasConstraint(BoolConstraint.False).Should().BeTrue();
                    x[validator.Symbol("result")].Should().BeNull();
                },
                x =>
                {
                    x[validator.Symbol("ObjectField")].HasConstraint<ObjectConstraint>().Should().BeFalse();
                    x[validator.Symbol("byteString")].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
                    x[validator.Symbol("success")].HasConstraint(BoolConstraint.False).Should().BeTrue();
                    x[validator.Symbol("result")].Should().BeNull();
                },
                x =>
                {
                    x[validator.Symbol("ObjectField")].HasConstraint<ObjectConstraint>().Should().BeFalse();
                    x[validator.Symbol("byteString")].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
                    x[validator.Symbol("success")].HasConstraint(BoolConstraint.True).Should().BeTrue();
                    x[validator.Symbol("result")].Should().BeNull();
                });
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
    var value = {value};
    CustomValidator(""Irrelevant"", value);
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
    var value = {value};
    CustomValidator(""Irrelevant"", value);
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
}
