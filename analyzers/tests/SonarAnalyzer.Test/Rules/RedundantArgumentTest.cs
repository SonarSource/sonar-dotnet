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
    public class RedundantArgumentTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<RedundantArgument>();
        private readonly VerifierBuilder codeFixBuilder = new VerifierBuilder<RedundantArgument>().WithCodeFix<RedundantArgumentCodeFix>();

        [TestMethod]
        public void RedundantArgument_CSharp8() =>
            builder.AddPaths("RedundantArgument.cs")
                .AddReferences(MetadataReferenceFacade.NetStandard21)
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

#if NET

        [TestMethod]
        public void RedundantArgument_CSharp9() =>
            builder.AddPaths("RedundantArgument.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void RedundantArgument_CSharp12() =>
            builder.AddPaths("RedundantArgument.CSharp12.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp12)
                .Verify();

#endif

        [TestMethod]
        public void RedundantArgument_CodeFix_No_Named_Arguments() =>
            codeFixBuilder.AddPaths("RedundantArgument.cs")
                .WithCodeFixedPaths("RedundantArgument.NoNamed.Fixed.cs")
                .WithCodeFixTitle(RedundantArgumentCodeFix.TitleRemove)
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .VerifyCodeFix();

        [TestMethod]
        public void RedundantArgument_CodeFix_Named_Arguments() =>
            codeFixBuilder.AddPaths("RedundantArgument.cs")
                .WithCodeFixedPaths("RedundantArgument.Named.Fixed.cs")
                .WithCodeFixTitle(RedundantArgumentCodeFix.TitleRemoveWithNameAdditions)
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .VerifyCodeFix();
    }
}
