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
    public class CheckArgumentExceptionTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<CheckArgumentException>();

        [TestMethod]
        public void CheckArgumentException() =>
            builder.AddPaths("CheckArgumentException.cs").Verify();

#if NET

        [TestMethod]
        public void CheckArgumentException_TopLevelStatements() =>
            builder.AddPaths("CheckArgumentException.TopLevelStatements.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void CheckArgumentException_CSharp9() =>
            builder.AddPaths("CheckArgumentException.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void CheckArgumentException_CSharp10() =>
            builder.AddPaths("CheckArgumentException.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [TestMethod]
        public void CheckArgumentException_CSharp12() =>
            builder.AddPaths("CheckArgumentException.CSharp12.cs").WithOptions(ParseOptionsHelper.FromCSharp12).Verify();

#endif

    }
}
