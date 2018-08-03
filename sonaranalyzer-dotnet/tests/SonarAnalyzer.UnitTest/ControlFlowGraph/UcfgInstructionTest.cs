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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.ControlFlowGraph
{
    [TestClass]
    public partial class UcfgInstructionTest
    {
        [TestMethod]
        public void Static_Ctors()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        static Class1()
        {
            Do();
                // %0 := Namespace.Class1.Do() [ Namespace.Class1 ]
        }

        public static void Do() {}
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Class1", true);
        }

        [TestMethod]
        public void ControllerMethod_Contains_EntryPoint_And_Attributes()
        {
            const string code = @"
using System.ComponentModel;
using System.Web.Mvc;
public class Class1 : Controller
{
    public class DummyAttribute : System.Attribute { }

    private string field;
    [HttpPost] // should be ignored
    public void Foo([Description]string s, [Missing]string x,
                    [Dummy] int i, [DummyAttribute]string s2) {}
        // %0 := __entrypoint [ s x const s2 ]
        // %1 := __annotate [ System.ComponentModel.DescriptionAttribute.DescriptionAttribute() s ]
        // s := __annotation [ %1 ]
        // i is a const so the attribute is ignored
        // the Missing attribute is unknown and is not included
        // %2 := __annotate [ Class1.DummyAttribute.DummyAttribute() s2 ]
        // s2 := __annotation [ %2 ]
}";
            var ucfg = UcfgVerifier.VerifyInstructions(code, "Foo");

            var entryPoints = UcfgVerifier.GetEntryPointInstructions(ucfg);
            entryPoints.Count.Should().Be(1);

            // Entry point location should be the "Foo" token.
            // Line numbers are 1-based, offsets are 0-based
            var actualLocation = entryPoints[0].Assigncall.Location;
            actualLocation.StartLine.Should().Be(10);
            actualLocation.EndLine.Should().Be(10);
            actualLocation.StartLineOffset.Should().Be(16);
            actualLocation.EndLineOffset.Should().Be(19);
        }

        [TestMethod]
        public void Annotation_EntryMethod_AttributeOnStringParameterIsHandled()
        {
            const string code = @"
namespace Namespace
{
    using System.Web.Mvc;

    public class FromBodyAttribute : System.Attribute { }

    public class CartController : Controller
    {
        public object Remove([FromBody] string itemId)
        {
            var data = itemId;
                // data := __id [ itemId ]
                // %0 := __entrypoint [ itemId ]
                // %1 := __annotate [ Namespace.FromBodyAttribute.FromBodyAttribute() itemId ]
                // itemId := __annotation [ %1 ]

            return null;
        }
    }
}";
            var ucfg = UcfgVerifier.VerifyInstructions(code, "Remove");

            var entryPoints = UcfgVerifier.GetEntryPointInstructions(ucfg);
            entryPoints.Count.Should().Be(1);

            // Entry point location should be the "Remove" token.
            // Line numbers are 1-based, offsets are 0-based
            var actualLocation = entryPoints[0].Assigncall.Location;
            actualLocation.StartLine.Should().Be(10);
            actualLocation.EndLine.Should().Be(10);
            actualLocation.StartLineOffset.Should().Be(22);
            actualLocation.EndLineOffset.Should().Be(28);
        }

        [TestMethod]
        public void Annotation_EntryMethod_AttributeOnNonStringParameterIsHandled()
        {
            // Bug 169
            const string code = @"
namespace Namespace
{
    using System.Web.Mvc;

    public class FromBodyAttribute : System.Attribute { }

    public class CartController : Controller
    {
        public object Remove([FromBody] long itemId)
        {
            var data = itemId;
                // data := __id [ const ]
                // %0 := __entrypoint [ const ]

            return null;
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Remove");
        }

        [TestMethod]
        public void Annotation_NotEntryMethod_AttibutesAreIgnored()
        {
            const string code = @"
namespace Namespace
{
    public class FromBodyAttribute : System.Attribute { }

    public class NotAController
    {
        public object Foo([FromBody] long itemId, [FromBody] string itemId2)
        {
            var data = itemId;
                // data := __id [ const ]
            var data = itemId2;
                // data := __id [ itemId2 ]
            return null;
        }
    }
}";
            var ucfg = UcfgVerifier.VerifyInstructions(code, "Foo");

            var entryPoints = UcfgVerifier.GetEntryPointInstructions(ucfg);
            entryPoints.Count.Should().Be(0);
        }

        [TestMethod]
        public void ConstantExpressions_Share_The_Same_Instance()
        {
            const string code = @"
public class Class1
{
    private string field;
    public void Foo(string s)
    {
        string a = ""a"";
            // a := __id [ const ]
        string b = ""b"";
            // b := __id [ const ]
        string c = ""c"";
            // c := __id [ const ]
    }
}";
            var ucfg = UcfgVerifier.GetUcfgForMethod(code, "Foo");

            var a = ucfg.BasicBlocks[0].Instructions[0].Assigncall.Args[0];
            var b = ucfg.BasicBlocks[0].Instructions[1].Assigncall.Args[0];
            var c = ucfg.BasicBlocks[0].Instructions[2].Assigncall.Args[0];

            // The constant expressions share the same instance of the Const value
            // for performance and simplicity. The protobuf serializer will deserialize
            // the values as a singleton again.
            a.Should().Be(b);
            a.Should().Be(c);
        }

        [TestMethod]
        public void BuiltInTypeAreConvertedToConstant()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(bool boolArg, byte byteArg, sbyte sbyteArg, char charArg, decimal decimalArg, double doubleArg,
            float floatArg, int intArg, uint uintArg, long longArg, ulong ulongArg, short shortArg, ushort ushortArg)
        {
            bool @bool = true;      // @bool := __id [ const ]
            byte @byte = 1;         // @byte := __id [ const ]
            sbyte @sbyte = 1;       // @sbyte := __id [ const ]
            char @char = 'c';       // @char := __id [ const ]
            decimal @decimal = 1;   // @decimal := __id [ const ]
            double @double = 1;     // @double := __id [ const ]
            float @float = 1;       // @float := __id [ const ]
            int @int = 1;           // @int := __id [ const ]
            uint @uint = 1;         // @uint := __id [ const ]
            long @long = 1;         // @long := __id [ const ]
            ulong @ulong = 1;       // @ulong := __id [ const ]
            short @short = 1;       // @short := __id [ const ]
            ushort @ushort = 1;     // @ushort := __id [ const ]

            Bar(true);              // %0 := Namespace.Class1.Bar(object) [ this const ]
            Bar((byte) 1);          // %1 := Namespace.Class1.Bar(object) [ this const ]
            Bar((sbyte) 1);         // %2 := Namespace.Class1.Bar(object) [ this const ]
            Bar('c');               // %3 := Namespace.Class1.Bar(object) [ this const ]
            Bar((decimal) 1);       // %4 := Namespace.Class1.Bar(object) [ this const ]
            Bar((double) 1);        // %5 := Namespace.Class1.Bar(object) [ this const ]
            Bar((float) 1);         // %6 := Namespace.Class1.Bar(object) [ this const ]
            Bar((int) 1);           // %7 := Namespace.Class1.Bar(object) [ this const ]
            Bar((uint) 1);          // %8 := Namespace.Class1.Bar(object) [ this const ]
            Bar((long) 1);          // %9 := Namespace.Class1.Bar(object) [ this const ]
            Bar((ulong) 1);         // %10 := Namespace.Class1.Bar(object) [ this const ]
            Bar((short) 1);         // %11 := Namespace.Class1.Bar(object) [ this const ]
            Bar((ushort) 1);        // %12 := Namespace.Class1.Bar(object) [ this const ]

            Bar(boolArg);           // %13 := Namespace.Class1.Bar(object) [ this const ]
            Bar(byteArg);           // %14 := Namespace.Class1.Bar(object) [ this const ]
            Bar(sbyteArg);          // %15 := Namespace.Class1.Bar(object) [ this const ]
            Bar(charArg);           // %16 := Namespace.Class1.Bar(object) [ this const ]
            Bar(decimalArg);        // %17 := Namespace.Class1.Bar(object) [ this const ]
            Bar(doubleArg);         // %18 := Namespace.Class1.Bar(object) [ this const ]
            Bar(floatArg);          // %19 := Namespace.Class1.Bar(object) [ this const ]
            Bar(intArg);            // %20 := Namespace.Class1.Bar(object) [ this const ]
            Bar(uintArg);           // %21 := Namespace.Class1.Bar(object) [ this const ]
            Bar(longArg);           // %22 := Namespace.Class1.Bar(object) [ this const ]
            Bar(ulongArg);          // %23 := Namespace.Class1.Bar(object) [ this const ]
            Bar(shortArg);          // %24 := Namespace.Class1.Bar(object) [ this const ]
            Bar(ushortArg);         // %25 := Namespace.Class1.Bar(object) [ this const ]
        }

        public void Bar(object o) {}
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void FieldChaining()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public Class1 field = new Class1();
        public static Class1 staticField = new Class1();

        public void Foo()
        {
            field.field.field = new Class1();
                // %0 := __id [ this.field ]
                // %1 := __id [ %0.field ]
                // %2 := new Namespace.Class1
                // %3 := Namespace.Class1.Class1() [ %2 ]
                // %1.field := __id [ %2 ]

            Class1.staticField.field = new Class1();
                // %4 := __id [ Namespace.Class1.staticField ]
                // %5 := new Namespace.Class1
                // %6 := Namespace.Class1.Class1() [ %5 ]
                // %4.field := __id [ %5 ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void PropertyChaining()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public Class1 Property { get; set; }
        public static Class1 StaticProperty { get; set; }

        public void Foo()
        {
            Property.Property.Property = new Class1();
                // %0 := Namespace.Class1.Property.get [ this ]
                // %1 := Namespace.Class1.Property.get [ %0 ]
                // %2 := new Namespace.Class1
                // %3 := Namespace.Class1.Class1() [ %2 ]
                // %4 := Namespace.Class1.Property.set [ %1 %2 ]

            Class1.StaticProperty.Property = new Class1();
                // %5 := Namespace.Class1.StaticProperty.get [ Namespace.Class1 ]
                // %6 := new Namespace.Class1
                // %7 := Namespace.Class1.Class1() [ %6 ]
                // %8 := Namespace.Class1.Property.set [ %5 %6 ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Pointers()
        {
            const string code = @"
using System;
namespace Namespace
{
    public class Class1
    {
        IntPtr ptrField;
        UIntPtr uPtrField;

        public void Foo(IntPtr ptrParam, UIntPtr uPtrParam)
        {
            int x = 100;
                // x := __id [ const ]

            int *ptr = &x;
                // ptr := __id [ const ]

            Console.WriteLine((int)ptr);
                // %0 := System.Console.WriteLine(int) [ System.Console const ]

            Console.WriteLine(*ptr);
                // %1 := System.Console.WriteLine(int) [ System.Console const ]

            ptrField = ptrParam;
                // this.ptrField := __id [ const ]
            uPtrField = uPtrParam;
                // this.uPtrField := __id [ const ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ControllerMethod_Contains_EntryPointWithNoArguments()
        {
            const string code = @"
using System.ComponentModel;
using System.Web.Mvc;
public class Class1 : Controller
{
    private string field;
    public void Foo() // %0 := __entrypoint [  ]
    {
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Bug169_CreationError_RegressionTest_NullRef()
        {
            // SimplCommerce\src\Modules\SimplCommerce.Module.Reviews\Controllers\ReviewApiController.cs :: ChangeStatus
            // SimplCommerce\src\Modules\SimplCommerce.Module.ShoppingCart\Controllers\CartController.cs :: Remove

            // Exception at: CreateFromAttributSyntax->CreateAnnotationCall->CreateFunctionCall->ApplyAsTarget

            const string code = @"
namespace Namespace
{
    using System.Web.Mvc;

    public class FromBodyAttribute : System.Attribute { }

    public class CartController : Controller
    {
        [HttpPost]
        public object Remove([FromBody] long itemId)
        {
            // %0 := __entrypoint [ const ]
            return null;
        }
    }
}
";
            UcfgVerifier.VerifyInstructions(code, "Remove");
        }

        [TestMethod]
        public void Bug170_CreationError_RegressionTest_SequenceContainedNullElement()
        {
            // SimplCommerce.Module.Catalog.Components.CategoryBreadcrumbViewComponent.Invoke(long ?, System.Collections.Generic.IEnumerable<long>)
            // SimplCommerce\src\Modules\SimplCommerce.Module.Shipping\Models\ShippingProvider.cs :: get_OnlyCountryIds
            // SimplCommerce\src\Modules\SimplCommerce.Module.Shipping\Models\ShippingProvider.cs :: get_OnlyStateOrProvinceIds

            // Exception at: UcfgInstructionFactory.CreateFunctionCall

            const string code = @"
namespace SimplCommerce.Module.Shipping.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class ShippingProvider //: EntityBase
    {
        public string OnlyCountryIdsString { get; set; }

        public IList<long> OnlyCountryIds
        {
            get
            {
                if (string.IsNullOrWhiteSpace(OnlyCountryIdsString))
                    // %0 := SimplCommerce.Module.Shipping.Models.ShippingProvider.OnlyCountryIdsString.get [ this ]
                    // %1 := string.IsNullOrWhiteSpace(string) [ string %0 ]
                {
                    return new List<long>();
                        // %2 := new System.Collections.Generic.List<T>
                        // %3 := System.Collections.Generic.List<T>.List() [ %2 ]
                }

                return OnlyCountryIdsString.Split(',')
                        // %4 := SimplCommerce.Module.Shipping.Models.ShippingProvider.OnlyCountryIdsString.get [ this ]
                        // %5 := string.Split(params char[]) [ %4 const ]
                    .Select(long.Parse)
                        // %6 := System.Linq.Enumerable.Select<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TResult>) [ System.Linq.Enumerable %5 const ]
                    .ToList();
                        // %7 := System.Linq.Enumerable.ToList<TSource>(System.Collections.Generic.IEnumerable<TSource>) [ System.Linq.Enumerable %6 ]
            }
        }
    }
}
";
            UcfgVerifier.VerifyInstructionsForPropertyGetter(code, "OnlyCountryIds");
        }

        [TestMethod]
        public void Bug171_CreationError_RegressionTest_UnexpectedMergedNamespaceSymbol()
        {
            // SimplCommerce\src\Modules\SimplCommerce.Module.PaymentPaypalExpress\Controllers\PaypalExpressController.cs :: GetAccessToken
            // SimplCommerce\src\SimplCommerce.WebHost\Program.cs :: BuildWebHost2

            // At: UcfgExpressionService.Create

            // This code gives a similar repro, except with "SourceNamespaceSymbol" instead of
            // "MergedNamespaceSymbol" (
            const string code = @"

namespace Ns1
{
    namespace Inner
    {
        public class Builder
        {
            public static Builder CreateDefaultBuilder() => null;
        }
    }
}

namespace Ns2
{
    public class Class1
    {
        public void BuildWebHost2(string[] args)
        {
            Ns1.Inner.Builder.CreateDefaultBuilder();
                // %0 := Ns1.Inner.Builder.CreateDefaultBuilder() [ Ns1.Inner.Builder ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "BuildWebHost2");
        }

        [TestMethod]
        public void Operator_UnsupportedTypes()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo()
        {
            int x;
            double d;
            x = 1 + 2;
                // x := __id [ const ]
            x = 1 - 2;
                // x := __id [ const ]
            x = 1 * 2;
                // x := __id [ const ]
            x = 1 / 2;
                // x := __id [ const ]
            d = 1.0 / 2;
                // d := __id [ const ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Operator_SupportedTypes()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(string parameterString, Class1 some, Class1 other)
        {
            var result = parameterString == null;
                // %0 := string.operator ==(string, string) [ string parameterString const ]
                // result := __id [ %0 ]
            result = parameterString != null;
                // %1 := string.operator !=(string, string) [ string parameterString const ]
                // result := __id [ %1 ]
            Class1 classResult = some + other;
                // %2 := Namespace.Class1.operator +(Namespace.Class1, Namespace.Class1) [ Namespace.Class1 some other ]
                // classResult := __id [ %2 ]
            classResult = classResult + some + other;
                // %3 := Namespace.Class1.operator +(Namespace.Class1, Namespace.Class1) [ Namespace.Class1 classResult some ]
                // %4 := Namespace.Class1.operator +(Namespace.Class1, Namespace.Class1) [ Namespace.Class1 %3 other ]
                // classResult := __id [ %4 ]
        }
        public static Class1 operator+ (Class1 left, Class1 right)
        {
            return null;
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void UnaryExpressions_AreIgnored()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo()
        {
            int negative = -1;
                // negative := __id [ const ]
            int positive = 1;
                // positive := __id [ const ]
            int result;

            result = +negative;  // result = -1
                // result := __id [ const ]
            result = +positive;  // result = 1
                // result := __id [ const ]

            result = -negative;  // result = 1
                // result := __id [ const ]
            result = -positive   // result = -1
                // result := __id [ const ]

            int count;
            int index = 0;
                // index := __id [ const ]

            count = index++;    // count = 0, index = 1
                // count := __id [ const ]
            count = ++index;    // count = 2, index = 2
                // count := __id [ const ]

            count = index--;    // count = 2, index = 1
                // count := __id [ const ]
            count = --index;    // count = 0, index = 0
                // count := __id [ const ]

            bool b = false;
                // b := __id [ const ]

            b = !b;
                // b := __id [ const ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Indexer_GetAccess()
        {
            const string code = @"
namespace Namespace
{
    using System.Collections.Generic;
    public class Class1
    {
        public void Foo(string s, List<string> list, Class1 myClass)
        {
            var character = s[0];
                // %0 := string.this[int].get [ s const ]
                // character := __id [ %0 ]

            var i = list[0];
                // %1 := System.Collections.Generic.List<T>.this[int].get [ list const ]
                // i := __id [ %1 ]

            var result = myClass[s, 0, 1.0];
                // %2 := Namespace.Class1.this[string, int, double].get [ myClass s const const ]
                // result := __id [ %2 ]
        }

        public string this[string s, int i, double d]
        {
            get { return ""bar""; }
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Indexer_SetAccess()
        {
            const string code = @"
namespace Namespace
{
    using System.Collections.Generic;
    public class Class1
    {
        public void Foo(string s, List<string> list, Class1 myClass)
        {
            list[0] = ""bar"";
                // %0 := System.Collections.Generic.List<T>.this[int].set [ list const ]

            myClass[s, 0, 1.0] = ""bar"";
                // %1 := Namespace.Class1.this[string, int, double].set [ myClass s const const ]
        }

        public string this[string s, int i, double d]
        {
            set {}
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }
    }
}
