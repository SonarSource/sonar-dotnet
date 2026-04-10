/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
    public class DisposableTypesNeedFinalizersTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<DisposableTypesNeedFinalizers>();

        [TestMethod]
        public void DisposableTypesNeedFinalizers() =>
            builder.AddPaths("DisposableTypesNeedFinalizers.cs").Verify();

        [TestMethod]
        public void DisposableTypesNeedFinalizers_CSharp9() =>
            builder.AddPaths("DisposableTypesNeedFinalizers.CSharp9.cs").WithOptions(LanguageOptions.FromCSharp9).Verify();

        [TestMethod]
        public void DisposableTypesNeedFinalizers_CSharp11() =>
            builder.AddPaths("DisposableTypesNeedFinalizers.CSharp11.cs").WithOptions(LanguageOptions.FromCSharp11).Verify();

        [TestMethod]
        public void DisposableTypesNeedFinalizers_InvalidCode() =>
            builder.AddSnippet("""
                public class Foo_05 : IDisposable
                {
                    private HandleRef;
                }
                """).VerifyNoIssuesIgnoreErrors();
    }
}
