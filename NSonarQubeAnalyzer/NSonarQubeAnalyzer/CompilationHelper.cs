using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace NSonarQubeAnalyzer
{
    public class CompilationHelper
    {
        public static Solution GetSolutionFromFiles(params string[] filePaths)
        {
            var project = new AdhocWorkspace().CurrentSolution.AddProject("foo", "foo.dll", LanguageNames.CSharp)
                .AddMetadataReference(MetadataReference.CreateFromAssembly(typeof(object).Assembly));

            foreach (var filePath in filePaths)
            {
                var file = new FileInfo(filePath);
                var document = project.AddDocument(file.Name, File.ReadAllText(file.FullName, Encoding.UTF8));
                project = document.Project;
            }

            return project.Solution;
        }

        public static Solution GetSolutionFromText(string text)
        {
            return new AdhocWorkspace().CurrentSolution.AddProject("foo", "foo.dll", LanguageNames.CSharp)
                .AddMetadataReference(MetadataReference.CreateFromAssembly(typeof (object).Assembly))
                .AddDocument("foo.cs", text)
                .Project
                .Solution;
        }
    }
}
