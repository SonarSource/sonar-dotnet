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

namespace SonarAnalyzer.Test.Wrappers
{
    [TestClass]
    public class IOperationWrapperSonarTest
    {
        [TestMethod]
        public void Null_Throws()
        {
            Action a = () => new IOperationWrapperSonar(null).ToString();
            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void ValidateReflection()
        {
            var sut = CreateWrapper(out var semanticModel);
            sut.Parent.Should().NotBeNull();
            sut.Parent.Kind.Should().Be(OperationKind.VariableDeclarator);
            sut.Instance.Should().NotBeNull();
            sut.Instance.Kind.Should().Be(OperationKind.VariableInitializer);
            sut.Children.Should().HaveCount(1);
            sut.Children.Single().Kind.Should().Be(OperationKind.Literal);
            sut.Language.Should().Be("C#");
            sut.IsImplicit.Should().Be(false);
            sut.SemanticModel.Should().Be(semanticModel);
        }

        [TestMethod]
        public void GetHashCode_ReturnsOperationHash()
        {
            var sut = CreateWrapper(out _);
            sut.GetHashCode().Should().Be(sut.Instance.GetHashCode());
        }

        [TestMethod]
        public void Equals_ComparesUnderlyingInstance()
        {
            var sut = CreateWrapper(out _);
            var other = new IOperationWrapperSonar(sut.Instance);
            sut.Equals(other).Should().BeTrue();
        }

        [TestMethod]
        public void Equals_NotEqual()
        {
            var sut = CreateWrapper(out _);
            sut.Parent.Should().NotBeNull();

            sut.Equals(null).Should().BeFalse();
            sut.Equals("Other type").Should().BeFalse();
            sut.Equals(sut.Parent).Should().BeFalse();
        }

        [TestMethod]
        public void ToString_ReturnsInstanceToString()
        {
            var sut = CreateWrapper(out _);
            sut.ToString().Should().Be(sut.Instance.ToString());
        }

        private static IOperationWrapperSonar CreateWrapper(out SemanticModel semanticModel)
        {
            const string code = @"
public class Sample
{
    public void Method()
    {
        var value = 42;
    }
}";
            var (tree, model) = TestHelper.CompileCS(code);
            var declaration = tree.Single<EqualsValueClauseSyntax>();
            semanticModel = model;
            return new IOperationWrapperSonar(model.GetOperation(declaration));
        }
    }
}
