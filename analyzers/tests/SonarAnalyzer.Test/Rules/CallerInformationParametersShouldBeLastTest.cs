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

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class CallerInformationParametersShouldBeLastTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<CallerInformationParametersShouldBeLast>();

        [TestMethod]
        public void CallerInformationParametersShouldBeLast() =>
            builder.AddPaths("CallerInformationParametersShouldBeLast.cs").Verify();

#if NET

        [TestMethod]
        public void CallerInformationParametersShouldBeLast_CSharp9() =>
            builder.AddPaths("CallerInformationParametersShouldBeLast.CSharp9.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void CallerInformationParametersShouldBeLast_CSharp10() =>
            builder.AddPaths("CallerInformationParametersShouldBeLast.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [TestMethod]
        public void CallerInformationParametersShouldBeLast_CSharp11() =>
            builder.AddPaths("CallerInformationParametersShouldBeLast.CSharp11.cs").WithOptions(ParseOptionsHelper.FromCSharp11)
                .VerifyNoIssues();   // overriding an abstract default implementation is compliant

#endif

        [TestMethod]
        public void CallerInformationParametersShouldBeLastInvalidSyntax() =>
            builder.AddPaths("CallerInformationParametersShouldBeLastInvalidSyntax.cs").WithLanguageVersion(LanguageVersion.CSharp7).WithConcurrentAnalysis(false).Verify();
    }
}
