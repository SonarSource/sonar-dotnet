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

import java.util.ArrayList;
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
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.doAnswer;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.spy;
import static org.mockito.Mockito.when;

public class TelemetryProcessorTest {

  private static final String PLUGIN_KEY = "PLUGIN_KEY";
  private static final String LANG_KEY = "LANG_KEY";
  private static final String LANG_NAME = "LANG_NAME";

  private SensorContextTester context;
  private TelemetryProcessor sensor;

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();
  @Rule
  public LogTester logTester = new LogTester();
  private TelemetryCollector collector;

  @Before
  public void prepare() {
    logTester.setLevel(Level.DEBUG);
    context = spy(SensorContextTester.create(temp.getRoot()));
    PluginMetadata metadata = mock(PluginMetadata.class);
    when(metadata.pluginKey()).thenReturn(PLUGIN_KEY);
    when(metadata.languageKey()).thenReturn(LANG_KEY);
    when(metadata.languageName()).thenReturn(LANG_NAME);
    collector = new TelemetryCollector();
    sensor = new TelemetryProcessor(collector, metadata);
  }

  @Test
  public void should_describe() {
    DefaultSensorDescriptor sensorDescriptor = new DefaultSensorDescriptor();
    sensor.describe(sensorDescriptor);
    assertThat(sensorDescriptor.name()).isEqualTo(LANG_NAME + " Telemetry processor");
    assertThat(sensorDescriptor.languages()).isEmpty();
  }

  @Test
  public void executeTelemetryProcessor_withNullCollector() {
    sensor = new TelemetryProcessor(null, mock(PluginMetadata.class));
    sensor.execute(context);
    assertThat(logTester.logs()).containsExactly(
      "TelemetryCollector is null, skipping telemetry processing.");
  }

  @Test
  public void executeTelemetryProcessor() {
    collector.addTelemetry(
      SonarAnalyzer.Telemetry.newBuilder()
        .setLanguageVersion("CS12")
        .addTargetFramework("TFM1")
        .addTargetFramework("TFM2")
        .build());
    collector.addTelemetry(
      SonarAnalyzer.Telemetry.newBuilder()
        .setLanguageVersion("CS12")
        .addTargetFramework("TFM1")
        .addTargetFramework("TFM2")
        .addTargetFramework("TFM3")
        .build());
    collector.addTelemetry(
      SonarAnalyzer.Telemetry.newBuilder()
        .setLanguageVersion("CS10")
        .addTargetFramework("TFM2")
        .build());
    final var telemetry = new ArrayList<Pair<String, String>>();
    doAnswer((Answer<Void>) invocationOnMock -> {
      // Intercept the call to context.addTelemetryProperty and capture the send telemetry in a list
      telemetry.add(Pair.of(invocationOnMock.getArgument(0), invocationOnMock.getArgument(1)));
      return null;
    }).when(context).addTelemetryProperty(anyString(), anyString());
    sensor.execute(context);
    assertThat(telemetry).containsExactlyInAnyOrder(
      Pair.of("PLUGIN_KEY.LANG_KEY.language_version.cs10", "1"),
      Pair.of("PLUGIN_KEY.LANG_KEY.language_version.cs12", "2"),
      Pair.of("PLUGIN_KEY.LANG_KEY.target_framework.tfm1", "2"),
      Pair.of("PLUGIN_KEY.LANG_KEY.target_framework.tfm2", "3"),
      Pair.of("PLUGIN_KEY.LANG_KEY.target_framework.tfm3", "1"));
    assertThat(logTester.logs()).containsExactly(
      "Found 3 telemetry messages reported by the analyzers.",
      "Aggregated 5 metrics.",
      "Adding metric: PLUGIN_KEY.LANG_KEY.language_version.cs10=1",
      "Adding metric: PLUGIN_KEY.LANG_KEY.language_version.cs12=2",
      "Adding metric: PLUGIN_KEY.LANG_KEY.target_framework.tfm2=3",
      "Adding metric: PLUGIN_KEY.LANG_KEY.target_framework.tfm1=2",
      "Adding metric: PLUGIN_KEY.LANG_KEY.target_framework.tfm3=1",
      "Added 5 metrics.");
  }
}
