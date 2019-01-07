/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class TypeHelperTest
    {
        private ClassDeclarationSyntax baseClassDeclaration;
        private ClassDeclarationSyntax derivedClassDeclaration1;
        private ClassDeclarationSyntax derivedClassDeclaration2;
        private SemanticModel semanticModel;

        [TestInitialize]
        public void Compile()
        {
            using (var workspace = new AdhocWorkspace())
            {
                var document = workspace.CurrentSolution.AddProject("foo", "foo.dll", LanguageNames.CSharp)
                    .AddMetadataReferences(FrameworkMetadataReference.Mscorlib)
                    .AddMetadataReferences(FrameworkMetadataReference.System)
                    .AddMetadataReferences(FrameworkMetadataReference.SystemCore)
                    .AddDocument("test", SymbolHelperTest.TestInput);
                var compilation = document.Project.GetCompilationAsync().Result;
                var tree = compilation.SyntaxTrees.First();
                this.semanticModel = compilation.GetSemanticModel(tree);

                this.baseClassDeclaration = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .First(m => m.Identifier.ValueText == "Base");
                this.derivedClassDeclaration1 = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .First(m => m.Identifier.ValueText == "Derived1");
                this.derivedClassDeclaration2 = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .First(m => m.Identifier.ValueText == "Derived2");
            }
        }

        [TestMethod]
        public void Type_DerivesOrImplementsAny()
        {
            var ctor = typeof(KnownType).GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Single(m => m.GetParameters().Length == 1);

            var baseType = (KnownType)ctor.Invoke(new object[] { "NS.Base" });
            var interfaceType = (KnownType)ctor.Invoke(new object[] { "NS.IInterface" });

            var derived1Type = this.semanticModel.GetDeclaredSymbol(this.derivedClassDeclaration1) as INamedTypeSymbol;
            var derived2Type = this.semanticModel.GetDeclaredSymbol(this.derivedClassDeclaration2) as INamedTypeSymbol;

            derived2Type.DerivesOrImplements(interfaceType).Should().BeTrue();
            derived1Type.DerivesOrImplements(interfaceType).Should().BeFalse();

            var baseTypes = ImmutableArray.Create(new[] { interfaceType, baseType });
            derived1Type.DerivesOrImplementsAny(baseTypes).Should().BeTrue();
        }

        [TestMethod]
        public void Type_Is()
        {
            var ctor = typeof(KnownType).GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Single(m => m.GetParameters().Length == 1);

            var baseKnownType = (KnownType)ctor.Invoke(new object[] { "NS.Base" });
            var baseKnownTypes = ImmutableArray.Create(new[] { baseKnownType });

            var baseType = this.semanticModel.GetDeclaredSymbol(this.baseClassDeclaration) as INamedTypeSymbol;

            baseType.Is(baseKnownType).Should().BeTrue();
            baseType.IsAny(baseKnownTypes).Should().BeTrue();
        }
    }
}
