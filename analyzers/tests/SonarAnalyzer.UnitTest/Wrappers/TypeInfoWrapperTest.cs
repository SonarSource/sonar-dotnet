/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StyleCop.Analyzers.Lightup;
using NullabilityInfo = StyleCop.Analyzers.Lightup.NullabilityInfo;
using NullableAnnotation = StyleCop.Analyzers.Lightup.NullableAnnotation;
using NullableFlowState = StyleCop.Analyzers.Lightup.NullableFlowState;

namespace SonarAnalyzer.UnitTest.Wrappers
{
    [TestClass]
    public class TypeInfoWrapperTest
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
            var (tree, semanticModel) = TestHelper.CompileCS(code);
            var identifier = tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>().Last(x => x.NameIs("o")); // o in o.ToString()
            var typeInfo = semanticModel.GetTypeInfo(identifier);
            var convertedNullability = typeInfo.ConvertedNullability();
            convertedNullability.Should().Be(new NullabilityInfo(NullableAnnotation.Annotated, NullableFlowState.MaybeNull))
                .And.BeEquivalentTo(new
                {
                    typeInfo.ConvertedNullability.Annotation,
                    typeInfo.ConvertedNullability.FlowState
                }, options => options.ComparingEnumsByName());

            var nullability = typeInfo.Nullability();
            nullability.Should().Be(new NullabilityInfo(NullableAnnotation.Annotated, NullableFlowState.MaybeNull))
                .And.BeEquivalentTo(new
                {
                    typeInfo.Nullability.Annotation,
                    typeInfo.Nullability.FlowState
                }, options => options.ComparingEnumsByName());
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
            var (tree, semanticModel) = TestHelper.CompileCS(code);
            var identifier = tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>().Last(x => x.NameIs("o")); // o in o.ToString()
            var typeInfo = semanticModel.GetTypeInfo(identifier);
            typeInfo.ConvertedNullability().Should().Be(new NullabilityInfo(NullableAnnotation.None, NullableFlowState.None));
            typeInfo.Nullability().Should().Be(new NullabilityInfo(NullableAnnotation.None, NullableFlowState.None));
        }

        [DataTestMethod]
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
            var (tree, semanticModel) = TestHelper.CompileCS(code);
            var identifier = tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>().Last(x => x.NameIs("o")); // o in o.ToString()
            var typeInfo = semanticModel.GetTypeInfo(identifier);
            var expected =
#if NET
                new NullabilityInfo(NullableAnnotation.NotAnnotated, NullableFlowState.NotNull);
#else
                new NullabilityInfo(NullableAnnotation.Annotated, NullableFlowState.MaybeNull);
#endif
            typeInfo.ConvertedNullability().Should().Be(expected);
            typeInfo.Nullability().Should().Be(expected);
        }
    }
}
