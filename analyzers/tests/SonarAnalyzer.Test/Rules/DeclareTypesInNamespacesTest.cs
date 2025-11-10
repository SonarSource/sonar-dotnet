/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.CSharp;
using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class DeclareTypesInNamespacesTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<CS.DeclareTypesInNamespaces>();
        private readonly VerifierBuilder nonConcurrent = new VerifierBuilder<CS.DeclareTypesInNamespaces>().WithConcurrentAnalysis(false);

        [TestMethod]
        public void DeclareTypesInNamespaces_CS() =>
            builder.AddPaths("DeclareTypesInNamespaces.cs", "DeclareTypesInNamespaces2.cs").WithAutogenerateConcurrentFiles(false).Verify();

        [TestMethod]
        public void DeclareTypesInNamespaces_CSharp7() =>
            nonConcurrent.AddPaths("DeclareTypesInNamespaces.CSharp7.cs").WithLanguageVersion(LanguageVersion.CSharp7).Verify();

        [TestMethod]
        public void DeclareTypesInNamespaces_CS_After8() =>
            nonConcurrent.AddPaths("DeclareTypesInNamespaces.AfterCSharp8.cs").WithOptions(LanguageOptions.FromCSharp8).Verify();

#if NET

        [TestMethod]
        public void DeclareTypesInNamespaces_CS_AfterCSharp9() =>
            builder
                .AddPaths("DeclareTypesInNamespaces.AfterCSharp9.cs", "DeclareTypesInNamespaces.AfterCSharp9.PartialProgramClass.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void DeclareTypesInNamespaces_CS_AfterCSharp10() =>
            nonConcurrent
                .AddPaths("DeclareTypesInNamespaces.AfterCSharp10.FileScopedNamespace.cs", "DeclareTypesInNamespaces.AfterCSharp10.RecordStruct.cs")
                .WithOptions(LanguageOptions.FromCSharp10)
                .Verify();

#endif

        [TestMethod]
        public void DeclareTypesInNamespaces_VB() =>
            new VerifierBuilder<VB.DeclareTypesInNamespaces>()
                .AddPaths("DeclareTypesInNamespaces.vb", "DeclareTypesInNamespaces2.vb")
                .WithAutogenerateConcurrentFiles(false)
                .Verify();
    }
}
