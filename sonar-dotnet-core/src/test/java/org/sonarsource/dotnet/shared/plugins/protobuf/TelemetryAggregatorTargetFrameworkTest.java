/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import java.util.Arrays;
import java.util.Collection;
import java.util.List;
import java.util.Map;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.junit.runners.Parameterized;
import org.junit.runners.Parameterized.Parameters;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;

import static org.assertj.core.api.Assertions.assertThat;

@RunWith(Parameterized.class)
public class TelemetryAggregatorTargetFrameworkTest {
  private final String targetFramework;
  private final String expectedKey;

  @Parameters
  public static Collection<Object[]> targetFrameworkInputs() {
    // targetFrameworks(String targetFramework, String expectedKey)
    // https://learn.microsoft.com/en-us/dotnet/standard/frameworks#supported-target-frameworks
    return Arrays.asList(new Object[][]
      {
        { "netcoreapp1.0", "plugin_key.language_key.target_framework.netcoreapp1_0"},
        { "   ", "plugin_key.language_key.target_framework.___"},
        { "netcoreapp3.1", "plugin_key.language_key.target_framework.netcoreapp3_1"},
        { "net5.0", "plugin_key.language_key.target_framework.net5_0"},
        { "netstandard1.0", "plugin_key.language_key.target_framework.netstandard1_0"},
        { "NETSTANDARD2.1", "plugin_key.language_key.target_framework.netstandard2_1"},
        { "net11", "plugin_key.language_key.target_framework.net11"},
        { "net481", "plugin_key.language_key.target_framework.net481"},
        { "netcore [netcore45]", "plugin_key.language_key.target_framework.netcore__netcore45_"},
        { "netcore45 [win] [win8]", "plugin_key.language_key.target_framework.netcore45__win___win8_"},
        { "netcore451 [win81]", "plugin_key.language_key.target_framework.netcore451__win81_"},
        { "netmf", "plugin_key.language_key.target_framework.netmf"},
        { "sl4", "plugin_key.language_key.target_framework.sl4"},
        { "sl5", "plugin_key.language_key.target_framework.sl5"},
        { "wp [wp7]", "plugin_key.language_key.target_framework.wp__wp7_"},
        { "wp7", "plugin_key.language_key.target_framework.wp7"},
        { "wpa81", "plugin_key.language_key.target_framework.wpa81"},
        { "uap [uap10.0]", "plugin_key.language_key.target_framework.uap__uap10_0_"},
        { "uap10.0 [win10] [netcore50]", "plugin_key.language_key.target_framework.uap10_0__win10___netcore50_"},
        { "net5.0-windows", "plugin_key.language_key.target_framework.net5_0_windows"},
        { "net6.0-maccatalyst", "plugin_key.language_key.target_framework.net6_0_maccatalyst"},
        { "net7.0-macos", "plugin_key.language_key.target_framework.net7_0_macos"},
        { "net7.0-tizen", "plugin_key.language_key.target_framework.net7_0_tizen"},
        { "net7.0-tvos", "plugin_key.language_key.target_framework.net7_0_tvos"},
        { "net8.0-android", "plugin_key.language_key.target_framework.net8_0_android"},
        { "aspnet50", "plugin_key.language_key.target_framework.aspnet50"},
        { "dnx", "plugin_key.language_key.target_framework.dnx"},
        { "dnx452", "plugin_key.language_key.target_framework.dnx452"},
        { "dotnet", "plugin_key.language_key.target_framework.dotnet"},
        { "dotnet50", "plugin_key.language_key.target_framework.dotnet50"},
        { "netcore50", "plugin_key.language_key.target_framework.netcore50"},
        { "win", "plugin_key.language_key.target_framework.win"},
        { "win8", "plugin_key.language_key.target_framework.win8"},
        { "winrt", "plugin_key.language_key.target_framework.winrt"},
      });
  }

  public TelemetryAggregatorTargetFrameworkTest(String targetFramework, String expectedKey)
  {
    this.targetFramework = targetFramework;
    this.expectedKey = expectedKey;
  }

  @Test
  public void targetFrameworks_sanitize() {
    var sut = new TelemetryAggregator("plugin_key", "language_key");
    var telemetries = List.of(SonarAnalyzer.Telemetry.newBuilder().setProjectFullPath("A.csproj").addTargetFramework(targetFramework).build());
    var result = sut.aggregate(telemetries);
    assertThat(result).singleElement().extracting(Map.Entry::getKey).isEqualTo(expectedKey);
  }

  @Test
  public void targetFrameworks_empty() {
    var sut = new TelemetryAggregator("plugin_key", "language_key");
    var telemetries = List.of(SonarAnalyzer.Telemetry.newBuilder().setProjectFullPath("A.csproj").addTargetFramework("").build());
    var result = sut.aggregate(telemetries);
    assertThat(result).isEmpty();
  }
}
