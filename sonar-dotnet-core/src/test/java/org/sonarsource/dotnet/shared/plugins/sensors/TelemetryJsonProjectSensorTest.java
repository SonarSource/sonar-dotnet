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
package org.sonarsource.dotnet.shared.plugins.sensors;

import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.util.Map;
import java.util.Optional;
import java.util.stream.Stream;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.mockito.Mockito;
import org.slf4j.event.Level;
import org.sonar.api.config.Configuration;
import org.sonar.api.testfixtures.log.LogTester;
import org.sonarsource.dotnet.shared.plugins.AbstractLanguageConfiguration;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.telemetryjson.TelemetryJsonCollector;
import org.sonarsource.dotnet.shared.plugins.testutils.FileUtils;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.tuple;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.when;
import static org.mockito.Mockito.withSettings;

public class TelemetryJsonProjectSensorTest {
  private static final String LANG_NAME = "LANG_NAME";
  private static final File TEST_DATA_DIR = new File("src/test/resources/TelemetryJsonSensorTest");

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Rule
  public LogTester logTester = new LogTester();

  private TelemetryJsonCollector collector;
  private TelemetryJsonProjectCollector sensor;

  @Before
  public void prepare() throws IOException {
    logTester.setLevel(Level.DEBUG);
    FileUtils.copyDirectory(TEST_DATA_DIR.toPath(), temp.getRoot().toPath());
    PluginMetadata metadata = mock(PluginMetadata.class);
    when(metadata.languageName()).thenReturn(LANG_NAME);
    Configuration configuration = mock(Configuration.class);
    when(configuration.get("sonar.working.directory")).thenReturn(Optional.of(temp.getRoot().toPath().resolve(".sonar").toString()));
    var languageConfiguration = mock(AbstractLanguageConfiguration.class, withSettings()
      .useConstructor(configuration, metadata)
      .defaultAnswer(Mockito.CALLS_REAL_METHODS));
    collector = new TelemetryJsonCollector();
    sensor = new TelemetryJsonProjectCollector(collector, languageConfiguration);
  }

  @Test
  public void executeTelemetrySensor() {
    sensor.execute();
    assertThat(collector.getTelemetry()).extracting(Map.Entry::getKey, Map.Entry::getValue).containsExactlyInAnyOrder(
      tuple("Other.key1", "value1"),
      tuple("S4NET.key1", "1"),
      tuple("S4NET.key2", "Value2"));
    assertThat(logTester.logs()).containsExactly(
      "Searching for telemetry json in " + temp.getRoot(),
      "Parsing of telemetry failed.");
    assertThat(temp.getRoot().listFiles(File::isFile)).as("Files are marked as processed.").extracting(File::getName)
      .containsExactlyInAnyOrder("Processed.Telemetry.Other.json", "Processed.Telemetry.S4NET.json");
  }

  @Test
  public void executeTelemetrySensorNonExistingOutputDir_printsDebugMessage() {
    var metadata = mock(PluginMetadata.class);
    var configuration = mock(Configuration.class);
    when(configuration.get("sonar.working.directory")).thenReturn(Optional.of("nonexistentPath/.sonar"));
    var languageConfiguration = mock(AbstractLanguageConfiguration.class, withSettings()
      .useConstructor(configuration, metadata)
      .defaultAnswer(Mockito.CALLS_REAL_METHODS));
    collector = new TelemetryJsonCollector();
    sensor = new TelemetryJsonProjectCollector(collector, languageConfiguration);
    sensor.execute();
    assertThat(collector.getTelemetry()).isEmpty();
    assertThat(logTester.logs()).containsExactly(
      "Searching for telemetry json in nonexistentPath",
      "Error occurred while loading telemetry json");
  }

  @Test
  public void executeTelemetrySensorFileCanNotBeOpened() {
    try (var mocked = Mockito.mockStatic(Files.class, Mockito.CALLS_REAL_METHODS)) {
      mocked.when(() -> Files.find(eq(temp.getRoot().toPath()), eq(1), any())).thenReturn(Stream.of(
        temp.getRoot().toPath().resolve("NonexistentFile.json"),
        temp.getRoot().toPath().resolve("Telemetry.S4NET.json")));
      sensor.execute();
      assertThat(collector.getTelemetry()).extracting(Map.Entry::getKey, Map.Entry::getValue).containsExactlyInAnyOrder(
        tuple("S4NET.key1", "1"),
        tuple("S4NET.key2", "Value2"));
      assertThat(logTester.logs()).containsExactly(
        "Searching for telemetry json in " + temp.getRoot());
      assertThat(temp.getRoot().listFiles(File::isFile)).as("Found files are marked as processed.").extracting(File::getName)
        .containsExactlyInAnyOrder("Telemetry.Other.json", "Processed.Telemetry.S4NET.json");
    }
  }

  @Test
  public void executeTelemetrySensorRenamedFileCanNotBeOpened_printsDebugMessage() {
    try (var mocked = Mockito.mockStatic(Files.class)) {
      var root = temp.getRoot().toPath();
      var telemetryJson = root.resolve("Telemetry.S4NET.json");
      var renamedTelemetryJson = root.resolve("Processed.Telemetry.S4NET.json");
      var nonexistingTelemetryJson = root.resolve("Non-existing.json");
      // find returns Telemetry.S4NET.json
      mocked.when(() -> Files.find(eq(root), eq(1), any())).thenReturn(Stream.of(telemetryJson));
      // move returns nonexistingTelemetryJson, which causes the FileInputStream to fail
      mocked.when(() -> Files.move(eq(telemetryJson), eq(renamedTelemetryJson))).thenReturn(nonexistingTelemetryJson);
      sensor.execute();
      assertThat(collector.getTelemetry()).isEmpty();
      assertThat(logTester.logs()).containsExactly(
        "Searching for telemetry json in " + temp.getRoot(),
        "Cannot open telemetry file " + telemetryJson + ", java.io.FileNotFoundException: " + nonexistingTelemetryJson + " (The system cannot find the file specified)");
    }
  }

  @Test
  public void executeTelemetrySensor_markedFilesAreIgnoredOnSecondRun() {
    try (var mocked = Mockito.mockStatic(Files.class, Mockito.CALLS_REAL_METHODS)) {
      sensor.execute();
      mocked.verify(() -> Files.find(eq(temp.getRoot().toPath()), eq(1), any()), times(1));
      mocked.verify(() -> Files.move(any(), any()), times(2).description("Two files are marked as processed."));
    }
    assertThat(collector.getTelemetry()).extracting(Map.Entry::getKey, Map.Entry::getValue).containsExactlyInAnyOrder(
      tuple("Other.key1", "value1"),
      tuple("S4NET.key1", "1"),
      tuple("S4NET.key2", "Value2"));
    assertThat(logTester.logs()).containsExactly(
      "Searching for telemetry json in " + temp.getRoot(),
      "Parsing of telemetry failed.");
    assertThat(temp.getRoot().listFiles(File::isFile)).as("Files are marked as processed.").extracting(File::getName)
      .containsExactlyInAnyOrder("Processed.Telemetry.Other.json", "Processed.Telemetry.S4NET.json");
    // Second execution. This simulates a second plugin trying to collect the Telemetry.*.json files from the root
    logTester.clear();
    try (var mocked = Mockito.mockStatic(Files.class, Mockito.CALLS_REAL_METHODS)) {
      sensor.execute();
      mocked.verify(() -> Files.find(eq(temp.getRoot().toPath()), eq(1), any()), times(1));
      mocked.verify(() -> Files.move(any(), any()), never().description("No files are marked as processed, because 'find' returned no files"));
    }
    assertThat(logTester.logs()).containsExactly(
      "Searching for telemetry json in " + temp.getRoot());
    assertThat(temp.getRoot().listFiles(File::isFile)).as("The two marked files are still marked as processed and unchanged.").extracting(File::getName)
      .containsExactlyInAnyOrder("Processed.Telemetry.Other.json", "Processed.Telemetry.S4NET.json");
  }
}
