/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class TooManyGenericParametersTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<TooManyGenericParameters>();

        [TestMethod]
        public void TooManyGenericParameters_DefaultValues() =>
            builder.AddPaths("TooManyGenericParameters.DefaultValues.cs").Verify();

        [TestMethod]
        public void TooManyGenericParameters_CustomValues() =>
            new VerifierBuilder()
            .AddAnalyzer(() => new TooManyGenericParameters { MaxNumberOfGenericParametersInClass = 4, MaxNumberOfGenericParametersInMethod = 4 })
            .AddPaths("TooManyGenericParameters.CustomValues.cs")
            .Verify();

#if NET

        [TestMethod]
        public void TooManyGenericParameters_CSharp9() =>
            builder.AddPaths("TooManyGenericParameters.CSharp9.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void TooManyGenericParameters_CSharp10() =>
            builder.AddPaths("TooManyGenericParameters.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [TestMethod]
        public void TooManyGenericParameters_CSharp11() =>
            builder.AddPaths("TooManyGenericParameters.CSharp11.cs").WithOptions(ParseOptionsHelper.FromCSharp11).Verify();

        [TestMethod]
        public void TooManyGenericParameters_CSharp12() =>
            builder.AddPaths("TooManyGenericParameters.CSharp12.cs").WithOptions(ParseOptionsHelper.FromCSharp12).Verify();

#endif

    }
}
