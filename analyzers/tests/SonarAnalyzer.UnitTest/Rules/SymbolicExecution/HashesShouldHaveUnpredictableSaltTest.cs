/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.SymbolicExecution;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class HashesShouldHaveUnpredictableSaltTest
    {
        private static readonly DiagnosticDescriptor[] OnlyDiagnostics = new[] { HashesShouldHaveUnpredictableSalt.S2053 };

        [TestMethod]
        public void HashesShouldHaveUnpredictableSalt_CS() =>
            Verifier.VerifyAnalyzer(
                @"TestCases\SymbolicExecution\Sonar\HashesShouldHaveUnpredictableSalt.cs",
                new SymbolicExecutionRunner(),
                ParseOptionsHelper.FromCSharp8,
                MetadataReferenceFacade.SystemSecurityCryptography,
                onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        public void HashesShouldHaveUnpredictableSalt_DoesNotRaiseIssuesForTestProject() =>
            Verifier.VerifyNoIssueReportedInTest(
                @"TestCases\SymbolicExecution\Sonar\HashesShouldHaveUnpredictableSalt.cs",
                new SymbolicExecutionRunner(),
                ParseOptionsHelper.FromCSharp8,
                MetadataReferenceFacade.SystemSecurityCryptography,
                onlyDiagnostics: OnlyDiagnostics);

#if NET

        [TestMethod]
        public void HashesShouldHaveUnpredictableSalt_CSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(
                @"TestCases\SymbolicExecution\Sonar\HashesShouldHaveUnpredictableSalt.CSharp9.cs",
                new SymbolicExecutionRunner(),
                MetadataReferenceFacade.SystemSecurityCryptography,
                onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        public void HashesShouldHaveUnpredictableSalt_CSharp10() =>
            Verifier.VerifyAnalyzerFromCSharp10Library(
                @"TestCases\SymbolicExecution\Sonar\HashesShouldHaveUnpredictableSalt.CSharp10.cs",
                new SymbolicExecutionRunner(),
                MetadataReferenceFacade.SystemSecurityCryptography,
                onlyDiagnostics: OnlyDiagnostics);

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
