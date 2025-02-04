/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.Core.Test.Semantics.Extensions;

[TestClass]
public class ITypeSymbolExtensionsTest
{
    private const string TestInput = """
        namespace NS
        {
            using System;
            using PropertyBag = System.Collections.Generic.Dictionary<string, object>;

            public abstract class Base
            {
                public class Nested
                {
                    public class NestedMore { }
                }
            }
            public class Derived1 : Base { }
            public class Derived2 : Base, IInterface { }
            public interface IInterface { }
        }
        """;

    private ClassDeclarationSyntax baseClassDeclaration;
    private ClassDeclarationSyntax derivedClassDeclaration1;
    private ClassDeclarationSyntax derivedClassDeclaration2;
    private SyntaxNode root;
    private SemanticModel model;

    [TestInitialize]
    public void Compile()
    {
        var snippet = new SnippetCompiler(TestInput);
        root = snippet.SyntaxTree.GetRoot();
        model = snippet.SemanticModel;
        baseClassDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First(x => x.Identifier.ValueText == "Base");
        derivedClassDeclaration1 = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First(x => x.Identifier.ValueText == "Derived1");
        derivedClassDeclaration2 = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First(x => x.Identifier.ValueText == "Derived2");
    }

    [TestMethod]
    public void IsAny_Null() =>
        ((ITypeSymbol)null).IsAny(KnownType.System_Boolean).Should().BeFalse();

    [TestMethod]
    public void ImplementsAny_Null() =>
        ((ITypeSymbol)null).ImplementsAny([KnownType.System_Boolean]).Should().BeFalse();

    [TestMethod]
    public void Type_DerivesOrImplementsAny()
    {
        var baseType = new KnownType("NS.Base");
        var interfaceType = new KnownType("NS.IInterface");

        var derived1Type = model.GetDeclaredSymbol(derivedClassDeclaration1) as INamedTypeSymbol;
        var derived2Type = model.GetDeclaredSymbol(derivedClassDeclaration2) as INamedTypeSymbol;

        derived2Type.DerivesOrImplements(interfaceType).Should().BeTrue();
        derived1Type.DerivesOrImplements(interfaceType).Should().BeFalse();

        var baseTypes = ImmutableArray.Create(interfaceType, baseType);
        derived1Type.DerivesOrImplementsAny(baseTypes).Should().BeTrue();
    }

    [TestMethod]
    public void Type_Is()
    {
        var baseKnownType = new KnownType("NS.Base");
        var baseKnownTypes = ImmutableArray.Create(baseKnownType);

        var baseType = model.GetDeclaredSymbol(baseClassDeclaration) as INamedTypeSymbol;

        baseType.Is(baseKnownType).Should().BeTrue();
        baseType.IsAny(baseKnownTypes).Should().BeTrue();
    }

    [TestMethod]
    public void Type_GetSymbolType_Alias()
    {
        var aliasUsing = root.DescendantNodesAndSelf().OfType<UsingDirectiveSyntax>().FirstOrDefault(x => x.Alias is not null);
        var symbol = model.GetDeclaredSymbol(aliasUsing);
        var type = symbol.GetSymbolType();
        symbol.ToString().Should().Be("PropertyBag");
        type.ToString().Should().Be("System.Collections.Generic.Dictionary<string, object>");
    }

    [DataTestMethod]
    [DataRow("System.Collections.Generic.IEnumerable<T>", "System.Collections.Generic.IEnumerable<T>", true)]
    [DataRow("System.Collections.Generic.IEnumerable<T>", "System.IDisposable", false)]
    [DataRow("System.Collections.Generic.IEnumerable<int>", "System.Collections.Generic.IEnumerable<T>", true)]
    [DataRow("System.Collections.Generic.IEnumerable<int>", "System.Collections.Generic.IEnumerable<string>", false)]
    [DataRow("System.Collections.Generic.IEnumerable<int>", "System.IDisposable", false)]
    [DataRow("System.Collections.Generic.List<T>", "System.Collections.Generic.IEnumerable<T>", true)]
    [DataRow("System.Collections.Generic.List<T>", "System.Collections.Generic.IEnumerable<int>", false)]
    [DataRow("System.Collections.Generic.List<T>", "System.IDisposable", false)]
    [DataRow("System.Collections.Generic.List<int>", "System.Collections.Generic.IEnumerable<T>", true)]
    [DataRow("System.Collections.Generic.List<int>", "System.Collections.Generic.IEnumerable<int>", true)]
    [DataRow("System.Collections.Generic.List<int>", "System.IDisposable", false)]
    public void Type_DerivesOrImplements_Type(string typeSymbolName, string typeName, bool expected)
    {
        var compilation = TestCompiler.CompileCS("""
            using System.Collections.Generic;
            public class IntList : List<int>, IEnumerable<string>
            {
                IEnumerator<string> IEnumerable<string>.GetEnumerator() => null;
            }
            """).Model.Compilation;
        var allTypes = compilation.GlobalNamespace.GetAllNamedTypes().ToList();
        var intList = allTypes.Single(x => x.Name == "IntList");
        allTypes.Add(intList.BaseType);
        allTypes.AddRange(intList.AllInterfaces);
        var typeSymbol = allTypes.Single(x => x.ToString() == typeSymbolName);
        var type = allTypes.Single(x => x.ToString() == typeName);

        typeSymbol.DerivesOrImplements(type).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("Open`3", "Open`3", true)]
    [DataRow("Half2`2", "Open`3", true)]
    [DataRow("Half1`1", "Open`3", true)]
    [DataRow("Closed", "Open`3", true)]
    [DataRow("Half2`2", "Half2`2", true)]
    [DataRow("Half1`1", "Half2`2", true)]
    [DataRow("Closed", "Half2`2", true)]
    [DataRow("Half1`1", "Half1`1", true)]
    [DataRow("Closed", "Half1`1", true)]
    [DataRow("Closed", "Closed", true)]
    [DataRow("Half2`2", "Closed", false)]
    [DataRow("Half1`1", "Closed", false)]
    [DataRow("Half2`2", "Half1`1", false)]
    public void Type_DerivesOrImplements_HalfClosed(string typeName, string derivesFromName, bool expected)
    {
        var compilation = TestCompiler.CompileCS("""
            public class Open<A, B, C> { }
            public class Half2<A, B>: Open<A, B, int> { }
            public class Half1<A>: Half2<A, int> { }
            public class Closed: Half1<int> { }
            """).Model.Compilation;
        var type = compilation.GetTypeByMetadataName(typeName);
        var derivesFrom = compilation.GetTypeByMetadataName(derivesFromName);
        type.DerivesOrImplements(derivesFrom).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("int")]
    [DataRow("System.Int32")]
    [DataRow("int?")]
    [DataRow("System.Nullable<int>")]
    [DataRow("CustomStruct")]
    [DataRow("CustomRefStruct")]
    [DataRow("RecordStruct")]
    public void IsStruct_Simple_True(string type)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            struct CustomStruct { }
            record struct RecordStruct { }
            ref struct CustomRefStruct { }

            ref struct Test
            {
                {{type}} field;
            }
            """);
        fieldSymbol.Type.IsStruct().Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("object")]
    [DataRow("System.IComparable")]
    public void IsStruct_Simple_False(string type)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            class Test
            {
                {{type}} field;
            }
            """);
        fieldSymbol.Type.IsStruct().Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("struct")]
    [DataRow("unmanaged")]
    public void IsStruct_Generic(string typeConstraint)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            using System;
            class Test<T> where T: {{typeConstraint}}
            {
                T field;
            }
            """);
        fieldSymbol.Type.IsStruct().Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("T")]
    [DataRow("T?")]
    [DataRow("Nullable<T>")]
    public void IsStruct_Generic_Nullable(string type)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            using System;
            class Test<T> where T: struct
            {
                {{type}} field;
            }
            """);
        fieldSymbol.Type.IsStruct().Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("where T: new()")]
    [DataRow("where T: class")]
    [DataRow("where T: class, new()")]
    [DataRow("where T: Exception")]
    [DataRow("where T: Enum")]
    [DataRow("where T: Enum, IComparable")]
    [DataRow("where T: Enum, new()")]
    [DataRow("where T: Enum, IComparable, new()")]
    [DataRow("where T: notnull")]
    public void IsStruct_False_Generic(string typeConstraint)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            using System;
            class Test<T> {{typeConstraint}}
            {
                T field;
            }
            """);
        fieldSymbol.Type.IsStruct().Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("")]                      // Unbounded (can be reference type or value type)
    [DataRow("where T: new()")]        // Unbounded
    [DataRow("where T: notnull")]      // Unbounded
    [DataRow("where T: class")]
    [DataRow("where T: class?")]
    [DataRow("where T: class, new()")]
    [DataRow("where T: Exception")]
    [DataRow("where T: Exception?")]
    [DataRow("where T: IComparable")]
    [DataRow("where T: IComparable?")]
    [DataRow("where T: Delegate")]
    [DataRow("where T: Delegate?")]
    public void IsStruct_False_Generic_NullableReferenceType(string typeConstraint)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            #nullable enable
            using System;
            class Test<T> {{typeConstraint}}
            {
                T? field;
            }
            """);
        fieldSymbol.Type.IsStruct().Should().BeFalse();
    }

    [TestMethod]
    public void IsStruct_False_Generic_Derived()
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            using System;
            class Test<T, U> where U: T
            {
                U field;
            }
            """);
        fieldSymbol.Type.IsStruct().Should().BeFalse();
    }

    [TestMethod]
    public void IsStruct_SelfRefrencingStruct()
    {
        var (tree, model) = TestCompiler.CompileCS("""
            interface Interface<T> where T: struct, Interface<T> { }
            struct Impl: Interface<Impl> { } // For demonstration how an implementation can look like

            class Test
            {
                static void Method<T>(Interface<T> parameter) where T: struct, Interface<T>
                {
                }
            }
            """);
        var parameter = tree.GetRoot().DescendantNodes().OfType<ParameterSyntax>().First();
        var parameterSymbol = (IParameterSymbol)model.GetDeclaredSymbol(parameter);
        parameterSymbol.Type.IsStruct().Should().BeFalse(); // parameter must be a struct, but even the compiler doesn't recognizes this
    }

    [DataTestMethod]
    [DataRow("int")]
    [DataRow("System.Int32")]
    [DataRow("CustomStruct")]
    [DataRow("CustomRefStruct")]
    [DataRow("RecordStruct")]
    public void IsNonNullableValueType_Simple_True(string type)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            struct CustomStruct { }
            record struct RecordStruct { }
            ref struct CustomRefStruct { }

            ref struct Test
            {
                {{type}} field;
            }
            """);
        fieldSymbol.Type.IsNonNullableValueType().Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("object")]
    [DataRow("System.IComparable")]
    [DataRow("int?")]
    [DataRow("System.Nullable<int>")]
    public void IsNonNullableValueType_Simple_False(string type)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            class Test
            {
                {{type}} field;
            }
            """);
        fieldSymbol.Type.IsNonNullableValueType().Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("struct")]
    [DataRow("unmanaged")]
    public void IsNonNullableValueType_Generic(string typeConstraint)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            using System;
            class Test<T> where T: {{typeConstraint}}
            {
                T field;
            }
            """);
        fieldSymbol.Type.IsNonNullableValueType().Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("T?")]
    [DataRow("Nullable<T>")]
    public void IsNonNullableValueType_Generic_ConstraintStruct_Nullable(string type)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            using System;
            class Test<T> where T: struct
            {
                {{type}} field;
            }
            """);
        fieldSymbol.Type.IsNonNullableValueType().Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("")]                      // Unbounded (can be reference type or value type)
    [DataRow("where T: new()")]        // Unbounded
    [DataRow("where T: notnull")]      // Unbounded
    [DataRow("where T: struct")]
    [DataRow("where T: unmanaged")]
    [DataRow("where T: Enum")]
    [DataRow("where T: struct, Enum")]
    [DataRow("where T: struct, Enum, IComparable")]
    [DataRow("where T: class")]
    [DataRow("where T: class?")]
    [DataRow("where T: class, new()")]
    [DataRow("where T: Exception")]
    [DataRow("where T: Exception?")]
    [DataRow("where T: IComparable")]
    [DataRow("where T: IComparable?")]
    [DataRow("where T: Delegate")]
    [DataRow("where T: Delegate?")]
    public void IsNonNullableValueType_Generic_Constraint_Nullable(string constraint)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            #nullable enable
            using System;
            class Test<T> {{constraint}}
            {
                T? field;
            }
            """);
        fieldSymbol.Type.IsNonNullableValueType().Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("int?")]
    [DataRow("System.Nullable<int>")]
    public void IsNullableValueType_Simple_True(string type)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            class Test
            {
                {{type}} field;
            }
            """);
        fieldSymbol.Type.IsNullableValueType().Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("int")]
    [DataRow("System.Int32")]
    [DataRow("CustomStruct")]
    [DataRow("CustomRefStruct")]
    [DataRow("RecordStruct")]
    [DataRow("object")]
    [DataRow("System.IComparable")]
    public void IsNullableValueType_Simple_False(string type)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            struct CustomStruct { }
            record struct RecordStruct { }
            ref struct CustomRefStruct { }

            ref struct Test
            {
                {{type}} field;
            }
            """);
        fieldSymbol.Type.IsNullableValueType().Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("T?")]
    [DataRow("Nullable<T>")]
    [DataRow("CustomStruct?")]
    [DataRow("RecordStruct?")]
    public void IsNullableValueType_Generic_ConstraintStruct_Nullable(string type)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            using System;
            struct CustomStruct { }
            record struct RecordStruct { }
            class Test<T> where T: struct
            {
                {{type}} field;
            }
            """);
        fieldSymbol.Type.IsNullableValueType().Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("struct")]
    [DataRow("unmanaged")]
    [DataRow("struct, Enum")]
    [DataRow("struct, Enum, IComparable")]
    public void IsNullableValueType_Generic_Constraint_Nullable(string constraint)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            using System;
            class Test<T> where T: {{constraint}}
            {
                T? field;
            }
            """);
        fieldSymbol.Type.IsNullableValueType().Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("struct")]
    [DataRow("unmanaged")]
    public void IsNullableValueType_Generic_ConstraintStruct_NonNullable(string typeConstraint)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            using System;
            class Test<T> where T: {{typeConstraint}}
            {
                T field;
            }
            """);
        fieldSymbol.Type.IsNullableValueType().Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("")]                      // Unbounded (can be reference type or value type)
    [DataRow("where T: new()")]        // Unbounded
    [DataRow("where T: notnull")]      // Unbounded
    [DataRow("where T: class")]
    [DataRow("where T: class?")]
    [DataRow("where T: class, new()")]
    [DataRow("where T: Enum")]
    [DataRow("where T: Exception")]
    [DataRow("where T: Exception?")]
    [DataRow("where T: IComparable")]
    [DataRow("where T: IComparable?")]
    [DataRow("where T: Delegate")]
    [DataRow("where T: Delegate?")]
    public void IsNullableValueType_False_Generic_NullableReferenceType(string typeConstraint)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            #nullable enable
            using System;
            class Test<T> {{typeConstraint}}
            {
                T? field;
            }
            """);
        fieldSymbol.Type.IsNullableValueType().Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("", "AttributeTargets")]
    [DataRow("where T: Enum", "T")]
    [DataRow("where T: struct, Enum", "T")]
    [DataRow("where T: Enum", "T?")] // With "#nullable enable" "?" means either nullable value type or nullable reference type (unbound generic) and T? is an Enum
    public void IsEnum_True(string typeConstraint, string fieldType)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            #nullable enable
            using System;
            class Test<T> {{typeConstraint}}
            {
                {{fieldType}} field;
            }
            """);
        fieldSymbol.Type.IsEnum().Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("", "int")]
    [DataRow("", "object")]
    [DataRow("", "IComparable")]
    [DataRow("where T: struct, Enum", "T?")] // Here ? means nullable value type because of the additional struct constraint and T? is an Nullable<Enum>
    [DataRow("where T: struct, Enum", "Nullable<T>")]
    public void IsEnum_False(string typeConstraint, string fieldType)
    {
        var fieldSymbol = FirstFieldSymbolFromCode($$"""
            #nullable enable
            using System;
            class Test<T> {{typeConstraint}}
            {
                {{fieldType}} field;
            }
            """);
        fieldSymbol.Type.IsEnum().Should().BeFalse();
    }

    [TestMethod]
    public void GetSelfAndBaseTypes_WhenSymbolIsNull_ReturnsEmpty() =>
        ((ITypeSymbol)null).GetSelfAndBaseTypes().Should().BeEmpty();

    [DataTestMethod]
    [DataRow("BaseClass<int>         ", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute", "NotInheritedAttribute")]
    [DataRow("DerivedOpenGeneric<int>", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute")]
    [DataRow("DerivedClosedGeneric   ", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute")]
    [DataRow("Implement              ")]
    public void GetAttributesWithInherited_TypeSymbol(string className, params string[] expectedAttributes)
    {
        className = className.TrimEnd();
        var code = $$"""
            using System;

            [AttributeUsage(AttributeTargets.All, Inherited = true)]
            public class InheritedAttribute : Attribute { }

            [AttributeUsage(AttributeTargets.All, Inherited = false)]
            public class NotInheritedAttribute : Attribute { }

            public class DerivedInheritedAttribute: InheritedAttribute { }

            public class DerivedNotInheritedAttribute: NotInheritedAttribute { }

            public class UnannotatedAttribute : Attribute { }

            [Inherited]
            [DerivedInherited]
            [NotInherited]
            [DerivedNotInherited]
            [Unannotated]
            public class BaseClass<T1> { }

            [Inherited]
            [DerivedInherited]
            [NotInherited]
            [DerivedNotInherited]
            [Unannotated]
            public interface IInterface { }

            public class DerivedOpenGeneric<T1>: BaseClass<T1> { }

            public class DerivedClosedGeneric: BaseClass<int> { }

            public class Implement: IInterface { }

            public class Program
            {
                public static void Main()
                {
                    new {{className}}();
                }
            }
            """;
        var compiler = new SnippetCompiler(code);
        var objectCreation = compiler.GetNodes<ObjectCreationExpressionSyntax>().Should().ContainSingle().Subject;
        if (compiler.GetSymbol<IMethodSymbol>(objectCreation) is { MethodKind: MethodKind.Constructor, ReceiverType: { } receiver })
        {
            var actual = receiver.GetAttributesWithInherited().Select(x => x.AttributeClass.Name).ToList();
            actual.Should().BeEquivalentTo(expectedAttributes);
        }
        else
        {
            Assert.Fail("Constructor could not be found.");
        }
        // GetAttributesWithInherited should behave like MemberInfo.GetCustomAttributes from runtime reflection:
        var type = compiler.EmitAssembly().GetType(className.Replace("<int>", "`1"), throwOnError: true);
        type.GetCustomAttributes(inherit: true).Select(x => x.GetType().Name).Should().BeEquivalentTo(expectedAttributes);
    }

    private static IFieldSymbol FirstFieldSymbolFromCode(string code)
    {
        var (tree, model) = TestCompiler.CompileCS(code);
        var field = tree.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>().First();
        var fieldSymbol = (IFieldSymbol)model.GetDeclaredSymbol(field.Declaration.Variables[0]);
        return fieldSymbol;
    }
}
