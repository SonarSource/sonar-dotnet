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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    public partial class UnusedPrivateMemberTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_Constructor_Accessibility()
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
", new CS.UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_Constructor_DirectReferences()
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
", new CS.UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_Constructor_Inheritance()
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
", new CS.UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_Empty_Constructors()
        {
            Verifier.VerifyCSharpAnalyzer(@"
public class PrivateConstructors
{
    private PrivateConstructors(int i) { } // Compliant, empty ctors are reported from another rule
}
", new CS.UnusedPrivateMember());
        }
    }
}
