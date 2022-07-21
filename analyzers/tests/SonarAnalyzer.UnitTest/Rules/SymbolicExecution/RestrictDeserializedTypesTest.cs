/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using SonarAnalyzer.Common;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class RestrictDeserializedTypesTest
    {
        private static readonly DiagnosticDescriptor[] OnlyDiagnostics = new[] { RestrictDeserializedTypes.S5773 };

#if NETFRAMEWORK // These serializers are available only when targeting .Net Framework
        [TestMethod]
        public void RestrictDeserializedTypesFormatters() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\RestrictDeserializedTypes.cs",
                new SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg),
                ParseOptionsHelper.FromCSharp8,
                AdditionalReferencesNetFx(),
                onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        public void RestrictDeserializedTypes_DoesNotRaiseIssuesForTestProject() =>
            OldVerifier.VerifyNoIssueReportedInTest(@"TestCases\SymbolicExecution\Sonar\RestrictDeserializedTypes.cs",
                new SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg),
                ParseOptionsHelper.FromCSharp8,
                AdditionalReferencesNetFx(),
                onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        public void RestrictDeserializedTypesJavaScriptSerializer() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\RestrictDeserializedTypes.JavaScriptSerializer.cs",
                new SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg),
                ParseOptionsHelper.FromCSharp8,
                AdditionalReferencesNetFx(),
                onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        public void RestrictDeserializedTypesLosFormatter() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\RestrictDeserializedTypes.LosFormatter.cs",
                new SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg),
                ParseOptionsHelper.FromCSharp8,
                AdditionalReferencesNetFx(),
                onlyDiagnostics: OnlyDiagnostics);

        private static IEnumerable<MetadataReference> AdditionalReferencesNetFx() =>
            FrameworkMetadataReference.SystemRuntimeSerialization
            .Union(FrameworkMetadataReference.SystemRuntimeSerializationFormattersSoap)
            .Union(FrameworkMetadataReference.SystemWeb)
            .Union(FrameworkMetadataReference.SystemWebExtensions);
#endif

#if NET
        [TestMethod]
        public void RestrictDeserializedTypesFormatters_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\SymbolicExecution\Sonar\RestrictDeserializedTypes.CSharp9.cs",
                new SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg),
                new[] { MetadataReferences.CoreMetadataReference.SystemRuntimeSerializationFormatters },
                onlyDiagnostics: OnlyDiagnostics);
#endif
    }
}
