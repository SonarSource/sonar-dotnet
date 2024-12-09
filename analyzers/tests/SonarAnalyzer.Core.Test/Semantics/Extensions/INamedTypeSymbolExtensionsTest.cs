/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Core.Test.Semantics.Extensions;

[TestClass]
public class INamedTypeSymbolExtensionsTest
{
    [TestMethod]
    public void GetAllNamedTypesForNamedType_WhenSymbolIsNull_ReturnsEmpty() =>
        ((INamedTypeSymbol)null).GetAllNamedTypes().Should().BeEmpty();

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
        var symbol = Substitute.For<INamedTypeSymbol>();
        symbol.Kind.Returns(SymbolKind.NamedType);
        symbol.TypeKind.Returns(typeKind);
        symbol.IsRecord.Returns(false);

        symbol.GetClassification().Should().Be(expected);
    }

    [TestMethod]
    public void GetClassification_NamedType_Unknown()
    {
        var symbol = Substitute.For<INamedTypeSymbol>();
        symbol.Kind.Returns(SymbolKind.NamedType);
        symbol.TypeKind.Returns((TypeKind)255);
#if DEBUG
        new Action(() => symbol.GetClassification()).Should().Throw<NotSupportedException>();
#else
        symbol.GetClassification().Should().Be("type");
#endif
    }

    [DataTestMethod]
    [DataRow(TypeKind.Class, "record")]
    [DataRow(TypeKind.Struct, "record struct")]
    public void GetClassification_Record(TypeKind typeKind, string expected)
    {
        var symbol = Substitute.For<INamedTypeSymbol>();
        symbol.Kind.Returns(SymbolKind.NamedType);
        symbol.TypeKind.Returns(typeKind);
        symbol.IsRecord.Returns(true);

        symbol.GetClassification().Should().Be(expected);
    }
}
