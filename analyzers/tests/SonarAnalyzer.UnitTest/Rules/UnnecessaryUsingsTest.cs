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

using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class UnnecessaryUsingsTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<UnnecessaryUsings>()
            .AddReferences(MetadataReferenceFacade.MicrosoftWin32Primitives)
            .AddReferences(MetadataReferenceFacade.SystemSecurityCryptography);

        [TestMethod]
        public void UnnecessaryUsings() =>
            builder.AddPaths("UnnecessaryUsings.cs", "UnnecessaryUsings2.cs", "UnnecessaryUsingsFNRepro.cs").WithAutogenerateConcurrentFiles(false).Verify();

#if NET

        [TestMethod]
        public void UnnecessaryUsings_CSharp10_GlobalUsings() =>
            builder.AddPaths("UnnecessaryUsings.CSharp10.Global.cs", "UnnecessaryUsings.CSharp10.Consumer.cs").WithTopLevelStatements().WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [TestMethod]
        public void UnnecessaryUsings_CSharp10_FileScopedNamespace() =>
            builder.AddPaths("UnnecessaryUsings.CSharp10.FileScopedNamespace.cs").WithOptions(ParseOptionsHelper.FromCSharp10).WithConcurrentAnalysis(false).Verify();

        [TestMethod]
        public void UnnecessaryUsings_CSharp9() =>
            builder.AddPaths("UnnecessaryUsings.CSharp9.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void UnnecessaryUsings_TupleDeconstruct_NetCore() =>
            builder.AddPaths("UnnecessaryUsings.TupleDeconstruct.NetCore.cs").Verify();

#elif NETFRAMEWORK

        [TestMethod]
        public void UnnecessaryUsings_TupleDeconstruct_NetFx() =>
            builder.AddPaths("UnnecessaryUsings.TupleDeconstruct.NetFx.cs").Verify();

#endif

        [TestMethod]
        public void UnnecessaryUsings_CodeFix() =>
            builder.AddPaths("UnnecessaryUsings.cs")
                .WithCodeFix<UnnecessaryUsingsCodeFix>()
                .WithCodeFixedPath("UnnecessaryUsings.Fixed.cs")
                .WithCodeFixedPathBatch("UnnecessaryUsings.Fixed.Batch.cs")
                .VerifyCodeFix();

        [TestMethod]
        public void EquivalentNameSyntax_Equals_Object()
        {
            var main = new EquivalentNameSyntax(SyntaxFactory.IdentifierName("Lorem"));
            object same = new EquivalentNameSyntax(SyntaxFactory.IdentifierName("Lorem"));
            object different = new EquivalentNameSyntax(SyntaxFactory.IdentifierName("Ipsum"));

            main.Equals(same).Should().BeTrue();
            main.Equals(null).Should().BeFalse();
            main.Equals("different type").Should().BeFalse();
            main.Equals(different).Should().BeFalse();
        }

        [TestMethod]
        public void EquivalentNameSyntax_Equals_EquivalentNameSyntax()
        {
            var main = new EquivalentNameSyntax(SyntaxFactory.IdentifierName("Lorem"));
            var same = new EquivalentNameSyntax(SyntaxFactory.IdentifierName("Lorem"));
            var different = new EquivalentNameSyntax(SyntaxFactory.IdentifierName("Ipsum"));

            main.Equals(same).Should().BeTrue();
            main.Equals(null).Should().BeFalse();
            main.Equals(different).Should().BeFalse();
        }
    }
}
