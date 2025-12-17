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
using NullabilityInfo = StyleCop.Analyzers.Lightup.NullabilityInfo;
using NullableAnnotation = SonarAnalyzer.ShimLayer.NullableAnnotation;
using NullableFlowState = SonarAnalyzer.ShimLayer.NullableFlowState;

namespace SonarAnalyzer.Test.Wrappers;

[TestClass]
public class TypeInfoExtensionsTest
{
    [TestMethod]
    public void NullabilityInfoFromShimEqualsOriginal()
    {
        var code = @"
#nullable enable
public class C
{
    public void M()
    {
        object? o = null;
        o.ToString();
    }
}
";
        var (tree, semanticModel) = TestCompiler.CompileCS(code);
        var identifier = tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>().Last(x => x.NameIs("o")); // o in o.ToString()
        var typeInfo = semanticModel.GetTypeInfo(identifier);

        var nullabilityShim = typeInfo.Nullability();
        nullabilityShim.Should().Be(new NullabilityInfo(NullableAnnotation.Annotated, NullableFlowState.MaybeNull))
            .And.BeEquivalentTo(new
            {
                typeInfo.Nullability.Annotation,
                typeInfo.Nullability.FlowState
            }, options => options.ComparingEnumsByValue());

        var convertedNullability = typeInfo.ConvertedNullability();
        convertedNullability.Should().Be(new NullabilityInfo(NullableAnnotation.Annotated, NullableFlowState.MaybeNull))
            .And.BeEquivalentTo(new
            {
                typeInfo.ConvertedNullability.Annotation,
                typeInfo.ConvertedNullability.FlowState
            }, options => options.ComparingEnumsByValue());
    }

    [TestMethod]
    public void NullabilityInfoFromShimEqualsOriginalConvertedNullabilityDiffersNullability()
    {
        var code = @"
#nullable enable

public class B { }

public class C {
    public void M() {
        C c = new C();
        M(c);
    }

    public void M(B? b) { }

    public static implicit operator B?(C c) => null;
}";
        var (tree, semanticModel) = TestCompiler.CompileCS(code);
        var identifier = tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>().Last(x => x.NameIs("c")); // c in M(c)
        var typeInfo = semanticModel.GetTypeInfo(identifier);

        var nullabilityShim = typeInfo.Nullability();
        nullabilityShim.Should().Be(new NullabilityInfo(NullableAnnotation.NotAnnotated, NullableFlowState.NotNull))
            .And.BeEquivalentTo(new
            {
                typeInfo.Nullability.Annotation,
                typeInfo.Nullability.FlowState
            }, options => options.ComparingEnumsByValue());

        var convertedNullability = typeInfo.ConvertedNullability();
        convertedNullability.Should().Be(new NullabilityInfo(NullableAnnotation.Annotated, NullableFlowState.MaybeNull))
            .And.BeEquivalentTo(new
            {
                typeInfo.ConvertedNullability.Annotation,
                typeInfo.ConvertedNullability.FlowState
            }, options => options.ComparingEnumsByValue());
    }

    [TestMethod]
    public void NullabilityInfoIsMissingIfNullableDisabled()
    {
        var code = @"
#nullable disable
public class C
{
    public void M()
    {
        object? o = null;
        o.ToString();
    }
}
";
        var (tree, semanticModel) = TestCompiler.CompileCS(code);
        var identifier = tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>().Last(x => x.NameIs("o")); // o in o.ToString()
        var typeInfo = semanticModel.GetTypeInfo(identifier);
        typeInfo.Nullability().Should().Be(new NullabilityInfo(NullableAnnotation.None, NullableFlowState.None));
        typeInfo.ConvertedNullability().Should().Be(new NullabilityInfo(NullableAnnotation.None, NullableFlowState.None));
    }

    [TestMethod]
    [DataRow("Debug.Assert(o != null);")]
    [DataRow("Debug.Fail(string.Empty);")]
    public void NullabilityInfoRecognizesDebug(string debug)
    {
        var code = $@"
#nullable enable
using System.Diagnostics;
public class C
{{
    public void M()
    {{
        object? o = null;
        {debug}
        o.ToString();
    }}
}}
";
        var (tree, semanticModel) = TestCompiler.CompileCS(code);
        var identifier = tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>().Last(x => x.NameIs("o")); // o in o.ToString()
        var typeInfo = semanticModel.GetTypeInfo(identifier);
        var expected =
#if NET
            new NullabilityInfo(NullableAnnotation.NotAnnotated, NullableFlowState.NotNull);
#else
            new NullabilityInfo(NullableAnnotation.Annotated, NullableFlowState.MaybeNull);
#endif
        typeInfo.Nullability().Should().Be(expected);
        typeInfo.ConvertedNullability().Should().Be(expected);
    }

    [TestMethod]
    [DataRow(typeof(NullableAnnotation), typeof(Microsoft.CodeAnalysis.NullableAnnotation))]
    [DataRow(typeof(NullableFlowState), typeof(Microsoft.CodeAnalysis.NullableFlowState))]
    public void NullableEnumsAreIdentical(Type fromShim, Type fromRoslyn)
    {
        fromShim.GetEnumNames().Should().Equal(fromRoslyn.GetEnumNames());
        var enumValues = fromShim.GetEnumValues().Cast<byte>();
        enumValues.Should().SatisfyRespectively(enumValues.Select(x => new Action<byte>(y => fromShim.GetEnumName(y).Should().Be(fromRoslyn.GetEnumName(y)))),
            "the member values should be assigned to the same member names");
    }
}
