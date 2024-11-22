/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
    public void UnusedPrivateMember_Field_Accessibility() =>
        builder.AddSnippet("""
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
            """).Verify();

    [TestMethod]
    public void UnusedPrivateMember_Field_MultipleDeclarations() =>
        builder.AddSnippet("""
            public class PrivateMembers
            {
                private int x, y, z; // Noncompliant {{Remove the unused private field 'x'.}}
            //  ^^^^^^^^^^^^^^^^^^^^

                private int a, b, c;
            //              ^ {{Remove the unused private field 'a'.}}
            //                    ^ @-1 {{Remove the unused private field 'c'.}}

                public int Method1() => b;
            }
            """).Verify();

    [TestMethod]
    public void UnusedPrivateMember_Fields_DirectReferences() =>
        builder.AddSnippet("""
            using System;

            public class FieldUsages
            {
                private int field1; // Noncompliant {{Remove this unread private field 'field1' or refactor the code to use its value.}}
                private int field2; // Noncompliant {{Remove this unread private field 'field2' or refactor the code to use its value.}}
                private int field3; // Noncompliant {{Remove this unread private field 'field3' or refactor the code to use its value.}}
                private int field4;
                private int field5;
                private int field6;
                public int Method1()
                {
                    field1 = 0;
                    this.field2 = 0;
                    int.TryParse("1", out field3);
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
                private int field11; // Compliant nameof(field11)
                public object Method2()
                {
                    var x = new[] { field10 };
                    var name = nameof(field11);
                    return null;
                }
            }
            """).Verify();

    [TestMethod]
    public void UnusedPrivateMember_Fields_StructLayout() =>
        builder.AddSnippet("""
            // https://github.com/SonarSource/sonar-dotnet/issues/6912
            using System.Runtime.InteropServices;

            public class Foo
            {
                [StructLayout(LayoutKind.Sequential)]
                private struct NetResource
                {
                    public string LocalName; // Compliant: Unused members in a struct with StructLayout attribute are compliant
                    public string RemoteName;
                }

                [DllImport("mpr.dll")]
                private static extern int WNetAddConnection2(NetResource netResource, string password, string username, int flags);

                public void Bar()
                {
                    var netResource = new NetResource
                    {
                        RemoteName = "foo"
                    };
                    WNetAddConnection2(netResource, "password", "username", 0);
                }
            }
            """).VerifyNoIssues();
}
