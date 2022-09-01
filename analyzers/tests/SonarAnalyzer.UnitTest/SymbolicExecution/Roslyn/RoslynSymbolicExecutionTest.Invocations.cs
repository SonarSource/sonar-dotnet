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
        [TestMethod]
        public void InstanceReference_SetsNotNull_VB()
        {
            const string code = @"
Dim FromMe As Sample = Me
Tag(""Me"", FromMe)";
            var validator = SETestContext.CreateVB(code).Validator;
            validator.ValidateContainsOperation(OperationKind.InstanceReference);
            validator.ValidateTag("Me", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
        }

        [TestMethod]
        public void Invocation_SetsNotNullOnInstance_CS()
        {
            const string code = @"
public class Sample
{
    public void Main(Sample instanceArg, Sample extensionArg)
    {
        var preserve = true;
        Sample extensionNull = null;
        Tag(""BeforeInstance"", instanceArg);
        Tag(""BeforeExtensionArg"", extensionArg);
        Tag(""BeforeExtensionNull"", extensionNull);
        Tag(""BeforePreserve"", preserve);

        instanceArg.InstanceMethod();
        extensionArg.ExtensionMethod();
        UntrackedSymbol().InstanceMethod(); // Is not invoked on any symbol, should not fail
        preserve.ExtensionMethod();
        preserve.ToString();

        Tag(""AfterInstance"", instanceArg);
        Tag(""AfterExtensionArg"", extensionArg);
        Tag(""AfterExtensionNull"", extensionNull);
        Tag(""AfterPreserve"", preserve);
    }

    private void InstanceMethod() { }
    private static void Tag(string name, object arg) { }
    private Sample UntrackedSymbol() => this;
}

public static class Extensions
{
    public static void ExtensionMethod(this Sample s) { }
    public static void ExtensionMethod(this bool b) { }
}";
            var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
            validator.ValidateContainsOperation(OperationKind.Invocation);
            validator.ValidateTag("BeforeInstance", x => x.Should().BeNull());
            validator.ValidateTag("BeforeExtensionArg", x => x.Should().BeNull());
            validator.ValidateTag("BeforeExtensionNull", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue());
            validator.ValidateTag("BeforePreserve", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
            validator.ValidateTag("AfterInstance", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue("Instance method should set NotNull constraint."));
            validator.ValidateTag("AfterExtensionArg", x => x.Should().BeNull("Extensions can run on null instances."));
            validator.ValidateTag("AfterExtensionNull", x => x.HasConstraint(ObjectConstraint.Null).Should().BeTrue("Extensions can run on null instances."));
            validator.ValidateTag("AfterPreserve", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue("Other constraints should not be removed."));
        }

        [TestMethod]
        public void Invocation_SetsNotNullOnInstance_VB()
        {
            const string code = @"
Public Class Sample

    Public Sub Main(InstanceArg As Sample, StaticArg As Sample, ExtensionArg As Sample)
        Tag(""BeforeInstance"", InstanceArg)
        Tag(""BeforeStatic"", StaticArg)
        Tag(""BeforeExtension"", ExtensionArg)

        InstanceArg.InstanceMethod()
        StaticArg.StaticMethod()
        ExtensionArg.ExtensionMethod()

        Tag(""AfterInstance"", InstanceArg)
        Tag(""AfterStatic"", StaticArg)
        Tag(""AfterExtension"", ExtensionArg)
    End Sub

    Private Sub InstanceMethod()
    End Sub

    Private Shared Sub StaticMethod()
    End Sub

    Private Shared Sub Tag(Name As String, Arg As Object)
    End Sub

End Class

Public Module Extensions

    <Runtime.CompilerServices.Extension>
    Public Sub ExtensionMethod(S As Sample)
    End Sub

End Module";
            var validator = new SETestContext(code, AnalyzerLanguage.VisualBasic, Array.Empty<SymbolicCheck>()).Validator;
            validator.ValidateContainsOperation(OperationKind.ObjectCreation);
            validator.ValidateTag("BeforeInstance", x => x.Should().BeNull());
            validator.ValidateTag("BeforeStatic", x => x.Should().BeNull());
            validator.ValidateTag("BeforeExtension", x => x.Should().BeNull());
            validator.ValidateTag("AfterInstance", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue("Instance method should set NotNull constraint."));
            validator.ValidateTag("AfterStatic", x => x.Should().BeNull("Static method can execute from null instances."));
            validator.ValidateTag("AfterExtension", x => x.Should().BeNull("Extensions can run on null instances."));
        }

        [DataTestMethod]
        [DataRow("DoSomething();")]
        [DataRow("this.DoSomething();")]
        [DataRow("(this).DoSomething();")]
        [DataRow("(((this))).DoSomething();")]
        public void InstanceMethodCallClearsFieldOnThis(string invocation)
        {
            var code = $@"
public class Sample
{{
    object field1 = null;
    object field2 = null;

    void CallToMethodsShouldResetFieldConstraints()
    {{
        field1 = new object();
        field2 = new object();
        {invocation}
        Tag(""Field1"", field1);
        Tag(""Field2"", field2);
    }}

    private void DoSomething() {{ }}
    private static void Tag(string name, object arg) {{ }}
}}";
            var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
            validator.ValidateContainsOperation(OperationKind.Invocation);
            // validator.ValidateTag("Field1", x => x.Should().BeNull());
            // validator.ValidateTag("Field2", x => x.Should().BeNull());
        }

        [DataTestMethod]
        [DataRow("otherInstance.DoSomething();")]
        [DataRow("(otherInstance).DoSomething();")]
        [DataRow("(true ? this : otherInstance).DoSomething();")]
        public void InstanceMethodCallClearsFieldOnThisOnly(string invocation)
        {
            var code = $@"
public class Sample
{{
    object field1 = null;
    object field2 = null;

    void CallToMethodsShouldResetFieldConstraints()
    {{
        field1 = new object();
        field2 = new object();
        var otherInstance = new Sample();
        {invocation}
        Tag(""Field1"", field1);
        Tag(""Field2"", field2);
    }}

    private void DoSomething() {{ }}
    private static void Tag(string name, object arg) {{ }}
}}";
            var validator = new SETestContext(code, AnalyzerLanguage.CSharp, Array.Empty<SymbolicCheck>()).Validator;
            validator.ValidateContainsOperation(OperationKind.Invocation);
            validator.ValidateTag("Field1", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("Field2", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
        }
    }
}
