using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using static SonarAnalyzer.UnitTest.MetadataReferences.MetadataReferenceFactory;
using Nuget = SonarAnalyzer.UnitTest.MetadataReferences.NuGetMetadataFactory;

namespace SonarAnalyzer.Rules.Tests.Framework
{
    public static class UsingsResolver
    {
        private const string LatestVersion = "LATEST";

        public static IEnumerable<MetadataReference> Resolve(FileInfo file)
        {
            Func<string, string> selector = file.Extension == ".cs" ? CSharp : VisualBasic;
            return Lines(file)
                .Select(selector)
                .SelectMany(Reference);
        }

        private static IEnumerable<MetadataReference> Reference(string ns)
        => ns switch
        {
            "Microsoft.VisualStudio.TestTools.UnitTesting" => Create(typeof(Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute)),
            "NUnit.Framework" => Create(typeof(NUnit.Framework.TestAttribute)),
            "System.Xml" => Create(typeof(System.Xml.XmlDocument)),
            "System.Security.Cryptography" => Create(typeof(System.Security.Cryptography.SHA1)),
            "System.Data.SqlClient" => Nuget.Create("System.Data.SqlClient", "4.5.0"),
            "Xunit" =>
                Nuget.Create("xunit.assert", LatestVersion).Concat(
                Nuget.Create("xunit.extensibility.core", LatestVersion)),

            _ => Array.Empty<MetadataReference>(),
        };

        private static IEnumerable<string> Lines(FileInfo file)
        {
            var reader = file.OpenText();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line.Trim();
            }
        }

        private static string CSharp(string line) =>
            line.StartsWith("using ")
            ? line[6..^1]
            : null;

        private static string VisualBasic(string line) =>
        line.StartsWith("Import ")
            ? line[7..]
            : null;
    }
}
