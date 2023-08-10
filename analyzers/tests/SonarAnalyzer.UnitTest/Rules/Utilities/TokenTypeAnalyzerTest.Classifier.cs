/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.UnitTest.Rules;

public partial class TokenTypeAnalyzerTest
{
    [TestMethod]
    public void ClassClassifications() =>
        ClassifierTestHarness.AssertTokenTypes("""
            [k:using] [u:System];
            [k:public] [k:class] [t:Test]
            {
                [c:// SomeComment]
                [k:public] [t:Test]() { }

                [c:/// <summary>
                /// A Prop
                /// </summary>
            ]   [k:int] [u:Prop] { [k:get]; }
                [k:void] [u:Method]<[t:T]>([t:T] [u:t]) [k:where] [t:T]: [k:class], [t:IComparable], [u:System].[u:Collections].[t:IComparer]
                {
                    [k:var] [u:i] = [n:1];
                    var s = [s:"Hello"];
                    [t:T] [u:local] = [k:default];
                }
            [u:}]
            """);

    [DataTestMethod]
    [DataRow(""" [s:"Text"] """)]
    [DataRow(""" [s:""] """)]
    [DataRow(""" [s:"  "] """)]
    [DataRow(""" [s:@""] """)]
    [DataRow(""" [s:$"]{true}[s:"] """)]
    [DataRow(""" [s:$"][s:  ]{true}[s:   ][s:"] """)]
    [DataRow(""" [s:$@"]{true}[s:"] """)]
    [DataRow("""" [s:""" """] """")]
    [DataRow("""" [s:$"""]{true}[s:"""] """")]
    [DataRow("""" [s:$"""][s:   ]{true}[s:   ][s:"""] """")]
    public void StringClassClassification(string stringExpression) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            public class Test
            {
                void Method()
                {
                    string s = {{stringExpression}};
                }
            }
            """);

    [TestMethod]
    public void IdentifierToken_QueryComprehensions() =>
        ClassifierTestHarness.AssertTokenTypes("""
            using System.Linq;
            public class Test {
                public void M()
                {
                    _ = from [u:i] in new int[0]
                        let [u:j] = i
                        join [u:k] in new int[0] on i equals k into [u:l]
                        select l into [u:m]
                        select m;
                }
            }
            """, false);

    [TestMethod]
    public void IdentifierToken_VariableDeclarator() =>
        ClassifierTestHarness.AssertTokenTypes("""
            public class Test {
                int [u:i] = 0, [u:j] = 0;
                public void M()
                {
                    int [u:k], [u:l];
                }
            }
            """, false);

    [TestMethod]
    public void IdentifierToken_LabeledStatement() =>
        ClassifierTestHarness.AssertTokenTypes("""
            public class Test {
                public void M()
                {
                    goto Label;
            [u:Label]:
                    ;
                }
            }
            """, false);

    [TestMethod]
    public void IdentifierToken_Catch() =>
        ClassifierTestHarness.AssertTokenTypes("""
            public class Test {
                public void M()
                {
                    try { }
                    catch(System.Exception [u:ex]) { }
                }
            }
            """, false);

    [TestMethod]
    public void IdentifierToken_ForEach() =>
        ClassifierTestHarness.AssertTokenTypes("""
            public class Test {
                public void M()
                {
                    foreach(var [u:i] in new int[0])
                    {
                    }
                }
            }
            """, false);

    [TestMethod]
    public void IdentifierToken_MethodParameterConstructorDestructorLocalFunctionPropertyEvent() =>
        ClassifierTestHarness.AssertTokenTypes("""
            public class Test {
                public int [u:Prop] { get; set; }
                public event System.EventHandler [u:TestEventField];
                public event System.EventHandler [u:TestEventDeclaration] { add { } remove { } }
                public [t:Test]() { }
                public void [u:Method]<[t:T]>(int [u:parameter]) { }
                ~[t:Test]()
                {
                    void [u:LocalFunction]() { }
                }
            }
            """, false);

    [TestMethod]
    public void IdentifierToken_BaseTypeDelegateEnumMember() =>
        ClassifierTestHarness.AssertTokenTypes("""
            public class [t:TestClass] { }
            public struct [t:TestStruct] { }
            public record [t:TestRecord] { }
            public record struct [t:TestRecordStruct] { }
            public delegate void [t:TestDelegate]();
            public enum [t:TestEnum] { [u:EnumMember] }
            """, false);

    [TestMethod]
    public void IdentifierToken_TupleDesignation() =>
        ClassifierTestHarness.AssertTokenTypes("""
            class Test
            {
                void M()
                {
                    var ([u:i], [u:j]) = (1, 2);
                    (int [u:i], int [u:j]) [u:t];
                }
            }
            """, false);

    [TestMethod]
    public void IdentifierToken_FunctionPointerUnmanagedCallingConvention() =>
        ClassifierTestHarness.AssertTokenTypes("""
            unsafe class Test
            {
                void M(delegate* unmanaged[[u:Cdecl]]<int, int> m) { }
            }
            """, false);

    [TestMethod]
    public void IdentifierToken_ExternAlias() =>
        ClassifierTestHarness.AssertTokenTypes("""
            extern alias [u:ThisIsAnAlias];
            public class Test {
            }
            """, false, true);

    [TestMethod]
    public void IdentifierToken_AccessorDeclaration() =>
        ClassifierTestHarness.AssertTokenTypes("""
            public class Test {
                public string Property { [u:unknown]; }
            }
            """, false, true);

    [DataTestMethod]
    [DataRow("assembly")]
    [DataRow("module")]
    [DataRow("event")]
    [DataRow("field")]
    [DataRow("method")]
    [DataRow("param")]
    [DataRow("property")]
    [DataRow("return")]
    [DataRow("type")]
    [DataRow("typevar")]
    public void IdentifierToken_AttributeTargetSpecifier_Keyword(string specifier) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            [[k:{{specifier}}]:System.Obsolete]
            public class Test {
            }
            """, false, true);

    [TestMethod]
    public void IdentifierToken_AttributeTargetSpecifier_UnknownSpecifier() =>
        ClassifierTestHarness.AssertTokenTypes("""
            [[k:unknown]:System.Obsolete]
            public class Test {
            }
            """, false, true);

    [DataTestMethod]
    [DataRow("using [u:System];", false)]
    [DataRow("using [u:x] = System.[t:Math];", false)]
    [DataRow("using x = [u:System].Math;", false)] // We cannot be sure without calling the model but we assume this will rarely be a type
    [DataRow("using [k:static] [u:System].[t:Math];", false)]
    [DataRow("using [k:static] [u:System].[u:Collections].[u:Generic].[t:List]<[k:int]>;", false)]
    [DataRow("using [k:static] [u:System].[u:Collections].[u:Generic].[t:List]<[u:System].[u:Collections].[u:Generic].[t:List]<[k:int]>>;", false)]
    [DataRow("using [k:static] [u:System].[u:Collections].[u:Generic].[t:HashSet]<[k:int]>.[t:Enumerator];", false)]
#if NET
    [DataRow("using [u:System].[u:Buffers];", false)]
#endif
    public void IdentifierToken_Usings(string syntax, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes(syntax /*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("namespace [u:A] { }", false)]
    [DataRow("namespace [u:A].[u:B] { }", false)]
    [DataRow("namespace [u:A];", false)]
    [DataRow("namespace [u:A].[u:B];", false)]
    public void IdentifierToken_Namespaces(string syntax, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes(syntax /*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("class TypeParameter: [t:List]<[t:Exception]> { }", false)]
    // We cannot be sure without calling the model but we assume this will rarely be a type
    [DataRow("class TypeParameter: System.Collections.Generic.[t:List]<System.[t:Exception]> { }", false)]
    [DataRow("class TypeParameter: [u:System].[u:Collections].[u:Generic].List<[u:System].Exception> { }", false)]
    [DataRow("class TypeParameter: [t:List]<[t:List]<[t:Exception]>> { }", false)]
    [DataRow("class TypeParameter: [t:Outer].[t:Inner] { }", false)] // To decide what we want to do
    [DataRow("class TypeParameter: [t:HashSet]<[t:List]<[k:int]>> { }", false)]
    [DataRow("class TypeParameter<T> : [t:List]<T> where T: [t:List]<[t:Exception]> { }", false)]
    [DataRow("class TypeParameter<[t:T]> : List<[t:T]> where [t:T]: List<Exception> { }", false)]
    [DataRow("class TypeParameter<[t:T1], [t:T2]> where [t:T1] : class where [t:T2] : [t:T1], new() { }", false)]
    public void IdentifierToken_TypeParameters(string syntax, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            class Outer { public class Inner { } }
            {{syntax}}
            """ /*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("class [t:BaseTypeList]: System.[t:Exception] { }", false)]
    [DataRow("class BaseTypeList: [u:System].Exception { }", false)]
    [DataRow("class BaseTypeList: [t:Outer].[t:Inner] { }", false)]
    [DataRow("""
             class BaseTypeList: [t:IFormattable], [t:IFormatProvider]
             {
                public object? GetFormat(Type? formatType) => default;
                public string ToString(string? format, IFormatProvider? formatProvider) => default;
             }
             """, false)]
    public void IdentifierToken_BaseTypeList(string syntax, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            class Outer { public class Inner { } }
            {{syntax}}
            """ /*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("class", false)]
    [DataRow("struct", false)]
    [DataRow("record", false)]
    [DataRow("record struct", false)]
#if NET
    [DataRow("interface", false)]
#endif
    public void IdentifierToken_BaseTypeList_DifferentTypeKind(string syntax, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            {{syntax}} X : [u:System].[t:IFormattable] { public string ToString(string? format, System.IFormatProvider? formatProvider) => null; }
            """ /*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("_ = typeof([u:System].[t:Exception]);", false)]
    [DataRow("_ = typeof([u:System].[u:Collections].[u:Generic].[t:Dictionary]<,>);", false)]
    [DataRow("_ = typeof([t:Inner]);", false)]
    [DataRow("_ = typeof([t:C].[t:Inner]);", false)]
    [DataRow("_ = typeof([t:Int32][]);", false)]
    [DataRow("_ = typeof([t:Int32]*);", false)]
    [DataRow("_ = typeof([k:delegate]*<[t:Int32], void>);", false)]
    public void IdentifierToken_TypeOf(string syntax, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            public class C
            {
                void TypeOf() { {{syntax}} }
                public class Inner { }
            }
            """ /*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("_ = nameof([t:Exception]);", false)]
    [DataRow("_ = nameof([u:System].[t:Exception]);", false)]
    [DataRow("_ = nameof([t:Dictionary]<[t:Int32], [t:Exception]>);", false)]
    [DataRow("_ = nameof([t:Dictionary]<[t:Int32], [u:System].[t:Exception]>);", false)]
    [DataRow("_ = nameof([u:System].[u:Collections].[u:Generic]);", false)]
    [DataRow("_ = nameof([u:NameOf]);", false)]
    [DataRow("_ = nameof([t:DateTimeKind].[u:Utc]);", false)]
    [DataRow("_ = nameof([t:Inner]);", false)]
    [DataRow("_ = nameof([t:C].[t:Inner]);", false)]
    public void IdentifierToken_NameOf(string syntax, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            public class C
            {
                void NameOf() { {{syntax}} }
                public class Inner { }
            }
            """ /*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("[n:42] is [t:Int32].[u:MinValue]", true)]                                          // IsPattern
    [DataRow("ex is [t:ArgumentException]", true)]
    [DataRow("ex is [u:System].[t:ArgumentException]", true)]
    [DataRow("ex is [t:ArgumentException] [u:argEx]", false)]
    [DataRow("ex is ArgumentException { InnerException: [t:InvalidOperationException] }", true)] // ConstantPattern: could also be a constant
    [DataRow("ex is ArgumentException { HResult: [t:Int32].[u:MinValue] }", true)]               // ConstantPattern: could also be a type
    [DataRow("ex is ArgumentException { HResult: [n:2] }", true)]
    [DataRow("ex is ArgumentException { [u:InnerException]: [t:InvalidOperationException] { } }", false)] // RecursivePattern.Type
    [DataRow("ex is ArgumentException { [u:InnerException].[u:InnerException]: [t:InvalidOperationException] { } [u:inner] }", false)]
    [DataRow("ex as [t:ArgumentException]", false)]
    [DataRow("ex as [u:System].ArgumentException", true)]
    [DataRow("([t:ArgumentException])ex", false)]
    [DataRow("([u:System].ArgumentException)ex", true)]
#if NET
    [DataRow("new object[0] is [[t:Exception]];", true)]
    [DataRow("new object[0] is [[t:Int32].[u:MinValue]];", true)]
#endif
    public void IdentifierToken_Expressions(string expression, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes(
            $$"""
              using System;
              using System.Collections.Generic;
              public class Test
              {
                  public void M(Exception ex)
                  {
                      var x = {{expression}};
                  }
              }
              """/*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("([k:string], [t:Exception]) => true,", true)]
    [DataRow("""([u:""], [k:null]) => true,""", true)] // This is wrong, it should have been a string literal
    [DataRow("([t:String] a, [t:Exception] b) => 1,", true)]
    [DataRow("([u:System].[t:String] a, [u:System].[t:Exception] b) => 1,", true)]
    [DataRow("([t:HashSet]<[t:Int32]> a, null) => 1,", true)]
    [DataRow("([t:List]<[t:List]<[t:Int32]>> a, null) => 1,", true)]
    [DataRow("([t:Test].[t:Inner], null) => 1,", true)]
    [DataRow("([t:Inner], null) => 1,", true)]
    [DataRow("([u:first]: [t:Inner], [u:second]: null) => 1,", true)]
    [DataRow("""("", [t:ArgumentException] { HResult: > 2 }) => true,""", false)]
    [DataRow("(([t:Int32], [u:System].[t:String]), second: null) => 1,", false)]
    public void IdentifierToken_Tuples(string switchBranch, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes(
            $$"""
              using System;
              using System.Collections.Generic;
              public class Test
              {
                  public void M(Object o, Exception exception)
                  {
                      var x = (first: o, second: exception) switch
                      {
                          {{switchBranch}}
                          _ => default,
                      };
                  }
                  public class Inner { }
              }
              """/*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("[t:Exception] ex;", false)]
    [DataRow("[u:System].Exception ex;", true)]
    [DataRow("[t:List]<[t:Exception]> ex;", false)]
    [DataRow("List<[u:System].Exception> ex;", true)]
    public void IdentifierToken_LocalDeclaration(string declaration, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes(
            $$"""
              using System;
              using System.Collections.Generic;
              public class Test
              {
                  public void M()
                  {
                      {{declaration}}
                  }
              }
              """ /*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("__refvalue([u:tf], Exception)", false)]
    [DataRow("__refvalue(tf, [t:Exception])", false)]
    [DataRow("__refvalue(tf, [u:System].Exception)", true)]
    [DataRow("__refvalue(tf, System.[t:Exception])", false)]
    public void IdentifierToken_Type_RefValue(string refValueExpression, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes(
            $$"""
              using System;

              public class Test
              {
                  public void M(TypedReference tf)
                  {
                      Exception tfValue = {{refValueExpression}};
                  }
              }
              """ /*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("default([t:Exception])", false)]
    [DataRow("default([u:System].Exception)", true)]
    public void IdentifierToken_Type_DefaultValue(string defaultValueExpression, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes(
            $$"""
              using System;

              public class Test
              {
                  public void M()
                  {
                      var x = {{defaultValueExpression}};
                  }
              }
              """ /*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("sizeof([t:Int32])", false)]
    [DataRow("sizeof([u:System].Int32)", true)]
    public void IdentifierToken_Type_SizeOfValue(string sizeOfExpression, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes(
            $$"""
              using System;

              public class Test
              {
                  public void M()
                  {
                      var x = {{sizeOfExpression}};
                  }
              }
              """ /*, allowSemanticModel */);

#if NET

    [DataTestMethod]
    [DataRow("stackalloc [t:Int32][2]", false)]
    [DataRow("stackalloc [u:System].Int32[2]", true)]
    [DataRow("stackalloc [u:Int32]", true, true)] // compilation error. Type can not be resolved (must be an array type)
    public void IdentifierToken_Type_StackAlloc(string stackAllocExpression, bool allowSemanticModel = true, bool ignoreCompilationErrors = false) =>
        ClassifierTestHarness.AssertTokenTypes(
            $$"""
              using System;

              public class Test
              {
                  public void M()
                  {
                      Span<int> x = {{stackAllocExpression}};
                  }
              }
              """, true, ignoreCompilationErrors);

#endif

    [DataTestMethod]
    [DataRow("""
        from [t:Int32] x in new long[10]
        select x
        """, false)]
    [DataRow("""
        from [u:System].Int32 x in new long[10]
        select x
        """, true)]
    [DataRow("""
        from x in new long[10]
        join [t:Int32] y in new long[0] on x equals y into g
        select g
        """, false)]
    public void IdentifierToken_Type_QueryComprehensions(string query, bool allowSemanticModel = true) =>
    ClassifierTestHarness.AssertTokenTypes(
        $$"""
              using System;
              using System.Linq;

              public class Test
              {
                  public void M()
                  {
                      _ = {{query}};
                  }
              }
              """ /*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("[t:Int32]", false)]
    [DataRow("[u:System].Int32", true)]
    public void IdentifierToken_Type_ForEach(string foreEachExpression, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes(
            $$"""
              using System;

              public class Test
              {
                  public void M()
                  {
                      foreach ({{foreEachExpression}} x in new int[0]) { }
                  }
              }
              """ /*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("[t:Exception]", false)]
    [DataRow("[u:System].Exception", true)]
    public void IdentifierToken_Type_Catch(string catchExpression, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            public class Test
            {
                public void M()
                {
                    try { }
                    catch ({{catchExpression}} ex) { }
                }
            }
            """ /*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("[t:Int32]", false)]
    [DataRow("[u:System].Int32", true)]
    public void IdentifierToken_Type_DelegateDeclaration(string returnType, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            delegate {{returnType}} M();
            """ /*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("[t:Int32]", false)]
    [DataRow("[u:System].Int32", true)]
    public void IdentifierToken_Type_MethodDeclaration(string returnType, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            public class Test
            {
                public {{returnType}} M() => default;
            }
            """ /*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("[t:Int32]", false)]
    [DataRow("[u:System].Int32", true)]
    public void IdentifierToken_Type_OperatorDeclaration(string returnType, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            public class Test
            {
                public static {{returnType}} operator + (Test x) => default; 
            }
            """ /*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("public static explicit operator [t:Int32](Test x)", false)]
    [DataRow("public static explicit operator [u:System].Int32(Test x)", true)]
    [DataRow("public static implicit operator [t:Int32](Test x)", false)]
    [DataRow("public static implicit operator [u:System].Int32(Test x)", true)]
    public void IdentifierToken_Type_ConversionOperatorDeclaration(string conversion, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            public class Test
            {
                {{conversion}} => default; 
            }
            """ /*, allowSemanticModel */);

    /* Add tests with indexers
expr.Length is >= 2
&& expr[new Index(0, fromEnd: false)] is 1
&& expr[new Range(new Index(1, fromEnd: false), new Index(1, fromEnd: true))] is var s
&& expr[new Index(1, fromEnd: true)] is 3
     */
}
