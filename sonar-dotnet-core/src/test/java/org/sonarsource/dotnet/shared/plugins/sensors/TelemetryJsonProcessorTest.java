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

import java.util.ArrayList;
import java.util.Map;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.mockito.stubbing.Answer;
import org.slf4j.event.Level;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.testfixtures.log.LogTester;
import org.sonarsource.dotnet.shared.plugins.AbstractLanguageConfiguration;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.telemetryjson.TelemetryJsonCollector;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.doAnswer;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.spy;
import static org.mockito.Mockito.when;

public class TelemetryJsonProcessorTest {
  private static final String PLUGIN_KEY = "PLUGIN_KEY";
  private static final String LANG_KEY = "LANG_KEY";
  private static final String LANG_NAME = "LANG_NAME";

  private SensorContextTester context;
  private TelemetryJsonProcessor sensor;

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();
  @Rule
  public LogTester logTester = new LogTester();
  private TelemetryJsonCollector collector;

  @Before
  public void prepare() {
    logTester.setLevel(Level.DEBUG);
    context = spy(SensorContextTester.create(temp.getRoot()));
    PluginMetadata metadata = mock(PluginMetadata.class);
    when(metadata.pluginKey()).thenReturn(PLUGIN_KEY);
    when(metadata.languageKey()).thenReturn(LANG_KEY);
    when(metadata.languageName()).thenReturn(LANG_NAME);
    var config = mock(AbstractLanguageConfiguration.class);
    collector = new TelemetryJsonCollector();
    sensor = new TelemetryJsonProcessor(collector, new TelemetryJsonProjectCollector(collector, config), metadata);
  }

  @Test
  public void should_describe() {
    DefaultSensorDescriptor sensorDescriptor = new DefaultSensorDescriptor();
    sensor.describe(sensorDescriptor);
    assertThat(sensorDescriptor.name()).isEqualTo(LANG_NAME + " Telemetry Json processor");
    assertThat(sensorDescriptor.languages()).isEmpty();
  }

  @Test
  public void executeTelemetryProcessor_withNullCollector() {
    sensor = new TelemetryJsonProcessor(null, new TelemetryJsonProjectCollector(new TelemetryJsonCollector(), mock(AbstractLanguageConfiguration.class)), mock(PluginMetadata.class));
    sensor.execute(context);
    assertThat(logTester.logs()).containsExactly(
      "TelemetryJsonCollector is null, skipping telemetry processing.");
  }

  @Test
  public void executeTelemetryProcessor() {
    collector.addTelemetry("key1", "value1");
    collector.addTelemetry("key2", "value2");
    final var telemetry = new ArrayList<Map.Entry<String, String>>();
    doAnswer((Answer<Void>) invocationOnMock -> {
      // Intercept the call to context.addTelemetryProperty and capture the send telemetry in a list
      telemetry.add(Map.entry(invocationOnMock.getArgument(0), invocationOnMock.getArgument(1)));
      return null;
    }).when(context).addTelemetryProperty(anyString(), anyString());
    sensor.execute(context);
    assertThat(telemetry).containsExactlyInAnyOrder(
      Map.entry("key1", "value1"),
      Map.entry("key2", "value2"));
    assertThat(logTester.logs()).containsExactly(
      "Found 2 telemetry messages.",
      "Adding metric: key1=value1",
      "Adding metric: key2=value2",
      "Added 2 metrics.");
  }

  @Test
  public void executeTelemetryProcessorWithTelemetryJsonProjectCollector() {
    collector.addTelemetry("key1", "value1");
    collector.addTelemetry("key2", "value2");
    var projectSensor = new TelemetryJsonProjectCollector(collector, mock(AbstractLanguageConfiguration.class)) {
      @Override
      public void execute() {
        collector.addTelemetry(Map.entry("projectKey1", "value1"));
        collector.addTelemetry(Map.entry("projectKey2", "value2"));
      }
    };
    final var telemetry = new ArrayList<Map.Entry<String, String>>();
    doAnswer((Answer<Void>) invocationOnMock -> {
      // Intercept the call to context.addTelemetryProperty and capture the send telemetry in a list
      telemetry.add(Map.entry(invocationOnMock.getArgument(0), invocationOnMock.getArgument(1)));
      return null;
    }).when(context).addTelemetryProperty(anyString(), anyString());
    var telemetryJsonProcessor = new TelemetryJsonProcessor(collector, projectSensor, mock(PluginMetadata.class));
    telemetryJsonProcessor.execute(context);
    assertThat(telemetry).containsExactlyInAnyOrder(
      Map.entry("key1", "value1"),
      Map.entry("key2", "value2"),
      Map.entry("projectKey1", "value1"),
      Map.entry("projectKey2", "value2"));
    assertThat(logTester.logs()).containsExactly(
      "Found 4 telemetry messages.",
      "Adding metric: key1=value1",
      "Adding metric: key2=value2",
      "Adding metric: projectKey1=value1",
      "Adding metric: projectKey2=value2",
      "Added 4 metrics.");
  }
}
