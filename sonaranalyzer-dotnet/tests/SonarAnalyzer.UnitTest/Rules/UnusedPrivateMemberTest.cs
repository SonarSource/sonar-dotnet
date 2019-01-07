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

extern alias csharp;
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class UnusedPrivateMemberTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void Field_Accessibility()
        {
            Verifier.VerifyCSharpAnalyzer(@"
public class PrivateMembers
{
    private int privateField; // Noncompliant {{Remove the unused private field 'privateField'.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^
    private static int privateStaticField; // Noncompliant

    private class InnerPrivateClass // Noncompliant
    {
        internal int internalField; // Noncompliant
        protected int protectedField; // Noncompliant
        protected internal int protectedInternalField; // Noncompliant
        public int publicField; // Noncompliant
        internal static int internalStaticField; // Noncompliant
        protected static int protectedStaticField; // Noncompliant
        protected internal static int protectedInternalStaticField; // Noncompliant
        public static int publicStaticField; // Noncompliant
    }
}

public class NonPrivateMembers
{
    internal int internalField;
    protected int protectedField;
    protected internal int protectedInternalField;
    public int publicField;
    internal static int internalStaticField;
    protected static int protectedStaticField;
    protected internal static int protectedInternalStaticField;
    public static int publicStaticField;

    public class InnerPublicClass
    {
        internal int internalField;
        protected int protectedField;
        protected internal int protectedInternalField;
        public int publicField;
        internal static int internalStaticField;
        protected static int protectedStaticField;
        protected internal static int protectedInternalStaticField;
        public static int publicStaticField;
    }
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Field_MultipleDeclarations()
        {
            Verifier.VerifyCSharpAnalyzer(@"
public class PrivateMembers
{
    private int x, y, z; // Noncompliant {{Remove the unused private field 'x'.}}
//  ^^^^^^^^^^^^^^^^^^^^

    private int a, b, c;
//              ^ {{Remove the unused private field 'a'.}}
//                    ^ @-1 {{Remove the unused private field 'c'.}}

    public int Method1() => b;
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Fields_DirectReferences()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
public class FieldUsages
{
    private int field1;
    private int field2;
    private int field3;
    private int field4;
    private int field5;
    private int field6;
    public int Method1()
    {
        field1 = 0;
        this.field2 = 0;
        int.TryParse(""1"", out field3);
        Console.Write(field4);
        Func<int> x = () => field5;
        return field6;
    }

    private int field7;
    public int ExpressionBodyMethod() => field7;

    private static int field8;
    public int Property { get; set; } = field8;

    public FieldUsages(int number) { }

    private static int field9;
    public FieldUsages() : this(field9) { }

    private int field10;
    private int field11;
    public object Method2()
    {
        var x = new[] { field10 };
        var name = nameof(field11);
        return null;
    }
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Property_Accessibility()
        {
            Verifier.VerifyCSharpAnalyzer(@"
public class PrivateMembers
{
    private int PrivateProperty { get; set; } // Noncompliant {{Remove the unused private property 'PrivateProperty'.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    private static int PrivateStaticProperty { get; set; } // Noncompliant
    private int this[string i] { get { return 5; } set { } } // Noncompliant

    private class InnerPrivateClass // Noncompliant
    {
        internal int InternalProperty { get; set; } // Noncompliant
        protected int ProtectedProperty { get; set; } // Noncompliant
        protected internal int ProtectedInternalProperty { get; set; } // Noncompliant
        public int PublicProperty { get; set; } // Noncompliant
        internal static int InternalStaticProperty { get; set; } // Noncompliant
        protected static int ProtectedStaticProperty { get; set; } // Noncompliant
        protected internal static int ProtectedInternalStaticProperty { get; set; } // Noncompliant
        public static int PublicStaticProperty { get; set; } // Noncompliant
    }
}

public class NonPrivateMembers
{
    internal int InternalProperty { get; set; }
    protected int ProtectedProperty { get; set; }
    protected internal int ProtectedInternalProperty { get; set; }
    public int PublicProperty { get; set; }
    internal static int InternalStaticProperty { get; set; }
    protected static int ProtectedStaticProperty { get; set; }
    protected internal static int ProtectedInternalStaticProperty { get; set; }
    public static int PublicStaticProperty { get; set; }

    public class InnerPublicClass
    {
        internal int InternalProperty { get; set; }
        protected int ProtectedProperty { get; set; }
        protected internal int ProtectedInternalProperty { get; set; }
        public int PublicProperty { get; set; }
        internal static int InternalStaticProperty { get; set; }
        protected static int ProtectedStaticProperty { get; set; }
        protected internal static int ProtectedInternalStaticProperty { get; set; }
        public static int PublicStaticProperty { get; set; }
    }
}

public interface IInterface
{
    int InterfaceProperty { get; set; }
}

public class InterfaceImpl : IInterface
{
    int IInterface.InterfaceProperty { get { return 0; } set { } }
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Properties_DirectReferences()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
public class PropertyUsages
{
    private int Property1 { get; set; }
    private int Property2 { get; set; }
    private int Property4 { get; set; }
    private int Property5 { get; set; }
    private int Property6 { get; set; }
    public int Method1(PropertyUsages other)
    {
        Property1 = 0;
        this.Property2 = 0;
        ((Property4)) = 0;
        Console.Write(Property4);
        new PropertyUsages().Property5 = 0;
        Func<int> x = () => Property5;
        other.Property6 = 0;
        return Property6;
    }

    private int Property7 { get; set; } = 0;
    public int ExpressionBodyMethod() => Property7;

    private static int Property8 { get; set; } = 0;
    public int SomeProperty { get; set; } = Property8;

    private static int Property9 { get; set; }
    static PropertyUsages()
    {
        Property9 = 0;
    }
    public PropertyUsages(int number) { }
    public PropertyUsages() : this(Property9) { }

    private int Property10 { get; set; }
    private int Property11 { get; set; }
    public object Method2()
    {
        if ((Property10 = 0) == 0) { }
        var x = new[] { Property10 };
        var name = nameof(Property11);
        return null;
    }

    private int this[string i] { get { return 5; } set { } }
    public void Method3()
    {
        var x = this[""5""];
        this[""5""] = 10;
    }
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Method_Accessibility()
        {
            Verifier.VerifyCSharpAnalyzer(@"
public class PrivateMembers
{
    private int PrivateMethod() { return 0; } // Noncompliant {{Remove the unused private method 'PrivateMethod'.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    private static int PrivateStaticMethod() { return 0; } // Noncompliant

    private class InnerPrivateClass // Noncompliant
    {
        internal int InternalMethod() { return 0; } // Noncompliant
        protected int ProtectedMethod() { return 0; } // Noncompliant
        protected internal int ProtectedInternalMethod() { return 0; } // Noncompliant
        public int PublicMethod() { return 0; } // Noncompliant
        internal static int InternalStaticMethod() { return 0; } // Noncompliant
        protected static int ProtectedStaticMethod() { return 0; } // Noncompliant
        protected internal static int ProtectedInternalStaticMethod() { return 0; } // Noncompliant
        public static int PublicStaticMethod() { return 0; } // Noncompliant
    }
}

public class NonPrivateMembers
{
    internal int InternalMethod() { return 0; }
    protected int ProtectedMethod() { return 0; }
    protected internal int ProtectedInternalMethod() { return 0; }
    public int PublicMethod() { return 0; }
    internal static int InternalStaticMethod() { return 0; }
    protected static int ProtectedStaticMethod() { return 0; }
    protected internal static int ProtectedInternalStaticMethod() { return 0; }
    public static int PublicStaticMethod() { return 0; }

    public class InnerPublicClass
    {
        internal int InternalMethod() { return 0; }
        protected int ProtectedMethod() { return 0; }
        protected internal int ProtectedInternalMethod() { return 0; }
        public int PublicMethod() { return 0; }
        internal static int InternalStaticMethod() { return 0; }
        protected static int ProtectedStaticMethod() { return 0; }
        protected internal static int ProtectedInternalStaticMethod() { return 0; }
        public static int PublicStaticMethod() { return 0; }
    }
}

public interface IInterface
{
    int InterfaceMethod();
}

public class InterfaceImpl : IInterface
{
    int IInterface.InterfaceMethod() => 0;
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Methods_DirectReferences()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
using System.Linq;
public class MethodUsages
{
    private int Method1() { return 0; }
    private int Method2() { return 0; }
    private int Method3() { return 0; }
    private int Method4() { return 0; }
    private int Method5() { return 0; }
    private int Method6() { return 0; }
    private int Method7() { return 0; }
    public int Test1(MethodUsages other)
    {
        int i;
        i = Method1();
        i = this.Method2();
        Console.Write(Method3());
        new MethodUsages().Method4();
        Func<int> x = () => Method5();
        other.Method6();
        return Method7();
    }

    private int Method8() { return 0; }
    public int ExpressionBodyMethod() => Method8();

    private static int Method9() { return 0; }
    public MethodUsages(int number) { }
    public MethodUsages() : this(Method9()) { }

    private int Method10() { return 0; }
    private int Method11() { return 0; }
    public object Test2()
    {
        var x = new[] { Method10() };
        var name = nameof(Method11);
        return null;
    }

    private int Method12(int i) { return 0; }
    public void Test3()
    {
        new[] { 1, 2, 3 }.Select(Method12);
    }
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void DebuggerDisplay_Attribute()
        {
            Verifier.VerifyCSharpAnalyzer(@"
// https://github.com/SonarSource/sonar-csharp/issues/1195
[System.Diagnostics.DebuggerDisplay(""{field1}"", Name = ""{Property1} {Property3}"", Type = ""{Method1()}"")]
public class MethodUsages
{
    private int field1;
    private int field2; // Noncompliant
    private int Property1 { get; set; }
    private int Property2 { get; set; } // Noncompliant
    private int Property3 { get; set; }
    private int Method1() { return 0; }
    private int Method2() { return 0; } // Noncompliant

    public void Method()
    {
        var x = Property3;
    }
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Constructor_Accessibility()
        {
            Verifier.VerifyCSharpAnalyzer(@"
public class PrivateConstructors
{
    private PrivateConstructors(int i) { var x = 5; } // Noncompliant {{Remove the unused private constructor 'PrivateConstructors'.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    static PrivateConstructors() { var x = 5; }

    private class InnerPrivateClass // Noncompliant
    {
        internal InnerPrivateClass(int i) { var x = 5; } // Noncompliant
        protected InnerPrivateClass(string s) { var x = 5; } // Noncompliant
        protected internal InnerPrivateClass(double d) { var x = 5; } // Noncompliant
        public InnerPrivateClass(char c) { var x = 5; } // Noncompliant
    }

    private class OtherPrivateClass // Noncompliant
    {
        private OtherPrivateClass() { var x = 5; } // Noncompliant
    }
}

public class NonPrivateMembers
{
    internal NonPrivateMembers(int i) { var x = 5; }
    protected NonPrivateMembers(string s) { var x = 5; }
    protected internal NonPrivateMembers(double d) { var x = 5; }
    public NonPrivateMembers(char c) { var x = 5; }

    public class InnerPublicClass
    {
        internal InnerPublicClass(int i) { var x = 5; }
        protected InnerPublicClass(string s) { var x = 5; }
        protected internal InnerPublicClass(double d) { var x = 5; }
        public InnerPublicClass(char c) { var x = 5; }
    }
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Constructor_DirectReferences()
        {
            Verifier.VerifyCSharpAnalyzer(@"
public abstract class PrivateConstructors
{
    public class Constructor1
    {
        public static readonly Constructor1 Instance = new Constructor1();
        private Constructor1() { var x = 5; }
    }

    public class Constructor2
    {
        public Constructor2(int a) { }
        private Constructor2() { var x = 5; }
    }

    public class Constructor3
    {
        public Constructor3(int a) : this() { }
        private Constructor3() { var x = 5; }
    }

    public class Constructor4
    {
        static Constructor4() { var x = 5; }
    }
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Constructor_Inheritance()
        {
            Verifier.VerifyCSharpAnalyzer(@"
public class Inheritance
{
    private abstract class BaseClass1
    {
        protected BaseClass1() { var x = 5; }
    }

    private class DerivedClass1 : BaseClass1 // Noncompliant {{Remove the unused private type 'DerivedClass1'.}}
    {
        public DerivedClass1() : base() { }
    }

    // https://github.com/SonarSource/sonar-csharp/issues/1398
    private abstract class BaseClass2
    {
        protected BaseClass2() { var x = 5; }
    }

    private class DerivedClass2 : BaseClass2 // Noncompliant {{Remove the unused private type 'DerivedClass2'.}}
    {
        public DerivedClass2() { }
    }
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Types_Accessibility()
        {
            Verifier.VerifyCSharpAnalyzer(@"
public class PrivateTypes
{
    private class InnerPrivateClass // Noncompliant {{Remove the unused private type 'InnerPrivateClass'.}}
    {
        protected class ProtectedClass { } // Noncompliant
        protected internal class ProtectedInternalClass { } // Noncompliant
        public class PublicClass { } // Noncompliant
    }

    private class PrivateClass { } // Noncompliant
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    internal class InternalClass { } // Noncompliant

    private struct PrivateStruct { } // Noncompliant
    internal struct InternalStruct { } // Noncompliant
}

public class NonPrivateTypes
{
    protected class ProtectedClass { }
    protected internal class ProtectedInternalClass { }
    public class PublicClass { }

    protected struct ProtectedStruct { }
    protected internal struct ProtectedInternalStruct { }
    public struct PublicStruct { }
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Types_InternalsVisibleTo()
        {
            Verifier.VerifyCSharpAnalyzer(@"
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("""")]
public class PrivateTypes
{
    private class PrivateClass { } // Noncompliant
    internal class InternalClass { } // Compliant, internal types are not reported when InternalsVisibleTo is present
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Types_Internals()
        {
            Verifier.VerifyCSharpAnalyzer(@"
// https://github.com/SonarSource/sonar-csharp/issues/1225
// https://github.com/SonarSource/sonar-csharp/issues/904
using System;
public class Class1
{
    public void Method1()
    {
        var x = Sample.Constants.X;
    }
}

public class Sample
{
    internal class Constants
    {
        public const int X = 5;
    }
}", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Types_DirectReferences()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System.Linq;
public class PrivateTypes
{
    private class PrivateClass1 { }
    private class PrivateClass2 { }
    private class PrivateClass3 { }
    private class PrivateClass4 { }
    private class PrivateClass5 // When Method() is removed, this class will raise issue
    {
        public void Method() // Noncompliant
        {
            var x = new PrivateClass5();
        }
    }
    public void Test1()
    {
        var x = new PrivateClass1();
        var t = typeof(PrivateClass2);
        var n = nameof(PrivateClass3);

        var o = new object[0];
        o.OfType<PrivateClass4>();
    }
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Empty_Constructors()
        {
            Verifier.VerifyCSharpAnalyzer(@"
public class PrivateConstructors
{
    private PrivateConstructors(int i) { } // Compliant, empty ctors are reported from another rule
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Methods_Main()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System.Threading.Tasks;
public class NewClass1
{
    // See https://github.com/SonarSource/sonar-csharp/issues/888
    static async Task Main() { } // Compliant - valid main method since C# 7.1
}

public class NewClass2
{
    static async Task<int> Main() { return 1; } // Compliant - valid main method since C# 7.1
}

public class NewClass3
{
    static async Task Main(string[] args) { } // Compliant - valid main method since C# 7.1
}

public class NewClass4
{
    static async Task<int> Main(string[] args) { return 1; } // Compliant - valid main method since C# 7.1
}

public class NewClass5
{
    static async Task<string> Main(string[] args) { return ""a""; } // Noncompliant
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Members_With_Attributes_Are_Not_Removable()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
public class FieldUsages
{
    [Obsolete]
    private int field1;

    [Obsolete]
    private int Property1 { get; set; }

    [Obsolete]
    private int Method1() { return 0; }

    [Obsolete]
    private class Class1 { }
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Assembly_Level_Attributes()
        {
            Verifier.VerifyCSharpAnalyzer(@"
[assembly: System.Reflection.AssemblyCompany(Foo.Constants.AppCompany)]
public static class Foo
{
    internal static class Constants // Compliant, detect usages from assembly level attributes.
    {
        public const string AppCompany = ""foo"";
    }
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMemberWithPartialClasses()
        {
            Verifier.VerifyAnalyzer(new[]{
                    @"TestCases\UnusedPrivateMember.part1.cs",
                    @"TestCases\UnusedPrivateMember.part2.cs" },
                new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Methods_EventHandler()
        {
            // Event handler methods are not reported because in WPF an event handler
            // could be added through XAML and no warning will be generated if the
            // method is removed, which could lead to serious problems that are hard
            // to diagnose.
            Verifier.VerifyCSharpAnalyzer(@"
using System;
public class NewClass
{
    private void Handler(object sender, EventArgs e) { } // Noncompliant
}
public partial class PartialClass
{
    private void Handler(object sender, EventArgs e) { } // intentional False Negative
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Unity3D_Ignored()
        {
            Verifier.VerifyCSharpAnalyzer(@"
// https://github.com/SonarSource/sonar-csharp/issues/159
public class UnityMessages1 : UnityEngine.MonoBehaviour
{
    private void SomeMethod(bool hasFocus) { } // Compliant
}

public class UnityMessages2 : UnityEngine.ScriptableObject
{
    private void SomeMethod(bool hasFocus) { } // Compliant
}

public class UnityMessages3 : UnityEditor.AssetPostprocessor
{
    private void SomeMethod(bool hasFocus) { } // Compliant
}

public class UnityMessages4 : UnityEditor.AssetModificationProcessor
{
    private void SomeMethod(bool hasFocus) { } // Compliant
}

// Unity3D does not seem to be available as a nuget package and we cannot use the original classes
namespace UnityEngine
{
    public class MonoBehaviour { }
    public class ScriptableObject { }
}
namespace UnityEditor
{
    public class AssetPostprocessor { }
    public class AssetModificationProcessor { }
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember()
        {
            Verifier.VerifyAnalyzer(@"TestCases\UnusedPrivateMember.cs",
                new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_FromCSharp7()
        {
            Verifier.VerifyAnalyzer(@"TestCases\UnusedPrivateMember.CSharp7.cs",
                new UnusedPrivateMember(),
                ParseOptionsHelper.FromCSharp7);
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void UnusedPrivateMember_CodeFix()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\UnusedPrivateMember.cs",
                @"TestCases\UnusedPrivateMember.Fixed.cs",
                @"TestCases\UnusedPrivateMember.Fixed.Batch.cs",
                new UnusedPrivateMember(),
                new UnusedPrivateMemberCodeFixProvider());
        }
    }
}
