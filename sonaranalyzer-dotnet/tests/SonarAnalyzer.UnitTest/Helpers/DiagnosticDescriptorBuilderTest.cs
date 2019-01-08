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
extern alias vbnet;

using System;
using System.Resources;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class DiagnosticDescriptorBuilderTest
    {
        private const string LanguageValue = "language";

        [TestMethod]
        public void GetHelpLink_CSharp()
        {
            var helpLink = DiagnosticDescriptorBuilder
                .GetHelpLink(csharp.SonarAnalyzer.RspecStrings.ResourceManager, "S1234");
            helpLink.Should().Be("https://rules.sonarsource.com/csharp/RSPEC-1234");
        }

        [TestMethod]
        public void GetHelpLink_VisualBasic()
        {
            var helpLink = DiagnosticDescriptorBuilder
                .GetHelpLink(vbnet.SonarAnalyzer.RspecStrings.ResourceManager, "S1234");
            helpLink.Should().Be("https://rules.sonarsource.com/vbnet/RSPEC-1234");
        }

        [TestMethod]
        public void GetDescriptor_SetsIsEnabledByDefaultToGivenValue()
        {
            // Arrange
            var diagnosticId = "FooBar";
            var mockedResourceManager = CreateMockedResourceManager(diagnosticId, isActivatedByDefault: true);

            DiagnosticDescriptorBuilder.GetDescriptor(diagnosticId, "", mockedResourceManager, true)
                .IsEnabledByDefault
                .Should()
                .BeTrue();

            DiagnosticDescriptorBuilder.GetDescriptor(diagnosticId, "", mockedResourceManager, false)
                .IsEnabledByDefault
                .Should()
                .BeFalse();

            DiagnosticDescriptorBuilder.GetDescriptor(diagnosticId, "", mockedResourceManager)
                .IsEnabledByDefault
                .Should()
                .BeTrue();
        }

        [TestMethod]
        public void GetDescriptor_WhenIsActivatedByDefaultAndIdeVisibilityNotHidden_HasOnlySonarWayAndLanguageTags()
        {
            // Arrange
            var diagnosticId = "foo";
            var mockedResourceManager = CreateMockedResourceManager(diagnosticId, true);

            // Act
            var result = DiagnosticDescriptorBuilder.GetDescriptor(diagnosticId, "", mockedResourceManager);

            // Assert
            result.CustomTags.Should().OnlyContain(DiagnosticDescriptorBuilder.SonarWayTag, LanguageValue);
        }

        [TestMethod]
        public void GetDescriptor_WhenIsNotActivatedByDefaultAndIdeVisibilityNotHidden_ContainsOnlyLanguage()
        {
            // Arrange
            var diagnosticId = "foo";
            var mockedResourceManager = CreateMockedResourceManager(diagnosticId, false);

            // Act
            var result = DiagnosticDescriptorBuilder.GetDescriptor(diagnosticId, "", mockedResourceManager);

            // Assert
            result.CustomTags.Should().OnlyContain(LanguageValue);
        }

        [TestMethod]
        public void GetUtilityDescriptor_Should_Contain_NotConfigurable_CustomTag()
        {
            // Arrange
            // Act
            var result = DiagnosticDescriptorBuilder.GetUtilityDescriptor("Foo", "");

            // Assert
#if DEBUG
            result.CustomTags.Should().NotContain(WellKnownDiagnosticTags.NotConfigurable);
#else
            result.CustomTags.Should().Contain(WellKnownDiagnosticTags.NotConfigurable);
#endif
        }

        [TestMethod]
        public void GetUtilityDescriptor_Should_Contain_NotConfigurable_CustomTag_SourceScope_Specified()
        {
            foreach (SourceScope sourceScope in Enum.GetValues(typeof(SourceScope)))
            {
                // Arrange
                // Act
                var result = DiagnosticDescriptorBuilder.GetUtilityDescriptor("Foo", "", sourceScope);

                // Assert
#if DEBUG
                result.CustomTags.Should().NotContain(WellKnownDiagnosticTags.NotConfigurable);
#else
                result.CustomTags.Should().Contain(WellKnownDiagnosticTags.NotConfigurable);
#endif
            }
        }

        private ResourceManager CreateMockedResourceManager(string diagnosticId, bool isActivatedByDefault)
        {
            var mockedResourceManager = new Mock<ResourceManager>();
            mockedResourceManager.Setup(x => x.GetString("HelpLinkFormat")).Returns("bar");
            mockedResourceManager.Setup(x => x.GetString("RoslynLanguage")).Returns(LanguageValue);

            mockedResourceManager.Setup(x => x.GetString($"{diagnosticId}_Title")).Returns("title");
            mockedResourceManager.Setup(x => x.GetString($"{diagnosticId}_Category")).Returns("category");
            mockedResourceManager.Setup(x => x.GetString($"{diagnosticId}_Description")).Returns("description");
            mockedResourceManager.Setup(x => x.GetString($"{diagnosticId}_IsActivatedByDefault")).Returns(isActivatedByDefault.ToString());

            return mockedResourceManager.Object;
        }
    }
}
