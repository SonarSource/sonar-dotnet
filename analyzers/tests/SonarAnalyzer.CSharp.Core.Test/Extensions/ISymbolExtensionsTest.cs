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

using ISymbolExtensionsCS = SonarAnalyzer.CSharp.Core.Extensions.ISymbolExtensions;

namespace SonarAnalyzer.Test.Extensions;

[TestClass]
public class ISymbolExtensionsTest
{
    [DataTestMethod]
    [DataRow("class SymbolMember();", true)]
    [DataRow("class SymbolMember() { }", true)]
    [DataRow("class SymbolMember(int a) { }", true)]
    [DataRow("class SymbolMember { }", false)]
    [DataRow("class SymbolMember(int a) { @SymbolMember() : this(1) { } };", true)]
    [DataRow("class SymbolMember { SymbolMember() { } };", false)]
    [DataRow("class Base(int i); class SymbolMember() : Base(1);", true)]
    [DataRow("""
        class Base(int i);
        class SymbolMember : Base
        {
            @SymbolMember() : base(1) { }
        }
        """, false)]
    [DataRow("struct SymbolMember();", true)]
    [DataRow("struct SymbolMember() { }", true)]
    [DataRow("struct SymbolMember(int a) { }", true)]
    [DataRow("struct SymbolMember { }", false)]
    [DataRow("struct SymbolMember(int a) { public @SymbolMember() : this(1) { } };", true)]
    [DataRow("struct SymbolMember { public @SymbolMember() { } };", false)]
    [DataRow("record SymbolMember();", true)]
    [DataRow("record SymbolMember() { }", true)]
    [DataRow("record SymbolMember(int a) { }", true)]
    [DataRow("record SymbolMember { }", false)]
    [DataRow("record SymbolMember(int a) { @SymbolMember() : this(1) { } };", true)]
    [DataRow("record SymbolMember { SymbolMember() { } };", false)]
    [DataRow("record struct SymbolMember();", true)]
    [DataRow("record struct SymbolMember() { }", true)]
    [DataRow("record struct SymbolMember(int a) { }", true)]
    [DataRow("record struct SymbolMember { }", false)]
    [DataRow("record struct SymbolMember(int a) { public @SymbolMember() : this(1) { } };", true)]
    [DataRow("record struct SymbolMember { public @SymbolMember() { } };", false)]
    [DataRow("record class SymbolMember();", true)]
    [DataRow("record class SymbolMember() { }", true)]
    [DataRow("record class SymbolMember(int a) { }", true)]
    [DataRow("record class SymbolMember { }", false)]
    [DataRow("record class SymbolMember(int a) { @SymbolMember() : this(1) { } };", true)]
    [DataRow("record class SymbolMember { @SymbolMember() { } };", false)]
    [DataRow("readonly struct SymbolMember();", true)]
    [DataRow("readonly struct SymbolMember() { }", true)]
    [DataRow("readonly struct SymbolMember(int a) { }", true)]
    [DataRow("readonly struct SymbolMember { }", false)]
    [DataRow("readonly struct SymbolMember(int a) { public @SymbolMember() : this(1) { } };", true)]
    [DataRow("readonly struct SymbolMember { public @SymbolMember() { } };", false)]
    public void IsPrimaryConstructor(string code, bool hasPrimaryConstructor)
    {
        var typeSymbol = new SnippetCompiler(code).GetDeclaredSymbol<INamedTypeSymbol>("SymbolMember");
        var methodSymbols = typeSymbol.GetMembers().OfType<IMethodSymbol>();

        methodSymbols.Count(ISymbolExtensionsCS.IsPrimaryConstructor).Should().Be(hasPrimaryConstructor ? 1 : 0);
    }
}
