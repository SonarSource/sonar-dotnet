/*
 * Sonar C# Plugin :: Core
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
package org.sonar.plugins.csharp.api.sensor;

import java.io.File;
import java.util.Collection;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.resources.Project;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;

/**
 * Abstract Sensor for C# plugins that will be executed on every sub-project.
 * Should be renamed... (not so easy to do so since squid is closed source)
 */
public abstract class AbstractRegularCSharpSensor extends AbstractCSharpSensor {
  
  private static final Logger LOG = LoggerFactory.getLogger(AbstractRegularCSharpSensor.class);
  
  protected final CSharpConfiguration configuration;
  
  /**
   * Creates an {@link AbstractRegularCSharpSensor} that has a {@link MicrosoftWindowsEnvironment} reference.
   * 
   * @param microsoftWindowsEnvironment
   *          the {@link MicrosoftWindowsEnvironment}
   */
  protected AbstractRegularCSharpSensor(CSharpConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment, String toolName, String executionMode) {
    super(microsoftWindowsEnvironment, toolName, executionMode);
    this.configuration = configuration;
  }
  
  /**
   * TODO remove ASAP after having updated the squid plugin
   * @param microsoftWindowsEnvironment
   */
  protected AbstractRegularCSharpSensor(MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    super(microsoftWindowsEnvironment, "Squid C#", "");
    configuration = null;
  }
  
  protected boolean isTestSensor() {
    return false;
  }
  
  protected boolean isCilSensor() {
    return false;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public boolean shouldExecuteOnProject(Project project) {
    return super.shouldExecuteOnProject(project) && (isTestProject(project) == isTestSensor()) && (!isCilSensor() || assembliesFound(project));
  }
  
  private boolean assembliesFound(Project project) {
    final boolean result;

      boolean reuseMode = MODE_REUSE_REPORT.equalsIgnoreCase(executionMode);
      if (reuseMode) {
        result = true;
      } else {

        final VisualStudioProject visualProject = getVSProject(project);
        Collection<File> assemblies;
        final String[] assemblyPatterns = configuration.getStringArray(CSharpConstants.ASSEMBLIES_TO_SCAN_KEY);
        final String buildConfigurations = configuration.getString(CSharpConstants.BUILD_CONFIGURATIONS_KEY,
            CSharpConstants.BUILD_CONFIGURATIONS_DEFVALUE);
        final String buildPlatform = configuration.getString(CSharpConstants.BUILD_PLATFORM_KEY,
            CSharpConstants.BUILD_PLATFORM_DEFVALUE);
        if (assemblyPatterns == null || assemblyPatterns.length == 0) {
          assemblies = visualProject.getGeneratedAssemblies(buildConfigurations, buildPlatform);
        } else {
          assemblies = findFiles(project, assemblyPatterns);
          if (assemblies.isEmpty()) {
            // fall back to the default VS output folder
            assemblies = visualProject.getGeneratedAssemblies(buildConfigurations, buildPlatform);
          }
        }

        if (assemblies.isEmpty()) {
          LOG.warn("No assembly to check with " + toolName);
          result = false;
        } else {
          result = true;
        }
      }
    return result;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public org.sonar.api.resources.File fromIOFile(File file, Project project) {
    if (isTestSensor()) {
      return org.sonar.api.resources.File.fromIOFile(file, project.getFileSystem().getTestDirs());
    }
    return super.fromIOFile(file, project);
  }

}
