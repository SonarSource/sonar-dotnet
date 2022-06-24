using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.UnitTest.Extensions
{
    [TestClass]
    public class AssignmentExpressionSyntaxExtensionsTests
    {
        [DataTestMethod]
        [DataRow("(var x, var y) = (1, 2)", "var x,var y")]
        public void AssignmentExpressionSyntaxExtensions_AssignmentTargets_DeconstructTargets(string assignment, string expectedTargets)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(WrapInMethod(assignment));
            var assignmentExpression = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>().First();
            var allTargets = assignmentExpression.AssignmentTargets();
            var allTargetsAsString = string.Join(",", allTargets.Select(x => x.ToString()));
            allTargetsAsString.Should().Be(expectedTargets);

        }

        private static string WrapInMethod(string code) =>
$@"
public class C
{{
    public int M()
    {{
        var t = {code};
        return 0;
    }}
}}
";
    }
}
