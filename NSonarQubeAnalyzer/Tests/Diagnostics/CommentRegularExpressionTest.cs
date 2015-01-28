namespace Tests.Diagnostics
{
    using Microsoft.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NSonarQubeAnalyzer.Diagnostics;
    using System.Collections.Immutable;

    [TestClass]
    public class CommentRegularExpressionTest
    {
        [TestMethod]
        public void CommentRegularExpression()
        {
            var rules = ImmutableArray.Create(
                new CommentRegularExpressionRule
                {
                    Descriptor = new DiagnosticDescriptor("id1", "", "", "SonarQube", DiagnosticSeverity.Warning, true),
                    RegularExpression = "(?i)TODO"
                });

            var diagnostic = new CommentRegularExpression();
            diagnostic.Rules = rules;
            Verifier.Verify(@"TestCases\CommentRegularExpression.cs", diagnostic);
        }
    }
}