/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.Text;
using NSubstitute;
using static SonarAnalyzer.Core.Common.AnalyzerConfiguration;

namespace SonarAnalyzer.Core.Test.Common;

[TestClass]
public class AnalyzerConfigurationTest
{
    private const string FirstSonarLintFilePath = @"bar\SonarLint.xml";
    private const string FirstSonarLintFileContent = "fake SonarLintXml content 1";
    private const string FirstRuleId = "S0000";
    private const string SecondSonarLintFilePath = @"qix\SonarLint.xml";
    private const string SecondSonarLintFileContent = "fake SonarLintXml content 2";
    private const string SecondRuleId = "S9999";
    private IRuleLoader ruleLoader;

    [TestInitialize]
    public void Initialize()
    {
        ruleLoader = Substitute.For<IRuleLoader>();
        ruleLoader.GetEnabledRules(FirstSonarLintFileContent).Returns(new HashSet<string> { FirstRuleId });
        ruleLoader.GetEnabledRules(SecondSonarLintFileContent).Returns(new HashSet<string> { SecondRuleId });
    }

    [TestMethod]
    public void AlwaysEnabled_WhenNotInitialized_ReturnsTrue() =>
        AlwaysEnabled.IsEnabled("S101").Should().BeTrue();

    [TestMethod]
    public void AlwaysEnabled_AnyValue_ReturnsTrue()
    {
        AlwaysEnabled.IsEnabled(null).Should().BeTrue();
        AlwaysEnabled.IsEnabled(string.Empty).Should().BeTrue();
        AlwaysEnabled.IsEnabled("foo").Should().BeTrue();
    }

    [TestMethod]
    public void ForceSonarCfg_DisabledByDefault()
    {
        AlwaysEnabled.ForceSonarCfg.Should().BeFalse();
        new HotspotConfiguration(ruleLoader).ForceSonarCfg.Should().BeFalse();
    }

    [TestMethod]
    public void ForceSonarCfg_DisabledByDefault_ExistExceptionalConfig()
    {
        AlwaysEnabled.ForceSonarCfg.Should().BeFalse();
        new HotspotConfiguration(ruleLoader).ForceSonarCfg.Should().BeFalse();
        AlwaysEnabledWithSonarCfg.ForceSonarCfg.Should().BeTrue();
    }

    [TestMethod]
    public void AlwaysEnabled_IgnoresInitialize()
    {
        var sut = AlwaysEnabled;

        sut.Initialize(null);
        sut.IsEnabled(FirstRuleId).Should().BeTrue();
    }

    [TestMethod]
    public void HotspotConfiguration_WhenInitializeIsCalledWithDifferentSonarLintPaths_UpdatesEnabledRules()
    {
        var sut = new HotspotConfiguration(ruleLoader);

        // act
        Initialize(sut, FirstSonarLintFilePath, FirstSonarLintFileContent);

        // assert
        sut.IsEnabled(FirstRuleId).Should().BeTrue();
        sut.IsEnabled(SecondRuleId).Should().BeFalse();

        // act
        Initialize(sut, SecondSonarLintFilePath, SecondSonarLintFileContent);

        // assert
        sut.IsEnabled(FirstRuleId).Should().BeFalse();
        sut.IsEnabled(SecondRuleId).Should().BeTrue();

        ruleLoader.Received(1).GetEnabledRules(FirstSonarLintFileContent);
        ruleLoader.Received(1).GetEnabledRules(SecondSonarLintFileContent);
    }

    [TestMethod]
    public void HotspotConfiguration_WhenInitializedTwiceWithTheSameFile_DoesNotUpdateEnabledRules()
    {
        var sut = new HotspotConfiguration(ruleLoader);

        // act
        Initialize(sut, FirstSonarLintFilePath, FirstSonarLintFileContent);
        Initialize(sut, FirstSonarLintFilePath, FirstSonarLintFileContent);

        // assert
        ruleLoader.Received(1).GetEnabledRules(Arg.Any<string>());
        ruleLoader.Received(1).GetEnabledRules(FirstSonarLintFileContent);
    }

    [TestMethod]
    public void HotspotConfiguration_WhenInitializeIsSecondTimeWithNonSonarLint_DoesNotUpdateEnabledRules()
    {
        var sut = new HotspotConfiguration(ruleLoader);

        // act
        Initialize(sut, FirstSonarLintFilePath, FirstSonarLintFileContent);
        Initialize(sut, "Foo.xml", "fake SonarLintXml content");

        // assert
        ruleLoader.Received(1).GetEnabledRules(Arg.Any<string>());
        ruleLoader.Received(1).GetEnabledRules(FirstSonarLintFileContent);
    }

    [TestMethod]
    public void HotspotConfiguration_WhenIsEnabledWithoutInitialized_ThrowException()
    {
        var sut = new HotspotConfiguration(ruleLoader);
        sut.Invoking(x => x.IsEnabled(string.Empty)).Should().Throw<InvalidOperationException>().WithMessage("Call Initialize() before calling IsEnabled().");
    }

    [TestMethod]
    public void HotspotConfiguration_WhenIsInitializedWithNull_ThrowsException()
    {
        var sut = new HotspotConfiguration(ruleLoader);
        sut.Invoking(x => x.Initialize(null)).Should().Throw<NullReferenceException>();
    }

    [TestMethod]
    public void HotspotConfiguration_GivenDifferentFileName_WillNotFinishInitialization()
    {
        var sut = new HotspotConfiguration(ruleLoader);
        Initialize(sut, "FooBarSonarLint.xml", "fake SonarLintXml content");

        ruleLoader.DidNotReceive().GetEnabledRules(Arg.Any<string>());
    }

    private static void Initialize(HotspotConfiguration sut, string path, string content) =>
        sut.Initialize(new AnalyzerOptions(GetAdditionalFiles(path, content)));

    private static ImmutableArray<AdditionalText> GetAdditionalFiles(string path, string content)
    {
        var additionalText = Substitute.For<AdditionalText>();
        additionalText.Path.Returns(path);
        additionalText.GetText(default).Returns(SourceText.From(content));
        return [additionalText];
    }
}
