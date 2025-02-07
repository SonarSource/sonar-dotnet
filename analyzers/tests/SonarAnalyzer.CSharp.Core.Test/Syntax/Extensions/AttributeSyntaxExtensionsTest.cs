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

using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Extensions;

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
        var attribute = syntaxTree.First<AttributeSyntax>();

        attribute.IsKnownType(KnownType.System_ObsoleteAttribute, compilation.GetSemanticModel(syntaxTree)).Should().Be(isKnownType);
    }

    [TestMethod]
    public void IsKnownType_TypeNotAnAttribute()
    {
        var compilation = CreateCompilation("[System.ObsoleteAttribute] public class X{}");
        var syntaxTree = compilation.SyntaxTrees.First();
        var attribute = syntaxTree.First<AttributeSyntax>();

        attribute.IsKnownType(KnownType.System_String, compilation.GetSemanticModel(syntaxTree)).Should().Be(false);
    }

    [TestMethod]
    public void IsKnownType_EmptyImmutableArray_ReturnsFalse()
    {
        var compilation = CreateCompilation("[System.ObsoleteAttribute] public class X{}");
        var syntaxTree = compilation.SyntaxTrees.First();
        var attribute = syntaxTree.First<AttributeSyntax>();

        attribute.IsKnownType(ImmutableArray<KnownType>.Empty, compilation.GetSemanticModel(syntaxTree)).Should().Be(false);
    }

    private static CSharpCompilation CreateCompilation(string code) =>
        CSharpCompilation.Create("TempAssembly.dll")
                         .AddSyntaxTrees(CSharpSyntaxTree.ParseText(code))
                         .AddReferences(MetadataReferenceFacade.ProjectDefaultReferences)
                         .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
}
