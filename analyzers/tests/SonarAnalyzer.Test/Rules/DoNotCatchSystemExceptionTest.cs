/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class DoNotCatchSystemExceptionTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<DoNotCatchSystemException>();

        [TestMethod]
        public void DoNotCatchSystemException() =>
            builder.AddReferences(NuGetMetadataReference.MicrosoftAzureWebJobsCore()).AddPaths("DoNotCatchSystemException.cs").Verify();

#if NET

        [TestMethod]
        public void DoNotCatchSystemException_CSharp9() =>
            builder.AddPaths("DoNotCatchSystemException.CSharp9.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void DoNotCatchSystemException_CSharp10() =>
            builder.AddPaths("DoNotCatchSystemException.CSharp10.cs").WithOptions(LanguageOptions.FromCSharp10).WithTopLevelStatements().Verify();

#endif

    }
}
