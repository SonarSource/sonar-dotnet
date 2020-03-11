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
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;
using SonarAnalyzer.UnitTest.TestFramework;
using CSharpSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using VBSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class BuilderPatternConditionTest
    {

        private InvocationContext context_CS;
        private InvocationContext context_VB;

        [TestInitialize]
        public void Initialize()
        {
            context_CS = CreateContext_CS();
            context_VB = CreateContext_VB();
        }

        [TestMethod]
        public void ConstructorIsSafe_CS()
        {
            var safeConstructor = new CSharpBuilderPatternCondition(true, new BuilderPatternDescriptor<CSharpSyntax.InvocationExpressionSyntax>(false, (context) => false));
            safeConstructor.InvalidBuilderInitialization(context_CS).Should().BeFalse();

            var unsafeConstructor = new CSharpBuilderPatternCondition(false, new BuilderPatternDescriptor<CSharpSyntax.InvocationExpressionSyntax>(false, (context) => false));
            unsafeConstructor.InvalidBuilderInitialization(context_CS).Should().BeTrue();
        }

        [TestMethod]
        public void ConstructorIsSafe_VB()
        {
            var safeConstructor = new VisualBasicBuilderPatternCondition(true, new BuilderPatternDescriptor<VBSyntax.InvocationExpressionSyntax>(false, (context) => false));
            safeConstructor.InvalidBuilderInitialization(context_VB).Should().BeFalse();

            var unsafeConstructor = new VisualBasicBuilderPatternCondition(false, new BuilderPatternDescriptor<VBSyntax.InvocationExpressionSyntax>(false, (context) => false));
            unsafeConstructor.InvalidBuilderInitialization(context_VB).Should().BeTrue();
        }

        [TestMethod]
        public void InvalidBuilderInitialization_CS()
        {
            var aaaInvalidator = new BuilderPatternDescriptor<CSharpSyntax.InvocationExpressionSyntax>(false, (context) => context.MethodName == "Aaa");
            var bbbValidator = new BuilderPatternDescriptor<CSharpSyntax.InvocationExpressionSyntax>(true, (context) => context.MethodName == "Bbb");
            var cccInvalidator = new BuilderPatternDescriptor<CSharpSyntax.InvocationExpressionSyntax>(false, (context) => context.MethodName == "Ccc");

            var condition1 = new CSharpBuilderPatternCondition(false, bbbValidator);
            condition1.InvalidBuilderInitialization(context_CS).Should().BeFalse(); // Invalid constructor validated by method

            var condition2 = new CSharpBuilderPatternCondition(false, cccInvalidator);
            condition2.InvalidBuilderInitialization(context_CS).Should().BeTrue(); // Invalid constructor invalidated by method

            var condition3 = new CSharpBuilderPatternCondition(true, cccInvalidator);
            condition3.InvalidBuilderInitialization(context_CS).Should().BeTrue(); // Valid constructor invalidated by method

            var condition4 = new CSharpBuilderPatternCondition(true, bbbValidator);
            condition4.InvalidBuilderInitialization(context_CS).Should().BeFalse(); // Valid constructor validated by method

            var condition5 = new CSharpBuilderPatternCondition(false, aaaInvalidator, bbbValidator);
            condition5.InvalidBuilderInitialization(context_CS).Should().BeFalse(); // Invalid constructor, invalidated by Aaa, validated by Bbb

            var condition6 = new CSharpBuilderPatternCondition(false, bbbValidator, aaaInvalidator); // Configuration order has no effect
            condition6.InvalidBuilderInitialization(context_CS).Should().BeFalse(); // Invalid constructor, invalidated by Aaa, validated by Bbb

            var condition7 = new CSharpBuilderPatternCondition(true, bbbValidator, cccInvalidator);
            condition7.InvalidBuilderInitialization(context_CS).Should().BeTrue(); // Valid constructor, validated by Bbb, invalidated by Ccc

            var condition8 = new CSharpBuilderPatternCondition(true, cccInvalidator, bbbValidator); // Configuration order has no effect
            condition8.InvalidBuilderInitialization(context_CS).Should().BeTrue(); // Valid constructor, validated by Bbb, invalidated by Ccc
        }

        [TestMethod]
        public void InvalidBuilderInitialization_VB()
        {
            var aaaInvalidator = new BuilderPatternDescriptor<VBSyntax.InvocationExpressionSyntax>(false, (context) => context.MethodName == "Aaa");
            var bbbValidator = new BuilderPatternDescriptor<VBSyntax.InvocationExpressionSyntax>(true, (context) => context.MethodName == "Bbb");
            var cccInvalidator = new BuilderPatternDescriptor<VBSyntax.InvocationExpressionSyntax>(false, (context) => context.MethodName == "Ccc");

            var condition1 = new VisualBasicBuilderPatternCondition(false, bbbValidator);
            condition1.InvalidBuilderInitialization(context_VB).Should().BeFalse(); // Invalid constructor validated by method

            var condition2 = new VisualBasicBuilderPatternCondition(false, cccInvalidator);
            condition2.InvalidBuilderInitialization(context_VB).Should().BeTrue(); // Invalid constructor invalidated by method

            var condition3 = new VisualBasicBuilderPatternCondition(true, cccInvalidator);
            condition3.InvalidBuilderInitialization(context_VB).Should().BeTrue(); // Valid constructor invalidated by method

            var condition4 = new VisualBasicBuilderPatternCondition(true, bbbValidator);
            condition4.InvalidBuilderInitialization(context_VB).Should().BeFalse(); // Valid constructor validated by method

            var condition5 = new VisualBasicBuilderPatternCondition(false, aaaInvalidator, bbbValidator);
            condition5.InvalidBuilderInitialization(context_VB).Should().BeFalse(); // Invalid constructor, invalidated by Aaa, validated by Bbb

            var condition6 = new VisualBasicBuilderPatternCondition(false, bbbValidator, aaaInvalidator); // Configuration order has no effect
            condition6.InvalidBuilderInitialization(context_VB).Should().BeFalse(); // Invalid constructor, invalidated by Aaa, validated by Bbb

            var condition7 = new VisualBasicBuilderPatternCondition(true, bbbValidator, cccInvalidator);
            condition7.InvalidBuilderInitialization(context_VB).Should().BeTrue(); // Valid constructor, validated by Bbb, invalidated by Ccc

            var condition8 = new VisualBasicBuilderPatternCondition(true, cccInvalidator, bbbValidator); // Configuration order has no effect
            condition8.InvalidBuilderInitialization(context_VB).Should().BeTrue(); // Valid constructor, validated by Bbb, invalidated by Ccc
        }

        private static InvocationContext CreateContext_CS()
        {
            const string source =
@"class X
{
    public Item Foo()
    {
        var ret = new Item();
        ret = ret.Aaa();
        return ret.Bbb().Ccc();
    }
};
class Item
{
    public Item Aaa() { return this;}
    public Item Bbb() { return this;}
    public Item Ccc() { return this;}
}";
            var snippet = new SnippetCompiler(source, false, AnalyzerLanguage.CSharp);
            return new InvocationContext(snippet.SyntaxTree.GetRoot().DescendantNodes()
                .OfType<CSharpSyntax.InvocationExpressionSyntax>()
                .Single(x => x.Expression.GetIdentifier()?.Identifier.ValueText == "Ccc"),
                "Ccc", snippet.SemanticModel);
        }

        private static InvocationContext CreateContext_VB()
        {
            const string source =
@"Class X

    Function Foo() As Item
        Dim Ret As New Item()
        Ret = Ret.Aaa()
        Return Ret.Bbb().Ccc()
    End Function

End Class

Class Item

    Public Function Aaa() As Item
        Return Me
    End Function

    Public Function Bbb() As Item
        Return Me
    End Function

    Public Function Ccc() As Item
        Return Me
    End Function

End Class";
            var snippet = new SnippetCompiler(source, false, AnalyzerLanguage.VisualBasic);
            return new InvocationContext(snippet.SyntaxTree.GetRoot().DescendantNodes()
                .OfType<VBSyntax.InvocationExpressionSyntax>()
                .Single(x => x.Expression.GetIdentifier()?.Identifier.ValueText == "Ccc"),
                "Ccc", snippet.SemanticModel);
        }
    }
}
