/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using SonarAnalyzer.Helpers;

using CSharp = Microsoft.CodeAnalysis.CSharp.Syntax;
using VisualBasic = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class SyntaxHelperTest
    {
        private const string SourceInputToString_CS =
@"class Example
{
    string Method(object input)
    {
        return input.ToString();
    }
}";

        private const string SourceInputToString_VB =
@"Class Example
    Function Method(Input As Object) As String
        Return Input.ToString()
    End Function
End Class";

        [TestMethod]
        public void GetName_CS()
        {
            var nodes = Parse_CS(SourceInputToString_CS);
            nodes.OfType<CSharp.MemberAccessExpressionSyntax>().Single().GetName().Should().Be("ToString");
            nodes.OfType<CSharp.IdentifierNameSyntax>().Select(x => x.GetName()).Should().BeEquivalentTo("input", "ToString");
            nodes.OfType<CSharp.InvocationExpressionSyntax>().Single().GetName().Should().BeEmpty();
        }

        [TestMethod]
        public void GetName_VB()
        {
            var nodes = Parse_VB(SourceInputToString_VB);
            nodes.OfType<VisualBasic.MemberAccessExpressionSyntax>().Single().GetName().Should().Be("ToString");
            nodes.OfType<VisualBasic.IdentifierNameSyntax>().Select(x => x.GetName()).Should().BeEquivalentTo("Input", "ToString");
            nodes.OfType<VisualBasic.InvocationExpressionSyntax>().Single().GetName().Should().BeEmpty();
        }

        [TestMethod]
        public void NameIs_CS()
        {
            var toString = Parse_CS(SourceInputToString_CS).OfType<CSharp.MemberAccessExpressionSyntax>().Single();
            toString.NameIs("ToString").Should().BeTrue();
            toString.NameIs("TOSTRING").Should().BeFalse();
            toString.NameIs("tostring").Should().BeFalse();
            toString.NameIs("test").Should().BeFalse();
            toString.NameIs("").Should().BeFalse();
            toString.NameIs(null).Should().BeFalse();
        }

        [TestMethod]
        public void NameIs_VB()
        {
            var toString = Parse_VB(SourceInputToString_VB).OfType<VisualBasic.MemberAccessExpressionSyntax>().Single();
            toString.NameIs("ToString").Should().BeTrue();
            toString.NameIs("TOSTRING").Should().BeTrue();
            toString.NameIs("tostring").Should().BeTrue();
            toString.NameIs("test").Should().BeFalse();
            toString.NameIs("").Should().BeFalse();
            toString.NameIs(null).Should().BeFalse();
        }

        [TestMethod]
        public void IsAssignmentToTuple_IsTrue()
        {
            const string code = @"
public class Sample
{
    public void Method()
    {
        int a, b;
        (a, b) = (42, 42);
    }
}";
            var argument = Parse_CS(code).OfType<CSharp.ArgumentSyntax>().First();
            argument.IsInTupleAssignmentTarget().Should().BeTrue();
        }

        [TestMethod]
        public void IsAssignmentToTuple_IsFalse()
        {
            const string code = @"
public class Sample
{
    public void TupleArg((int, int) tuple) { }

    public void Method(int methodArgument)
    {
        int a = 42;
        var t = (a, methodArgument);
        var x = (42, 42);
        var nested = (42, (42, 42));
        TupleArg((42, 42));
        Method(0);
    }
}";
            var arguments = Parse_CS(code).OfType<CSharp.ArgumentSyntax>().ToArray();

            arguments.Should().HaveCount(12);
            foreach (var argument in arguments)
            {
                argument.IsInTupleAssignmentTarget().Should().BeFalse();
            }
        }

        private static SyntaxNode[] Parse_CS(string source) =>
            Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().ToArray();

        private static SyntaxNode[] Parse_VB(string source) =>
            Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(source).GetRoot().DescendantNodes().ToArray();
    }
}
