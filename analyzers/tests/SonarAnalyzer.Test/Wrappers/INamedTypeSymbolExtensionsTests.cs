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

using Microsoft.CodeAnalysis.CSharp.Syntax;
using StyleCop.Analyzers.Lightup;
using NullableAnnotation = StyleCop.Analyzers.Lightup.NullableAnnotation;

namespace SonarAnalyzer.Test.Wrappers;

[TestClass]
public class INamedTypeSymbolExtensionsTests
{
    [DataTestMethod]
    [DataRow("#nullable enable", "?", NullableAnnotation.Annotated)]
    [DataRow("#nullable enable", "", NullableAnnotation.NotAnnotated)]
    [DataRow("", "?", NullableAnnotation.Annotated)]
    [DataRow("", "", NullableAnnotation.None)]
    public void TypeArgumentNullableAnnotationsFromShimEqualsOriginal(string nullable, string questionMark, NullableAnnotation expected)
    {
        var code = $$"""
            {{nullable}}
            using System.Collections.Generic;
            public class C
            {
                public void M()
                {
                    IEnumerable<object{{questionMark}}> o = new object[0];
                    o.ToString();
                }
            }
            """;
        ValidateTypeArgumentNullableAnnotations(code, expected);
    }

    [TestMethod]
    public void TypeArgumentNullableAnnotationsFromShimEqualsOriginal_MultipleTypeArguments()
    {
        var code = """
            #nullable enable
            using System;
            public class C
            {
                public void M()
                {
                    Func<object, object?, object?, object, object?> o = null;
                    o.ToString();
                }
            }
            """;
        ValidateTypeArgumentNullableAnnotations(code,
            NullableAnnotation.NotAnnotated, NullableAnnotation.Annotated, NullableAnnotation.Annotated, NullableAnnotation.NotAnnotated, NullableAnnotation.Annotated);
    }

    [TestMethod]
    public void TypeArgumentNullableAnnotationsFromShimEqualsOriginal_NoTypeArguments()
    {
        var code = """
            #nullable enable
            using System;
            public class C
            {
                public void M()
                {
                    object? o = null;
                    o.ToString();
                }
            }
            """;
        ValidateTypeArgumentNullableAnnotations(code);
    }

    private static void ValidateTypeArgumentNullableAnnotations(string code, params NullableAnnotation[] expected)
    {
        var (tree, semanticModel) = TestHelper.CompileCS(code);
        var identifier = tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>().Last(x => x.NameIs("o")); // o in o.ToString()
        var namedType = semanticModel.GetTypeInfo(identifier).Type.Should().BeAssignableTo<INamedTypeSymbol>().Which;
        var typeArgumentNullabilityShim = namedType.TypeArgumentNullableAnnotations();
        var typeArgumentNullability = namedType.TypeArgumentNullableAnnotations;
        typeArgumentNullabilityShim.Should().BeEquivalentTo(expected).And.BeEquivalentTo(typeArgumentNullability.Select(x => (NullableAnnotation)x));
    }
}
