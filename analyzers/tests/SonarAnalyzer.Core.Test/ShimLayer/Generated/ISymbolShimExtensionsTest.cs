/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.CSharp.Syntax;
using NullableAnnotation = SonarAnalyzer.ShimLayer.NullableAnnotation;

namespace SonarAnalyzer.Core.Test.ShimLayer.Generated;

[TestClass]
public class ISymbolShimExtensionsTest
{
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void NullableAnnotation_SimpleAnnotations(bool nullable)
    {
        var nullableAnnotation = nullable ? "?" : string.Empty;
        var (tree, model) = TestCompiler.CompileCS($$"""
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

        IFieldSymbolShimExtensions.get_NullableAnnotation(GetSymbol<IFieldSymbol>("ObjectField")).Should().Be(expected);
        IPropertySymbolShimExtensions.get_NullableAnnotation(GetSymbol<IPropertySymbol>("Property")).Should().Be(expected);
        IEventSymbolShimExtensions.get_NullableAnnotation(GetSymbol<IEventSymbol>("MyEvent")).Should().Be(expected);
        IParameterSymbolShimExtensions.get_NullableAnnotation(GetSymbol<IParameterSymbol>("parameter")).Should().Be(expected);
        ILocalSymbolShimExtensions.get_NullableAnnotation(GetSymbol<ILocalSymbol>("local")).Should().Be(expected);
        IArrayTypeSymbolShimExtensions.get_ElementNullableAnnotation(GetSymbol<IFieldSymbol>("ArrayField").Type.Should().BeAssignableTo<IArrayTypeSymbol>().Subject).Should().Be(expected);
        IMethodSymbolShimExtensions.get_ReturnNullableAnnotation(GetSymbol<IMethodSymbol>("Method")).Should().Be(expected);

        T GetSymbol<T>(string name) where T : ISymbol
        {
            var node = tree.GetRoot().DescendantTokens().Single(x => x.ValueText == name).Parent;
            node.Should().NotBeNull();
            return model.GetDeclaredSymbol(node).Should().BeAssignableTo<T>().Which;
        }
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void NullableAnnotation_Receiver(bool nullable)
    {
        var nullableAnnotation = nullable ? "?" : string.Empty;
        var (tree, model) = TestCompiler.CompileCS($$"""
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
        var symbol = model.GetSymbolInfo(invocation).Symbol.Should().BeAssignableTo<IMethodSymbol>().Subject;

        var expected = nullable ? NullableAnnotation.Annotated : NullableAnnotation.NotAnnotated;
        IMethodSymbolShimExtensions.get_ReceiverNullableAnnotation(symbol).Should().Be(expected);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void NullableAnnotation_ReferenceTypeConstrain(bool nullable)
    {
        var nullableAnnotation = nullable ? "?" : string.Empty;
        var (tree, model) = TestCompiler.CompileCS($$"""
            #nullable enable
            using System;
            class C<T> where T: class
            {
                T{{nullableAnnotation}} field;
            }
            """);
        var fieldDeclaration = tree.GetRoot().DescendantNodesAndSelf().OfType<FieldDeclarationSyntax>().First();
        var symbol = model.GetDeclaredSymbol(fieldDeclaration.Declaration.Variables[0]).Should().BeAssignableTo<IFieldSymbol>().Which.Type.Should().BeAssignableTo<ITypeParameterSymbol>().Subject;

        var expected = nullable ? NullableAnnotation.Annotated : NullableAnnotation.NotAnnotated;
        ITypeParameterSymbolShimExtensions.get_NullableAnnotation(symbol).Should().Be(expected);
        ITypeParameterSymbolShimExtensions.get_ReferenceTypeConstraintNullableAnnotation(symbol).Should().Be(NullableAnnotation.NotAnnotated);
    }
}
