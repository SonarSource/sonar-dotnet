using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Rules.Tests.Framework
{
    public sealed class AnalyzerInfo
    {
        public Type AnalyzerType { get; }
        public string Language { get; }
        public IReadOnlyCollection<string> DiagnosticIds { get; }
        public string Name => AnalyzerType.Name;

        public AnalyzerInfo(Type analyzerType)
        {
            AnalyzerType = analyzerType ?? throw new ArgumentNullException(nameof(analyzerType));
            DiagnosticIds = analyzerType.GetCustomAttributes<RuleAttribute>(true).Select(attr => attr.Key).ToArray();
            Language = analyzerType.GetCustomAttribute<DiagnosticAnalyzerAttribute>(false)?.Languages.FirstOrDefault();
        }

        public DiagnosticAnalyzer Instance()
        {
            if (instance is null)
            {
                instance = (DiagnosticAnalyzer)Activator.CreateInstance(AnalyzerType);
            }
            return instance;
        }
        private DiagnosticAnalyzer instance;

        public override string ToString() => $"{string.Join(",", DiagnosticIds)}: {Name ?? Unknown} ({Language ?? Unknown})";
        private const string Unknown = "?";

        public static IEnumerable<AnalyzerInfo> FromAssembly(params Assembly[] assemblies) =>
            assemblies
                .SelectMany(assembly => assembly.ExportedTypes)
                .Where(tp => !tp.IsAbstract && typeof(DiagnosticAnalyzer).IsAssignableFrom(tp))
                .Select(tp => new AnalyzerInfo(tp));
    }
}
