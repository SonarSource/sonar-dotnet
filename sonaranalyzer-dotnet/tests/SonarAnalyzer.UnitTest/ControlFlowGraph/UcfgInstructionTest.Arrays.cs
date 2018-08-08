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
        public void Assignments_Array_Get()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(string s, string[] a, string[][] jagged, string[,] multi)
        {
            s = a[0];
                // %0 := __arrayGet [ a ]
                // s := __id [ %0 ]

            s = ((a[0]));
                // %1 := __arrayGet [ a ]
                // s := __id [ %1 ]

            Bar(a[0]);
                // %2 := __arrayGet [ a ]
                // %3 := Namespace.Class1.Bar(string) [ this %2 ]

            s = jagged[0][0];
                // %4 := __arrayGet [ jagged ]
                // %5 := __arrayGet [ %4 ]
                // s := __id [ %5 ]

            s = multi[0, 0];
                // %6 := __arrayGet [ multi ]
                // s := __id [ %6 ]
        }

        public void Bar(string s) {}
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Assignments_Array_Set()
        {
            const string code = @"
using System.Collections.Generic;
namespace Namespace
{
    public class Class1
    {
        public void Foo(string s, string[] a, string[][] jagged, string[,] multi)
        {
            a[0] = s;
                // %0 := __arraySet [ a s ]

            ((a[0])) = s;
                // %1 := __arraySet [ a s ]

            jagged[0][0] = s;
                // %2 := __arrayGet [ jagged ]
                // %3 := __arraySet [ %2 s ]

            multi[0, 0] = s;
                // %4 := __arraySet [ multi s ]

            ((jagged[0]))[0] = s;
                // %5 := __arrayGet [ jagged ]
                // %6 := __arraySet [ %5 s ]

            a[0] = a[1];
                // %7 := __arrayGet [ a ]
                // %8 := __arraySet [ a %7 ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Assignments_Array_Set_FieldPropertyMethodNew()
        {
            const string code = @"
using System.Collections.Generic;
namespace Namespace
{
    public class Class1
    {
        public void Foo()
        {
            var a = new object[1];
                // %0 := new object[]
                // a := __id [ %0 ]

            a[0] = Field;
                // %1 := __id [ this.Field ]
                // %2 := __arraySet [ a %1 ]

            a[0] = Property;
                // %3 := Namespace.Class1.Property.get [ this ]
                // %4 := __arraySet [ a %3 ]

            a[0] = GetData();
                // %5 := Namespace.Class1.GetData() [ this ]
                // %6 := __arraySet [ a %5 ]

            a[0] = new Class1();
                // %7 := new Namespace.Class1
                // %8 := Namespace.Class1.Class1() [ %7 ]
                // %9 := __arraySet [ a %7 ]
        }

        private string Field = null;
        private string Property { get; } = null;
        private object GetData() { return null; }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ArrayCreation_WithNew()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public Class1[] ArrayProperty { get; set; }
        public Class1[] arrayField;

        public void Foo()
        {
            var localStringArray = new string[42];
                // %0 := new string[]
                // localStringArray := __id [ %0 ]

            var localClassArray = new Class1[0];
                // %1 := new Namespace.Class1[]
                // localClassArray := __id [ %1 ]

            var localMultiDimArray = new int[10,2];
                // %2 := new int[*,*]
                // localMultiDimArray := __id [ %2 ]

            // Array with initializer
            var localArrayInit = new string[] { ""aaa"", ""bbb"", ""ccc"" };
                // %3 := new string[]
                // %4 := __arraySet [ %3 const ]
                // %5 := __arraySet [ %3 const ]
                // %6 := __arraySet [ %3 const ]
                // localArrayInit := __id [ %3 ]

            ArrayProperty = new Class1[1];
                // %7 := new Namespace.Class1[]
                // %8 := Namespace.Class1.ArrayProperty.set [ this %7 ]

            ArrayProperty[1] = new Class1();
                // %9 := Namespace.Class1.ArrayProperty.get [ this ]
                // %10 := new Namespace.Class1
                // %11 := Namespace.Class1.Class1() [ %10 ]
                // %12 := __arraySet [ %9 %10 ]

            arrayField = new Class1[1];
                // %13 := new Namespace.Class1[]
                // this.arrayField := __id [ %13 ]

            arrayField[0] = new Class1();
                // %14 := __id [ this.arrayField ]
                // %15 := new Namespace.Class1
                // %16 := Namespace.Class1.Class1() [ %15 ]
                // %17 := __arraySet [ %14 %15 ]

            Bar(new Class1[1]);
                // %18 := new Namespace.Class1[]
                // %19 := Namespace.Class1.Bar(Namespace.Class1[]) [ this %18 ]
        }

        public void Bar(Class1[] array) {}
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ArrayCreation_WithNew_NonExistentType()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo()
        {
            // References a type that does not exist
            // This doesn't fail as we can still get the array type symbol,
            // even though the ElementKind is unknown.
            var xxx = new NonExistentType[42];   // %0 := new NonExistentType[]
                                                 // xxx := __id [ %0 ]
        }
    }
}";

            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ArrayCreation_WithCreateInstance()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo()
        {
            var a = System.Array.CreateInstance(typeof(string), 10);
                // %0 := System.Array.CreateInstance(System.Type, int) [ System.Array const const ]
                // a := __id [ %0 ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ArrayCreation_New_AssignToVariable_Int()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(int data)
        {
            // All valid array syntaxes except x = { data };

            int[] a1 = new int[3];
                // %0 := new int[]
                // a1 := __id [ %0 ]

            int[] a2 = new int[] { 0 };
                // %1 := new int[]
                // %2 := __arraySet [ %1 const ]
                // a2 := __id [ %1 ]

            int[] a3 = new int[2] { 0, 1 };
                // %3 := new int[]
                // %4 := __arraySet [ %3 const ]
                // %5 := __arraySet [ %3 const ]
                // a3 := __id [ %3 ]

            int[] a4 = new[] { 42 };
                // %6 := new int[]
                // %7 := __arraySet [ %6 const ]
                // a4 := __id [ %6 ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ArrayCreation_New_AssignToVariable_String()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(string data)
        {
            // All valid array syntaxes except x = { data };

            string[] a1 = new string[3];
                // %0 := new string[]
                // a1 := __id [ %0 ]

            string[] a2 = new string[] { null };
                // %1 := new string[]
                // %2 := __arraySet [ %1 const ]
                // a2 := __id [ %1 ]

            string[] a3 = new string[2] { ""x"", data };
                // %3 := new string[]
                // %4 := __arraySet [ %3 const ]
                // %5 := __arraySet [ %3 data ]
                // a3 := __id [ %3 ]

            string[] a4 = new[] { data };
                // %6 := new string[]
                // %7 := __arraySet [ %6 data ]
                // a4 := __id [ %6 ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ArrayCreation_New_AssignToVariable_Typed()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(Class1 data)
        {
            // All valid array syntaxes except x = { data };

            Class1[] a1 = new Class1[3];
                // %0 := new Namespace.Class1[]
                // a1 := __id [ %0 ]

            Class1[] a2 = new Class1[] { new Class1(), null };
                // %1 := new Namespace.Class1[]
                // %2 := new Namespace.Class1
                // %3 := Namespace.Class1.Class1() [ %2 ]
                // %4 := __arraySet [ %1 %2 ]
                // %5 := __arraySet [ %1 const ]
                // a2 := __id [ %1 ]

            Class1[] a3 = new Class1[2] { data, new Class1() };
                // %6 := new Namespace.Class1[]
                // %7 := new Namespace.Class1
                // %8 := Namespace.Class1.Class1() [ %7 ]
                // %9 := __arraySet [ %6 data ]
                // %10 := __arraySet [ %6 %7 ]
                // a3 := __id [ %6 ]

            Class1[] a4 = new[] { data };
                // %11 := new Namespace.Class1[]
                // %12 := __arraySet [ %11 data ]
                // a4 := __id [ %11 ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ArrayCreation_New_AssignToVariable_Object()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(object data)
        {
            // All valid array syntaxes

            object[] a1 = new object[3];
                // %0 := new object[]
                // a1 := __id [ %0 ]

            object[] a2 = new object[2] { data, new Class1() };
                // %1 := new object[]
                // %2 := new Namespace.Class1
                // %3 := Namespace.Class1.Class1() [ %2 ]
                // %4 := __arraySet [ %1 data ]
                // %5 := __arraySet [ %1 %2 ]
                // a2 := __id [ %1 ]

            object[] a3 = new object[] { new object() };
                // %6 := new object[]
                // %7 := new object
                // %8 := object.Object() [ %7 ]
                // %9 := __arraySet [ %6 %7 ]
                // a3 := __id [ %6 ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ArrayCreation_New_AssignmentRhsIsInitializer()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(object data)
        {
            object[] a1 = { data, 1 };
                // %0 := new object[]
                // a1 := __id [ %0 ]
                // %1 := __arraySet [ a1 data ]
                // %2 := __arraySet [ a1 const ]

            object[] a2 = { data, null };
                // %3 := new object[]
                // a2 := __id [ %3 ]
                // %4 := __arraySet [ a2 data ]
                // %5 := __arraySet [ a2 const ]

            object[] a3 = new[] { data, this };
                // %6 := new object[]
                // %7 := __arraySet [ %6 data ]
                // %8 := __arraySet [ %6 this ]
                // a3 := __id [ %6 ]

            object[] a4 = { data, null, new Class1() };
                // %9 := new Namespace.Class1
                // %10 := Namespace.Class1.Class1() [ %9 ]
                // %11 := new object[]
                // a4 := __id [ %11 ]
                // %12 := __arraySet [ a4 data ]
                // %13 := __arraySet [ a4 const ]
                // %14 := __arraySet [ a4 %9 ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ArrayCreation_New_ObjectArrayInitializer_PrimitivesAreNotIgnored()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(object data)
        {
            // Note: there is an optimisation in Java - arraySets are skipped for primitives.
            // We don't have this optimisation in C#.

            object[] a = new object[] { 1, data, null, 2.0, new string[]{} };
                // %0 := new object[]
                // %1 := new string[]
                // %2 := __arraySet [ %0 const ]
                // %3 := __arraySet [ %0 data ]
                // %4 := __arraySet [ %0 const ]
                // %5 := __arraySet [ %0 const ]
                // %6 := __arraySet [ %0 %1 ]
                // a := __id [ %0 ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ArrayCreation_New_ArrayInitializerInArrayInitializer()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(string data)
        {
            object[] a1 = new object[]
            {
                this,
                new string[] { data },
                new Class1[] { this}
            };

                // %0 := new object[]
                // %1 := new string[]
                // %2 := __arraySet [ %1 data ]
                // %3 := new Namespace.Class1[]
                // %4 := __arraySet [ %3 this ]
                // %5 := __arraySet [ %0 this ]
                // %6 := __arraySet [ %0 %1 ]
                // %7 := __arraySet [ %0 %3 ]
                // a1 := __id [ %0 ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ArrayCreation_New_AssignToField()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(string data)
        {
            // Assign to field and property
            // Deliberately not covering all of possible creation syntaxes here - too many

            IntField = new int[3];
                // %0 := new int[]
                // this.IntField := __id [ %0 ]

            StringField = new string[2] { ""123"", data };
                // %1 := new string[]
                // %2 := __arraySet [ %1 const ]
                // %3 := __arraySet [ %1 data ]
                // this.StringField := __id [ %1 ]

            Class1Field = new Class1[] { new Class1() };
                // %4 := new Namespace.Class1[]
                // %5 := new Namespace.Class1
                // %6 := Namespace.Class1.Class1() [ %5 ]
                // %7 := __arraySet [ %4 %5 ]
                // this.Class1Field := __id [ %4 ]

           StringField = new[] { data };
                // %8 := new string[]
                // %9 := __arraySet [ %8 data ]
                // this.StringField := __id [ %8 ]
        }

        private int[] IntField;
        private string[] StringField;
        private Class1[] Class1Field;
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ArrayCreation_New_AssignToProperty()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(string data)
        {
            // Initialization in assignent
            IntProperty = new int[] { 0 };
                // %0 := new int[]
                // %1 := __arraySet [ %0 const ]
                // %2 := Namespace.Class1.IntProperty.set [ this %0 ]

            StringProperty = new string[] { data };
                // %3 := new string[]
                // %4 := __arraySet [ %3 data ]
                // %5 := Namespace.Class1.StringProperty.set [ this %3 ]

            Class1Property = new Class1[3];
                // %6 := new Namespace.Class1[]
                // %7 := Namespace.Class1.Class1Property.set [ this %6 ]

            Class1Property = new Class1[2] { null, this };
                // %8 := new Namespace.Class1[]
                // %9 := __arraySet [ %8 const ]
                // %10 := __arraySet [ %8 this ]
                // %11 := Namespace.Class1.Class1Property.set [ this %8 ]
        }

        private int[] IntProperty { get; set;} 
        public string[] StringProperty { get; set; }
        public Class1[] Class1Property { get; set; }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ArrayCreation_New_AsMethodParameter()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(string data)
        {
            // Initialization in method call
            Bar(new int[3]);
                // %0 := new int[]
                // %1 := Namespace.Class1.Bar(int[]) [ this %0 ]

            Bar(new string[2] { data });
                // %2 := new string[]
                // %3 := __arraySet [ %2 data ]
                // %4 := Namespace.Class1.Bar(string[]) [ this %2 ]

            Bar(new Namespace.Class1[] { this });
                // %5 := new Namespace.Class1[]
                // %6 := __arraySet [ %5 this ]
                // %7 := Namespace.Class1.Bar(Namespace.Class1[]) [ this %5 ]
        }

        public void Bar(int[] args) { }
        public void Bar(string[] args) { }
        public void Bar(Class1[] args) { }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ArrayCreation_New_Jagged()
        {
            const string code = @"
namespace Namespace
{
    // See MSDN: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/arrays/jagged-arrays
    using System.Collections.Generic;
    public class Class1
    {
        public void Foo(int intdata1, int intdata2,
            string stringdata1, string stringdata2,
            Class1 objdata1, Class1 objdata2)
        {
            int[][] intJagged1 = new int[2][];
                // %0 := new int[][]
                // intJagged1 := __id [ %0 ]

            int[][] intJagged2 = new int[][]
            {
                new int [] { intdata1 },
                new int [] { intdata2 }
            };
                // %1 := new int[][]
                // %2 := new int[]
                // %3 := __arraySet [ %2 const ]
                // %4 := new int[]
                // %5 := __arraySet [ %4 const ]
                // %6 := __arraySet [ %1 %2 ]
                // %7 := __arraySet [ %1 %4 ]
                // intJagged2 := __id [ %1 ]

            string[][] stringJagged1 = new string[2][];
                // %8 := new string[][]
                // stringJagged1 := __id [ %8 ]

            string[][] stringJagged2 = new string[][]
            {
                new string [] { stringdata1 },
                new string [] { stringdata2 }
            };
                // %9 := new string[][]
                // %10 := new string[]
                // %11 := __arraySet [ %10 stringdata1 ]
                // %12 := new string[]
                // %13 := __arraySet [ %12 stringdata2 ]
                // %14 := __arraySet [ %9 %10 ]
                // %15 := __arraySet [ %9 %12 ]
                // stringJagged2 := __id [ %9 ]

            Class1[][] classJagged1 = new Class1[2][];
                // %16 := new Namespace.Class1[][]
                // classJagged1 := __id [ %16 ]

            Class1[][] classJagged2 = new Class1[][]
            {
                new Class1 [] { objdata1 },
                new Class1 [] { objdata2 }
            };
                // %17 := new Namespace.Class1[][]
                // %18 := new Namespace.Class1[]
                // %19 := __arraySet [ %18 objdata1 ]
                // %20 := new Namespace.Class1[]
                // %21 := __arraySet [ %20 objdata2 ]
                // %22 := __arraySet [ %17 %18 ]
                // %23 := __arraySet [ %17 %20 ]
                // classJagged2 := __id [ %17 ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ArrayCreation_New_Multi_NestedArrayInitializers()
        {
            const string code = @"
namespace Namespace
{
    // See MSDN: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/arrays/multidimensional-arrays
    using System.Collections.Generic;
    public class Class1
    {
        public void Foo(int intdata1, int intdata2,
            string stringdata1, string stringdata2,
            Class1 objdata1, Class1 objdata2)
        {
            int[,] intMulti1 = new int[1, 2];
                // %0 := new int[*,*]
                // intMulti1 := __id [ %0 ]

            // BUG: should handle the nested array initializers
            // SONARSEC-181: [C#] Handle multi-dimensional array initializers correctly
            int[,] intMulti2 = new int[,]
            {
                { 1, 2 }, { intdata1, intdata2 }
            };
                // %1 := new int[*,*]
                // %2 := __arraySet [ %1 const ]
                // %3 := __arraySet [ %1 const ]
                // %4 := __arraySet [ %1 const ]
                // %5 := __arraySet [ %1 const ]
                // intMulti2 := __id [ %1 ]

            string[,] stringMulti1 = new string[1, 2];
                // %6 := new string[*,*]
                // stringMulti1 := __id [ %6 ]

            string[,] stringMulti2 = new string[,]
            {
                { stringdata1, stringdata2 }, { null, "" }
            };
                // %7 := new string[*,*]
                // %8 := __arraySet [ %7 stringdata1 ]
                // %9 := __arraySet [ %7 stringdata2 ]
                // %10 := __arraySet [ %7 const ]
                // %11 := __arraySet [ %7 const ]
                // stringMulti2 := __id [ %7 ]

            Class1[,] class1Multi1 = new Class1[1, 2];
                // %12 := new Namespace.Class1[*,*]
                // class1Multi1 := __id [ %12 ]

            Class1[,] class1Multi2 = new Class1[,]
            {
                { objdata2, objdata1}, { null, new Class1() }
            };
                // %13 := new Namespace.Class1[*,*]
                // %14 := __arraySet [ %13 objdata2 ]
                // %15 := __arraySet [ %13 objdata1 ]
                // %16 := new Namespace.Class1
                // %17 := Namespace.Class1.Class1() [ %16 ]
                // %18 := __arraySet [ %13 const ]
                // %19 := __arraySet [ %13 %16 ]
                // class1Multi2 := __id [ %13 ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]  // Regression test for https://jira.sonarsource.com/browse/SONARSEC-199
        public void Peach_Exception_UnexpectedTypeOfTargetForTheInstruction_Akka()
        {
            // Adpated from akka\src\core\Akka\IO\SocketEventArgsPool.cs
            const string code = @"
namespace Akka.IO
{
    using System;
    using System.Net.Sockets;
     internal class PreallocatedSocketEventAgrsPool
    {
        private EventHandler<SocketAsyncEventArgs> _onComplete = null;
         private SocketAsyncEventArgs CreateSocketAsyncEventArgs()
        {
            var e = new SocketAsyncEventArgs { UserToken = null };
                // %0 := new System.Net.Sockets.SocketAsyncEventArgs
                // %1 := System.Net.Sockets.SocketAsyncEventArgs.SocketAsyncEventArgs() [ %0 ]
                // %2 := System.Net.Sockets.SocketAsyncEventArgs.UserToken.set [ %0 const ]
                // e := __id [ %0 ]

            e.Completed += _onComplete;
                // %3 := __id [ this._onComplete ]
                // %4 := System.Net.Sockets.SocketAsyncEventArgs.Completed.add [ System.Net.Sockets.SocketAsyncEventArgs const %3 ]

            return e;
        }
    }
}
";
            UcfgVerifier.VerifyInstructions(code, "CreateSocketAsyncEventArgs");
        }

        [TestMethod] //https://jira.sonarsource.com/browse/SONARSEC-201
        public void Peach_Exception_InvalidTarget_ArrayAccess()
        {
            const string code = @"
// Adapted from https://github.com/Microsoft/automatic-graph-layout/blob/5679bd68b0080b80527212e4229e4dea7e290014/GraphLayout/MSAGL/Layout/Incremental/ProcrustesCircleConstraint.cs#L245
namespace MSAGL
{
    public class Point
    {
        public Point(int X, int Y)
        {
        }
        public int X;
        public int Y;
    }

    public class Dummy
    {
        private static Point[] Transpose(Point[] X)
        {
            return new Point[] { new Point(X[0].X, X[1].X), new Point(X[0].Y, X[1].Y) };
                // %0 := new MSAGL.Point[]
                // %1 := __arrayGet [ X ]
                // %2 := __id [ %1.X ]
                // %3 := __arrayGet [ X ]
                // %4 := __id [ %3.X ]
                // %5 := new MSAGL.Point
                // %6 := MSAGL.Point.Point(int, int) [ %5 %2 %4 ]
                // %7 := __arrayGet [ X ]
                // %8 := __id [ %7.Y ]
                // %9 := __arrayGet [ X ]
                // %10 := __id [ %9.Y ]
                // %11 := new MSAGL.Point
                // %12 := MSAGL.Point.Point(int, int) [ %11 %8 %10 ]
                // %13 := __arraySet [ %0 %5 ]
                // %14 := __arraySet [ %0 %11 ]
        }
    }
}
";
            UcfgVerifier.VerifyInstructions(code, "Transpose");
        }

        [TestMethod] // https://jira.sonarsource.com/browse/SONARSEC-200
        public void Peach_Exception_InvalidTarget_Enum()
        {
            const string code = @"
// Adapted from https://github.com/Microsoft/automatic-graph-layout/blob/ba8a85105b8a420762b53fb0c8513b7f10fb30d9/GraphLayout/MSAGL/Layout/Incremental/Multipole/KDTree.cs
namespace Microsoft.Msagl.Layout.Incremental
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;

    public class KDTree
    {
        private Particle[] particlesBy(Particle.Dim d)
        {
            return null;
        }

        public void Constructor(Particle[] particles, int bucketSize)
        {
            Particle[][] ps = new Particle[][]
                // %0 := new Microsoft.Msagl.Layout.Incremental.Particle[][]
            {
                particlesBy(Particle.Dim.Horizontal),
                    // %1 := __id [ Microsoft.Msagl.Layout.Incremental.Particle.Dim.Horizontal ]
                    // %2 := Microsoft.Msagl.Layout.Incremental.KDTree.particlesBy(Microsoft.Msagl.Layout.Incremental.Particle.Dim) [ this %1 ]
                particlesBy(Particle.Dim.Vertical)
                    // %3 := __id [ Microsoft.Msagl.Layout.Incremental.Particle.Dim.Vertical ]
                    // %4 := Microsoft.Msagl.Layout.Incremental.KDTree.particlesBy(Microsoft.Msagl.Layout.Incremental.Particle.Dim) [ this %3 ]
            };
                // %5 := __arraySet [ %0 %2 ]
                // %6 := __arraySet [ %0 %4 ]
                // ps := __id [ %0 ]
        }
    }

    public class Particle
    {
        internal enum Dim { Horizontal = 0, Vertical = 1 };
    }
}
";
            UcfgVerifier.VerifyInstructions(code, "Constructor");
        }
    }
}
