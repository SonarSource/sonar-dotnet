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

using Moq;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.Test.Extensions;

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
        var symbol = new Mock<INamedTypeSymbol>();
        symbol.Setup(x => x.Kind).Returns(SymbolKind.NamedType);
        symbol.Setup(x => x.TypeKind).Returns(typeKind);
        symbol.Setup(x => x.IsRecord).Returns(false);

        symbol.Object.GetClassification().Should().Be(expected);
    }

    [TestMethod]
    public void GetClassification_NamedType_Unknown()
    {
        var symbol = new Mock<INamedTypeSymbol>();
        symbol.Setup(x => x.Kind).Returns(SymbolKind.NamedType);
        symbol.Setup(x => x.TypeKind).Returns((TypeKind)255);
#if DEBUG
        new Action(() => symbol.Object.GetClassification()).Should().Throw<NotSupportedException>();
#else
        symbol.Object.GetClassification().Should().Be("type");
#endif
    }

    [DataTestMethod]
    [DataRow(TypeKind.Class, "record")]
    [DataRow(TypeKind.Struct, "record struct")]
    public void GetClassification_Record(TypeKind typeKind, string expected)
    {
        var symbol = new Mock<INamedTypeSymbol>();
        symbol.Setup(x => x.Kind).Returns(SymbolKind.NamedType);
        symbol.Setup(x => x.TypeKind).Returns(typeKind);
        symbol.Setup(x => x.IsRecord).Returns(true);

        symbol.Object.GetClassification().Should().Be(expected);
    }
}
