/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

    [TestMethod]
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
            """, allowSemanticModel: false);

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
            """, allowSemanticModel: false);

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
            """, allowSemanticModel: false);

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
            """, allowSemanticModel: false);

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
            """, allowSemanticModel: false);

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
            """, allowSemanticModel: false);

    [TestMethod]
    public void IdentifierToken_BaseTypeDelegateEnumMember() =>
        ClassifierTestHarness.AssertTokenTypes("""
            public class [t:TestClass] { }
            public struct [t:TestStruct] { }
            public record [t:TestRecord] { }
            public record struct [t:TestRecordStruct] { }
            public delegate void [t:TestDelegate]();
            public enum [t:TestEnum] { [u:EnumMember] }
            """, allowSemanticModel: false);

    [TestMethod]
    public void IdentifierToken_TupleDesignation() =>
        ClassifierTestHarness.AssertTokenTypes("""
            class Test
            {
                void M()
                {
                    [k:var] ([u:i], [u:j]) = (1, 2);
                    (int [u:i], int [u:j]) [u:t];
                }
            }
            """, allowSemanticModel: false);

    [TestMethod]
    public void IdentifierToken_FunctionPointerUnmanagedCallingConvention() =>
        ClassifierTestHarness.AssertTokenTypes("""
            unsafe class Test
            {
                void M(delegate* unmanaged[[u:Cdecl]]<int, int> m) { }
            }
            """, allowSemanticModel: false);

    [TestMethod]
    public void IdentifierToken_ExternAlias() =>
        ClassifierTestHarness.AssertTokenTypes("""
            extern alias [u:ThisIsAnAlias];
            public class Test {
            }
            """, allowSemanticModel: false, ignoreCompilationErrors: true);

    [TestMethod]
    public void IdentifierToken_AccessorDeclaration() =>
        ClassifierTestHarness.AssertTokenTypes("""
            public class Test {
                public string Property { [u:unknown]; }
            }
            """, allowSemanticModel: false, ignoreCompilationErrors: true);

    [TestMethod]
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
            """, allowSemanticModel: false, ignoreCompilationErrors: true);

    [TestMethod]
    public void IdentifierToken_AttributeTargetSpecifier_UnknownSpecifier() =>
        ClassifierTestHarness.AssertTokenTypes("""
            [[k:unknown]:System.Obsolete]
            public class Test {
            }
            """, allowSemanticModel: false, ignoreCompilationErrors: true);

    [TestMethod]
    [DataRow("using [u:System];", false)]
    [DataRow("using [u:System].[u:Collections].[u:Generic];", false)]
    [DataRow("using [k:global]::[u:System].[u:Collections].[u:Generic];", false)]
    [DataRow("using [t:X] = System.Math;", true)]
    [DataRow("using X = [u:System].Math;", true)]
    [DataRow("using X = System.[t:Math];", true)]
    [DataRow("using X = [t:InGlobalNamespace];", true)]
    [DataRow("using X = [k:global]::[t:InGlobalNamespace];", true)]
    [DataRow("using [u:X] = System.Collections;", true)]
    [DataRow("using X = [u:System].Collections;", true)]
    [DataRow("using X = System.[u:Collections];", true)]
    [DataRow("using [k:static] [u:System].[t:Math];", true)]
    [DataRow("using [k:static] [u:System].[u:Collections].[u:Generic].[t:List]<[k:int]>;", true)]
    [DataRow("using [k:static] [u:System].[u:Collections].[u:Generic].[t:List]<[u:System].[u:Collections].[u:Generic].[t:List]<[k:int]>>;", true)]
    [DataRow("using [k:static] [u:System].[u:Collections].[u:Generic].[t:HashSet]<[k:int]>.[t:Enumerator];", true)]
#if NET
    [DataRow("using [u:System].[u:Buffers];", false)]
#endif
    public void IdentifierToken_Usings(string usings, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            {{usings}}

            class InGlobalNamespace { }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("namespace [u:A] { }", false)]
    [DataRow("namespace [u:A].[u:B] { }", false)]
    [DataRow("namespace [u:A];", false)]
    [DataRow("namespace [u:A].[u:B];", false)]
    public void IdentifierToken_Namespaces(string syntax, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes(syntax, allowSemanticModel);

    [TestMethod]
    [DataRow("class TypeParameter: [t:List]<[t:Exception]> { }", false)]
    // We cannot be sure without calling the model but we assume this will rarely be a type
    [DataRow("class TypeParameter: System.Collections.Generic.[t:List]<System.[t:Exception]> { }", false)]
    [DataRow("class TypeParameter: [u:System].[u:Collections].[u:Generic].List<[u:System].Exception> { }", true)]
    [DataRow("class TypeParameter: [t:List]<[t:List]<[t:Exception]>> { }", false)]
    [DataRow("class TypeParameter: [t:Outer].Inner { }", true)]
    [DataRow("class TypeParameter: Outer.[t:Inner] { }", false)]
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
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("class [t:BaseTypeList]: System.[t:Exception] { }", false)]
    [DataRow("class BaseTypeList: [u:System].Exception { }", true)]
    [DataRow("class BaseTypeList: Outer.[t:Inner] { }", false)]
    [DataRow("class BaseTypeList: [t:Outer].Inner { }", true)]
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
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("class", false)]
    [DataRow("struct", false)]
    [DataRow("record", false)]
    [DataRow("record struct", false)]
#if NET
    [DataRow("interface", false)]
#endif
    public void IdentifierToken_BaseTypeList_DifferentTypeKind(string syntax, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            {{syntax}} X : System.[t:IFormattable]
            {
                public string ToString(string? format, System.IFormatProvider? formatProvider) => null;
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("_ = typeof(System.[t:Exception]);", false)]
    [DataRow("_ = typeof([u:System].Exception);", true)]
    [DataRow("_ = typeof([u:System].[u:Collections].[u:Generic].[t:Dictionary]<,>);", true)]
    [DataRow("_ = typeof([t:Inner]);", false)]
    [DataRow("_ = typeof([t:C].[t:Inner]);", true)]
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
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[t:Exception]", false)]
    [DataRow("[u:System].Exception", true)]
    [DataRow("System.[t:Exception]", false)]
    [DataRow("[t:List]<[t:Int32]>", false)]
    [DataRow("[t:List]<[t:Int32]>", false)]
    [DataRow("[t:HashSet]<[t:Int32]>.[t:Enumerator]", false)]
    [DataRow("System.Collections.Generic.[t:HashSet]<[t:Int32]>.[t:Enumerator]", false)]
    [DataRow("[u:System].[u:Collections].[u:Generic].HashSet<Int32>.Enumerator", true)]
    public void IdentifierToken_TypeInDeclaration(string type, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            public class C
            {
                void Parameter({{type}} parameter)
                {
                }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("_ = nameof([t:Exception]);", true)]
    [DataRow("_ = nameof([u:System].[t:Exception]);", true)]
    [DataRow("_ = nameof([t:Dictionary]<[t:Int32], [t:Exception]>);", true)]
    [DataRow("_ = nameof([t:Dictionary]<[t:Int32], [u:System].[t:Exception]>);", true)]
    [DataRow("_ = nameof([u:System].[u:Collections].[u:Generic]);", true)]
    [DataRow("_ = nameof([u:NameOf]);", true)]
    [DataRow("_ = nameof([t:DateTimeKind].[u:Utc]);", true)]
    [DataRow("_ = nameof([t:Inner]);", true)]
    [DataRow("_ = nameof([t:C].[t:Inner]);", true)]
    public void IdentifierToken_NameOf(string syntax, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            public class C
            {
                void NameOf() { {{syntax}} }
                public class Inner { }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("_ = [k:value];", true)]
    [DataRow("_ = this.[u:value];", false)]
    [DataRow("int [u:Value] = 0; _ = [u:Value]++;", false)]
    [DataRow("_ = nameof([k:value]);", true)]
    [DataRow("_ = nameof([k:value].ToString);", true)]
    [DataRow("_ = [k:value].ToString();", true)]
    [DataRow("_ = [k:value].InnerException.InnerException;", true)]
    [DataRow("_ = [k:value]?.InnerException.InnerException;", true)]
    [DataRow("_ = [k:value].InnerException?.InnerException;", true)]
    [DataRow("_ = [k:value]?.InnerException?.InnerException;", true)]
    [DataRow("_ = [k:value] ?? new Exception();", true)]
    public void IdentifierToken_ValueInPropertySetter(string valueAccess, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            public class C
            {
                private int value;

                Exception Property
                {
                    set
                    {
                        {{valueAccess}}
                    }
                }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("_ = [k:value];", true)]
    [DataRow("_ = this.[u:value];", false)]
    [DataRow("int [u:Value] = 0; _ = [u:Value]++;", false)]
    public void IdentifierToken_ValueInIndexerSetter(string valueAccess, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            public class C
            {
                private int value;

                int this[int i]
                {
                    set
                    {
                        {{valueAccess}}
                    }
                }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("_ = [k:value];", true)]
    [DataRow("_ = this.[u:value];", false)]
    public void IdentifierToken_ValueInEventAddRemove(string valueAccess, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            public class C
            {
                private int value;

                event EventHandler SomeEvent
                {
                    add
                    {
                        {{valueAccess}}
                    }
                    remove
                    {
                        {{valueAccess}}
                    }
                }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("_ = [u:value];", true)]
    [DataRow("_ = this.[u:value];", false)]
    [DataRow("ValueMethod([u:value]: 1);", true)] // could be false, but it is an edge case not worth investing.
    [DataRow("ValueMethod(value: [u:value]);", true)]
    public void IdentifierToken_ValueInOtherPlaces(string valueAccess, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            public class C
            {
                private int value;
                public void M()
                {
                    int value = 0;
                    {{valueAccess}}
                }
                public void ValueMethod(int value) { }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[n:42] is [t:Int32].[u:MinValue]", true)]                                          // IsPattern
    [DataRow("ex is [t:ArgumentException]", true)]
    [DataRow("ex is [u:System].[t:ArgumentException]", true)]
    [DataRow("ex is [t:ArgumentException] [u:argEx]", false)]
    [DataRow("ex is ArgumentException { InnerException: [t:InvalidOperationException] }", true)] // ConstantPattern: could also be a constant
    [DataRow("ex is ArgumentException { HResult: [t:Int32].[u:MinValue] }", true)]               // ConstantPattern: could also be a type
    [DataRow("ex is ArgumentException { HResult: [n:2] }", true)]
    [DataRow("ex is ArgumentException { [u:InnerException]: [t:InvalidOperationException] { } }", false)] // RecursivePattern.Type
    [DataRow("ex is ArgumentException { [u:InnerException].[u:InnerException]: [t:InvalidOperationException] { } [u:inner] }", true)] // TODO false, InnerException.InnerException can be classified without semModel
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
              """, allowSemanticModel);

    [TestMethod]
    [DataRow("([k:string], [t:Exception]) => true,", true)]
    [DataRow("""([s:""], [k:null]) => true,""", true)]
    [DataRow("""([s:""], [k:var] b) => true,""", true)]
    [DataRow("([t:String] a, [t:Exception] b) => 1,", true)]
    [DataRow("([u:System].[t:String] a, [u:System].[t:Exception] b) => 1,", true)]
    [DataRow("([t:HashSet]<[t:Int32]> a, null) => 1,", true)]
    [DataRow("([t:List]<[t:List]<[t:Int32]>> a, null) => 1,", true)]
    [DataRow("([t:Test].[t:Inner], null) => 1,", true)]
    [DataRow("([t:Inner], null) => 1,", true)]
    [DataRow("([u:first]: [t:Inner], [u:second]: null) => 1,", true)]
    [DataRow("""("", [t:ArgumentException] { HResult: > 2 }) => true,""", false)]
    [DataRow("(([t:Int32], System.[t:String], Int32.[u:MaxValue]), second: null) => 1,", true)]
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
              """, allowSemanticModel);

    [TestMethod]
    [DataRow("([t:String] s1, [t:String] s2)", false)]
    [DataRow("([u:System].String s1, int i)", true)]
    [DataRow("(([t:String] s, [k:int] i), [t:Boolean] b)", false)]
    public void IdentifierToken_TupleDeclaration(string tupleDeclaration, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            public class Test
            {
                public {{tupleDeclaration}} M() => default;
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[t:Exception] ex;", false)]
    [DataRow("[u:System].Exception ex;", true)]
    [DataRow("[t:List]<[t:Exception]> ex;", false)]
    [DataRow("List<[u:System].Exception> ex;", true)]
    [DataRow("[k:var] i = 1;", false)]
    [DataRow("[k:dynamic] i = 1;", false)]
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
              """, allowSemanticModel);

    [TestMethod]
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
              """, allowSemanticModel);

    [TestMethod]
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
              """, allowSemanticModel);

    [TestMethod]
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
              """, allowSemanticModel);

#if NET

    [TestMethod]
    [DataRow("stackalloc [t:Int32][2]", false)]
    [DataRow("stackalloc [u:System].Int32[2]", true)]
    [DataRow("stackalloc [t:Int32]", false, true)] // compilation error. Type can not be resolved (must be an array type)
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
              """, allowSemanticModel, ignoreCompilationErrors);

    [TestMethod]
    [DataRow("[t:Int32]", false)]
    [DataRow("[t:Int32]?", false)]
    [DataRow("System.[t:Int32]", false)]
    [DataRow("System.Nullable<[t:Int32]>", false)]
    [DataRow("[t:T]", false)]
    public void IdentifierToken_Type_Ref(string refTypeName, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            public class Test<T>
            {
                {{refTypeName}} item;

                public ref {{refTypeName}} M()
                {
                    return ref item;
                }
            }
            """, allowSemanticModel);

#endif

    [TestMethod]
    [DataRow("""
        from [t:Int32] x in c
        select x
        """, false)]
    [DataRow("""
        from [u:System].Int32 x in c
        select x
        """, true)]
    [DataRow("""
        from x in c
        join [t:Int32] y in c on x equals y into g
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
                      var c = new long[0];
                      _ = {{query}};
                  }
              }
              """, allowSemanticModel);

    [TestMethod]
    [DataRow("[t:Int32]", false)]
    [DataRow("[u:System].Int32", true)]
    public void IdentifierToken_Type_ForEach(string forEachVariableType, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes(
            $$"""
              using System;

              public class Test
              {
                  public void M()
                  {
                      foreach ({{forEachVariableType}} x in new int[0]) { }
                  }
              }
              """, allowSemanticModel);

    [TestMethod]
    [DataRow("[t:Exception]", false)]
    [DataRow("[u:System].Exception", true)]
    public void IdentifierToken_Type_Catch(string catchType, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            public class Test
            {
                public void M()
                {
                    try { }
                    catch ({{catchType}} ex) { }
                }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[t:Int32]", false)]
    [DataRow("[u:System].Int32", true)]
    public void IdentifierToken_Type_DelegateDeclaration(string returnType, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            delegate {{returnType}} M();
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[t:Int32]", false)]
    [DataRow("[u:System].Int32", true)]
    public void IdentifierToken_Type_MethodDeclaration(string returnType, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            public class Test
            {
                public {{returnType}} M() => default;
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[t:Int32]", false)]
    [DataRow("[u:System].Int32", true)]
    public void IdentifierToken_Type_OperatorDeclaration(string returnType, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            public class Test
            {
                public static {{returnType}} operator + (Test x) => default;
            }
            """, allowSemanticModel);

    [TestMethod]
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
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("public [t:Int32] Prop { get; set; }", false)]
    [DataRow("public [t:Int32] Prop { get => 1; set { } }", false)]
    [DataRow("public [t:Int32] this[int index] { get => 1; set { } }", false)]
    [DataRow("public event [t:EventHandler] E;", false)]
    [DataRow("public event [t:EventHandler] E { add { }  remove { } }", false)]
    public void IdentifierToken_Type_PropertyDeclaration(string property, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            public class Test
            {
                {{property}}
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("public [k:partial] [t:Int32] Prop { get; set; }", "public [k:partial] Int32 Prop { get => 1; set { } }")]
    [DataRow("public partial [t:Int32] this[int index] { get; set; }", "public partial Int32 this[int index] { get => 1; set { } }")]
    public void IdentifierToken_Type_PartialPropertyDeclaration(string propertyDeclaration, string propertyImplementation) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            public partial class Test
            {
                {{propertyDeclaration}}
            }
            public partial class Test
            {
                {{propertyImplementation}}
            }
            """);

    [TestMethod]
    [DataRow("[t:Int32]", false)]
    [DataRow("[u:System].Int32", true)]
    public void IdentifierToken_Type_LocalFunction(string returnType, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            public class Test
            {
                public void M()
                {
                    {{returnType}} LocalFunction() => default;
                }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[t:Int32]", false)]
    [DataRow("[u:System].Int32", true)]
    public void IdentifierToken_Type_ParenthesizedLambda(string returnType, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            public class Test
            {
                public void M()
                {
                    var _ = {{returnType}}() => default;
                }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[[t:Obsolete]]", false)]
    [DataRow("[[u:System].Obsolete]", true)]
    public void IdentifierToken_Type_Attribute(string attributeDeclaration, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            {{attributeDeclaration}}
            public class Test
            {
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[t:IFormattable]", false)]
    [DataRow("[u:System].IFormattable", true)]
    public void IdentifierToken_Type_ExplicitInterfaceSpecifier(string interfaceName, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            public class Test: IFormattable
            {
                string {{interfaceName}}.ToString(string? format, IFormatProvider? formatProvider) => default;
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[t:Int32]?", false)]
    [DataRow("[u:System].Int32?", true)]
    [DataRow("[k:int]?", false)]
    [DataRow("[t:IDisposable]?", false)]
    [DataRow("[t:IDisposable]?[]", false)]
    [DataRow("[t:IDisposable]?[]?", false)]
    public void IdentifierToken_Type_Nullable(string typeName, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            #nullable enable

            using System;

            public class Test
            {
                {{typeName}} someField;
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[k:global]::[t:Test]", false)]
    [DataRow("[k:global]::[u:System].Int32", true)]
    [DataRow("[k:global]::System.[t:Int32]", false)]
    public void IdentifierToken_Type_Global(string typeName, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            public class Test
            {
                {{typeName}} M() => default;
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[u:aInstance]", false)]                              // Some simple identifier syntax in an ordinary expression context must be boud to a field/property/local or something else that produces a value, but it can not be a type
    [DataRow("[u:ToString]()", false)]                             // Some simple invocation syntax in an ordinary expression. A type can not be invoked.
    [DataRow("aInstance.InstanceProp.[u:InstanceProp]", false)]    // Most right can not be a type in an ordinary expression context
    [DataRow("[u:aInstance].[u:InstanceProp].InstanceProp", true)] // Could be types
    [DataRow("[t:A].StaticProp", true)]                            // Here it starts with a type
    [DataRow("A.[u:StaticProp]", false)]                           // Most right hand side can not be a type
    [DataRow("[t:A].[t:B].[u:StaticProp].InstanceProp", true)]     // Mixture: some nested types and then properties
    [DataRow("A.B.StaticProp.[u:InstanceProp]", false)]            // The most right hand side
    [DataRow("A.B.[u:StaticProp]?.[u:InstanceProp]", false)]       // Can not be a type on the left side of a ?.
    [DataRow("[u:Prop]?.[u:InstanceProp]?.[u:InstanceProp]", false)] // Can not be a type on the left side of a ?. or on the right
    [DataRow("global::[t:A].StaticProp", true)]                    // Can be a namespace or type
    [DataRow("this.[u:Prop]", false)]                              // Right of this: must be a property/field
    [DataRow("int.[u:MaxValue]", false)]                           // Right of pre-defined type: must be a property/field because pre-defined types do not have nested types
    [DataRow("this.[u:Prop].[u:InstanceProp].[u:InstanceProp]", false)] // must be properties or fields
    [DataRow("(true ? Prop : Prop).[u:InstanceProp].[u:InstanceProp]", false)] // Right of some expression: must be properties or fields
    [DataRow("[t:A]<int>.StaticProp", true)] // TODO: false, Generic name. Must be a type because not in an invocation context, like A<int>()
    [DataRow("A<int>.[u:StaticProp]", false)] // Most right hand side
    [DataRow("A<int>.[u:StaticProp].InstanceProp", true)] // Not the right hand side, could be a nested type
    [DataRow("A<int>.[t:B].StaticProp", true)] // Not the right hand side, is a nested type
    [DataRow("[t:A]<int>.[u:StaticProp]?.[u:InstanceProp]", true)] // TODO: false, Can all be infered from the positions
    [DataRow("[t:A]<int>.[t:B]<int>.[u:StaticProp]", true)] // TODO: false, Generic names must be types and StaticProp is most right hand side
    [DataRow("[t:A]<int>.[u:StaticM]<int>().[u:InstanceProp]", true)] // TODO: false, A must be a type StaticM is invoked and InstanceProp is after the invocation
    [DataRow("A<int>.StaticM<int>().[u:InstanceProp].InstanceProp", false)] // Is right from invocation
    public void IdentifierToken_SimpleMemberAccess_InOrdinaryExpression(string memberAccessExpression, bool allowSemanticModel) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            public class A
            {
                public static A StaticProp { get; }
                public A InstanceProp { get; }
                public A this[int i] => new A();

                public class B
                {
                    public static A StaticProp { get; }
                }
            }

            public class A<TA> : A
            {
                public A<TA> InstanceProp { get; }
                public static A<TA> StaticProp { get; }

                public class B<TB> : B
                {
                    public static B<TB> StaticProp { get; }
                    public A<TM> M<TM>() => new A<TM>();
                    public static A<TM> StaticM<TM>() => new A<TM>();
                }

                public A<TM> M<TM>() => new A<TM>();
                public static A<TM> StaticM<TM>() => new A<TM>();
            }

            public class Test
            {
                public A Prop { get; }
                public void M()
                {
                    var aInstance = new A<int>();
                    // Ordinary expression context: no typeof(), no nameof(), no ExpressionColon (in pattern) or the like
                    _ = {{memberAccessExpression}};
                }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("M([u:MethodGroup]<int>);", false)]
    [DataRow("M([u:MethodGroup]<T>);", false)]
    [DataRow("M(C<T>.[u:StaticMethodGroup]<T>);", false)]
    [DataRow("global::[u:System].Diagnostics.Debug.WriteLine(\"Message\");", true)]
    [DataRow("global::System.Diagnostics.[t:Debug].WriteLine(\"Message\");", true)]
    [DataRow("M([t:C]<T>.StaticMethodGroup<T>);", true)] // TODO false, must be a type
    public void IdentifierToken_SimpleMemberAccess_GenericMethodGroup(string invocation, bool allowSemanticModel) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            public class C<T> {
                public void M(Action a)
                {
                    {{invocation}}
                }

                public void MethodGroup<TM>() { }

                public static void StaticMethodGroup<TM>() { }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("_ = [u:i];")]
    [DataRow("[u:i] = [u:i] + [u:i];")]
    [DataRow("_ = [u:i].[u:ToString]().[u:ToString]();")]          // Heuristic: "i" is lower case and identified as "not a type".
    [DataRow("_ = [u:ex].[u:InnerException].[u:InnerException];")] // Heuristic: "ex" is lower case and identified as "not a type".
    [DataRow("_ = [u:ex]?.[u:ToString]();")]
    [DataRow("_ = ([u:ex] ?? new Exception()).[u:ToString]();")]
    [DataRow("[u:ex] ??= new Exception();")]
    [DataRow("_ = String.Format([k:string].[u:Empty], [u:i]);")]
    [DataRow("_ = [u:ex] is ArgumentException { };")]
    [DataRow("_ = [u:i] == [u:i] ? [u:b] : ![u:b];")]
    [DataRow("if ([u:i] == [u:i]) { }")]
    [DataRow("if ([u:b]) { }")]
    [DataRow("switch ([u:i]) { }")]
    [DataRow("_ = [u:i] switch { _ => true };")]
    [DataRow("foreach (var [u:e] in [u:l]) { }")]
    [DataRow("for(int [u:x] = 0; [u:b]; [u:x]++) { }")]
    [DataRow("while([u:b]) { }")]
    [DataRow("do i++; while([u:b]);")]
    [DataRow("_ = l.[u:Where]([u:x] => [u:x] == null);")]
    [DataRow("_ = ([u:i], [u:i]);")]
    [DataRow("_ = int.TryParse(string.Empty, out [u:i]);")]
    [DataRow("_ = new int[[u:i]];")]
    [DataRow("await [u:t];")]
    [DataRow("_ = unchecked([u:i] + [u:i]);")]
    [DataRow("_ = [u:l][[u:i]];")]
    [DataRow("_ = (byte)([u:i]);")]
    [DataRow("_ = new { [u:A] = [u:i] };")]
    [DataRow("""
        _ = from [u:x] in [u:l]
            select x;
        """)]
    [DataRow("""
        _ = from [u:x] in [u:l]
            let [u:y] = [u:x]
            join [u:z] in [u:l] on [u:x] equals [u:z]
            where [u:x] == [u:z]
            orderby [u:x], [u:z]
            select new { [u:x], Y = [u:y] };
        """)]
    [DataRow("_ = ([u:i]);")]
    [DataRow("_ = +[u:i];")]
    [DataRow("_ = ++[u:i];")]
    [DataRow("_ = -[u:i];")]
    [DataRow("_ = --[u:i];")]
    [DataRow("_ = ~[u:i];")]
    [DataRow("_ = ![u:b];")]
    [DataRow("_ = [u:i]++;")]
    [DataRow("_ = [u:i]--;")]
    [DataRow("_ = [u:l]!;")]
    [DataRow("_ = [u:i] + [u:i];")]
    [DataRow("_ = [u:i] - [u:i];")]
    [DataRow("_ = [u:i] / [u:i];")]
    [DataRow("_ = [u:i] * [u:i];")]
    [DataRow("_ = [u:i] % [u:i];")]
    [DataRow("_ = [u:i] >> [u:i];")]
    [DataRow("_ = [u:i] << [u:i];")]
    [DataRow("_ = [u:b] && [u:b];")]
    [DataRow("_ = [u:b] || [u:b];")]
    [DataRow("_ = [u:i] & [u:i];")]
    [DataRow("_ = [u:i] | [u:i];")]
    [DataRow("_ = [u:i] ^ [u:i];")]
    [DataRow("_ = [u:i] == [u:i];")]
    [DataRow("_ = [u:i] != [u:i];")]
    [DataRow("_ = [u:i] < [u:i];")]
    [DataRow("_ = [u:i] <= [u:i];")]
    [DataRow("_ = [u:i] > [u:i];")]
    [DataRow("_ = [u:i] >= [u:i];")]
    [DataRow("_ = [u:i] is iConst;")] // iConst could be a type
    [DataRow("_ = [u:ex] as [t:ArgumentException];")]
    [DataRow("_ = [u:ex] ?? [u:ex];")]
    [DataRow("i += [u:i];")]
    [DataRow("i -= [u:i];")]
    [DataRow("i *= [u:i];")]
    [DataRow("i /= [u:i];")]
    [DataRow("i %= [u:i];")]
    [DataRow("i &= [u:i];")]
    [DataRow("i ^= [u:i];")]
    [DataRow("i |= [u:i];")]
    [DataRow("i >>= [u:i];")]
    [DataRow("i <<= [u:i];")]
    [DataRow("ex ??= [u:ex];")]
    [DataRow("Func<int> _ = delegate() { return [u:i]; };")] // Illustration: There is an AnonymousMethodExpressionSyntax.ExpressionBody property, but it is never set. Only block syntax is allowed.
    [DataRow("Func<int> _ = () => [u:i];")]
    [DataRow("Func<int, int> _ = [u:i] => [u:i];")]
    [DataRow("_ = [u:b] ? 1 : throw [u:ex];")]
    [DataRow("_ = ex switch { _ when [u:b] => true };")]
    [DataRow("_ = i switch { [u:iConst] => true };", true)] // could be a type
    [DataRow("_ = i switch { >[u:iConst] => [u:b] };")]
    [DataRow("""_ = $"{[u:i]}";""")]
    [DataRow("""_ = $"{[u:i]:000}";""")]
    [DataRow("""_ = $"{[u:i],[u:iConst]}";""")]
    [DataRow("""
        switch(i)
        {
            case 42:
                goto case [u:iConst];
            case iConst:
                break;
        }
        """)]
    [DataRow("""
        switch(AttributeTargets.Assembly)
        {
            case AttributeTargets.Class:
                goto case AttributeTargets.[u:Enum];
            case AttributeTargets.Enum:
                break;
        }
        """)]
    [DataRow("""
        goto [u:label];
        label: ;
        """)]
    [DataRow("return [u:i];")]
    [DataRow("throw [u:ex];")]
    [DataRow("IEnumerable<int> YieldReturn() { yield return [u:i]; }")]
    [DataRow("using(var x = [u:d]);")]
    [DataRow("lock([u:d]);")]
    [DataRow("try { } catch when ([u:b]) { }")]
    public void IdentifierToken_SingleExpressionIdentifier(string statement, bool allowSemanticModel = false) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Threading.Tasks;

            public class Test
            {
                public async Task<int> M()
                {
                    const int iConst = 0;
                    var b = true;
                    var i = 0;
                    var ex = new Exception();
                    var l = new List<Exception>();
                    Task t = null;
                    IDisposable d = null;
                    {{statement}}
                    return 0;
                }
            }
            """, allowSemanticModel);

    [TestMethod]
    [WorkItem(8388)] // https://github.com/SonarSource/sonar-dotnet/pull/8388
    [DataRow("""_ = [u:iPhone].Latest;""")]              // [u:iPhone] -> False classification because of heuristic. Should be t:
    [DataRow("""_ = [u:iPhone].[u:Latest];""")]          // [u:iPhone] -> False classification because of heuristic. Should be t:
    [DataRow("""[u:iPhone].[u:iPhone15Pro].[u:M]();""")] // [u:iPhone] -> False classification because of heuristic. Should be t:
    [DataRow("""[u:iPhone15Pro].[u:M]();""")]            // Correct
    public void IdentifierToken_MemberAccess_FalseClassification(string statement) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            public class iPhone
            {
                public static iPhone iPhone15Pro;
                public static iPhone Latest;

                public void M()
                {
                    {{statement}}
                }
            }
            """, allowSemanticModel: false);
#if NET

    [TestMethod]
    [DataRow("_ = [u:r] with { [u:A] = [u:iConst] };", false)]
    [DataRow("_ = [u:r] is ([u:iConst], [t:Int32]);", true)] // semantic model must be called for iConst and Int32
    [DataRow("_ = [u:l][^[u:iConst]];", false)]
    [DataRow("_ = [u:a][[u:iConst]..^([u:iConst]-1)];", false)]
    [DataRow("foreach((var [u:x], int [u:y]) in ts);", false)]
    [DataRow("foreach(var ([u:x], [u:y]) in ts);", false)]
    public void IdentifierToken_SingleExpressionIdentifier_NetCore(string statement, bool allowSemanticModel) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Threading.Tasks;

            public record R(int A, int B);

            public class Test
            {
                public async Task M()
                {
                    const int iConst = 0;
                    var l = new List<Exception>();
                    var ts = new (int, int)[0];
                    var a = new int[0];
                    var r = new R(1, 1);
                    {{statement}}
                }
            }
            """, allowSemanticModel);

#endif

    [TestMethod]
    [DataRow("[Obsolete([u:sConst])]")]
    [DataRow("[AttributeUsage(AttributeTargets.All, AllowMultiple = [u:bConst])]")]
    [DataRow("[AttributeUsage([t:AttributeTargets].All, AllowMultiple = [u:bConst])]", true)]
    public void IdentifierToken_SingleExpressionIdentifier_Attribute(string attribute, bool allowSemanticModel = false) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Threading.Tasks;

            {{attribute}}
            public class TestAttribute: Attribute
            {
                const string sConst ="Test";
                const bool bConst = true;
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[u:System]", true)]
    [DataRow("[t:Exception]", true)]
    [DataRow("[t:List]<[t:Int32]>", true)] // TODO false, generic names are always types in nameof
    [DataRow("[t:HashSet]<[t:Int32]>.Enumerator", true)]
    [DataRow("HashSet<Int32>.[t:Enumerator]", true)]
    [DataRow("[u:System].[u:Linq].[t:Enumerable].[u:Where]", true)]
    public void IdentifierToken_SimpleMemberAccess_NameOf(string memberaccess, bool allowSemanticModel) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            public class C
            {
                public void M()
                {
                    _ = nameof({{memberaccess}});
                }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("{ [u:InnerException].[u:InnerException]: {} }", true)] // TODO false, in SubpatternSyntax.ExpressionColon context. Must be properties
    [DataRow("{ [u:InnerException].[u:InnerException].[u:Data]: {} }", true)] // TODO false, in SubpatternSyntax.ExpressionColon context. Must be properties
    public void IdentifierToken_SimpleMemberAccess_ExpressionColon(string pattern, bool allowSemanticModel) =>
        // found in SubpatternSyntax.ExpressionColon
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            public class C
            {
                public void M(Exception ex)
                {
                    _ = ex is {{pattern}};
                }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[t:Int32]", true)]
    [DataRow("[u:i]", true)]
    [DataRow("[t:Int32].[u:MaxValue]", true)]
    [DataRow("[u:System].[t:Int32]", true)]
    [DataRow("[t:T]", true)]
    [DataRow("[t:HashSet]<int>.[t:Enumerator]", true)]
    [DataRow("not [t:Int32]", true)]
    [DataRow("not [u:i]", true)]
    [DataRow("not [t:Int32].[u:MaxValue]", true)]
    [DataRow("not [u:System].[t:Int32]", true)]
    public void IdentifierToken_SimpleMemberAccess_Is(string pattern, bool allowSemanticModel) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            public class C
            {
                public void M<T>(object o)
                {
                    const int i = 42;
                    _ = o is {{pattern}};
                }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[t:Int32]", true)]
    [DataRow("[u:i]", true)]
    [DataRow("[t:Int32].[u:MaxValue]", true)]
    [DataRow("[u:System].[t:Int32].[u:MaxValue]", true)]
    [DataRow("not [t:Int32]", true)]
    [DataRow("not [u:i]", true)]
    [DataRow("not [t:Int32].[u:MaxValue]", true)]
    [DataRow("not [u:System].[t:Int32]", true)]
    public void IdentifierToken_SimpleMemberAccess_SwitchArm(string pattern, bool allowSemanticModel) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            public class C
            {
                public void M(object o)
                {
                    const int i = 42;
                    switch(o)
                    {
                        case {{pattern}}: break;
                    }
                }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[t:Int32]", true)]
    [DataRow("[u:i]", true)]
    [DataRow("[t:Int32].[u:MaxValue]", true)]
    [DataRow("[u:System].[t:Int32].[u:MaxValue]", true)]
    [DataRow("not [t:Int32]", true)]
    [DataRow("not [u:i]", true)]
    [DataRow("not [t:Int32].[u:MaxValue]", true)]
    [DataRow("not [u:System].[t:Int32]", true)]
    public void IdentifierToken_SimpleMemberAccess_SwitchExpression(string pattern, bool allowSemanticModel) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            public class C
            {
                public void M(object o)
                {
                    const int i = 42;
                    _ = o switch
                    {
                        {{pattern}} => true,
                    };
                }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[k:scoped] ref S s2 = ref s1;", false)]
    [DataRow("scoped ref [t:S] s2 = ref s1;", false)]
    [DataRow("ref [t:S] s2 = ref s1;", false)]
    [DataRow("scoped [t:S] s2 = s1;", false)]
    [DataRow("[k:scoped] [k:ref] [k:readonly] [t:S] [u:s2] = [k:ref] [u:s1];", false)]
    [DataRow("[k:int] [u:scoped] = 1;", false)]
    public void IdentifierToken_Scoped_Local(string localDeclaration, bool allowSemanticModel) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            public ref struct S { }

            public class C
            {
                public void M(ref S s1)
                {
                    {{localDeclaration}}
                }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[k:scoped] ref S s", false)]
    [DataRow("scoped ref [t:S] s", false)]
    [DataRow("ref [t:S] s", false)]
    [DataRow("scoped [t:S] s", false)]
    [DataRow("[k:scoped] [k:ref] [t:S] [u:s]", false)]
    [DataRow("[k:int] [u:scoped]", false)]
    public void IdentifierToken_Scoped_Parameter(string parameterDeclaration, bool allowSemanticModel) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            public ref struct S { }

            public class C
            {
                public void M({{parameterDeclaration}})
                {
                }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("var d = [u:dateTimePointer]->Date;", false)]
    [DataRow("var d = dateTimePointer->[u:Date];", false)]
    [DataRow("var d = (*[u:dateTimePointer]).Date;", false)]
    [DataRow("var d = (*dateTimePointer).[u:Date];", false)]
    [DataRow("var d = [u:dateTimePointer][0];", false)]
    [DataRow("[u:dateTimePointer][0] = *(&[u:dateTimePointer][0]);", false)]
    [DataRow("[t:Int32]* iPointer;", false)]
    [DataRow("[t:Int32]?* iPointer;", false)]
    [DataRow("[t:Int32]?** iPointerPointer;", false)]
    [DataRow("[t:Nullable]<[t:Int32]>** iPointerPointer;", false)]
    [DataRow("[u:System].Int32* iPointer;", true)]
    [DataRow("System.[t:Int32]* iPointer;", false)]
    [DataRow("[k:void]* voidPointer;", false)]
    [DataRow("DateTime d = default; M(&[u:d]);", false)]
    [DataRow("[t:DateTime]** dt = &[u:dateTimePointer];", false)]
    [DataRow("_ = (*(&[u:dateTimePointer]))->Date;", false)]
    [DataRow("_ = (**(&[u:dateTimePointer])).Date;", false)]
    public void IdentifierToken_Unsafe_Pointers(string statement, bool allowSemanticModel) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            public class C
            {
                public unsafe void M(DateTime* dateTimePointer)
                {
                    {{statement}}
                }
            }
            """, allowSemanticModel);

    [TestMethod]
    [DataRow("[k:int] @int;", false)]
    [DataRow("[k:volatile] [t:@volatile] [u:@volatile];", false)]
    [DataRow("[t:Int32] [u:@someName];", false)]
    public void IdentifierToken_KeywordEscaping(string fieldDeclaration, bool allowSemanticModel) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;

            public class @volatile { }
            
            public class C
            {
                {{fieldDeclaration}}
            }
            """, allowSemanticModel);

    [TestMethod]
    public void CSharp12Syntax_Classification() =>
        ClassifierTestHarness.AssertTokenTypes("""
            using System;

            class PrimaryConstructor([t:Int32] [u:i] = [n:1])
            {
                public PrimaryConstructor(int a1, int a2) : [k:this]([u:a1])
                {
                    var f = ([t:Int32] [u:i] = [n:1]) => i;
                }
            }
            """);

#if NET
    [TestMethod]
    public void KeywordToken_AllowsAntiConstraintAndParameterModifiers() =>
        ClassifierTestHarness.AssertTokenTypes("""
            class Allows<T> where T: [k:allows] [k:ref] [k:struct]
            {
                public void M1([k:scoped] [t:T] [u:t])
                { }
                public void M1([k:ref] [k:readonly] T t)
                { }
                public void M2([k:in] T t)
                { }
            }
            """);
#endif
}
