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
        public void UnusedPrivateMember_Types_Accessibility()
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
", new CS.UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_Types_InternalsVisibleTo()
        {
            Verifier.VerifyCSharpAnalyzer(@"
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("""")]
public class PrivateTypes
{
    private class PrivateClass { } // Noncompliant
    internal class InternalClass { } // Compliant, internal types are not reported when InternalsVisibleTo is present
}
", new CS.UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_Types_Internals()
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
}", new CS.UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_Types_DirectReferences()
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
", new CS.UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_SupportTypeKinds()
        {
            Verifier.VerifyCSharpAnalyzer(@"
public class PrivateTypes
{
    private class MyPrivateClass { } // Noncompliant
    private struct MyPrivateStruct { } // Noncompliant
    private enum MyPrivateEnum { } // Noncompliant
    private interface MyPrivateInterface { } // Noncompliant
    private delegate int MyPrivateDelegate(int x, int y); // Noncompliant

    public class MyPublicClass { }
    public struct MyPublicStruct { }
    public enum MyPublicEnum { }
    public interface MyPublicInterface { }
    public delegate int MyPublicDelegate(int x, int y);

    private class Something : MyPublicInterface {}

    public void Foo()
    {
        new MyPublicClass();
        new MyPublicStruct();
        new MyPublicEnum();
        new Something();

        MyPublicDelegate handler = PerformCalculation;
    }

    public static int PerformCalculation(int x, int y) => x + y;
}
", new CS.UnusedPrivateMember());
        }
    }
}
