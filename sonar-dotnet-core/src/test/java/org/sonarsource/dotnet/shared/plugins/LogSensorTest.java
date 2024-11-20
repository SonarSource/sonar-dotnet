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
import java.util.Collections;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.slf4j.event.Level;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.testfixtures.log.LogTester;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class LogSensorTest {
  private static final String LANG_KEY = "LANG_KEY";
  private static final String LANG_NAME = "LANG_NAME";
  // see src/test/resources/ProtobufImporterTest/README.md for explanation. log.pb is copy of custom-log.pb
  private static final File TEST_DATA_DIR = new File("src/test/resources/LogSensorTest");

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Rule
  public LogTester logTester = new LogTester();

  private SensorContextTester context;
  private LogSensor sensor;

  @Before
  public void prepare() throws Exception {
    logTester.setLevel(Level.DEBUG);
    context = SensorContextTester.create(temp.getRoot());
    PluginMetadata metadata = mock(PluginMetadata.class);
    when(metadata.languageKey()).thenReturn(LANG_KEY);
    when(metadata.languageName()).thenReturn(LANG_NAME);
    ModuleConfiguration configuration = mock(ModuleConfiguration.class);
    when(configuration.protobufReportPaths()).thenReturn(Collections.singletonList(TEST_DATA_DIR.toPath()));
    sensor = new LogSensor(metadata, configuration);
  }

  @Test
  public void should_describe() {
    DefaultSensorDescriptor sensorDescriptor = new DefaultSensorDescriptor();
    sensor.describe(sensorDescriptor);
    assertThat(sensorDescriptor.name()).isEqualTo(LANG_NAME + " Analysis Log");
    assertThat(sensorDescriptor.languages()).isEmpty();     // should not filter per language
  }

  @Test
  public void executeLogsMessages() {
    sensor.execute(context);
    assertThat(logTester.logs()).hasSize(4);
  }
}
