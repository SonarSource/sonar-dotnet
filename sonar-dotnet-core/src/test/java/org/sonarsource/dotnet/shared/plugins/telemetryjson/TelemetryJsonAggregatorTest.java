/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
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
package org.sonarsource.dotnet.shared.plugins.telemetryjson;

import java.util.Map;
import java.util.stream.Stream;
import org.junit.Test;

import static org.assertj.core.api.Assertions.assertThat;

public class TelemetryJsonAggregatorTest {

  private static final String TARGET_FRAMEWORK_MONIKER = "dotnetenterprise.s4net.build.target_framework_moniker";
  private static final String USING_MICROSOFT_NET_SDK = "dotnetenterprise.cnt.s4net.build.using_microsoft_net_sdk";
  private static final String DETERMINISTIC = "dotnetenterprise.cnt.s4net.build.deterministic";
  private static final String NUGET_PROJECT_STYLE = "dotnetenterprise.cnt.s4net.build.nuget_project_style";

  @Test
  public void flatMapTelemetry_PassesThroughUnknownTelemetry()
  {
    var sut = new TelemetryJsonAggregator();
    var result = sut.flatMapTelemetry(Stream.of(
      Map.entry("someKey", "someValue"),
      Map.entry("someKey", "someValue")));
    assertThat(result).containsExactly(
      Map.entry("someKey", "someValue"),
      Map.entry("someKey", "someValue"));
  }

  @Test
  public void flatMapTelemetry_AllSpecialKeysGetCounted() {
    var sut = new TelemetryJsonAggregator();
    var result = sut.flatMapTelemetry(Stream.of(
      Map.entry(TARGET_FRAMEWORK_MONIKER, ".NETCoreApp,Version=v9.0"),
      Map.entry(USING_MICROSOFT_NET_SDK, "true"),
      Map.entry(DETERMINISTIC, "false"),
      Map.entry(NUGET_PROJECT_STYLE, "PackageReference")));
    assertThat(result).containsExactlyInAnyOrder(
      Map.entry(TARGET_FRAMEWORK_MONIKER + "._netcoreapp_version_v9_0", "1"),
      Map.entry(USING_MICROSOFT_NET_SDK + ".true", "1"),
      Map.entry(DETERMINISTIC + ".false", "1"),
      Map.entry(NUGET_PROJECT_STYLE + ".packagereference", "1"));
  }

  @Test
  public void flatMapTelemetry_MixedAggregatedAndPassThroughKeys()
  {
    var sut = new TelemetryJsonAggregator();
    var result = sut.flatMapTelemetry(Stream.of(
      Map.entry(TARGET_FRAMEWORK_MONIKER, ".NETCoreApp,Version=v9.0"),
      Map.entry(TARGET_FRAMEWORK_MONIKER, ".NETStandard,Version=v1.6"),
      Map.entry("key1", "value1"),
      Map.entry(TARGET_FRAMEWORK_MONIKER, ".NETStandard,Version=v2.0"),
      Map.entry(TARGET_FRAMEWORK_MONIKER, ".NETFramework,Version=v4.7.2"),
      Map.entry(TARGET_FRAMEWORK_MONIKER, ".NETCoreApp,Version=v9.0"),
      Map.entry(TARGET_FRAMEWORK_MONIKER, ".NETFramework,Version=v4.7.2"),
      Map.entry("key2", "value2"),
      Map.entry(TARGET_FRAMEWORK_MONIKER, ".NETStandard,Version=v1.6"),
      Map.entry(TARGET_FRAMEWORK_MONIKER, ".NETCoreApp,Version=v9.0"),
      Map.entry(USING_MICROSOFT_NET_SDK, "true"),
      Map.entry(USING_MICROSOFT_NET_SDK, "true"),
      Map.entry(DETERMINISTIC, "false"),
      Map.entry(NUGET_PROJECT_STYLE, "PackageReference")));
    assertThat(result).containsExactlyInAnyOrder(
      Map.entry("key1", "value1"),
      Map.entry("key2", "value2"),
      Map.entry(TARGET_FRAMEWORK_MONIKER + "._netstandard_version_v2_0", "1"),
      Map.entry(TARGET_FRAMEWORK_MONIKER + "._netframework_version_v4_7_2", "2"),
      Map.entry(TARGET_FRAMEWORK_MONIKER + "._netstandard_version_v1_6", "2"),
      Map.entry(TARGET_FRAMEWORK_MONIKER + "._netcoreapp_version_v9_0", "3"),
      Map.entry(USING_MICROSOFT_NET_SDK + ".true", "2"),
      Map.entry(DETERMINISTIC + ".false", "1"),
      Map.entry(NUGET_PROJECT_STYLE + ".packagereference", "1"));
  }
}
