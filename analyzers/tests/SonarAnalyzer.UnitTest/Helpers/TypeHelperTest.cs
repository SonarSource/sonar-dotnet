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

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class TypeHelperTest
    {
        private ClassDeclarationSyntax baseClassDeclaration;
        private ClassDeclarationSyntax derivedClassDeclaration1;
        private ClassDeclarationSyntax derivedClassDeclaration2;
        private SyntaxNode root;
        private SemanticModel semanticModel;

        [TestInitialize]
        public void Compile()
        {
            using (var workspace = new AdhocWorkspace())
            {
                var document = workspace.CurrentSolution.AddProject("foo", "foo.dll", LanguageNames.CSharp)
                    .AddDocument("test", SymbolHelperTest.TestInput);
                var compilation = document.Project.GetCompilationAsync().Result;
                var tree = compilation.SyntaxTrees.First();
                semanticModel = compilation.GetSemanticModel(tree);

                root = tree.GetRoot();
                baseClassDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .First(m => m.Identifier.ValueText == "Base");
                derivedClassDeclaration1 = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .First(m => m.Identifier.ValueText == "Derived1");
                derivedClassDeclaration2 = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .First(m => m.Identifier.ValueText == "Derived2");
            }
        }

        [TestMethod]
        public void Type_DerivesOrImplementsAny()
        {
            var baseType = new KnownType("NS.Base");
            var interfaceType = new KnownType("NS.IInterface");

            var derived1Type = semanticModel.GetDeclaredSymbol(derivedClassDeclaration1) as INamedTypeSymbol;
            var derived2Type = semanticModel.GetDeclaredSymbol(derivedClassDeclaration2) as INamedTypeSymbol;

            derived2Type.DerivesOrImplements(interfaceType).Should().BeTrue();
            derived1Type.DerivesOrImplements(interfaceType).Should().BeFalse();

            var baseTypes = ImmutableArray.Create(new[] { interfaceType, baseType });
            derived1Type.DerivesOrImplementsAny(baseTypes).Should().BeTrue();
        }

        [TestMethod]
        public void Type_Is()
        {
            var baseKnownType = new KnownType("NS.Base");
            var baseKnownTypes = ImmutableArray.Create(new[] { baseKnownType });

            var baseType = semanticModel.GetDeclaredSymbol(baseClassDeclaration) as INamedTypeSymbol;

            baseType.Is(baseKnownType).Should().BeTrue();
            baseType.IsAny(baseKnownTypes).Should().BeTrue();
        }

        [TestMethod]
        public void Type_GetSymbolType_Alias()
        {
            var aliasUsing = root.DescendantNodesAndSelf().OfType<UsingDirectiveSyntax>().FirstOrDefault(x => x.Alias is not null);
            var symbol = semanticModel.GetDeclaredSymbol(aliasUsing);
            var type = symbol.GetSymbolType();
            symbol.ToString().Should().Be("PropertyBag");
            type.ToString().Should().Be("System.Collections.Generic.Dictionary<string, object>");
        }

        [DataTestMethod]
        [DataRow("int")]
        [DataRow("System.Int32")]
        [DataRow("int?")]
        [DataRow("System.Nullable<int>")]
        [DataRow("CustomStruct")]
        [DataRow("CustomRefStruct")]
        public void IsStruct_Simple(string type)
        {
            var fieldSymbol = FirstFieldSymbolFromCode($$"""
                struct CustomStruct { }
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
        public void IsStruct_False_Simple(string type)
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
        [DataRow("Enum")]
        [DataRow("Enum, IComparable")]
        [DataRow("Enum, new()")]
        [DataRow("Enum, IComparable, new()")]
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
            var (tree, model) = TestHelper.CompileCS("""
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

        private static IFieldSymbol FirstFieldSymbolFromCode(string code)
        {
            var (tree, model) = TestHelper.CompileCS(code);
            var field = tree.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>().First();
            var fieldSymbol = (IFieldSymbol)model.GetDeclaredSymbol(field.Declaration.Variables[0]);
            return fieldSymbol;
        }
    }
}
