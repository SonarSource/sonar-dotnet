/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.fxcop.runner;

import static org.hamcrest.Matchers.endsWith;
import static org.junit.Assert.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import java.io.File;

import org.apache.commons.configuration.BaseConfiguration;
import org.apache.commons.configuration.Configuration;
import org.apache.commons.io.FileUtils;
import org.junit.Before;
import org.junit.BeforeClass;
import org.junit.Test;
import org.sonar.api.resources.ProjectFileSystem;

import com.sonar.csharp.fxcop.FxCopConstants;

public class FxCopCommandTest {

  private static FxCopCommand fxCopCommand;
  private static Configuration configuration;
  private static ProjectFileSystem projectFileSystem;
  private static File fakeFxCopProgramFile;

  @BeforeClass
  public static void initStatic() throws Exception {
    fakeFxCopProgramFile = FileUtils.toFile(FxCopCommandTest.class.getResource("/Runner/FakeFxCopConfigFile.xml"));
    configuration = new BaseConfiguration();
    projectFileSystem = mock(ProjectFileSystem.class);
    when(projectFileSystem.getBasedir()).thenReturn(FileUtils.toFile(FxCopCommandTest.class.getResource("/Runner")));
  }

  @Before
  public void init() throws Exception {
    configuration.clear();
    configuration.addProperty(FxCopConstants.EXECUTABLE_KEY, fakeFxCopProgramFile.getAbsolutePath());
  }

  @Test
  public void testToArray() throws Exception {
    configuration.addProperty(FxCopConstants.ASSEMBLIES_TO_SCAN_KEY, "FakeAssemblies/Fake1.assembly, FakeAssemblies/Fake2.assembly");
    fxCopCommand = new FxCopCommand(configuration, projectFileSystem);
    fxCopCommand.setFxCopConfigFile(fakeFxCopProgramFile);
    String[] commands = fxCopCommand.toArray();
    assertThat(commands[1], endsWith("FakeFxCopConfigFile.xml"));
    assertThat(commands[2], endsWith("fxcop-report.xml"));
    assertThat(commands[3], endsWith("Fake1.assembly"));
    assertThat(commands[4], endsWith("Fake2.assembly"));
  }

  @Test(expected = IllegalStateException.class)
  public void testWithNoAssembly() throws Exception {
    fxCopCommand = new FxCopCommand(configuration, projectFileSystem);
    fxCopCommand.setFxCopConfigFile(fakeFxCopProgramFile);
    fxCopCommand.toArray();
  }

  @Test(expected = IllegalStateException.class)
  public void testWithNullFxCopConfigFile() throws Exception {
    fxCopCommand = new FxCopCommand(configuration, projectFileSystem);
    fxCopCommand.toArray();
  }

}
