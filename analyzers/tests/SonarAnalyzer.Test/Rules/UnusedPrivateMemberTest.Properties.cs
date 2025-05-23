﻿/*
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

namespace SonarAnalyzer.Test.Rules;

public partial class UnusedPrivateMemberTest
{
    [TestMethod]
    public void UnusedPrivateMember_Property_Accessibility() =>
        builder.AddSnippet(@"
public class PrivateMembers
{
    private int PrivateProperty { get; set; } // Noncompliant {{Remove the unused private property 'PrivateProperty'.}}
//              ^^^^^^^^^^^^^^^
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
").Verify();

    [TestMethod]
    public void UnusedPrivateMember_Properties_DirectReferences() =>
        builder.AddSnippet("""
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
                    var x = this["5"];
                    this["5"] = 10;
                }

                private int Property12 { get; set; } = 42; // FN
            }
            """).VerifyNoIssues();

    [TestMethod]
    public void UnusedPrivateMember_Properties_Accessors() =>
        builder.AddSnippet(@"
using System;
public class PropertyUsages
{
    public int AProperty { private get; set; } // Noncompliant {{Remove the unused private getter 'get_AProperty'.}}
    public int BProperty { get; private set; } // Noncompliant {{Remove the unused private setter 'set_BProperty'.}}
    public int CProperty { internal get; set; } // Compliant
    public int DProperty { get; internal set; } // Compliant
    public int EProperty { protected get; set; } // Compliant
    public int E2Property { get; protected set; } // Compliant
    public int FProperty { get; private set; } // Compliant
    public int GProperty { private get; set; } // Noncompliant {{Remove the unused private getter 'get_GProperty'.}}
    public int HProperty { get; private set; } // Noncompliant {{Remove the unused private setter 'set_HProperty'.}}
    public int IProperty { private get; set; } // Compliant
    public int JProperty { get; private set; } // Compliant: both read and write
    public int KProperty { private get; set; } // Compliant: both read and write
    public int LProperty { get; private set; } // FN: private set is used in the constructor, not necessary
    protected int MProperty { private get; set; } // Noncompliant {{Remove the unused private getter 'get_MProperty'.}}

    public PropertyUsages()
    {
        LProperty = 42;
    }

    public void Method()
    {
        FProperty = HProperty;
        GProperty = IProperty;

        JProperty = KProperty;
        KProperty = JProperty;
    }

    public interface ISomeInterface
    {
        string Something { get; }
        string SomethingElse { get; }
    }

    public class SomeClass : ISomeInterface
    {
        public string Something { get; private set; } // Compliant
        public string SomethingElse { get; private set; } // Noncompliant

        public void Method(string str)
        {
            Something = str;
        }
    }
}
").Verify();
}
