
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Helpers;

namespace Tests.Diagnostics.Helpers
{
    [TestClass]
    public class ConditionalHelperTest
    {
        private const string Source = @"
namespace Test
{
    class TestClass
    {        
        public void DoSomething(){}
        public void IfMethod()
        {
            if (true)
                DoSomething();
            else if (true)
                DoSomething();
            else
                DoSomething();  
        }

        public void SwitchMethod()
        {
            var i = 5;
            switch(i)
            {
                case 3:
                    DoSomething();
                    break;
                case 5:
                    DoSomething();
                    break;
                default:
                    DoSomething();
                    break;
            }  
        }
    }
}";
        private Solution solution;
        private Compilation compilation;
        private SyntaxTree syntaxTree;
        private SemanticModel semanticModel;
        private MethodDeclarationSyntax ifMethod;
        private MethodDeclarationSyntax switchMethod;

        [TestInitialize]
        public void TestSetup()
        {
            solution =
                new AdhocWorkspace().CurrentSolution.AddProject("foo", "foo.dll", LanguageNames.CSharp)
                    .AddMetadataReference(MetadataReference.CreateFromAssembly(typeof(object).Assembly))
                    .AddDocument("foo.cs", Source)
                    .Project
                    .Solution;

            compilation = solution.Projects.First().GetCompilationAsync().Result;
            syntaxTree = compilation.SyntaxTrees.First();
            semanticModel = compilation.GetSemanticModel(syntaxTree);

            ifMethod = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First(m => m.Identifier.ValueText == "IfMethod");
            switchMethod = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First(m => m.Identifier.ValueText == "SwitchMethod");
        }

        [TestMethod]
        public void GetPrecedingIfsInConditionChain()
        {
            var ifStatement1 = ifMethod.DescendantNodes().OfType<IfStatementSyntax>().First();
            ifStatement1.GetPrecedingIfsInConditionChain().Count.Should().Be(0);

            var ifStatement2 = ifMethod.DescendantNodes().OfType<IfStatementSyntax>().Last();
            var preceding = ifStatement2.GetPrecedingIfsInConditionChain();
            preceding.Count.Should().Be(1);

            ifStatement1.ShouldBeEquivalentTo(preceding[0]);
        }

        [TestMethod]
        public void GetPrecedingStatementsInConditionChain()
        {
            var ifStatement1 = ifMethod.DescendantNodes().OfType<IfStatementSyntax>().First();
            ifStatement1.GetPrecedingStatementsInConditionChain().Count().Should().Be(0);

            var ifStatement2 = ifMethod.DescendantNodes().OfType<IfStatementSyntax>().Last();
            var preceding = ifStatement2.GetPrecedingStatementsInConditionChain().ToList();
            preceding.Count().Should().Be(1);

            ifStatement1.Statement.ShouldBeEquivalentTo(preceding[0]);
        }

        [TestMethod]
        public void GetPrecedingConditionsInConditionChain()
        {
            var ifStatement1 = ifMethod.DescendantNodes().OfType<IfStatementSyntax>().First();
            ifStatement1.GetPrecedingConditionsInConditionChain().Count().Should().Be(0);

            var ifStatement2 = ifMethod.DescendantNodes().OfType<IfStatementSyntax>().Last();
            var preceding = ifStatement2.GetPrecedingConditionsInConditionChain().ToList();
            preceding.Count().Should().Be(1);

            ifStatement1.Condition.ShouldBeEquivalentTo(preceding[0]);
        }

        [TestMethod]
        public void GetPrecedingSections()
        {
            var sections = switchMethod.DescendantNodes().OfType<SwitchSectionSyntax>().ToList();

            sections.Last().GetPrecedingSections().Count().Should().Be(2);
            sections.First().GetPrecedingSections().Count().Should().Be(0);
            sections.Last().GetPrecedingSections().First().ShouldBeEquivalentTo(sections.First());
        }
    }
}
