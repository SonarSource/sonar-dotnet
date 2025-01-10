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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import java.util.AbstractMap;
import java.util.Arrays;
import java.util.Collection;
import java.util.Locale;
import java.util.Map;
import java.util.regex.Pattern;
import java.util.stream.Stream;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;

import static java.util.function.Function.identity;
import static java.util.stream.Collectors.counting;
import static java.util.stream.Collectors.groupingBy;

public class TelemetryAggregator {
  public final String languageVersion;
  public final String targetFramework;
  // Replace any non-word-character and non-digit with "_"
  private final Pattern sanitizeKeyPattern = Pattern.compile("[^a-zA-Z0-9]");

  public TelemetryAggregator(String pluginKey, String language) {

    var pluginLanguageKey = pluginKey + "." + language + ".";
    this.languageVersion = pluginLanguageKey + "language_version.%s";
    this.targetFramework = pluginLanguageKey + "target_framework.%s";
  }

  /**
   * Returns a string based key-value-pair from the given key and value.
   */
  private static <V> Map.Entry<String, String> kvp(String key, V value) {
    return new AbstractMap.SimpleImmutableEntry<>(key, value.toString());
  }

  private String key(String pattern, String... keys) {
    // Sanitize any values to comply with the requirement for telemetry keys:
    // https://discuss.sonarsource.com/t/20103/8
    var formattedKeys = sanitizeKeys(keys);
    return pattern.formatted((Object[]) formattedKeys);
  }

  private String[] sanitizeKeys(String[] keys) {
    return Arrays.stream(keys).map(this::sanitizeKey).toArray(String[]::new);
  }

  private String sanitizeKey(String x) {
    return sanitizeKeyPattern.matcher(x).replaceAll("_").toLowerCase(Locale.ROOT);
  }

  private Stream<Map.Entry<String, String>> languageVersion(Stream<String> languageVersions) {
    var countBy = languageVersions.filter(x -> !x.isEmpty()).collect(groupingBy(identity(), counting()));
    return countBy.entrySet().stream().map(x -> kvp(
      key(languageVersion, x.getKey()),
      x.getValue()));
  }

  private Stream<Map.Entry<String, String>> targetFrameworks(Stream<Collection<String>> targetFrameworks) {
    var countBy = targetFrameworks
      .flatMap(Collection::stream)
      .filter(x -> !x.isEmpty())
      .collect(groupingBy(identity(), counting()));
    return countBy.entrySet().stream().map(x -> kvp(
      key(targetFramework, x.getKey()),
      x.getValue()));
  }

  public Collection<Map.Entry<String, String>> aggregate(Collection<SonarAnalyzer.Telemetry> telemetries) {
    return Stream.of(
      languageVersion(telemetries.stream().map(SonarAnalyzer.Telemetry::getLanguageVersion)),
      targetFrameworks(telemetries.stream().map(SonarAnalyzer.Telemetry::getTargetFrameworkList)))
      .flatMap(identity())
      .toList();
  }
}
