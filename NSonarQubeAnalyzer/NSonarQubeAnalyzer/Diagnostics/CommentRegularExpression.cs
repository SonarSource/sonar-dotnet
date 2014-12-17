namespace NSonarQubeAnalyzer.Diagnostics
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    public class CommentRegularExpressionRule
    {
        public DiagnosticDescriptor Descriptor;
        public string RegularExpression;
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CommentRegularExpression : DiagnosticsRule
    {
        public ImmutableArray<CommentRegularExpressionRule> Rules { get; set; }

        /// <summary>
        /// Rule ID
        /// </summary>
        public override string RuleId
        {
            get
            {
                return "CommentRegularExpression";
            }
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return this.Rules.Select(r => r.Descriptor).ToImmutableArray(); } }

        /// <summary>
        /// Configure the rule from the supplied settings
        /// </summary>
        /// <param name="settings">XML settings</param>
        public override void Configure(XDocument settings)
        {
            var commentRegexpRules = from e in settings.Descendants("Rule")
                                     where
                                         this.RuleId.Equals(
                                             (e.Elements("ParentKey").SingleOrDefault() ?? XElement.Parse("<Dummy />")).Value)
                                     select e;

            if (!commentRegexpRules.Any())
            {
                return;
            }

            var builder = ImmutableArray.CreateBuilder<CommentRegularExpressionRule>();

            foreach (var commentRegexpRule in commentRegexpRules)
            {
                string key = commentRegexpRule.Elements("Key").Single().Value;
                var parameters = commentRegexpRule.Descendants("Parameter");
                string message =
                    (from e in parameters
                     where "message".Equals(e.Elements("Key").Single().Value)
                     select e.Elements("Value").Single().Value).Single();
                string regularExpression = (from e in parameters
                                            where "regularExpression".Equals(e.Elements("Key").Single().Value)
                                            select e.Elements("Value").Single().Value).Single();

                builder.Add(
                    new CommentRegularExpressionRule
                    {
                        // TODO: Add rule description
                        Descriptor =
                            new DiagnosticDescriptor(key, "TODO", message, "SonarQube", DiagnosticSeverity.Warning, true),
                        RegularExpression = regularExpression
                    });

                this.Rules = builder.ToImmutable();
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(
                c =>
                {
                    var comments = from t in c.Tree.GetCompilationUnitRoot().DescendantTrivia()
                                     where IsComment(t)
                                     select t;

                    foreach (var comment in comments)
                    {
                        var text = comment.ToString();
                        foreach (var rule in this.Rules)
                        {
                            if (Regex.IsMatch(text, rule.RegularExpression))
                            {
                                c.ReportDiagnostic(Diagnostic.Create(rule.Descriptor, comment.GetLocation()));
                            }
                        }
                    }
                });
        }

        private static bool IsComment(SyntaxTrivia trivia)
        {
            return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
        }
    }
}