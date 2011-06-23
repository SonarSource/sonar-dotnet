/*
 * Sonar C# Plugin :: Gallio
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package org.sonar.plugins.csharp.gallio;

import java.io.File;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.DependsUpon;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.sensor.AbstractCSharpSensor;

/**
 * Collects the FXCop reporting into sonar.
 */
@DependsUpon(CSharpConstants.CSHARP_CORE_EXECUTED)
public class GallioSensor extends AbstractCSharpSensor {

  private static final Logger LOG = LoggerFactory.getLogger(GallioSensor.class);

  private ProjectFileSystem fileSystem;
  private CSharpConfiguration configuration;
  private String executionMode;

  /**
   * Constructs a {@link GallioSensor}.
   * 
   * @param fileSystem
   * @param ruleFinder
   * @param fxCopRunner
   * @param profileExporter
   * @param rulesProfile
   */
  public GallioSensor(ProjectFileSystem fileSystem, CSharpConfiguration configuration,
      MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    super(microsoftWindowsEnvironment);
    this.fileSystem = fileSystem;
    this.configuration = configuration;
    this.executionMode = configuration.getString(GallioConstants.MODE, "");
  }

  /**
   * {@inheritDoc}
   */
  public boolean shouldExecuteOnProject(Project project) {
    boolean skipMode = GallioConstants.MODE_SKIP.equalsIgnoreCase(executionMode);
    if (skipMode) {
      LOG.info("Gallio plugin won't execute as it is set to 'skip' mode.");
    }

    return super.shouldExecuteOnProject(project) && !skipMode;
  }

  @Override
  public void analyse(Project project, SensorContext context) {
    LOG.info("EXECUTING...");
  }

}