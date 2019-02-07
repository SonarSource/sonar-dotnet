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
        public void UnusedPrivateMember_Method_Accessibility()
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
", new CS.UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_Methods_DirectReferences()
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
", new CS.UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_Methods_Main()
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
", new CS.UnusedPrivateMember());
        }
    }
}
