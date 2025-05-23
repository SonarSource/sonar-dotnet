﻿/*
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
using StyleCop.Analyzers.Lightup;
using NullableAnnotation = StyleCop.Analyzers.Lightup.NullableAnnotation;

namespace SonarAnalyzer.Test.Wrappers;

[TestClass]
public class ISymbolNullableExtensionsTest
{
    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void NullableAnnotation_SimpleAnnitations(bool nullable)
    {
        var nullableAnnotation = nullable ? "?" : string.Empty;
        var (tree, model) = TestHelper.CompileCS($$"""
            #nullable enable
            using System;
            class C
            {
                object{{nullableAnnotation}} ObjectField;
                object{{nullableAnnotation}}[] ArrayField;
                object{{nullableAnnotation}} Property { get; set; }
                event EventHandler{{nullableAnnotation}} MyEvent;

                object{{nullableAnnotation}} Method(object{{nullableAnnotation}} parameter)
                {
                    object{{nullableAnnotation}} local;
                    return null;
                }
            }
            """);
        var expected = nullable ? NullableAnnotation.Annotated : NullableAnnotation.NotAnnotated;

        GetSymbol<IFieldSymbol>("ObjectField").NullableAnnotation().Should().Be(expected);
        GetSymbol<IPropertySymbol>("Property").NullableAnnotation().Should().Be(expected);
        GetSymbol<IEventSymbol>("MyEvent").NullableAnnotation().Should().Be(expected);
        GetSymbol<IParameterSymbol>("parameter").NullableAnnotation().Should().Be(expected);
        GetSymbol<ILocalSymbol>("local").NullableAnnotation().Should().Be(expected);
        GetSymbol<IFieldSymbol>("ArrayField").Type.Should().BeAssignableTo<IArrayTypeSymbol>().Which.ElementNullableAnnotation().Should().Be(expected);
        GetSymbol<IMethodSymbol>("Method").ReturnNullableAnnotation().Should().Be(expected);

        T GetSymbol<T>(string name) where T : ISymbol
        {
            var node = tree.GetRoot().DescendantTokens().Single(x => x.ValueText == name).Parent;
            node.Should().NotBeNull();
            return model.GetDeclaredSymbol(node).Should().BeAssignableTo<T>().Which;
        }
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void NullableAnnotation_Receiver(bool nullable)
    {
        var nullableAnnotation = nullable ? "?" : string.Empty;
        var (tree, model) = TestHelper.CompileCS($$"""
        #nullable enable
        using System;
        static class C
        {
            static object ExtensionMethod(this object{{nullableAnnotation}} receiver)
            {
                return receiver.ExtensionMethod();
            }
        }
        """);
        var invocation = tree.GetRoot().DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().First();
        var symbol = model.GetSymbolInfo(invocation).Symbol.Should().BeAssignableTo<IMethodSymbol>().Which;

        var expected = nullable ? NullableAnnotation.Annotated : NullableAnnotation.NotAnnotated;
        symbol.ReceiverNullableAnnotation().Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void NullableAnnotation_ReferenceTypeConstrain(bool nullable)
    {
        var nullableAnnotation = nullable ? "?" : string.Empty;
        var (tree, model) = TestHelper.CompileCS($$"""
        #nullable enable
        using System;
        class C<T> where T: class
        {
            T{{nullableAnnotation}} field;
        }
        """);
        var fieldDeclaration = tree.GetRoot().DescendantNodesAndSelf().OfType<FieldDeclarationSyntax>().First();
        var symbol = model.GetDeclaredSymbol(fieldDeclaration.Declaration.Variables[0]).Should().BeAssignableTo<IFieldSymbol>().Which.Type.Should().BeAssignableTo<ITypeParameterSymbol>().Which;

        var expected = nullable ? NullableAnnotation.Annotated : NullableAnnotation.NotAnnotated;
        symbol.NullableAnnotation().Should().Be(expected);
        symbol.ReferenceTypeConstraintNullableAnnotation().Should().Be(NullableAnnotation.NotAnnotated);
    }
}
