/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class EmptyMethodTest
    {
        private readonly VerifierBuilder verifierCS = new VerifierBuilder<CS.EmptyMethod>();
        private readonly VerifierBuilder verifierVB = new VerifierBuilder<VB.EmptyMethod>();

        [Ignore][TestMethod]
        public void EmptyMethod() =>
            verifierCS.AddPaths("EmptyMethod.cs").WithOptions(ParseOptionsHelper.FromCSharp8).AddReferences(MetadataReferenceFacade.NETStandard21).Verify();

#if NET

        [Ignore][TestMethod]
        public void EmptyMethod_CSharp10() =>
            verifierCS.AddPaths("EmptyMethod.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [Ignore][TestMethod]
        public void EmptyMethod_CSharp9() =>
            verifierCS.AddPaths("EmptyMethod.CSharp9.cs").WithTopLevelStatements().Verify();

#endif

        [Ignore][TestMethod]
        public void EmptyMethod_CodeFix_Throw() =>
            verifierCS.WithCodeFix<CS.EmptyMethodCodeFix>()
                .AddPaths("EmptyMethod.cs")
                .WithCodeFixedPaths("EmptyMethod.Throw.Fixed.cs")
                .WithCodeFixTitle(CS.EmptyMethodCodeFix.TitleThrow)
                .VerifyCodeFix();

        [Ignore][TestMethod]
        public void EmptyMethod_CodeFix_Comment() =>
            verifierCS.WithCodeFix<CS.EmptyMethodCodeFix>()
                .AddPaths("EmptyMethod.cs")
                .WithCodeFixedPaths("EmptyMethod.Comment.Fixed.cs")
                .WithCodeFixTitle(CS.EmptyMethodCodeFix.TitleComment)
                .VerifyCodeFix();

        [Ignore][TestMethod]
        public void EmptyMethod_VB() =>
            verifierVB.AddPaths("EmptyMethod.vb").Verify();

        [Ignore][TestMethod]
        public void EmptyMethod_WithVirtualOverride_RaisesIssueForMainProject_CS() =>
            verifierCS.AddPaths("EmptyMethod.OverrideVirtual.cs").Verify();

        [Ignore][TestMethod]
        public void EmptyMethod_WithVirtualOverride_DoesNotRaiseIssuesForTestProject_CS() =>
            verifierCS.AddPaths("EmptyMethod.OverrideVirtual.cs").AddTestReference().VerifyNoIssueReported();

        [Ignore][TestMethod]
        public void EmptyMethod_WithVirtualOverride_RaisesIssueForMainProject_VB() =>
            verifierVB.AddPaths("EmptyMethod.OverrideVirtual.vb").Verify();

        [Ignore][TestMethod]
        public void EmptyMethod_WithVirtualOverride_DoesNotRaiseIssuesForTestProject_VB() =>
            verifierVB.AddPaths("EmptyMethod.OverrideVirtual.vb").AddTestReference().VerifyNoIssueReported();
    }
}
