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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.SymbolicExecution.Sonar.Analyzers;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class HashesShouldHaveUnpredictableSaltTest
    {
        private readonly VerifierBuilder sonarVerifier = new VerifierBuilder<SymbolicExecutionRunner>().WithBasePath(@"SymbolicExecution\Sonar")
            .WithOnlyDiagnostics(HashesShouldHaveUnpredictableSalt.S2053)
            .AddReferences(MetadataReferenceFacade.SystemSecurityCryptography);

        [TestMethod]
        public void HashesShouldHaveUnpredictableSalt_CSharp8() =>
            sonarVerifier.AddPaths("HashesShouldHaveUnpredictableSalt.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

        [TestMethod]
        public void HashesShouldHaveUnpredictableSalt_DoesNotRaiseIssuesForTestProject() =>
            sonarVerifier.AddPaths("HashesShouldHaveUnpredictableSalt.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .AddTestReference()
                .VerifyNoIssueReported();

#if NET

        [TestMethod]
        public void HashesShouldHaveUnpredictableSalt_CSharp9() =>
            sonarVerifier.AddPaths("HashesShouldHaveUnpredictableSalt.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void HashesShouldHaveUnpredictableSalt_CSharp10() =>
            sonarVerifier.AddPaths("HashesShouldHaveUnpredictableSalt.CSharp10.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .Verify();

        [TestMethod]
        public void HashesShouldHaveUnpredictableSalt_CSharp11() =>
            sonarVerifier.AddPaths("HashesShouldHaveUnpredictableSalt.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

#endif

        [TestMethod]
        public void HashesShouldHaveUnpredictableSalt_LocationContext_Equals()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("public class Test {}");
            var location = Location.Create(tree, TextSpan.FromBounds(0, 6));

            var context1 = new HashesShouldHaveUnpredictableSalt.LocationContext(location, "message");
            var context2 = new HashesShouldHaveUnpredictableSalt.LocationContext(location, "2nd message");
            var context3 = new HashesShouldHaveUnpredictableSalt.LocationContext(null, "message");
            var context4 = new HashesShouldHaveUnpredictableSalt.LocationContext(location, "message");

            context1.Equals(null).Should().BeFalse();
            context1.Equals(new object()).Should().BeFalse();
            context1.Equals(context2).Should().BeFalse();
            context1.Equals(context3).Should().BeFalse();
            context1.Equals(context1).Should().BeTrue();
            context1.Equals(context4).Should().BeTrue();
        }
    }
}
