/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

extern alias csharp;
using System;
using csharp::SonarAnalyzer.Rules.CSharp;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class MarkAssemblyWithNeutralResourcesLanguageAttributeTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void MarkAssemblyWithNeutralResourcesLanguageAttribute_WhenResxAndAttribute_DoesntThrow()
        {
            Verifier.VerifyAnalyzer(
                new[]
                {
                    @"TestCases\MarkAssemblyWithNeutralResourcesLanguageAttribute.cs",
                    @"ResourceTests\SomeResources.Designer.cs"
                },
                new MarkAssemblyWithNeutralResourcesLanguageAttribute());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void MarkAssemblyWithNeutralResourcesLanguageAttribute_WhenAttributeButNoResx_DoesntThrow()
        {
            Verifier.VerifyAnalyzer(
                @"TestCases\MarkAssemblyWithNeutralResourcesLanguageAttribute.cs",
                new MarkAssemblyWithNeutralResourcesLanguageAttribute());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void MarkAssemblyWithNeutralResourcesLanguageAttribute_WhenResxButNoAttribute_Throws()
        {
            Action action = () => Verifier.VerifyAnalyzer(
                 new[]
                {
                    @"TestCases\MarkAssemblyWithNeutralResourcesLanguageAttributeNonCompliant.cs",
                    @"ResourceTests\SomeResources.Designer.cs"
                }, new MarkAssemblyWithNeutralResourcesLanguageAttribute());
            action.Should().Throw<UnexpectedDiagnosticException>().WithMessage("Issue with message 'Mark this assembly with 'System.Resources.NeutralResourcesLanguageAttribute'.' not expected on line 1");
        }
    }
}
