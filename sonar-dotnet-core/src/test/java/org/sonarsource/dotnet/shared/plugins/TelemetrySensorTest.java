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
package org.sonarsource.dotnet.shared.plugins;

import java.io.File;
import java.util.ArrayList;
import java.util.Arrays;
import org.apache.commons.lang3.tuple.Pair;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.mockito.stubbing.Answer;
import org.slf4j.event.Level;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.testfixtures.log.LogTester;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.doAnswer;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.spy;
import static org.mockito.Mockito.when;

public class TelemetrySensorTest {
  private static final String PLUGIN_KEY = "PLUGIN_KEY";
  private static final String LANG_KEY = "LANG_KEY";
  private static final String LANG_NAME = "LANG_NAME";
  // see src/test/resources/TelemetrySensorTest/README.md for explanation.
  private static final File TEST_DATA_DIR = new File("src/test/resources/TelemetrySensorTest");

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();
  @Rule
  public LogTester logTester = new LogTester();

  private SensorContextTester context;
  private TelemetrySensor sensor;

  @Before
  public void prepare() {
    logTester.setLevel(Level.DEBUG);
    context = spy(SensorContextTester.create(temp.getRoot()));
    PluginMetadata metadata = mock(PluginMetadata.class);
    when(metadata.pluginKey()).thenReturn(PLUGIN_KEY);
    when(metadata.languageKey()).thenReturn(LANG_KEY);
    when(metadata.languageName()).thenReturn(LANG_NAME);
    ModuleConfiguration configuration = mock(ModuleConfiguration.class);
    when(configuration.protobufReportPaths()).thenReturn(
      Arrays.stream(TEST_DATA_DIR.toPath().toFile().listFiles(File::isDirectory)).map(File::toPath).toList());
    sensor = new TelemetrySensor(metadata, configuration);
  }

  @Test
  public void should_describe() {
    DefaultSensorDescriptor sensorDescriptor = new DefaultSensorDescriptor();
    sensor.describe(sensorDescriptor);
    assertThat(sensorDescriptor.name()).isEqualTo(LANG_NAME + " Telemetry");
    assertThat(sensorDescriptor.languages()).isEmpty();
  }

  @Test
  public void executeTelemetrySensor() {
    final var telemetry = new ArrayList<Pair<String, String>>();
    doAnswer((Answer<Void>) invocationOnMock -> {
      // Intercept the call to context.addTelemetryProperty and capture the send telemetry in a list
      telemetry.add(Pair.of(invocationOnMock.getArgument(0), invocationOnMock.getArgument(1)));
      return null;
    }).when(context).addTelemetryProperty(anyString(), anyString());
    sensor.execute(context);
    assertThat(telemetry).containsSequence(
      Pair.of("PLUGIN_KEY.LANG_KEY.language_version.cs12", "2"),
      Pair.of("PLUGIN_KEY.LANG_KEY.target_framework.tfm2", "2"),
      Pair.of("PLUGIN_KEY.LANG_KEY.target_framework.tfm1", "2"),
      Pair.of("PLUGIN_KEY.LANG_KEY.target_framework.tfm3", "1"));
    assertThat(logTester.logs()).containsExactly(
      "Start importing telemetry.",
      "Start sending telemetry.",
      "Found telemetry for 2 projects.",
      "Aggregated 4 telemetry messages.",
      "Send 4 telemetry messages.",
      "Finished sending telemetry.");
  }
}
