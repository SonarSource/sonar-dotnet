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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ThrowReservedExceptionsTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ThrowReservedExceptions>();

    [TestMethod]
    public void ThrowReservedExceptions_CS() =>
        builderCS.AddPaths("ThrowReservedExceptions.cs").Verify();

    [TestMethod]
    public void ThrowReservedExceptions_CS_Latest() =>
        builderCS.AddPaths("ThrowReservedExceptions.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();

    [TestMethod]
    public void ThrowReservedExceptions_VB() =>
        new VerifierBuilder<VB.ThrowReservedExceptions>().AddPaths("ThrowReservedExceptions.vb").Verify();
}
