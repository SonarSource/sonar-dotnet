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

namespace SonarAnalyzer.UnitTest.ControlFlowGraph
{
    public partial class UcfgInstructionTest
    {
        [TestMethod]
        public void AddAssignment_LocalVariable()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public string fieldString;
        public string PropertyString { get; set; }

        public void Foo(string parameterString, string[] values)
        {
            var localString = ""foo"";
                // localString := __id [ const ]

            localString += (((""foo"")));
                // %0 := __concat [ localString const ]
                // localString := __id [ %0 ]

            localString += localString;
                // %1 := __concat [ localString localString ]
                // localString := __id [ %1 ]

            localString += parameterString;
                // %2 := __concat [ localString parameterString ]
                // localString := __id [ %2 ]

            localString += fieldString;
                // %3 := __id [ this.fieldString ]
                // %4 := __concat [ localString %3 ]
                // localString := __id [ %4 ]

            localString += PropertyString;
                // %5 := Namespace.Class1.PropertyString.get [ this ]
                // %6 := __concat [ localString %5 ]
                // localString := __id [ %6 ]

            localString += localString += localString += ""123"";
                // %7 := __concat [ localString const ]
                // localString := __id [ %7 ]
                // %8 := __concat [ localString localString ]
                // localString := __id [ %8 ]
                // %9 := __concat [ localString localString ]
                // localString := __id [ %9 ]

            values[0] += Passthrough(localString += ""abc"");
                // %10 := __arrayGet [ values ]
                // %11 := __concat [ localString const ]
                // localString := __id [ %11 ]
                // %12 := Namespace.Class1.Passthrough(string) [ this localString ]
                // %13 := __concat [ %10 %12 ]
                // %14 := __arraySet [ values %13 ]
        }

        public string Passthrough(string s) => s;
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void AddAssignment_Parameter()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public string fieldString;
        public string PropertyString { get; set; }

        public void Foo(string parameterString, string[] values)
        {
            var localString = ""foo"";
                // localString := __id [ const ]

            parameterString += (((""foo"")));
                // %0 := __concat [ parameterString const ]
                // parameterString := __id [ %0 ]

            parameterString += localString;
                // %1 := __concat [ parameterString localString ]
                // parameterString := __id [ %1 ]

            parameterString += parameterString;
                // %2 := __concat [ parameterString parameterString ]
                // parameterString := __id [ %2 ]

            parameterString += fieldString;
                // %3 := __id [ this.fieldString ]
                // %4 := __concat [ parameterString %3 ]
                // parameterString := __id [ %4 ]

            parameterString += PropertyString;
                // %5 := Namespace.Class1.PropertyString.get [ this ]
                // %6 := __concat [ parameterString %5 ]
                // parameterString := __id [ %6 ]

            parameterString += parameterString += parameterString += ""123"";
                // %7 := __concat [ parameterString const ]
                // parameterString := __id [ %7 ]
                // %8 := __concat [ parameterString parameterString ]
                // parameterString := __id [ %8 ]
                // %9 := __concat [ parameterString parameterString ]
                // parameterString := __id [ %9 ]

            values[0] += Passthrough(parameterString += ""abc"");
                // %10 := __arrayGet [ values ]
                // %11 := __concat [ parameterString const ]
                // parameterString := __id [ %11 ]
                // %12 := Namespace.Class1.Passthrough(string) [ this parameterString ]
                // %13 := __concat [ %10 %12 ]
                // %14 := __arraySet [ values %13 ]
        }

        public string Passthrough(string s) => s;
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void AddAssignment_Field()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public string fieldString;
        public string PropertyString { get; set; }

        public void Foo(string parameterString, string[] values)
        {
            var localString = ""foo"";
                // localString := __id [ const ]

            fieldString += (((""foo"")));
                // %0 := __id [ this.fieldString ]
                // %1 := __concat [ %0 const ]
                // this.fieldString := __id [ %1 ]

            fieldString += localString;
                // %2 := __id [ this.fieldString ]
                // %3 := __concat [ %2 localString ]
                // this.fieldString := __id [ %3 ]

            fieldString += parameterString;
                // %4 := __id [ this.fieldString ]
                // %5 := __concat [ %4 parameterString ]
                // this.fieldString := __id [ %5 ]

            fieldString += fieldString;
                // %6 := __id [ this.fieldString ]
                // %7 := __id [ this.fieldString ]
                // %8 := __concat [ %6 %7 ]
                // this.fieldString := __id [ %8 ]

            fieldString += PropertyString;
                // %9 := __id [ this.fieldString ]
                // %10 := Namespace.Class1.PropertyString.get [ this ]
                // %11 := __concat [ %9 %10 ]
                // this.fieldString := __id [ %11 ]

            fieldString += fieldString += fieldString += ""123"";
                // %12 := __id [ this.fieldString ]
                // %13 := __id [ this.fieldString ]
                // %14 := __id [ this.fieldString ]
                // %15 := __concat [ %14 const ]
                // this.fieldString := __id [ %15 ]
                // %16 := __id [ this.fieldString ]
                // %17 := __concat [ %13 %16 ]
                // this.fieldString := __id [ %17 ]
                // %18 := __id [ this.fieldString ]
                // %19 := __concat [ %12 %18 ]
                // this.fieldString := __id [ %19 ]

            values[0] += Passthrough(fieldString += ""abc"");
                // %20 := __arrayGet [ values ]
                // %21 := __id [ this.fieldString ]
                // %22 := __concat [ %21 const ]
                // this.fieldString := __id [ %22 ]
                // %23 := Namespace.Class1.Passthrough(string) [ this this.fieldString ]
                // %24 := __concat [ %20 %23 ]
                // %25 := __arraySet [ values %24 ]
        }

        public string Passthrough(string s) => s;
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void AddAssignment_Property()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public string fieldString;
        public string PropertyString { get; set; }

        public void Foo(string parameterString, string[] values)
        {
            var localString = ""foo"";
                // localString := __id [ const ]

            PropertyString += (((""foo"")));
                // %0 := Namespace.Class1.PropertyString.get [ this ]
                // %1 := __concat [ %0 const ]
                // %2 := Namespace.Class1.PropertyString.set [ this %1 ]

            PropertyString += localString;
                // %3 := Namespace.Class1.PropertyString.get [ this ]
                // %4 := __concat [ %3 localString ]
                // %5 := Namespace.Class1.PropertyString.set [ this %4 ]

            PropertyString += parameterString;
                // %6 := Namespace.Class1.PropertyString.get [ this ]
                // %7 := __concat [ %6 parameterString ]
                // %8 := Namespace.Class1.PropertyString.set [ this %7 ]

            PropertyString += fieldString;
                // %9 := Namespace.Class1.PropertyString.get [ this ]
                // %10 := __id [ this.fieldString ]
                // %11 := __concat [ %9 %10 ]
                // %12 := Namespace.Class1.PropertyString.set [ this %11 ]

            PropertyString += PropertyString;
                // %13 := Namespace.Class1.PropertyString.get [ this ]
                // %14 := Namespace.Class1.PropertyString.get [ this ]
                // %15 := __concat [ %13 %14 ]
                // %16 := Namespace.Class1.PropertyString.set [ this %15 ]

            PropertyString += PropertyString += PropertyString += ""123"";
                // %17 := Namespace.Class1.PropertyString.get [ this ]
                // %18 := Namespace.Class1.PropertyString.get [ this ]
                // %19 := Namespace.Class1.PropertyString.get [ this ]
                // %20 := __concat [ %19 const ]
                // %21 := Namespace.Class1.PropertyString.set [ this %20 ]
                // %22 := __concat [ %18 %21 ]
                // %23 := Namespace.Class1.PropertyString.set [ this %22 ]
                // %24 := __concat [ %17 %23 ]
                // %25 := Namespace.Class1.PropertyString.set [ this %24 ]

            values[0] += Passthrough(PropertyString += ""abc"");
                // %26 := __arrayGet [ values ]
                // %27 := Namespace.Class1.PropertyString.get [ this ]
                // %28 := __concat [ %27 const ]
                // %29 := Namespace.Class1.PropertyString.set [ this %28 ]
                // %30 := Namespace.Class1.Passthrough(string) [ this %29 ]
                // %31 := __concat [ %26 %30 ]
                // %32 := __arraySet [ values %31 ]
        }

        public string Passthrough(string s) => s;
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }
    }
}
