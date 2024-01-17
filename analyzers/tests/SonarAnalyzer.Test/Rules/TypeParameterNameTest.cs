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

using SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class TypeParameterNameTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<TypeParameterName>();

        [TestMethod]
        public void TypeParameterName_S119() =>
            builder.WithOnlyDiagnostics(TypeParameterName.S119)
                   .AddPaths("TypeParameterName.vb")
                   .Verify();

        [TestMethod]
        public void TypeParameterName_S2373() =>
            builder.WithOnlyDiagnostics(TypeParameterName.S2373)
                   .AddPaths("TypeParameterName.vb")
                   .Verify();

        [TestMethod]
        public void TypeParameterName_S119_CustomRegex() =>
            new VerifierBuilder().AddAnalyzer(() => new TypeParameterName { Pattern = "^[A-Z]{2}\\d{2}$" })
                   .WithOnlyDiagnostics(TypeParameterName.S119)
                   .AddPaths("TypeParameterName_CustomRegex.vb")
                   .Verify();
    }
}
