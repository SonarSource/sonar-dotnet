/*
 * SonarSource :: .NET :: Core
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
import java.util.stream.Collectors;
import java.util.stream.Stream;

public class TelemetryJsonAggregator {
  private static final String TARGET_FRAMEWORK_MONIKER = "dotnetenterprise.s4net.build.target_framework_moniker";
  private static final String CNT_SUFFIX = ".cnt";

  public Stream<Map.Entry<String, String>> flatMapTelemetry(Stream<Map.Entry<String, String>> telemetry) {
    var partitioned = telemetry.collect(Collectors.partitioningBy(entry ->
      // ".cnt" suffix is an indicator, that the telemetry is supposed to be aggregated by counting the number of occurrences
      entry.getKey().endsWith(CNT_SUFFIX)
        // we can not rename "dotnetenterprise.s4net.build.target_framework_moniker" to follow the convention because it was already released
        || entry.getKey().equals(TARGET_FRAMEWORK_MONIKER)));

    var passThrough = partitioned.get(false).stream();

    var aggregated = partitioned.get(true).stream()
      .collect(Collectors.groupingBy(
        entry -> stripCntSuffix(entry.getKey()) + "." + TelemetryUtils.sanitizeKey(entry.getValue()),
        Collectors.counting()))
      .entrySet().stream()
      .map(entry -> Map.entry(entry.getKey(), Long.toString(entry.getValue())));

    return Stream.concat(passThrough, aggregated);
  }

  private static String stripCntSuffix(String key) {
    if (key.endsWith(CNT_SUFFIX)) {
      return key.substring(0, key.length() - CNT_SUFFIX.length());
    }
    return key;
  }
}
