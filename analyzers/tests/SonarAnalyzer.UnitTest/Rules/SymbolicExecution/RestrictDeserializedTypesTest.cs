/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

extern alias csharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using csharp::SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.SymbolicExecution;
using SonarAnalyzer.UnitTest.TestFramework;
#if NETFRAMEWORK
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.UnitTest.MetadataReferences;
#endif

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class RestrictDeserializedTypesTest
    {
#if NETFRAMEWORK // These serializers are available only when targeting .Net Framework
        [TestMethod]
        [TestCategory("Rule")]
        public void RestrictDeserializedTypesFormatters() =>
            Verifier.VerifyAnalyzer(@"TestCases\RestrictDeserializedTypes.cs",
                GetAnalyzer(),
                ParseOptionsHelper.FromCSharp8,
                GetAdditionalReferences());

        [TestMethod]
        [TestCategory("Rule")]
        public void RestrictDeserializedTypesJavaScriptSerializer() =>
            Verifier.VerifyAnalyzer(@"TestCases\RestrictDeserializedTypes.JavaScriptSerializer.cs",
                GetAnalyzer(),
                ParseOptionsHelper.FromCSharp8,
                GetAdditionalReferences());

        [TestMethod]
        [TestCategory("Rule")]
        public void RestrictDeserializedTypesLosFormatter() =>
            Verifier.VerifyAnalyzer(@"TestCases\RestrictDeserializedTypes.LosFormatter.cs",
                GetAnalyzer(),
                ParseOptionsHelper.FromCSharp8,
                GetAdditionalReferences());

        private static IEnumerable<MetadataReference> GetAdditionalReferences() =>
            FrameworkMetadataReference.SystemRuntimeSerialization
            .Union(FrameworkMetadataReference.SystemRuntimeSerializationFormattersSoap)
            .Union(FrameworkMetadataReference.SystemWeb)
            .Union(FrameworkMetadataReference.SystemWebExtensions);
#endif

#if NET
        [TestMethod]
        [TestCategory("Rule")]
        public void RestrictDeserializedTypesFormatters_CSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\RestrictDeserializedTypes.CSharp9.cs",
                GetAnalyzer(),
                new[] { MetadataReferences.CoreMetadataReference.SystemRuntimeSerializationFormatters });
#endif

        private static SonarDiagnosticAnalyzer GetAnalyzer() =>
            new SymbolicExecutionRunner(new RestrictDeserializedTypes());
    }
}
