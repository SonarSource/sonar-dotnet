/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.MetadataReferences;

namespace SonarAnalyzer.UnitTest.Extensions
{
    [TestClass]
    public class AttributeSyntaxExtensionsTest
    {
        [TestMethod]
        [DataRow("[System.ObsoleteAttribute] public class X{}", true)]
        [DataRow("using System; [ObsoleteAttribute] public class X{}", true)]
        [DataRow("using System; [Obsolete] public class X{}", true)]
        [DataRow("using System; [Attribute] public class X{}", false)]
        [DataRow("using System; [AttributeUsageAttribute] public class X{}", false)]
        public void IsKnownType_ChecksAttributeType(string code, bool isKnownType)
        {
            var compilation = CreateCompilation(code);
            var syntaxTree = compilation.SyntaxTrees.First();
            var attribute = syntaxTree.GetRoot().DescendantNodes().OfType<AttributeSyntax>().First();

            attribute.IsKnownType(KnownType.System_ObsoleteAttribute, compilation.GetSemanticModel(syntaxTree)).Should().Be(isKnownType);
        }

        [TestMethod]
        public void IsKnownType_TypeNotAnAttribute()
        {
            var compilation = CreateCompilation("[System.ObsoleteAttribute] public class X{}");
            var syntaxTree = compilation.SyntaxTrees.First();
            var attribute = syntaxTree.GetRoot().DescendantNodes().OfType<AttributeSyntax>().First();

            attribute.IsKnownType(KnownType.System_String, compilation.GetSemanticModel(syntaxTree)).Should().Be(false);
        }

        private static CSharpCompilation CreateCompilation(string code) =>
            CSharpCompilation.Create("TempAssembly.dll")
                             .AddSyntaxTrees(CSharpSyntaxTree.ParseText(code))
                             .AddReferences(MetadataReferenceFacade.ProjectDefaultReferences)
                             .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
