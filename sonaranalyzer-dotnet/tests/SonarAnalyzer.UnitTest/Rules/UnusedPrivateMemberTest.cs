/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using csharp::SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class UnusedPrivateMemberTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember()
        {
            Verifier.VerifyAnalyzer(@"TestCases\UnusedPrivateMember.cs", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_new()
        {
            Verifier.VerifyAnalyzer(@"TestCases\UnusedPrivateMember.NEW.cs", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedFields()
        {
            Verifier.VerifyCSharpAnalyzer(@"
namespace Tests.Diagnostics
{
    using System;
    using System.ComponentModel;

    public class FieldAccessibility
    {
        private int privateField; // Noncompliant
        private static int privateStaticField; // Noncompliant

        // not private fields are compliant
        internal int internalField;
        protected int protectedField;
        protected internal int protectedInternalField;
        public int publicField;
        internal static int internalStaticField;
        protected static int protectedStaticField;
        protected static internal int protectedInternalStaticField;
        public static int publicStaticField;
    }

    public class FieldUsages
    {
        private int field1;
        private int field2;
        private int field3;
        private int field4;
        private int field5;
        private int field6;
        private int field7;
        private int field8;
        private int field9;
        [DefaultValue(5)]
        private int field10; // fields with attribute are not removable
        private int field11;

        public void Method1()
        {
            field1 = 0;
            this.field2 = 0;
            int.TryParse(""1"", out field3);
            Console.Write(field4);
            Func<int> x = () => field5;
            return field6;
        }

        public void ExpressionBodyMethod() => field7;

        public int Property { get; set; } = field8;

        public FieldUsages(int number) { }

        public FieldUsages() : this(field9) { }

        public object Method2()
        {
            var x = new { field11 };
        }
    }
}
", new UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedProperties()
        {
            Verifier.VerifyCSharpAnalyzer(@"
namespace Tests.Diagnostics
{
    using System;
    using System.ComponentModel;

    public class PropertyAccessibility
    {
        private int PrivateProperty { get; set; } // Noncompliant {{Remove the unused private property 'PrivateProperty'.}}
        private static int PrivateStaticProperty { get; set; } // Noncompliant

        public int PublicPropertyPrivateSet { get; private set; }
        public int PublicPropertyNoSet { get; }

        // not private Propertys are compliant
        internal int InternalProperty { get; set; }
        protected int ProtectedProperty { get; set; }
        protected internal int ProtectedInternalProperty { get; set; }
        public int PublicProperty { get; set; }
        internal static int InternalStaticProperty { get; set; }
        protected static int ProtectedStaticProperty { get; set; }
        protected static internal int ProtectedInternalStaticProperty { get; set; }
        public static int PublicStaticProperty { get; set; }
    }

    public class PropertyUsages
    {
        private int Property1 { get; set; }
        private int Property2 { get; set; }
        private int Property3 { get; }
        private int Property4 { get; }
        private int Property5 { get; }
        private int Property6 { get; }
        private int Property7 { get; }
        private int Property8 { get; }
        [DefaultValue(5)]
        private int Property9 { get; set; } // Properties with attribute are not removable
        private int Property10 { get; }
        private int Property11 { get { return 5; } set { } }  // Noncompliant {{Remove the unused private get accessor in property 'Property11'.}}
        private int Property12 { get; set; }
        private int Property13 { get; set; }
        private int Property14 { get { return 5; } }
        private int Property15 { get; set; } = 5;

        public void Method1()
        {
            int value;

            Property1 = 0;
            value = Property1;

            ((this.Property2)) = 0;
            value = this.Property2;

            Console.Write(this?.Property3);

            Func<int> x = () => Property4;

            return Property5;
        }

        public void ExpressionBodyMethod() => Property6;

        public int Property { get; set; } = Property7;

        public PropertyUsages(int number) { }

        public PropertyUsages() : this(Property8) { }

        public object Method2()
        {
            var x = new { Property10 };

            ((Property11)) = 10;

            Property12++;

            Property13 += 10;

            int value;

            value = Property14;

            value = Property15;
        }
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
