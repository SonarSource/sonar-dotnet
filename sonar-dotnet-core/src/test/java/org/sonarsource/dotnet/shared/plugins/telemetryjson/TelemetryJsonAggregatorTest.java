/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource SA
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

  @Test
  public void aggregatePassesThroughUnknownTelemetry()
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
  public void aggregate_MixedFrameworkMonikersGetGroupedIntoASingleEntryForEachGroup()
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
      Map.entry(TARGET_FRAMEWORK_MONIKER, ".NETCoreApp,Version=v9.0")));
    assertThat(result).containsExactly(
      Map.entry("key1", "value1"),
      Map.entry("key2", "value2"),
      Map.entry(TARGET_FRAMEWORK_MONIKER + "._netstandard_version_v2_0", "1"),
      Map.entry(TARGET_FRAMEWORK_MONIKER + "._netframework_version_v4_7_2", "2"),
      Map.entry(TARGET_FRAMEWORK_MONIKER + "._netstandard_version_v1_6", "2"),
      Map.entry(TARGET_FRAMEWORK_MONIKER + "._netcoreapp_version_v9_0", "3"));
  }
}
