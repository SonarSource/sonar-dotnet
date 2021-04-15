using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Rules.Tests.Framework;
using SonarAnalyzer.UnitTest.TestFramework;

namespace Rules
{
    public class Verify
    {
        [VerifyAssembly(typeof(SonarAnalyzer.Rules.CSharp.ArrayCovariance))]
        [VerifyAssembly(typeof(SonarAnalyzer.Rules.VisualBasic.ArrayCreationLongSyntax))]
        public void Cases(
            string testCase,
            DiagnosticAnalyzer analyzer,
            ParseOptions[] options,
            OutputKind output,
            IEnumerable<MetadataReference> additionalReferences) =>
            Verifier.VerifyAnalyzer(
                testCase,
                analyzer,
                options,
                outputKind: output,
                additionalReferences: additionalReferences);
    }
}
