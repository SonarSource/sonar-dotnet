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
package org.sonarsource.dotnet.shared.plugins.sensors;

import java.io.File;
import java.nio.file.Paths;
import java.util.Arrays;
import java.util.List;
import java.util.Map;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.slf4j.event.Level;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.testfixtures.log.LogTester;
import org.sonarsource.dotnet.shared.plugins.ModuleConfiguration;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.telemetryjson.TelemetryJsonCollector;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.tuple;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class TelemetryJsonSensorTest {
  private static final String PLUGIN_KEY = "PLUGIN_KEY";
  private static final String LANG_KEY = "LANG_KEY";
  private static final String LANG_NAME = "LANG_NAME";
  private static final File TEST_DATA_DIR = new File("src/test/resources/TelemetryJsonSensorTest");

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Rule
  public LogTester logTester = new LogTester();

  private SensorContextTester context;
  private TelemetryJsonCollector collector;
  private TelemetryJsonSensor sensor;

  @Before
  public void prepare() {
    logTester.setLevel(Level.DEBUG);
    context = SensorContextTester.create(temp.getRoot());
    PluginMetadata metadata = mock(PluginMetadata.class);
    when(metadata.pluginKey()).thenReturn(PLUGIN_KEY);
    when(metadata.languageKey()).thenReturn(LANG_KEY);
    when(metadata.languageName()).thenReturn(LANG_NAME);
    ModuleConfiguration configuration = mock(ModuleConfiguration.class);
    when(configuration.telemetryJsonPaths()).thenReturn(
      Arrays.stream(TEST_DATA_DIR.toPath().toFile().listFiles(File::isDirectory))
        .flatMap(x -> Arrays.stream(x.toPath().toFile().listFiles()))
        .map(File::toPath).toList());
    collector = new TelemetryJsonCollector();
    sensor = new TelemetryJsonSensor(collector, metadata, configuration);
  }

  @Test
  public void should_describe() {
    DefaultSensorDescriptor sensorDescriptor = new DefaultSensorDescriptor();
    sensor.describe(sensorDescriptor);
    assertThat(sensorDescriptor.name()).isEqualTo(LANG_NAME + " Telemetry Json");
    assertThat(sensorDescriptor.languages()).containsExactlyInAnyOrder("LANG_KEY");
  }

  @Test
  public void execute_TelemetrySensor() {
    sensor.execute(context);
    assertThat(collector.getTelemetry()).extracting(Map.Entry::getKey, Map.Entry::getValue).containsExactlyInAnyOrder(
      tuple("key1", "12"),
      tuple("key1", "42"),
      tuple("key2", "Black"),
      tuple("key3", "true"),
      tuple("key4", "3.14"),
      tuple("key1", "-12"),
      tuple("key1", "-42"),
      tuple("key2", "White"),
      tuple("key3", "false"),
      tuple("key4", "1.41"),
      tuple("key6", "value for key6"),
      tuple("key7", "value for key7"));
    assertThat(logTester.logs()).containsExactly(
      """
        Could not parse telemetry property {"key5":[]}""");
  }

  @Test
  public void execute_TelemetrySensor_filesDontExist() {
    var testContext = SensorContextTester.create(temp.getRoot());
    var file = Paths.get(TEST_DATA_DIR.toString(), "nonexistentFile.json");
    var pluginMetadata = mock(PluginMetadata.class);
    when(pluginMetadata.pluginKey()).thenReturn(PLUGIN_KEY);
    when(pluginMetadata.languageKey()).thenReturn(LANG_KEY);
    when(pluginMetadata.languageName()).thenReturn(LANG_NAME);
    var configuration = mock(ModuleConfiguration.class);
    when(configuration.telemetryJsonPaths()).thenReturn(List.of(file));
    var telemetryJsonCollector = new TelemetryJsonCollector();
    var telemetryJsonSensor = new TelemetryJsonSensor(telemetryJsonCollector, pluginMetadata, configuration);
    telemetryJsonSensor.execute(testContext);
    assertThat(logTester.logs()).singleElement().isEqualTo(
      "Cannot open telemetry file " + file + ", java.io.FileNotFoundException: " + file + " (The system cannot find the file specified)");
  }
}
