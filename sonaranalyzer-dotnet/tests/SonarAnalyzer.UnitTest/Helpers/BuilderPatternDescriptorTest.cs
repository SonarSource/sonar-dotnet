/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class BuilderPatternDescriptorTest
    {
        [TestMethod]
        public void IsMatch()
        {
            var context = CreateContext();
            InvocationCondition trueCondition = _ => true;
            InvocationCondition falseCondition = _ => false;
            BuilderPatternDescriptor<InvocationExpressionSyntax> descriptor;

            descriptor = new BuilderPatternDescriptor<InvocationExpressionSyntax>(true);
            descriptor.IsMatch(context).Should().BeTrue();

            descriptor = new BuilderPatternDescriptor<InvocationExpressionSyntax>(true, trueCondition);
            descriptor.IsMatch(context).Should().BeTrue();

            descriptor = new BuilderPatternDescriptor<InvocationExpressionSyntax>(true, trueCondition, trueCondition);
            descriptor.IsMatch(context).Should().BeTrue();

            descriptor = new BuilderPatternDescriptor<InvocationExpressionSyntax>(true, falseCondition);
            descriptor.IsMatch(context).Should().BeFalse();

            descriptor = new BuilderPatternDescriptor<InvocationExpressionSyntax>(true, trueCondition, falseCondition);
            descriptor.IsMatch(context).Should().BeFalse();
        }

        [TestMethod]
        public void IsValid_Literal()
        {
            var context = CreateContext();

            var trueDescriptor = new BuilderPatternDescriptor<InvocationExpressionSyntax>(true);
            trueDescriptor.IsValid(null).Should().BeTrue();
            trueDescriptor.IsValid(context.Invocation as InvocationExpressionSyntax).Should().BeTrue();

            var falseDescriptor = new BuilderPatternDescriptor<InvocationExpressionSyntax>(false);
            falseDescriptor.IsValid(null).Should().BeFalse();
            falseDescriptor.IsValid(context.Invocation as InvocationExpressionSyntax).Should().BeFalse();
        }

        [TestMethod]
        public void IsValid_Lambda()
        {
            var context = CreateContext();

            var descriptor = new BuilderPatternDescriptor<InvocationExpressionSyntax>(invocation => invocation != null);
            descriptor.IsValid(null).Should().BeFalse();
            descriptor.IsValid(context.Invocation as InvocationExpressionSyntax).Should().BeTrue();
        }

        private static InvocationContext CreateContext()
        {
            const string source = @"class X{void Foo(object x){x.ToString()}};";
            var snippet = new SnippetCompiler(source, true, AnalyzerLanguage.CSharp);
            return new InvocationContext(snippet.SyntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().Single(), "ToString", snippet.SemanticModel);
        }
    }
}
