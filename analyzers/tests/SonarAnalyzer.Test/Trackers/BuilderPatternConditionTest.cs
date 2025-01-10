/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using SonarAnalyzer.Core.Trackers;
using SonarAnalyzer.CSharp.Core.Syntax.Extensions;
using SonarAnalyzer.CSharp.Core.Trackers;
using SonarAnalyzer.VisualBasic.Core.Syntax.Extensions;
using SonarAnalyzer.VisualBasic.Core.Trackers;
using BuilderPatternDescriptorCS = SonarAnalyzer.Core.Trackers.BuilderPatternDescriptor<Microsoft.CodeAnalysis.CSharp.SyntaxKind, Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>;
using BuilderPatternDescriptorVB = SonarAnalyzer.Core.Trackers.BuilderPatternDescriptor<Microsoft.CodeAnalysis.VisualBasic.SyntaxKind, Microsoft.CodeAnalysis.VisualBasic.Syntax.InvocationExpressionSyntax>;
using CSharpSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using VBSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.Test.Trackers;

[TestClass]
public class BuilderPatternConditionTest
{
    private InvocationContext csContext;
    private InvocationContext vbContext;

    [TestInitialize]
    public void Initialize()
    {
        csContext = CreateContext_CS();
        vbContext = CreateContext_VB();
    }

    [TestMethod]
    public void ConstructorIsSafe_CS()
    {
        var safeConstructor = new CSharpBuilderPatternCondition(true, new BuilderPatternDescriptorCS(false, (context) => false));
        safeConstructor.IsInvalidBuilderInitialization(csContext).Should().BeFalse();

        var unsafeConstructor = new CSharpBuilderPatternCondition(false, new BuilderPatternDescriptorCS(false, (context) => false));
        unsafeConstructor.IsInvalidBuilderInitialization(csContext).Should().BeTrue();
    }

    [TestMethod]
    public void ConstructorIsSafe_VB()
    {
        var safeConstructor = new VisualBasicBuilderPatternCondition(true, new BuilderPatternDescriptorVB(false, (context) => false));
        safeConstructor.IsInvalidBuilderInitialization(vbContext).Should().BeFalse();

        var unsafeConstructor = new VisualBasicBuilderPatternCondition(false, new BuilderPatternDescriptorVB(false, (context) => false));
        unsafeConstructor.IsInvalidBuilderInitialization(vbContext).Should().BeTrue();
    }

    [TestMethod]
    public void IsInvalidBuilderInitialization_CS()
    {
        var aaaInvalidator = new BuilderPatternDescriptorCS(false, (context) => context.MethodName == "Aaa");
        var bbbValidator = new BuilderPatternDescriptorCS(true, (context) => context.MethodName == "Bbb");
        var cccInvalidator = new BuilderPatternDescriptorCS(false, (context) => context.MethodName == "Ccc");

        var condition1 = new CSharpBuilderPatternCondition(false, bbbValidator);
        condition1.IsInvalidBuilderInitialization(csContext).Should().BeFalse(); // Invalid constructor validated by method

        var condition2 = new CSharpBuilderPatternCondition(false, cccInvalidator);
        condition2.IsInvalidBuilderInitialization(csContext).Should().BeTrue(); // Invalid constructor invalidated by method

        var condition3 = new CSharpBuilderPatternCondition(true, cccInvalidator);
        condition3.IsInvalidBuilderInitialization(csContext).Should().BeTrue(); // Valid constructor invalidated by method

        var condition4 = new CSharpBuilderPatternCondition(true, bbbValidator);
        condition4.IsInvalidBuilderInitialization(csContext).Should().BeFalse(); // Valid constructor validated by method

        var condition5 = new CSharpBuilderPatternCondition(false, aaaInvalidator, bbbValidator);
        condition5.IsInvalidBuilderInitialization(csContext).Should().BeFalse(); // Invalid constructor, invalidated by Aaa, validated by Bbb

        var condition6 = new CSharpBuilderPatternCondition(false, bbbValidator, aaaInvalidator); // Configuration order has no effect
        condition6.IsInvalidBuilderInitialization(csContext).Should().BeFalse(); // Invalid constructor, invalidated by Aaa, validated by Bbb

        var condition7 = new CSharpBuilderPatternCondition(true, bbbValidator, cccInvalidator);
        condition7.IsInvalidBuilderInitialization(csContext).Should().BeTrue(); // Valid constructor, validated by Bbb, invalidated by Ccc

        var condition8 = new CSharpBuilderPatternCondition(true, cccInvalidator, bbbValidator); // Configuration order has no effect
        condition8.IsInvalidBuilderInitialization(csContext).Should().BeTrue(); // Valid constructor, validated by Bbb, invalidated by Ccc

        var dddContext = new InvocationContext(FindMethodInvocation_CS(csContext.Node.SyntaxTree, "Ddd"), "Ddd", csContext.Model);
        var condition9 = new CSharpBuilderPatternCondition(false);
        condition9.IsInvalidBuilderInitialization(dddContext).Should().BeFalse(); // Invalid constructor is not tracked through array propagation
    }

    [TestMethod]
    public void IsInvalidBuilderInitialization_VB()
    {
        var aaaInvalidator = new BuilderPatternDescriptorVB(false, (context) => context.MethodName == "Aaa");
        var bbbValidator = new BuilderPatternDescriptorVB(true, (context) => context.MethodName == "Bbb");
        var cccInvalidator = new BuilderPatternDescriptorVB(false, (context) => context.MethodName == "Ccc");

        var condition1 = new VisualBasicBuilderPatternCondition(false, bbbValidator);
        condition1.IsInvalidBuilderInitialization(vbContext).Should().BeFalse(); // Invalid constructor validated by method

        var condition2 = new VisualBasicBuilderPatternCondition(false, cccInvalidator);
        condition2.IsInvalidBuilderInitialization(vbContext).Should().BeTrue(); // Invalid constructor invalidated by method

        var condition3 = new VisualBasicBuilderPatternCondition(true, cccInvalidator);
        condition3.IsInvalidBuilderInitialization(vbContext).Should().BeTrue(); // Valid constructor invalidated by method

        var condition4 = new VisualBasicBuilderPatternCondition(true, bbbValidator);
        condition4.IsInvalidBuilderInitialization(vbContext).Should().BeFalse(); // Valid constructor validated by method

        var condition5 = new VisualBasicBuilderPatternCondition(false, aaaInvalidator, bbbValidator);
        condition5.IsInvalidBuilderInitialization(vbContext).Should().BeFalse(); // Invalid constructor, invalidated by Aaa, validated by Bbb

        var condition6 = new VisualBasicBuilderPatternCondition(false, bbbValidator, aaaInvalidator); // Configuration order has no effect
        condition6.IsInvalidBuilderInitialization(vbContext).Should().BeFalse(); // Invalid constructor, invalidated by Aaa, validated by Bbb

        var condition7 = new VisualBasicBuilderPatternCondition(true, bbbValidator, cccInvalidator);
        condition7.IsInvalidBuilderInitialization(vbContext).Should().BeTrue(); // Valid constructor, validated by Bbb, invalidated by Ccc

        var condition8 = new VisualBasicBuilderPatternCondition(true, cccInvalidator, bbbValidator); // Configuration order has no effect
        condition8.IsInvalidBuilderInitialization(vbContext).Should().BeTrue(); // Valid constructor, validated by Bbb, invalidated by Ccc

        var dddContext = new InvocationContext(FindMethodInvocation_VB(vbContext.Node.SyntaxTree, "Ddd"), "Ddd", vbContext.Model);
        var condition9 = new VisualBasicBuilderPatternCondition(false);
        condition9.IsInvalidBuilderInitialization(dddContext).Should().BeFalse(); // Invalid constructor is not tracked through array propagation
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

    public Item Boo()
    {
        var arr = new Item[] {new Item()};
        return arr[0].Ddd();
    }
};
class Item
{
    public Item Aaa() { return this;}
    public Item Bbb() { return this;}
    public Item Ccc() { return this;}
    public Item Ddd() { return this;}
}";
        var snippet = new SnippetCompiler(source, false, AnalyzerLanguage.CSharp);
        return new InvocationContext(FindMethodInvocation_CS(snippet.SyntaxTree, "Ccc"), "Ccc", snippet.SemanticModel);
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

    Function Boo() As Item
        Dim Arr() As Item = {New Item}
        Return Arr(0).Ddd()
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

    Public Function Ddd() As Item
        Return Me
    End Function

End Class";
        var snippet = new SnippetCompiler(source, false, AnalyzerLanguage.VisualBasic);
        return new InvocationContext(FindMethodInvocation_VB(snippet.SyntaxTree, "Ccc"), "Ccc", snippet.SemanticModel);
    }

    private static SyntaxNode FindMethodInvocation_CS(SyntaxTree tree, string name) =>
        tree.GetRoot().DescendantNodes()
            .OfType<CSharpSyntax.InvocationExpressionSyntax>()
            .Single(x => SyntaxNodeExtensionsCSharp.GetIdentifier(x.Expression)?.ValueText == name);

    private static SyntaxNode FindMethodInvocation_VB(SyntaxTree tree, string name) =>
        tree.GetRoot().DescendantNodes()
            .OfType<VBSyntax.InvocationExpressionSyntax>()
            .Single(x => SyntaxNodeExtensionsVisualBasic.GetIdentifier(x.Expression)?.ValueText == name);
}
