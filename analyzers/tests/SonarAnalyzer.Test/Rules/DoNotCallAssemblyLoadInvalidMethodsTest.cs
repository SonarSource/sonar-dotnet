/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class DoNotCallAssemblyLoadInvalidMethodsTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<DoNotCallAssemblyLoadInvalidMethods>();

        [TestMethod]
        public void DoNotCallAssemblyLoadInvalidMethods() =>
            builder.AddPaths("DoNotCallAssemblyLoadInvalidMethods.cs")
                .AddReferences(MetadataReferenceFacade.SystemSecurityPermissions)
                .Verify();

#if NET

        [TestMethod]
        public void DoNotCallAssemblyLoadInvalidMethods_CSharp9() =>
            builder.AddPaths("DoNotCallAssemblyLoadInvalidMethods.CSharp9.cs")
                .WithOptions(LanguageOptions.FromCSharp9)
                .WithTopLevelStatements()
                .AddReferences(MetadataReferenceFacade.SystemSecurityPermissions)
                .Verify();

#endif

#if NETFRAMEWORK // The overloads with Evidence are obsolete on .Net Framework 4.8 and not available on .Net Core

        [TestMethod]
        public void DoNotCallAssemblyLoadInvalidMethods_EvidenceParameter() =>
            builder.AddPaths("DoNotCallAssemblyLoadInvalidMethods.Evidence.cs")
                .Verify();

#endif

    }
}
