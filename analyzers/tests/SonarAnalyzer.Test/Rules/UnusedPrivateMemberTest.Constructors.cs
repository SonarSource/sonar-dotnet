/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Test.Rules;

public partial class UnusedPrivateMemberTest
{
    [TestMethod]
    public void UnusedPrivateMember_Constructor_Accessibility() =>
        builder.AddSnippet(@"
public class PrivateConstructors
{
    private PrivateConstructors(int i) { var x = 5; } // Noncompliant {{Remove the unused private constructor 'PrivateConstructors'.}}
//          ^^^^^^^^^^^^^^^^^^^
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
").Verify();

    [TestMethod]
    public void UnusedPrivateMember_Constructor_DirectReferences() =>
        builder.AddSnippet("""
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
                    private Constructor2() { var x = 5; } // Compliant - FN
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

            """).VerifyNoIssues();

    [TestMethod]
    public void UnusedPrivateMember_Constructor_Inheritance() =>
        builder.AddSnippet(@"
public class Inheritance
{
    private abstract class BaseClass1
    {
        protected BaseClass1() { var x = 5; }
    }

    private class DerivedClass1 : BaseClass1 // Noncompliant {{Remove the unused private class 'DerivedClass1'.}}
    {
        public DerivedClass1() : base() { }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/1398
    private abstract class BaseClass2
    {
        protected BaseClass2() { var x = 5; }
    }

    private class DerivedClass2 : BaseClass2 // Noncompliant {{Remove the unused private class 'DerivedClass2'.}}
    {
        public DerivedClass2() { }
    }
}
").Verify();

    [TestMethod]
    public void UnusedPrivateMember_Empty_Constructors() =>
        builder.AddSnippet("""
            public class PrivateConstructors
            {
                private PrivateConstructors(int i) { } // Compliant, empty ctors are reported from another rule
            }

            """).VerifyNoIssues();

    [TestMethod]
    public void UnusedPrivateMember_Illegal_Interface_Constructor() =>
        // While typing code in IDE, we can end up in a state where an interface has a constructor defined.
        // Even though this results in a compiler error (CS0526), IDE will still trigger rules on the interface.
        builder.AddSnippet(@"
public interface IInterface
{
    // UnusedPrivateMember rule does not trigger AD0001 error from NullReferenceException
    IInterface() {} // Error [CS0526]
}
").Verify();

    [DataTestMethod]
    [DataRow("private", "Remove the unused private constructor 'Foo'.")]
    [DataRow("protected", "Remove unused constructor of private type 'Foo'.")]
    [DataRow("internal", "Remove unused constructor of private type 'Foo'.")]
    [DataRow("public", "Remove unused constructor of private type 'Foo'.")]
    public void UnusedPrivateMember_NonPrivateConstructorInPrivateClass(string accessModifier, string expectedMessage) =>
        builder.AddSnippet($$$"""
public class Some
{
    private class Foo // Noncompliant
    {
        {{{accessModifier}}} Foo() // Noncompliant {{{{{expectedMessage}}}}}
        {
            var a = 1;
        }
    }
}
""").Verify();

#if NET

    [TestMethod]
    public void UnusedPrivateMember_RecordPositionalConstructor() =>
        builder.AddSnippet("""
            // https://github.com/SonarSource/sonar-dotnet/issues/5381
            public abstract record Foo
            {
                Foo(string value)
                {
                    Value = value;
                }

                public string Value { get; }

                public sealed record Bar(string Value) : Foo(Value);
            }
            """).WithOptions(LanguageOptions.FromCSharp9).VerifyNoIssues();

    [TestMethod]
    public void UnusedPrivateMember_NonExistentRecordPositionalConstructor() =>
    builder.AddSnippet(@"
public abstract record Foo
{
    public sealed record Bar(string Value) : RandomRecord(Value); // Error [CS0246, CS1729] no suitable method found to override
}").WithOptions(LanguageOptions.FromCSharp10).Verify();

#endif

}
