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

import java.nio.file.Path;
import java.util.List;

import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.slf4j.event.Level;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.testfixtures.log.LogTester;
import org.sonarsource.dotnet.shared.plugins.sensors.MethodDeclarationsSensor;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class MethodDeclarationsSensorTest {
  private static final String LANG_NAME = "LANG_NAME";
  private static final Path TEST_DATA_DIR = Path.of("src/test/resources/MethodDeclarationsSensorTest/protobuf-files");

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();
  @Rule
  public LogTester logTester = new LogTester();

  private MethodDeclarationsCollector collector;
  private SensorContextTester context;
  private MethodDeclarationsSensor sensor;

  @Before
  public void prepare() {
    logTester.setLevel(Level.DEBUG);
    context = SensorContextTester.create(temp.getRoot());
    PluginMetadata metadata = mock(PluginMetadata.class);
    when(metadata.languageName()).thenReturn(LANG_NAME);
    ModuleConfiguration configuration = mock(ModuleConfiguration.class);
    when(configuration.protobufReportPaths()).thenReturn(List.of(TEST_DATA_DIR));
    collector = new MethodDeclarationsCollector();
    sensor = new MethodDeclarationsSensor(collector, metadata, configuration);
  }

  @Test
  public void should_describe() {
    DefaultSensorDescriptor sensorDescriptor = new DefaultSensorDescriptor();
    sensor.describe(sensorDescriptor);
    assertThat(sensorDescriptor.name()).isEqualTo(LANG_NAME + " Method Declarations");
  }

  @Test
  public void executeMethodDeclarationSensor() {
    sensor.execute(context);
    assertThat(collector.getMethodDeclarations()).satisfiesExactlyInAnyOrder(
      m -> {
        assertThat(m.getFilePath()).endsWith("MethodDeclarationsSensorTest\\TestMethodImport\\TestMethodImport.Tests\\TestBase.cs");
        assertThat(m.getAssemblyName()).isEqualTo("TestMethodImport.Tests");
        assertThat(m.getMethodDeclarationsCount()).isEqualTo(1);
        assertThat(m.getMethodDeclarations(0).getTypeName()).isEqualTo("TestMethodImport.Tests.TestBase");
        assertThat(m.getMethodDeclarations(0).getMethodName()).isEqualTo("TestMethodInBaseClass");
      },
      m -> {
        assertThat(m.getFilePath()).endsWith("MethodDeclarationsSensorTest\\TestMethodImport\\TestMethodImport.Tests\\TestClass.cs");
        assertThat(m.getAssemblyName()).isEqualTo("TestMethodImport.Tests");
        assertThat(m.getMethodDeclarationsCount()).isEqualTo(2);
        assertThat(m.getMethodDeclarations(0).getTypeName()).isEqualTo("TestMethodImport.Tests.TestClass");
        assertThat(m.getMethodDeclarations(0).getMethodName()).isEqualTo("TestMethod");
        assertThat(m.getMethodDeclarations(1).getTypeName()).isEqualTo("TestMethodImport.Tests.TestClass");
        assertThat(m.getMethodDeclarations(1).getMethodName()).isEqualTo("TestMethodInBaseClass");
      }
    );
    assertThat(logTester.logs()).containsExactly("Start importing method declarations.");
  }
}
