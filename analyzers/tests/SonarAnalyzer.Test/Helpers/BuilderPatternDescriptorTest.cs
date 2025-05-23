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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using BuilderPatternDescriptor = SonarAnalyzer.Helpers.BuilderPatternDescriptor<Microsoft.CodeAnalysis.CSharp.SyntaxKind, Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>;

namespace SonarAnalyzer.Test.Helpers
{
    [TestClass]
    public class BuilderPatternDescriptorTest
    {
        [TestMethod]
        public void IsMatch()
        {
            var context = CreateContext();
            TrackerBase<SyntaxKind, InvocationContext>.Condition trueCondition = _ => true;
            TrackerBase<SyntaxKind, InvocationContext>.Condition falseCondition = _ => false;
            BuilderPatternDescriptor descriptor;

            descriptor = new BuilderPatternDescriptor(true);
            descriptor.IsMatch(context).Should().BeTrue();

            descriptor = new BuilderPatternDescriptor(true, trueCondition);
            descriptor.IsMatch(context).Should().BeTrue();

            descriptor = new BuilderPatternDescriptor(true, trueCondition, trueCondition);
            descriptor.IsMatch(context).Should().BeTrue();

            descriptor = new BuilderPatternDescriptor(true, falseCondition);
            descriptor.IsMatch(context).Should().BeFalse();

            descriptor = new BuilderPatternDescriptor(true, trueCondition, falseCondition);
            descriptor.IsMatch(context).Should().BeFalse();
        }

        [TestMethod]
        public void IsValid_Literal()
        {
            var context = CreateContext();

            var trueDescriptor = new BuilderPatternDescriptor(true);
            trueDescriptor.IsValid(null).Should().BeTrue();
            trueDescriptor.IsValid(context.Node as InvocationExpressionSyntax).Should().BeTrue();

            var falseDescriptor = new BuilderPatternDescriptor(false);
            falseDescriptor.IsValid(null).Should().BeFalse();
            falseDescriptor.IsValid(context.Node as InvocationExpressionSyntax).Should().BeFalse();
        }

        [TestMethod]
        public void IsValid_Lambda()
        {
            var context = CreateContext();

            var descriptor = new BuilderPatternDescriptor(invocation => invocation != null);
            descriptor.IsValid(null).Should().BeFalse();
            descriptor.IsValid(context.Node as InvocationExpressionSyntax).Should().BeTrue();
        }

        private static InvocationContext CreateContext()
        {
            const string source = @"class X{void Foo(object x){x.ToString()}};";
            var snippet = new SnippetCompiler(source, true, AnalyzerLanguage.CSharp);
            return new InvocationContext(snippet.SyntaxTree.Single<InvocationExpressionSyntax>(), "ToString", snippet.SemanticModel);
        }
    }
}
