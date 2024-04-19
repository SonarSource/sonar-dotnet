/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using CodeAnalysisAccessibility = Microsoft.CodeAnalysis.Accessibility; // This is needed because there is an Accessibility namespace in the windows forms binaries.
using VB = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.Test.Helpers
{
    [TestClass]
    public class SymbolHelperTest
    {
        internal const string TestInput = @"
namespace NS
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;
  using PropertyBag = System.Collections.Generic.Dictionary<string, object>;

  public class Base
  {
    public class Nested
    {
      public class NestedMore
      {}
    }

    public virtual void Method1() { }
    protected virtual void Method2() { }
    public abstract int Property { get; set; }

    public void Method4(){}
  }
  private class Derived1 : Base
  {
    public override int Property { get; set; }
  }
  public class Derived2 : Base, IInterface
  {
    public override int Property { get; set; }
    public int Property2 { get; set; }
    public void Method3(){}

    public abstract void Method5();
    public void EventHandler(object o, System.EventArgs args){}
  }
  public interface IInterface
  {
    int Property2 { get; set; }
    void Method3();
    void Method4<T, V>(List<T> param1, List<int> param2, List<V> param3, IList<int> param4);
  }
  public class AssemblyLoad
  {
    public AssemblyLoad()
    {
      AppDomain.CurrentDomain.AssemblyResolve += LoadAnyVersion;
    }
    Assembly LoadAnyVersion(object sender, ResolveEventArgs args) => null;
  }
}
";

        private SnippetCompiler testCode;

        [TestInitialize]
        public void Compile()
        {
            testCode = new SnippetCompiler(TestInput, ignoreErrors: true, language: AnalyzerLanguage.CSharp);
        }

        [TestMethod]
        public void Symbol_IsPublicApi()
        {
            ISymbol symbol = testCode.GetMethodSymbol("Base.Method1");
            SymbolHelper.IsPubliclyAccessible(symbol).Should().BeTrue();

            symbol = testCode.GetMethodSymbol("Base.Method2");
            symbol.IsPubliclyAccessible().Should().BeTrue();

            symbol = testCode.GetPropertySymbol("Base.Property");
            symbol.IsPubliclyAccessible().Should().BeTrue();

            symbol = testCode.GetPropertySymbol("IInterface.Property2");
            symbol.IsPubliclyAccessible().Should().BeTrue();

            symbol = testCode.GetPropertySymbol("Derived1.Property");
            symbol.IsPubliclyAccessible().Should().BeFalse();
        }

        [TestMethod]
        public void Symbol_IsInterfaceImplementationOrMemberOverride()
        {
            ISymbol symbol = testCode.GetMethodSymbol("Base.Method1");
            symbol.GetInterfaceMember().Should().BeNull();
            symbol.GetOverriddenMember().Should().BeNull();

            symbol = testCode.GetPropertySymbol("Derived2.Property");
            symbol.GetOverriddenMember().Should().NotBeNull();

            symbol = testCode.GetPropertySymbol("Derived2.Property2");
            symbol.GetInterfaceMember().Should().NotBeNull();

            symbol = testCode.GetMethodSymbol("Derived2.Method3");
            symbol.GetInterfaceMember().Should().NotBeNull();
        }

        [TestMethod]
        public void Symbol_TryGetOverriddenOrInterfaceMember()
        {
            var methodSymbol = testCode.GetMethodSymbol("Base.Method1");
            var actualOverriddenMethod = methodSymbol.GetOverriddenMember();
            actualOverriddenMethod.Should().BeNull();

            var expectedOverriddenProperty = testCode.GetPropertySymbol("Base.Property");
            var propertySymbol = testCode.GetPropertySymbol("Derived2.Property");

            var actualOverriddenProperty = propertySymbol.GetOverriddenMember();
            actualOverriddenProperty.Should().NotBeNull();
            actualOverriddenProperty.Should().Be(expectedOverriddenProperty);

            var expectedOverriddenMethod = testCode.GetMethodSymbol("IInterface.Method3");
            methodSymbol = testCode.GetMethodSymbol("Derived2.Method3");

            actualOverriddenMethod = methodSymbol.GetInterfaceMember();
            actualOverriddenMethod.Should().NotBeNull();
            actualOverriddenMethod.Should().Be(expectedOverriddenMethod);
        }

        [TestMethod]
        public void Symbol_IsChangeable()
        {
            var symbol = testCode.GetMethodSymbol("Base.Method1");
            symbol.IsChangeable().Should().BeFalse();

            symbol = testCode.GetMethodSymbol("Base.Method4");
            symbol.IsChangeable().Should().BeTrue();

            symbol = testCode.GetMethodSymbol("Derived2.Method5");
            symbol.IsChangeable().Should().BeFalse();

            symbol = testCode.GetMethodSymbol("Derived2.Method3");
            symbol.IsChangeable().Should().BeFalse();
        }

        [TestMethod]
        public void Symbol_IsProbablyEventHandler()
        {
            var symbol = testCode.GetMethodSymbol("Derived2.Method3");
            symbol.IsEventHandler().Should().BeFalse();

            symbol = testCode.GetMethodSymbol("Derived2.EventHandler");
            symbol.IsEventHandler().Should().BeTrue();
        }

        [TestMethod]
        public void Symbol_IsProbablyEventHandler_ResolveEventHandler()
        {
            var symbol = testCode.GetMethodSymbol("AssemblyLoad.LoadAnyVersion");
            symbol.IsEventHandler().Should().BeTrue();
        }

        [TestMethod]
        public void Symbol_GetSelfAndBaseTypes()
        {
            var objectType = testCode.GetTypeByMetadataName("System.Object");
            var baseTypes = objectType.GetSelfAndBaseTypes().ToList();
            baseTypes.Should().ContainSingle();
            baseTypes.First().Should().Be(objectType);

            var derived1Type = testCode.GetTypeSymbol("Derived1") as INamedTypeSymbol;
            baseTypes = derived1Type.GetSelfAndBaseTypes().ToList();
            baseTypes.Should().HaveCount(3);
            baseTypes.Should().HaveElementAt(0, derived1Type);
            baseTypes.Should().HaveElementAt(1, testCode.GetTypeSymbol("Base").Should().BeAssignableTo<INamedTypeSymbol>().Subject);
            baseTypes.Should().HaveElementAt(2, objectType);
        }

        [TestMethod]
        public void Symbol_GetAllNamedTypes_Namespace()
        {
            var nsSymbol = testCode.GetNamespaceSymbol("NS");

            var typeSymbols = nsSymbol.GetAllNamedTypes();
            typeSymbols.Should().HaveCount(7);
        }

        [TestMethod]
        public void Symbol_GetAllNamedTypes_Type()
        {
            var typeSymbol = testCode.GetTypeSymbol("Base") as INamedTypeSymbol;
            var typeSymbols = typeSymbol.GetAllNamedTypes();
            typeSymbols.Should().HaveCount(3);
        }

        [TestMethod]
        public void Symbol_IsKnownType()
        {
            var method4 = (MethodDeclarationSyntax)testCode.GetMethodDeclaration("IInterface.Method4");

            method4.ParameterList
                .Parameters[0]
                .Type
                .IsKnownType(KnownType.System_Collections_Generic_List_T, testCode.SemanticModel)
                .Should().BeTrue();

            method4.ParameterList
                .Parameters[1]
                .Type
                .IsKnownType(KnownType.System_Collections_Generic_List_T, testCode.SemanticModel)
                .Should().BeTrue();

            method4.ParameterList
                .Parameters[2]
                .Type
                .IsKnownType(KnownType.System_Collections_Generic_List_T, testCode.SemanticModel)
                .Should().BeTrue();

            method4.ParameterList
                .Parameters[3]
                .Type
                .IsKnownType(KnownType.System_Collections_Generic_List_T, testCode.SemanticModel)
                .Should().BeFalse();
        }

        [TestMethod]
        public void IsAnyAttributeInOverridingChain_WhenMethodSymbolIsNull_ReturnsFalse() =>
            SymbolHelper.IsAnyAttributeInOverridingChain((IMethodSymbol)null).Should().BeFalse();

        [TestMethod]
        public void IsAnyAttributeInOverridingChain_WhenPropertySymbolIsNull_ReturnsFalse() =>
            SymbolHelper.IsAnyAttributeInOverridingChain((IPropertySymbol)null).Should().BeFalse();

        [TestMethod]
        public void AnyAttributeDerivesFrom_WhenSymbolIsNull_ReturnsFalse() =>
            SymbolHelper.AnyAttributeDerivesFrom(null, KnownType.Void).Should().BeFalse();

        [TestMethod]
        public void AnyAttributeDerivesFromAny_WhenSymbolIsNull_ReturnsFalse() =>
            SymbolHelper.AnyAttributeDerivesFromAny(null, ImmutableArray.Create(KnownType.Void)).Should().BeFalse();

        [TestMethod]
        public void GetAttributesForKnownType_WhenSymbolIsNull_ReturnsEmpty() =>
            SymbolHelper.GetAttributes(null, KnownType.Void).Should().BeEmpty();

        [TestMethod]
        public void GetAttributesForKnownTypes_WhenSymbolIsNull_ReturnsEmpty() =>
            SymbolHelper.GetAttributes(null, ImmutableArray.Create(KnownType.Void)).Should().BeEmpty();

        [TestMethod]
        public void GetParameters_WhenSymbolIsNotMethodOrProperty_ReturnsEmpty() =>
            Mock.Of<ISymbol>(x => x.Kind == SymbolKind.Alias).GetParameters().Should().BeEmpty();

        [TestMethod]
        public void GetInterfaceMember_WhenSymbolIsNull_ReturnsEmpty() =>
            ((ISymbol)null).GetInterfaceMember().Should().BeNull();

        [TestMethod]
        public void GetOverriddenMember_WhenSymbolIsNull_ReturnsEmpty() =>
            ((ISymbol)null).GetOverriddenMember().Should().BeNull();

        [TestMethod]
        public void GetAllNamedTypesForNamespace_WhenSymbolIsNull_ReturnsEmpty() =>
            ((INamespaceSymbol)null).GetAllNamedTypes().Should().BeEmpty();

        [TestMethod]
        public void GetAllNamedTypesForNamedType_WhenSymbolIsNull_ReturnsEmpty() =>
            ((INamedTypeSymbol)null).GetAllNamedTypes().Should().BeEmpty();

        [TestMethod]
        public void GetSelfAndBaseTypes_WhenSymbolIsNull_ReturnsEmpty() =>
            ((ITypeSymbol)null).GetSelfAndBaseTypes().Should().BeEmpty();

        [TestMethod]
        public void GetEffectiveAccessibility_WhenSymbolIsNull_ReturnsNotApplicable() =>
            ((ISymbol)null).GetEffectiveAccessibility().Should().Be(CodeAnalysisAccessibility.NotApplicable);

        [DataTestMethod]
        [DataRow(SymbolKind.Alias, "alias")]
        [DataRow(SymbolKind.ArrayType, "array")]
        [DataRow(SymbolKind.Assembly, "assembly")]
        [DataRow(SymbolKind.Discard, "discard")]
        [DataRow(SymbolKind.DynamicType, "dynamic")]
        [DataRow(SymbolKind.ErrorType, "error")]
        [DataRow(SymbolKind.Event, "event")]
        [DataRow(SymbolKind.Field, "field")]
        [DataRow(SymbolKind.FunctionPointerType, "function pointer")]
        [DataRow(SymbolKind.Label, "label")]
        [DataRow(SymbolKind.Local, "local")]
        [DataRow(SymbolKind.Namespace, "namespace")]
        [DataRow(SymbolKind.NetModule, "netmodule")]
        [DataRow(SymbolKind.Parameter, "parameter")]
        [DataRow(SymbolKind.PointerType, "pointer")]
        [DataRow(SymbolKind.Preprocessing, "preprocessing")]
        [DataRow(SymbolKind.Property, "property")]
        [DataRow(SymbolKind.RangeVariable, "range variable")]
        [DataRow(SymbolKind.TypeParameter, "type parameter")]
        public void GetClassification_SimpleKinds(SymbolKind symbolKind, string expected)
        {
            var fakeSymbol = new Mock<ISymbol>();
            fakeSymbol.Setup(x => x.Kind).Returns(symbolKind);
            fakeSymbol.Object.GetClassification().Should().Be(expected);
        }

        [TestMethod]
        public void GetClassification_UnknowKind()
        {
            var fakeSymbol = new Mock<ISymbol>();
            fakeSymbol.Setup(x => x.Kind).Returns((SymbolKind)999);
#if DEBUG
            new Action(() => fakeSymbol.Object.GetClassification()).Should().Throw<NotSupportedException>();
#else
            fakeSymbol.Object.GetClassification().Should().Be("symbol");
#endif
        }

        [DataTestMethod]
        [DataRow(TypeKind.Array, "array")]
        [DataRow(TypeKind.Class, "class")]
        [DataRow(TypeKind.Delegate, "delegate")]
        [DataRow(TypeKind.Dynamic, "dynamic")]
        [DataRow(TypeKind.Enum, "enum")]
        [DataRow(TypeKind.Error, "error")]
        [DataRow(TypeKind.FunctionPointer, "function pointer")]
        [DataRow(TypeKind.Interface, "interface")]
        [DataRow(TypeKind.Module, "module")]
        [DataRow(TypeKind.Pointer, "pointer")]
        [DataRow(TypeKind.Struct, "struct")]
        [DataRow(TypeKind.Structure, "struct")]
        [DataRow(TypeKind.Submission, "submission")]
        [DataRow(TypeKind.TypeParameter, "type parameter")]
        [DataRow(TypeKind.Unknown, "unknown")]
        public void GetClassification_NamedTypes(TypeKind typeKind, string expected)
        {
            var fakeSymbol = new Mock<INamedTypeSymbol>();
            fakeSymbol.Setup(x => x.Kind).Returns(SymbolKind.NamedType);
            fakeSymbol.Setup(x => x.TypeKind).Returns(typeKind);
            fakeSymbol.Setup(x => x.IsRecord).Returns(false);

            fakeSymbol.Object.GetClassification().Should().Be(expected);
        }

        [TestMethod]
        public void GetClassification_NamedType_Unknown()
        {
            var fakeSymbol = new Mock<INamedTypeSymbol>();
            fakeSymbol.Setup(x => x.Kind).Returns(SymbolKind.NamedType);
            fakeSymbol.Setup(x => x.TypeKind).Returns((TypeKind)255);
#if DEBUG
            new Action(() => fakeSymbol.Object.GetClassification()).Should().Throw<NotSupportedException>();
#else
            fakeSymbol.Object.GetClassification().Should().Be("type");
#endif
        }

        [DataTestMethod]
        [DataRow(TypeKind.Class, "record")]
        [DataRow(TypeKind.Struct, "record struct")]
        public void GetClassification_Record(TypeKind typeKind, string expected)
        {
            var fakeSymbol = new Mock<INamedTypeSymbol>();
            fakeSymbol.Setup(x => x.Kind).Returns(SymbolKind.NamedType);
            fakeSymbol.Setup(x => x.TypeKind).Returns(typeKind);
            fakeSymbol.Setup(x => x.IsRecord).Returns(true);

            fakeSymbol.Object.GetClassification().Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(MethodKind.AnonymousFunction, "method")]
        [DataRow(MethodKind.BuiltinOperator, "operator")]
        [DataRow(MethodKind.Constructor, "constructor")]
        [DataRow(MethodKind.Conversion, "operator")]
        [DataRow(MethodKind.DeclareMethod, "method")]
        [DataRow(MethodKind.DelegateInvoke, "method")]
        [DataRow(MethodKind.Destructor, "destructor")]
        [DataRow(MethodKind.EventAdd, "method")]
        [DataRow(MethodKind.EventRaise, "method")]
        [DataRow(MethodKind.EventRemove, "method")]
        [DataRow(MethodKind.ExplicitInterfaceImplementation, "method")]
        [DataRow(MethodKind.FunctionPointerSignature, "method")]
        [DataRow(MethodKind.LambdaMethod, "method")]
        [DataRow(MethodKind.LocalFunction, "local function")]
        [DataRow(MethodKind.Ordinary, "method")]
        [DataRow(MethodKind.PropertyGet, "getter")]
        [DataRow(MethodKind.PropertySet, "setter")]
        [DataRow(MethodKind.ReducedExtension, "method")]
        [DataRow(MethodKind.SharedConstructor, "constructor")]
        [DataRow(MethodKind.StaticConstructor, "constructor")]
        [DataRow(MethodKind.UserDefinedOperator, "operator")]
        public void GetClassification_Method(MethodKind methodKind, string expected)
        {
            var fakeSymbol = new Mock<IMethodSymbol>();
            fakeSymbol.Setup(x => x.Kind).Returns(SymbolKind.Method);
            fakeSymbol.Setup(x => x.MethodKind).Returns(methodKind);

            fakeSymbol.Object.GetClassification().Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow("BaseClass<int>         ", "VirtualMethod               ", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute", "NotInheritedAttribute")]
        [DataRow("DerivedOpenGeneric<int>", "VirtualMethod               ", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute")]
        [DataRow("DerivedClosedGeneric   ", "VirtualMethod               ", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute")]
        [DataRow("DerivedNoOverrides<int>", "VirtualMethod               ", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute", "NotInheritedAttribute")]
        [DataRow("DerivedOpenGeneric<int>", "GenericVirtualMethod<int>   ", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute")]
        [DataRow("DerivedClosedGeneric   ", "GenericVirtualMethod<int>   ", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute")]
        [DataRow("DerivedNoOverrides<int>", "GenericVirtualMethod<int>   ", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute", "NotInheritedAttribute")]
        [DataRow("DerivedOpenGeneric<int>", "NonVirtualMethod            ")]
        [DataRow("DerivedClosedGeneric   ", "NonVirtualMethod            ")]
        [DataRow("DerivedNoOverrides<int>", "NonVirtualMethod            ", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute", "NotInheritedAttribute")]
        [DataRow("DerivedOpenGeneric<int>", "GenericNonVirtualMethod<int>")]
        [DataRow("DerivedClosedGeneric   ", "GenericNonVirtualMethod<int>")]
        [DataRow("DerivedNoOverrides<int>", "GenericNonVirtualMethod<int>", "InheritedAttribute", "DerivedInheritedAttribute", "DerivedNotInheritedAttribute", "UnannotatedAttribute", "NotInheritedAttribute")]
        public void GetAttributesWithInherited_MethodSymbol(string className, string methodName, params string[] expectedAttributes)
        {
            className = className.TrimEnd();
            methodName = methodName.TrimEnd();
            var code = $$"""
                using System;

                [AttributeUsage(AttributeTargets.All, Inherited = true)]
                public class InheritedAttribute : Attribute { }

                [AttributeUsage(AttributeTargets.All, Inherited = false)]
                public class NotInheritedAttribute : Attribute { }

                public class DerivedInheritedAttribute: InheritedAttribute { }

                public class DerivedNotInheritedAttribute: NotInheritedAttribute { }

                public class UnannotatedAttribute : Attribute { }

                public class BaseClass<T1>
                {
                    [Inherited]
                    [DerivedInherited]
                    [NotInherited]
                    [DerivedNotInherited]
                    [Unannotated]
                    public virtual void VirtualMethod() { }

                    [Inherited]
                    [DerivedInherited]
                    [NotInherited]
                    [DerivedNotInherited]
                    [Unannotated]
                    public void NonVirtualMethod() { }

                    [Inherited]
                    [DerivedInherited]
                    [NotInherited]
                    [DerivedNotInherited]
                    [Unannotated]
                    public void GenericNonVirtualMethod<T2>() { }

                    [Inherited]
                    [DerivedInherited]
                    [NotInherited]
                    [DerivedNotInherited]
                    [Unannotated]
                    public virtual void GenericVirtualMethod<T2>() { }
                }

                public class DerivedOpenGeneric<T1>: BaseClass<T1>
                {
                    public override void VirtualMethod() { }
                    public new void NonVirtualMethod() { }
                    public new void GenericNonVirtualMethod<T2>() { }
                    public override void GenericVirtualMethod<T2>() { }
                }

                public class DerivedClosedGeneric: BaseClass<int>
                {
                    public override void VirtualMethod() { }
                    public new void NonVirtualMethod() { }
                    public new void GenericNonVirtualMethod<T2>() { }
                    public override void GenericVirtualMethod<T2>() { }
                }

                public class DerivedNoOverrides<T>: BaseClass<T> { }

                public class Program
                {
                    public static void Main()
                    {
                        new {{className}}().{{methodName}}();
                    }
                }
                """;
            var compiler = new SnippetCompiler(code);
            var invocationExpression = compiler.GetNodes<InvocationExpressionSyntax>().Should().ContainSingle().Subject;
            var method = compiler.GetSymbol<IMethodSymbol>(invocationExpression);
            var actual = method.GetAttributesWithInherited().Select(x => x.AttributeClass.Name).ToList();
            actual.Should().BeEquivalentTo(expectedAttributes);

            // GetAttributesWithInherited should behave like MemberInfo.GetCustomAttributes from runtime reflection:
            var type = compiler.EmitAssembly().GetType(className.Replace("<int>", "`1"), throwOnError: true);
            var methodInfo = type.GetMethod(methodName.Replace("<int>", string.Empty));
            methodInfo.GetCustomAttributes(inherit: true).Select(x => x.GetType().Name).Should().BeEquivalentTo(expectedAttributes);
        }

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

        [TestMethod]
        public void IsStaticConstructor_CS()
        {
            var code = """
                public class Program
                {
                    static Program() { _ = "static"; }
                    public Program() { }
                    ~Program() { }
                    void AMethod() { }
                    int AProperty { get; set; }
                    int this[int i] => 42;

                    enum AnEnum { A, B, C }
                }
                """;
            var compiler = new SnippetCompiler(code);
            var assignment = compiler.GetNodes<AssignmentExpressionSyntax>().Should().ContainSingle().Subject;

            var staticConstructorDeclaration = compiler.GetNodes<ConstructorDeclarationSyntax>().Single(x => x.Contains(assignment));
            compiler.SemanticModel.GetDeclaredSymbol(staticConstructorDeclaration).IsStaticConstructor().Should().BeTrue();
            var instanceConstructorDeclaration = compiler.GetNodes<ConstructorDeclarationSyntax>().Single(x => !x.Contains(assignment));
            compiler.SemanticModel.GetDeclaredSymbol(instanceConstructorDeclaration).IsStaticConstructor().Should().BeFalse();

            AssertNotAStaticConstructor<DestructorDeclarationSyntax>();
            AssertNotAStaticConstructor<MethodDeclarationSyntax>();
            AssertNotAStaticConstructor<PropertyDeclarationSyntax>();
            AssertNotAStaticConstructor<IndexerDeclarationSyntax>();
            AssertNotAStaticConstructor<EnumDeclarationSyntax>();

            void AssertNotAStaticConstructor<TSyntaxNodeType>() where TSyntaxNodeType : SyntaxNode =>
                compiler.SemanticModel.GetDeclaredSymbol(compiler.GetNodes<TSyntaxNodeType>().Single()).IsStaticConstructor().Should().BeFalse();
        }

        [TestMethod]
        public void IsStaticConstructor_VB()
        {
            var code = """
                Public Class Program
                    Shared Sub New()
                        Dim x
                        x = "static"
                    End Sub

                    Public Sub New()
                    End Sub

                    Sub AMethod()
                    End Sub

                    Property AProperty As Integer

                    Enum AnEnum
                        A
                        B
                        C
                    End Enum
                End Class
                """;
            var compiler = new SnippetCompiler(code, false, AnalyzerLanguage.VisualBasic);
            var assignment = compiler.GetNodes<VB.AssignmentStatementSyntax>().Should().ContainSingle().Subject;

            var sharedConstructorBlock = compiler.GetNodes<VB.ConstructorBlockSyntax>().Single(x => x.Contains(assignment));
            compiler.SemanticModel.GetDeclaredSymbol(sharedConstructorBlock).IsStaticConstructor().Should().BeTrue();
            var instanceConstructorBlock = compiler.GetNodes<VB.ConstructorBlockSyntax>().Single(x => !x.Contains(assignment));
            compiler.SemanticModel.GetDeclaredSymbol(instanceConstructorBlock).IsStaticConstructor().Should().BeFalse();

            AssertNotAStaticConstructor<VB.MethodBlockSyntax>();
            AssertNotAStaticConstructor<VB.PropertyStatementSyntax>();
            AssertNotAStaticConstructor<VB.EnumBlockSyntax>();

            void AssertNotAStaticConstructor<TSyntaxNodeType>() where TSyntaxNodeType : SyntaxNode =>
                compiler.SemanticModel.GetDeclaredSymbol(compiler.GetNodes<TSyntaxNodeType>().Single()).IsStaticConstructor().Should().BeFalse();
        }
    }
}
