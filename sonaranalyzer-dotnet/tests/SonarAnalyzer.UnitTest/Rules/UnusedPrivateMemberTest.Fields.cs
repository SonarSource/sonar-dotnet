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
        public void UnusedPrivateMember_Field_Accessibility()
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
", new CS.UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_Field_MultipleDeclarations()
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
", new CS.UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_Fields_DirectReferences()
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
", new CS.UnusedPrivateMember());
        }
    }
}
