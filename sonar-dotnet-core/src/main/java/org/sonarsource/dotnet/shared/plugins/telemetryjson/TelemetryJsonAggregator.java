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

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.stream.Stream;

public class TelemetryJsonAggregator {
  private static final String TARGET_FRAMEWORK_MONIKER = "dotnetenterprise.s4net.build.target_framework_moniker";

  public Stream<Map.Entry<String, String>> flatMapTelemetry(Stream<Map.Entry<String, String>> telemetry) {
    final Map<String, Integer> countedEntries = new HashMap<>();
    final List<Map.Entry<String, String>> simpleEntries = new ArrayList<>();
    telemetry.forEach(entry ->
    {
      switch (entry.getKey()) {
        case TARGET_FRAMEWORK_MONIKER -> countValues(countedEntries, entry);
        default -> simpleEntries.add(entry);
      }
    });
    return Stream.concat(simpleEntries.stream(), countedEntries.entrySet().stream().map(x -> Map.entry(x.getKey(), Integer.toString(x.getValue()))));
  }

  private static void countValues(Map<String, Integer> countedEntries, Map.Entry<String, String> entry) {
    countedEntries.merge(entry.getKey() + "." + TelemetryUtils.sanitizeKey(entry.getValue()), 1, Integer::sum);
  }
}
