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

using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CSharp.Core.Syntax.Extensions;
using StyleCop.Analyzers.Lightup;
using NullableAnnotation = StyleCop.Analyzers.Lightup.NullableAnnotation;

namespace SonarAnalyzer.Test.Wrappers;

[TestClass]
public class INamedTypeSymbolExtensionsTests
{
    [TestMethod]
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
        var (tree, semanticModel) = TestCompiler.CompileCS(code);
        var identifier = tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>().Last(x => x.NameIs("o")); // o in o.ToString()
        var namedType = semanticModel.GetTypeInfo(identifier).Type.Should().BeAssignableTo<INamedTypeSymbol>().Which;
        var typeArgumentNullabilityShim = namedType.TypeArgumentNullableAnnotations();
        var typeArgumentNullability = namedType.TypeArgumentNullableAnnotations;
        typeArgumentNullabilityShim.Should().BeEquivalentTo(expected).And.BeEquivalentTo(typeArgumentNullability.Select(x => (NullableAnnotation)x));
    }
}
