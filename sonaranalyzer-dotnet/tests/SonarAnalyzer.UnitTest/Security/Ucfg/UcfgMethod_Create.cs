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

using csharp::SonarAnalyzer.ControlFlowGraph.CSharp;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.Security.Ucfg
{
    [TestClass]
    public class UcfgMethod_Create
    {
        [TestMethod]
        public void GetMethodId_Methods()
        {
            const string code = @"
using System;
using System.Collections.Generic;
namespace Namespace
{
    public class Class1
    {
        public void PrimitiveTypes1(string s) { }
        public void PrimitiveTypes2(String s) { }
        public void PrimitiveTypes3(System.String s) { }

        public void ArrayTypes1(string[] s) { }
        public void ArrayTypes2(string[,] s) { }
        public void ArrayTypes3(string[][] s) { }

        public void UserType(Class1 s) { }
        public void SystemType(Uri uri) { }
        public void GenericMethod1<T1,T2>(T1 x) { }

        public void Nullables(int? x, Nullable<int> y) { }

        public void GenericArgument(IEnumerable<string> strings) { }

        public void Overload1(string s) { }
        public void Overload1(string s, string s) { }

        public void NestedMethods()
        {
            void Foo() { } // We don't really support nested, but we should not throw
        }

        public class Class2
        {
            public void InnerClass(Class2 s) { }
        }
    }
    public class GenericClass<T1>
    {
        public void GenericMethod2(T1 x) { }
        public void GenericMethod3<T2>(T1 x, T2 y) { }
    }
    public class BaseClass<T1>
    {
        public virtual void Method1(T1 x) { }
        public virtual void Method2(T1 x) { }
    }
    public class Descendant : BaseClass<string>
    {
        public override void Method1(string x) { }
    }
    namespace Inner
    {
        public class Class2
        {
            public string InnerNamespace() { }
        }
    }
}
public class Class3
{
    public void NoNamespace(Class3 s) { }
}
public static class Extensions
{
    public static void Extension(this string s, int x) { }
}";
            var (syntaxTree, semanticModel) = TestHelper.Compile(code);

            GetMethodId("PrimitiveTypes1").Should().Be("Namespace.Class1.PrimitiveTypes1(string)");
            GetMethodId("PrimitiveTypes2").Should().Be("Namespace.Class1.PrimitiveTypes2(string)");
            GetMethodId("PrimitiveTypes3").Should().Be("Namespace.Class1.PrimitiveTypes3(string)");
            GetMethodId("ArrayTypes1").Should().Be("Namespace.Class1.ArrayTypes1(string[])");
            GetMethodId("ArrayTypes2").Should().Be("Namespace.Class1.ArrayTypes2(string[*,*])");
            GetMethodId("ArrayTypes3").Should().Be("Namespace.Class1.ArrayTypes3(string[][])");
            GetMethodId("UserType").Should().Be("Namespace.Class1.UserType(Namespace.Class1)");
            GetMethodId("SystemType").Should().Be("Namespace.Class1.SystemType(System.Uri)");
            GetMethodId("GenericMethod1").Should().Be("Namespace.Class1.GenericMethod1<T1, T2>(T1)");
            GetMethodId("Nullables").Should().Be("Namespace.Class1.Nullables(int?, int?)");
            GetMethodId("GenericArgument").Should().Be("Namespace.Class1.GenericArgument(System.Collections.Generic.IEnumerable<string>)");
            GetMethodId("Overload1").Should().Be("Namespace.Class1.Overload1(string)");
            GetMethodId("Overload1", skip: 1).Should().Be("Namespace.Class1.Overload1(string, string)");
            GetMethodId("GenericMethod2").Should().Be("Namespace.GenericClass<T1>.GenericMethod2(T1)");
            GetMethodId("GenericMethod3").Should().Be("Namespace.GenericClass<T1>.GenericMethod3<T2>(T1, T2)");
            GetMethodId("InnerClass").Should().Be("Namespace.Class1.Class2.InnerClass(Namespace.Class1.Class2)");
            GetMethodId("Method1").Should().Be("Namespace.BaseClass<T1>.Method1(T1)");
            GetMethodId("Method1", skip: 1).Should().Be("Namespace.Descendant.Method1(string)");
            GetMethodId("InnerNamespace").Should().Be("Namespace.Inner.Class2.InnerNamespace()");
            GetMethodId("NoNamespace").Should().Be("Class3.NoNamespace(Class3)");
            GetMethodId("Extension").Should().Be("Extensions.Extension(string, int)");

            string GetMethodId(string methodName, int skip = 0) =>
                semanticModel.GetDeclaredSymbol(syntaxTree.GetMethod(methodName, skip)).ToUcfgMethodId();
        }

        [TestMethod]
        public void GetMethodId_Properties()
        {
            const string code = @"
using System;
namespace Namespace
{
    public class Class1
    {
        public string Property1
        {
            get { return null; }
            set { }
        }
        public string Property2 => null;
    }
    public class GenericClass<T1>
    {
        public T1 Property3 { get; set; }
    }
    public class BaseClass<T1>
    {
        public virtual T1 Property4
        {
            get { return default(T1); }
            set { }
        }
    }
    public class Descendant : BaseClass<string>
    {
        public override string Property4
        {
            get { return null; }
            set { }
        }
    }
    namespace Inner
    {
        public class Class2
        {
            public string Property5 { get; set; }
        }
    }
}
public class Class3
{
    public string Property6 { get; set; }
}";
            var (syntaxTree, semanticModel) = TestHelper.Compile(code);

            PropertyGetId("Property1").Should().Be("Namespace.Class1.Property1.get");
            PropertySetId("Property1").Should().Be("Namespace.Class1.Property1.set");

            PropertyGetId("Property2").Should().Be("Namespace.Class1.Property2.get");
            PropertySetId("Property2").Should().Be("__unknown");

            PropertyGetId("Property3").Should().Be("Namespace.GenericClass<T1>.Property3.get");
            PropertySetId("Property3").Should().Be("Namespace.GenericClass<T1>.Property3.set");

            PropertyGetId("Property4").Should().Be("Namespace.BaseClass<T1>.Property4.get");
            PropertySetId("Property4").Should().Be("Namespace.BaseClass<T1>.Property4.set");

            PropertyGetId("Property4", 1).Should().Be("Namespace.Descendant.Property4.get");
            PropertySetId("Property4", 1).Should().Be("Namespace.Descendant.Property4.set");

            PropertyGetId("Property5").Should().Be("Namespace.Inner.Class2.Property5.get");
            PropertySetId("Property5").Should().Be("Namespace.Inner.Class2.Property5.set");

            PropertyGetId("Property6").Should().Be("Class3.Property6.get");
            PropertySetId("Property6").Should().Be("Class3.Property6.set");

            string PropertyGetId(string propertyName, int skip = 0) =>
                semanticModel.GetDeclaredSymbol(syntaxTree.GetProperty(propertyName, skip)).GetMethod.ToUcfgMethodId();

            string PropertySetId(string propertyName, int skip = 0) =>
                semanticModel.GetDeclaredSymbol(syntaxTree.GetProperty(propertyName, skip)).SetMethod.ToUcfgMethodId();
        }

        [TestMethod]
        public void GetMethodId_Constructors()
        {
            const string code = @"
using System;
namespace Namespace
{
    public class Class1
    {
        public Class1() { }
        public Class1(string s) { }
    }
}
";
            var (syntaxTree, semanticModel) = TestHelper.Compile(code);

            CtorId("Class1").Should().Be("Namespace.Class1.Class1()");
            CtorId("Class1", skip: 1).Should().Be("Namespace.Class1.Class1(string)");

            string CtorId(string className, int skip = 0) =>
                semanticModel.GetDeclaredSymbol(syntaxTree.GetConstructor(className, skip)).ToUcfgMethodId();
        }

        [TestMethod]
        public void GetMethodId_Explicit_Interface_Implementations()
        {
            const string code = @"
using System;
using System.Collections.Generic;
namespace Namespace
{
    public class Bar : IBar
    {
        void IBar.Foo(string s) { }
        public void Foo(string s) { }
    }

    public class Foo : Bar
    {
        void IBar.Foo(string s) { }
    }

    public interface IBar
    {
        void Foo(string s);
    }
}
";

            var (syntaxTree, semanticModel) = TestHelper.Compile(code);

            GetMethodId("Foo").Should().Be("Namespace.Bar.Namespace.IBar.Foo(string)");
            GetMethodId("Foo", skip: 1).Should().Be("Namespace.Bar.Foo(string)");

            string GetMethodId(string methodName, int skip = 0) =>
                semanticModel.GetDeclaredSymbol(syntaxTree.GetMethod(methodName, skip)).ToUcfgMethodId();
        }
    }
}
