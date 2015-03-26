using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

namespace Tests.Diagnostics
{
    [TestClass]
    public class UnusedLocalVariableTest
    {
        [TestMethod]
        public void UnusedLocalVariable()
        {
            var solution =
                new AdhocWorkspace().CurrentSolution.AddProject("foo", "foo.dll", LanguageNames.CSharp)
                    .AddMetadataReference(MetadataReference.CreateFromAssembly(typeof(object).Assembly))
                    .AddDocument("foo.cs", File.ReadAllText(@"TestCases\UnusedLocalVariable.cs", Encoding.UTF8))
                    .Project
                    .Solution;

            Verifier.Verify(solution, new UnusedLocalVariable() { CurrentSolution = solution });
        }
    }
}
