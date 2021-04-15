using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Rules.Tests.Framework
{
    public sealed class TestCaseInfo
    {
        public TestCaseInfo(
            string name,
            string scenario,
            FileInfo location,
            IEnumerable<LanguageVersionInfo> languages,
            OutputKind output,
            IEnumerable<MetadataReference> additional,
            bool isFix)
        {
            Name = name;
            Scenario = scenario;
            Location = location;
            SupportedLanguageVersions = languages.ToArray();
            Output = output;
            AdditionalReferences = additional.ToArray();
            IsFix = isFix;
        }

        public string Name { get; }
        public string Scenario { get; }
        public FileInfo Location { get; }
        public IReadOnlyCollection<LanguageVersionInfo> SupportedLanguageVersions { get; }
        public OutputKind Output { get; }
        public bool IsFix { get; }

        public IEnumerable<MetadataReference> AdditionalReferences { get; }

        public bool IsApplicable(AnalyzerInfo analyzer) =>
            analyzer != null
            && Name == analyzer.Name
            && SupportedLanguageVersions.Any(info => info.Language == analyzer.Language);

        public override string ToString() =>
            $"{Name} ({SupportedLanguageVersions.FirstOrDefault()?.Language}), scenario: {Scenario ?? "Default"}, location: {Location.Name}";

        public static IEnumerable<TestCaseInfo> FromDirectory(DirectoryInfo directory)
            => (directory ?? throw new ArgumentNullException(nameof(directory)))
            .EnumerateFiles("*.*", SearchOption.AllDirectories)
            .Where(file => file.Extension == ".cs" || file.Extension == ".vb")
            .Select(file => FromFile(file));

        private static TestCaseInfo FromFile(FileInfo file)
        {
            var parts = Path.GetFileNameWithoutExtension(file.Name).Split('.').ToList();
            var isFix = parts.Contains("Fixed");
            if (isFix)
            {
                parts.Remove("Fixed");
            }
            var scenario = parts.Skip(1).FirstOrDefault();
            var name = parts[0];
            var output = parts.Contains("Console")
                ? OutputKind.ConsoleApplication
                : OutputKind.DynamicallyLinkedLibrary;

            return new TestCaseInfo(
                name,
                scenario,
                file,
                Supported(file, scenario),
                output,
                UsingsResolver.Resolve(file),
                isFix);
        }

        private static IEnumerable<LanguageVersionInfo> Supported(FileInfo location, string scenario)
        {
            IEnumerable<LanguageVersionInfo> supported = null;

            using var reader = location.OpenText();
            var header = reader.ReadLine() ?? string.Empty;

            var version = header.IndexOf("version:");
            if(version != -1)
            {
                supported = LanguageVersionInfo.Parse(header[(version + 8)..]);
            }

            return supported
                ?? LanguageVersionInfo.Parse(scenario)
                ?? (location.Extension == ".cs"
                ? LanguageVersionInfo.CSharp()
                : LanguageVersionInfo.VisualBasic());
        }
    }
}
