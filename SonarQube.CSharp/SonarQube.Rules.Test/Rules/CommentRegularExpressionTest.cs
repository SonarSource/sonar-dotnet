using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
{
    [TestClass]
    public class CommentRegularExpressionTest
    {
        [TestMethod]
        public void CommentRegularExpression()
        {
            var rules = ImmutableArray.Create(
                new CommentRegularExpressionRule
                {
                    Descriptor = Analyzers.Rules.CommentRegularExpression.CreateDiagnosticDescriptor("id1", ""),
                    RegularExpression = "(?i)TODO"
                });

            var diagnostic = new CommentRegularExpression {Rules = rules};
            Verifier.Verify(@"TestCases\CommentRegularExpression.cs", diagnostic);
        }
    }
}
