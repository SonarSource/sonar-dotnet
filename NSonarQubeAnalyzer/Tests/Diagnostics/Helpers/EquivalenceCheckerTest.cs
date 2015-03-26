using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Helpers;



namespace Tests.Diagnostics.Helpers
{
    [TestClass]
    public class EquivalenceCheckerTest
    {
        private const string Source = @"
namespace Test
{
    class TestClass
    {
        int Property {get;set;}
        public void Method1()
        {
            var x = Property;
            Console.WriteLine(x);
        }

        public void Method2()
        {
            var x = Property;
            Console.WriteLine(x);
        }

        public void Method3()
        {
            var x = Property+2;
            Console.Write(x);            
        }
    }
}";

        private Solution solution;
        private Compilation compilation;
        private SyntaxTree syntaxTree;
        private SemanticModel semanticModel;
        private List<MethodDeclarationSyntax> methods;
        private EquivalenceChecker eqChecker;

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
            eqChecker = new EquivalenceChecker(semanticModel);

            methods = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        }

        [TestMethod]
        public void AreEquivalent_Node()
        {
            var result = eqChecker.AreEquivalent(
                methods.First(m => m.Identifier.ValueText == "Method1").Body,
                methods.First(m => m.Identifier.ValueText == "Method2").Body);
            result.Should().BeTrue();

            result = eqChecker.AreEquivalent(
                methods.First(m => m.Identifier.ValueText == "Method1").Body,
                methods.First(m => m.Identifier.ValueText == "Method3").Body);
            result.Should().BeFalse();

            var expandedMethod1 = Simplifier.Expand(methods.First(m => m.Identifier.ValueText == "Method1").Body,
                semanticModel, solution.Workspace);

            var expandedMethod2 = Simplifier.Expand(methods.First(m => m.Identifier.ValueText == "Method2").Body,
                semanticModel, solution.Workspace);

            result = eqChecker.AreEquivalent(
                methods.First(m => m.Identifier.ValueText == "Method1").Body,
                methods.First(m => m.Identifier.ValueText == "Method2").Body, false);
            result.Should().BeFalse();

            result = eqChecker.AreEquivalent(
                expandedMethod1,
                methods.First(m => m.Identifier.ValueText == "Method2").Body, false);
            result.Should().BeTrue();

            result = eqChecker.AreEquivalent(
                methods.First(m => m.Identifier.ValueText == "Method1").Body,
                expandedMethod2, false, false);
            result.Should().BeFalse();
        }

        [TestMethod]
        public void AreEquivalent_List()
        {
            var result = eqChecker.AreEquivalent(
                methods.First(m => m.Identifier.ValueText == "Method1").Body.Statements,
                methods.First(m => m.Identifier.ValueText == "Method2").Body.Statements);
            result.Should().BeTrue();

            result = eqChecker.AreEquivalent(
                methods.First(m => m.Identifier.ValueText == "Method1").Body.Statements,
                methods.First(m => m.Identifier.ValueText == "Method3").Body.Statements);
            result.Should().BeFalse();

            var expandedMethod1 = Simplifier.Expand(methods.First(m => m.Identifier.ValueText == "Method1").Body,
                semanticModel, solution.Workspace);

            var expandedMethod2 = Simplifier.Expand(methods.First(m => m.Identifier.ValueText == "Method2").Body,
                semanticModel, solution.Workspace);

            result = eqChecker.AreEquivalent(
                methods.First(m => m.Identifier.ValueText == "Method1").Body.Statements,
                methods.First(m => m.Identifier.ValueText == "Method2").Body.Statements, false);
            result.Should().BeFalse();

            result = eqChecker.AreEquivalent(
                expandedMethod1.Statements,
                methods.First(m => m.Identifier.ValueText == "Method2").Body.Statements, false);
            result.Should().BeTrue();

            result = eqChecker.AreEquivalent(
                methods.First(m => m.Identifier.ValueText == "Method1").Body.Statements,
                expandedMethod2.Statements, false, false);
            result.Should().BeFalse();
        }

        [TestMethod]
        public void GetExpandedList()
        {
            var expandedList = eqChecker.GetExpandedList(
                methods.First(m => m.Identifier.ValueText == "Method1").Body.Statements);

            var result = eqChecker.AreEquivalent(expandedList,
                methods.First(m => m.Identifier.ValueText == "Method1").Body.Statements, false, false);

            result.Should().BeFalse();

            result = eqChecker.AreEquivalent(expandedList,
                methods.First(m => m.Identifier.ValueText == "Method1").Body.Statements, false, true);

            result.Should().BeTrue();
        }
    }
}
