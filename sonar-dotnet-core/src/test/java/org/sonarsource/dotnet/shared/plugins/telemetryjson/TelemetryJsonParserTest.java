/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

import java.io.StringReader;
import java.util.Map;
import org.junit.Rule;
import org.junit.Test;
import org.slf4j.event.Level;
import org.sonar.api.testfixtures.log.LogTester;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.tuple;

public class TelemetryJsonParserTest {
  @Rule
  public LogTester logTester = new LogTester();

  @Test
  public void parseLineDelimitedKeyValuePairsWithDifferentTypes() {
    var sut = new TelemetryJsonParser();
    try (var reader = new StringReader("""
        { key1: "value1" }
        { key2: 2 }
        { key3: true }
        { "key4": 3.14 }
        { "key5": 1, "key6": 2 }
        { key1: Duplicate }
      """)) {
      var result = sut.parse(reader);
      assertThat(result).extracting(Map.Entry::getKey, Map.Entry::getValue).containsExactly(
        tuple("key1", "value1"),
        tuple("key2", "2"),
        tuple("key3", "true"),
        tuple("key4", "3.14"),
        tuple("key5", "1"),
        tuple("key6", "2"),
        tuple("key1", "Duplicate")
      );
      assertThat(logTester.getLogs()).isEmpty();
    }
  }

  @Test
  public void parsingArrayWritesDebug() {
    logTester.setLevel(Level.DEBUG);
    var sut = new TelemetryJsonParser();
    try (var reader = new StringReader("[{ key1: 42 }]")) {
      var result = sut.parse(reader);
      assertThat(result).isEmpty();
      assertThat(logTester.logs()).containsExactly("""
        Could not parse telemetry entry [{"key1":42}]""");
    }
  }

  @Test
  public void parsingComplexPropertyWritesDebug() {
    logTester.setLevel(Level.DEBUG);
    var sut = new TelemetryJsonParser();
    try (var reader = new StringReader("""
        { key1: "value1" }
        { key2: { nested: 42 } }
        { key3: true }
      """)) {
      var result = sut.parse(reader);
      assertThat(result).extracting(Map.Entry::getKey, Map.Entry::getValue).containsExactly(
        tuple("key1", "value1"),
        tuple("key3", "true"));
      assertThat(logTester.logs()).containsExactly("""
        Could not parse telemetry property {"key2":{"nested":42}}""");
    }
  }

  @Test
  public void parsingMalformedJsonProperty() {
    logTester.setLevel(Level.DEBUG);
    var sut = new TelemetryJsonParser();
    try (var reader = new StringReader("""
        { key1: 12 }
        { key2: # }
        { key3: "valid" }
      """)) {
      var result = sut.parse(reader);
      assertThat(result).extracting(Map.Entry::getKey, Map.Entry::getValue).containsExactly(
        tuple("key1", "12"));
      assertThat(logTester.logs()).containsExactly("Parsing of telemetry failed.");
    }
  }

  @Test
  public void parsingMalformedJson() {
    logTester.setLevel(Level.DEBUG);
    var sut = new TelemetryJsonParser();
    try (var reader = new StringReader("""
        { key1: 42 }
        { key2: 12
        { key3: -1 }
      """)) {
      var result = sut.parse(reader);
      assertThat(result).extracting(Map.Entry::getKey, Map.Entry::getValue).containsExactly(
        tuple("key1", "42"));
      assertThat(logTester.logs()).containsExactly("Parsing of telemetry failed.");
    }
  }

  @Test
  public void parsingEmptyJson() {
    logTester.setLevel(Level.DEBUG);
    var sut = new TelemetryJsonParser();
    try (var reader = new StringReader("")) {
      var result = sut.parse(reader);
      assertThat(result).extracting(Map.Entry::getKey, Map.Entry::getValue).isEmpty();
      assertThat(logTester.logs()).containsExactly("Telemetry is empty.");
    }
  }
}
