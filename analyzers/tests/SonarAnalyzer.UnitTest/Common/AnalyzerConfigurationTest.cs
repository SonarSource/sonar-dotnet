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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.Common;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using static SonarAnalyzer.Common.AnalyzerConfiguration;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class AnalyzerConfigurationTest
    {
        private const string FirstSonarLintFilePath = @"bar\SonarLint.xml";
        private const string FirstSonarLintFileContent = "fake SonarLintXml content 1";
        private const string FirstRuleId = "S0000";
        private const string SecondSonarLintFilePath = @"qix\SonarLint.xml";
        private const string SecondSonarLintFileContent = "fake SonarLintXml content 2";
        private const string SecondRuleId = "S9999";
        private Mock<IRuleLoader> ruleLoaderMock;

        [TestInitialize]
        public void Initialize()
        {
            ruleLoaderMock = new Mock<IRuleLoader>(MockBehavior.Strict);
            ruleLoaderMock.Setup(r => r.GetEnabledRules(FirstSonarLintFileContent)).Returns(new HashSet<string> { FirstRuleId });
            ruleLoaderMock.Setup(r => r.GetEnabledRules(SecondSonarLintFileContent)).Returns(new HashSet<string> { SecondRuleId });
        }

        [TestMethod]
        public void AlwaysEnabled_WhenNotInitialized_ReturnsTrue()
        {
            var sut = AlwaysEnabled;
            IsTrue(sut.IsEnabled("S101"));
        }

        [TestMethod]
        public void AlwaysEnabled_AnyValue_ReturnsTrue()
        {
            var sut = AlwaysEnabled;
            IsTrue(sut.IsEnabled(null));
            IsTrue(sut.IsEnabled(""));
            IsTrue(sut.IsEnabled("foo"));
        }

        [TestMethod]
        public void AlwaysEnabled_IgnoresInitialize()
        {
            var sut = AlwaysEnabled;

            sut.Initialize(null);

            IsTrue(sut.IsEnabled(FirstRuleId));
        }

        [TestMethod]
        public void HotspotConfiguration_WhenInitializeIsCalledWithDifferentSonarLintPaths_UpdatesEnabledRules()
        {
            var sut = new HotspotConfiguration(ruleLoaderMock.Object);

            // act
            Initialize(sut, FirstSonarLintFilePath, FirstSonarLintFileContent);

            // assert
            IsTrue(sut.IsEnabled(FirstRuleId));
            IsFalse(sut.IsEnabled(SecondRuleId));

            // act
            Initialize(sut, SecondSonarLintFilePath, SecondSonarLintFileContent);

            // assert
            IsFalse(sut.IsEnabled(FirstRuleId));
            IsTrue(sut.IsEnabled(SecondRuleId));

            ruleLoaderMock.Verify(r => r.GetEnabledRules(FirstSonarLintFileContent), Times.Once);
            ruleLoaderMock.Verify(r => r.GetEnabledRules(SecondSonarLintFileContent), Times.Once);
        }

        [TestMethod]
        public void HotspotConfiguration_WhenInitializedTwiceWithTheSameFile_DoesNotUpdateEnabledRules()
        {
            var sut = new HotspotConfiguration(ruleLoaderMock.Object);

            // act
            Initialize(sut, FirstSonarLintFilePath, FirstSonarLintFileContent);
            Initialize(sut, FirstSonarLintFilePath, FirstSonarLintFileContent);

            // assert
            ruleLoaderMock.Verify(r => r.GetEnabledRules(It.IsAny<string>()), Times.Once);
            ruleLoaderMock.Verify(r => r.GetEnabledRules(FirstSonarLintFileContent), Times.Once);
        }

        [TestMethod]
        public void HotspotConfiguration_WhenInitializeIsSecondTimeWithNonSonarLint_DoesNotUpdateEnabledRules()
        {
            var sut = new HotspotConfiguration(ruleLoaderMock.Object);

            // act
            Initialize(sut, FirstSonarLintFilePath, FirstSonarLintFileContent);
            Initialize(sut, "Foo.xml", "fake SonarLintXml content");

            // assert
            ruleLoaderMock.Verify(r => r.GetEnabledRules(It.IsAny<string>()), Times.Once);
            ruleLoaderMock.Verify(r => r.GetEnabledRules(FirstSonarLintFileContent), Times.Once);
        }

        [TestMethod]
        public void HotspotConfiguration_WhenIsEnabledWithoutInitialized_ThrowException()
        {
            var sut = new HotspotConfiguration(ruleLoaderMock.Object);
            sut.Invoking(x => x.IsEnabled("")).Should().Throw<InvalidOperationException>().WithMessage("Call Initialize() before calling IsEnabled().");
        }

        [TestMethod]
        public void HotspotConfiguration_WhenIsInitializedWithNull_ThrowsException()
        {
            var sut = new HotspotConfiguration(ruleLoaderMock.Object);
            sut.Invoking(x => x.Initialize(null)).Should().Throw<NullReferenceException>();
        }

        [TestMethod]
        public void HotspotConfiguration_GivenDifferentFileName_WillNotFinishInitialization()
        {
            var sut = new HotspotConfiguration(ruleLoaderMock.Object);

            Initialize(sut, "FooBarSonarLint.xml", "fake SonarLintXml content");

            ruleLoaderMock.Verify(r => r.GetEnabledRules(It.IsAny<string>()), Times.Never);
        }

        private static void Initialize(HotspotConfiguration sut, string path, string content) =>
            sut.Initialize(new AnalyzerOptions(GetAdditionalFiles(path, content)));

        private static ImmutableArray<AdditionalText> GetAdditionalFiles(string path, string content)
        {
            var additionalText = new Mock<AdditionalText>();
            additionalText.Setup(x => x.Path).Returns(path);
            additionalText
                .Setup(x => x.GetText(default))
                .Returns(SourceText.From(content));

            return ImmutableArray.Create(additionalText.Object);
        }
    }
}
