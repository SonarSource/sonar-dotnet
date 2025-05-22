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
package org.sonarsource.dotnet.shared.plugins.sensors;

import java.io.File;
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
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.config.Configuration;
import org.sonar.api.testfixtures.log.LogTester;
import org.sonarsource.dotnet.shared.plugins.AbstractLanguageConfiguration;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.sensors.TelemetryJsonProjectCollector.Impl;
import org.sonarsource.dotnet.shared.plugins.telemetryjson.TelemetryJsonCollector;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.tuple;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;
import static org.mockito.Mockito.withSettings;

public class TelemetryJsonProjectSensorTest {
  private static final String LANG_NAME = "LANG_NAME";
  private static final File TEST_DATA_DIR = new File("src/test/resources/TelemetryJsonSensorTest");

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Rule
  public LogTester logTester = new LogTester();

  private SensorContextTester context;
  private TelemetryJsonCollector collector;
  private TelemetryJsonProjectCollector sensor;

  @Before
  public void prepare() {
    logTester.setLevel(Level.DEBUG);
    context = SensorContextTester.create(temp.getRoot());
    PluginMetadata metadata = mock(PluginMetadata.class);
    when(metadata.languageName()).thenReturn(LANG_NAME);
    Configuration configuration = mock(Configuration.class);
    when(configuration.get("sonar.working.directory")).thenReturn(Optional.of(TEST_DATA_DIR.toPath().resolve(".sonar").toString()));
    var languageConfiguration = mock(AbstractLanguageConfiguration.class, withSettings()
      .useConstructor(configuration, metadata)
      .defaultAnswer(Mockito.CALLS_REAL_METHODS));
    collector = new TelemetryJsonCollector();
    sensor = new Impl(collector, languageConfiguration);
  }

  @Test
  public void executeTelemetrySensor() {
    sensor.execute(context);
    assertThat(collector.getTelemetry()).extracting(Map.Entry::getKey, Map.Entry::getValue).containsExactlyInAnyOrder(
      tuple("Other.key1", "value1"),
      tuple("S4NET.key1", "1"),
      tuple("S4NET.key2", "Value2"));
    assertThat(logTester.logs()).containsExactly(
      "Searching for telemetry json in " + TEST_DATA_DIR,
      "Parsing of telemetry failed.");
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
    sensor = new Impl(collector, languageConfiguration);
    sensor.execute(context);
    assertThat(collector.getTelemetry()).isEmpty();
    assertThat(logTester.logs()).containsExactly(
      "Searching for telemetry json in nonexistentPath",
      "Error occurred while loading telemetry json");
  }

  @Test
  public void executeTelemetrySensorFileCanNotBeOpened_printsDebugMessage() {
    try (var mocked = Mockito.mockStatic(Files.class)) {
      mocked.when(() -> Files.find(eq(TEST_DATA_DIR.toPath()), eq(1), any())).thenReturn(Stream.of(
        TEST_DATA_DIR.toPath().resolve("NonexistentFile.json"),
        TEST_DATA_DIR.toPath().resolve("Telemetry.S4NET.json")));
      sensor.execute(context);
      assertThat(collector.getTelemetry()).extracting(Map.Entry::getKey, Map.Entry::getValue).containsExactlyInAnyOrder(
        tuple("S4NET.key1", "1"),
        tuple("S4NET.key2", "Value2"));
      var nonexistentFile = TEST_DATA_DIR.toPath().resolve("NonexistentFile.json");
      assertThat(logTester.logs()).containsExactly(
        "Searching for telemetry json in " + TEST_DATA_DIR,
        "Cannot open telemetry file " + nonexistentFile + ", java.io.FileNotFoundException: " + nonexistentFile + " (The system cannot find the file specified)");
    }
  }
}
