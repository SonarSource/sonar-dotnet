/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.ShimLayer.Generator.Model.Test;

[TestClass]
public class TypeLoaderTest
{
    private const string NewTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.FileScopedNamespaceDeclarationSyntax"; // This type is not part of Roslyn 1.3.2

    [TestMethod]
    public void LoadBaseline_AllAssemblies()
    {
        using var typeLoader = new TypeLoader();
        typeLoader.LoadBaseline().Select(x => x.Type).Should()
            .ContainSingle(x => x.FullName == "Microsoft.CodeAnalysis.CSharp.CSharpCompilation")
            .And.ContainSingle(x => x.FullName == "Microsoft.CodeAnalysis.Compilation")
            .And.NotContain(x => x.FullName == NewTypeName)
            .And.HaveCount(555);
    }

    [TestMethod]
    public void LoadLatest_AllAssemblies() =>
        TypeLoader.LoadLatest().Select(x => x.Type).Should()
            .ContainSingle(x => x.FullName == "Microsoft.CodeAnalysis.CSharp.CSharpCompilation")
            .And.ContainSingle(x => x.FullName == "Microsoft.CodeAnalysis.Compilation")
            .And.ContainSingle(x => x.FullName == NewTypeName)
            .And.HaveCountGreaterThan(750);

    [TestMethod]
    public void Load_AllTypes() =>
        TypeLoader.LoadLatest().Select(x => x.Type).Should()
            .ContainSingle(x => x.FullName == "Microsoft.CodeAnalysis.Compilation", "it should return classes")
            .And.ContainSingle(x => x.FullName == "Microsoft.CodeAnalysis.SymbolInfo", "it should return structs")
            .And.ContainSingle(x => x.FullName == "Microsoft.CodeAnalysis.SyntaxList`1", "it should return generic types")
            .And.ContainSingle(x => x.FullName == "Microsoft.CodeAnalysis.IOperation+OperationList", "it should return nested types")
            .And.ContainSingle(x => x.FullName == "Microsoft.CodeAnalysis.OutputKind", "it should return enums")
            .And.ContainSingle(x => x.FullName == "Microsoft.CodeAnalysis.IMethodSymbol", "it should return interfaces")
            .And.NotContain(x => !x.IsEnum && !x.IsGenericType && !x.IsInterface && !x.IsNested && !x.IsClass && !x.IsValueType, "there should be no unexpected types");

    [TestMethod]
    public void Load_InheritedMembers() =>
        TypeLoader.LoadLatest().Single(x => x.Type.FullName == "Microsoft.CodeAnalysis.CSharp.CSharpCompilation").Members.Should()
            .ContainSingle(x => x.DeclaringType.Name == "Compilation" && x.ToString() == "Microsoft.CodeAnalysis.Compilation RemoveAllSyntaxTrees()", "it's in the base type")
            .And.ContainSingle(x => x.DeclaringType.Name == "CSharpCompilation" && x.ToString() == "Microsoft.CodeAnalysis.CSharp.CSharpCompilation RemoveAllSyntaxTrees()", "it shadows")
            .And.ContainSingle(x => x.DeclaringType.Name == "Compilation" && x.ToString() == "Boolean ContainsSyntaxTree(Microsoft.CodeAnalysis.SyntaxTree)", "it's in the base type")
            .And.ContainSingle(x => x.DeclaringType.Name == "CSharpCompilation" && x.ToString() == "Microsoft.CodeAnalysis.CSharp.CSharpCompilation RemoveAllSyntaxTrees()", "it overrides")
            .And.ContainSingle(x => x.DeclaringType.Name == "Object" && x.ToString() == "Int32 GetHashCode()", "it should contain all members down to System.Object");

    [TestMethod]
    public void Load_ContainsInheritedInterfaces() =>
        TypeLoader.LoadLatest().Single(x => x.Type.FullName == "Microsoft.CodeAnalysis.IMethodSymbol").Members.Should()
            .ContainSingle(x => x.DeclaringType.Name == "IMethodSymbol" && x.ToString() == "Boolean IsGenericMethod")
            .And.ContainSingle(x => x.DeclaringType.Name == "ISymbol" && x.ToString() == "System.String Name", "it's from the base interface");

    [TestMethod]
    public void Load_ExplicitInterfaces_Ignored()
    {
        const string ImplicitInterface = "IFormattable";
        const string ImplicitInterfaceMember = "System.String ToString(System.String, System.IFormatProvider)";
        var node = TypeLoader.LoadLatest().Single(x => x.Type.FullName == "Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode");
        // Make sure that the member exists on the type (it's explicit interface implementation)
        node.Type.GetInterface(ImplicitInterface).Should().NotBeNull("CSharpSyntaxNode implements IFormattable")
            .And.Subject.GetMembers().Should().ContainSingle(x => x.ToString() == ImplicitInterfaceMember);
        // Then assert with that member
        node.Members.Should().NotContain(x => x.ToString() == ImplicitInterfaceMember);
    }
}
