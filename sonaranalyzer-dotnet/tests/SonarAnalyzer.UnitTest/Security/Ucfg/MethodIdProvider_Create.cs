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
using csharp::SonarAnalyzer.Security.Ucfg;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.Security.Ucfg
{
    [TestClass]
    public class MethodIdProvider_Create
    {
        [TestMethod]
        public void MethodIdProvider_Create_Methods()
        {
            const string code = @"
using System;
namespace Namespace
{
    public class Class1
    {
        public void PrimitiveTypes1(string s) { }
        public void PrimitiveTypes2(String s) { }
        public void PrimitiveTypes3(System.String s) { }

        public void ArrayTypes(string[] s) { } // TODO: improve array type signature

        public void UserType(Class1 s) { }
        public void SystemType(Uri uri) { }
        public void GenericMethod1<T1,T2>(T1 x) { }

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
}";
            var (syntaxTree, semanticModel) = TestHelper.Compile(code);

            MethodId("PrimitiveTypes1").Should().Be("project.dll;Namespace.Class1.PrimitiveTypes1(mscorlib;string)");
            MethodId("PrimitiveTypes2").Should().Be("project.dll;Namespace.Class1.PrimitiveTypes2(mscorlib;string)");
            MethodId("PrimitiveTypes3").Should().Be("project.dll;Namespace.Class1.PrimitiveTypes3(mscorlib;string)");
            MethodId("ArrayTypes").Should().Be("project.dll;Namespace.Class1.ArrayTypes(__unknown)");
            MethodId("UserType").Should().Be("project.dll;Namespace.Class1.UserType(project.dll;Namespace.Class1)");
            MethodId("SystemType").Should().Be("project.dll;Namespace.Class1.SystemType(System;System.Uri)");
            MethodId("GenericMethod1").Should().Be("project.dll;Namespace.Class1.GenericMethod1<T1,T2>(T1)");
            MethodId("Overload1").Should().Be("project.dll;Namespace.Class1.Overload1(mscorlib;string)");
            MethodId("Overload1", skip: 1).Should().Be("project.dll;Namespace.Class1.Overload1(mscorlib;string,mscorlib;string)");
            MethodId("GenericMethod2").Should().Be("project.dll;Namespace.GenericClass<T1>.GenericMethod2(T1)");
            MethodId("GenericMethod3").Should().Be("project.dll;Namespace.GenericClass<T1>.GenericMethod3<T2>(T1,T2)");
            MethodId("InnerClass").Should().Be("project.dll;Namespace.Class1.Class2.InnerClass(project.dll;Namespace.Class1.Class2)");
            MethodId("Method1").Should().Be("project.dll;Namespace.BaseClass<T1>.Method1(T1)");
            MethodId("Method1", skip: 1).Should().Be("project.dll;Namespace.Descendant.Method1(mscorlib;string)");
            MethodId("InnerNamespace").Should().Be("project.dll;Namespace.Inner.Class2.InnerNamespace()");
            MethodId("NoNamespace").Should().Be("project.dll;Class3.NoNamespace(project.dll;Class3)");

            string MethodId(string methodName, int skip = 0) =>
                MethodIdProvider.Create(semanticModel.GetDeclaredSymbol(syntaxTree.GetMethod(methodName, skip)));
        }

        [TestMethod]
        public void MethodIdProvider_Create_Properties()
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

            PropertyGetId("Property1").Should().Be("project.dll;Namespace.Class1.get_Property1()");
            PropertySetId("Property1").Should().Be("project.dll;Namespace.Class1.set_Property1(mscorlib;string)");

            PropertyGetId("Property2").Should().Be("project.dll;Namespace.Class1.get_Property2()");
            PropertySetId("Property2").Should().Be(KnownMethodId.Unknown);

            PropertyGetId("Property3").Should().Be("project.dll;Namespace.GenericClass<T1>.get_Property3()");
            PropertySetId("Property3").Should().Be("project.dll;Namespace.GenericClass<T1>.set_Property3(T1)");

            PropertyGetId("Property4").Should().Be("project.dll;Namespace.BaseClass<T1>.get_Property4()");
            PropertySetId("Property4").Should().Be("project.dll;Namespace.BaseClass<T1>.set_Property4(T1)");

            PropertyGetId("Property4", 1).Should().Be("project.dll;Namespace.Descendant.get_Property4()");
            PropertySetId("Property4", 1).Should().Be("project.dll;Namespace.Descendant.set_Property4(mscorlib;string)");

            PropertyGetId("Property5").Should().Be("project.dll;Namespace.Inner.Class2.get_Property5()");
            PropertySetId("Property5").Should().Be("project.dll;Namespace.Inner.Class2.set_Property5(mscorlib;string)");

            PropertyGetId("Property6").Should().Be("project.dll;Class3.get_Property6()");
            PropertySetId("Property6").Should().Be("project.dll;Class3.set_Property6(mscorlib;string)");

            string PropertyGetId(string propertyName, int skip = 0) =>
                MethodIdProvider.Create(semanticModel.GetDeclaredSymbol(syntaxTree.GetProperty(propertyName, skip)).GetMethod);

            string PropertySetId(string propertyName, int skip = 0) =>
                MethodIdProvider.Create(semanticModel.GetDeclaredSymbol(syntaxTree.GetProperty(propertyName, skip)).SetMethod);
        }
    }
}
