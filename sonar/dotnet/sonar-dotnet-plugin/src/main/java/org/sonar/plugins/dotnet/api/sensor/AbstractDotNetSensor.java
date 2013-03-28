/*
 * Sonar .NET Plugin :: Core
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
package org.sonar.plugins.dotnet.api.sensor;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.Sensor;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.File;
import org.sonar.api.resources.Project;
import org.sonar.plugins.dotnet.api.microsoft.MicrosoftWindowsEnvironment;
import org.sonar.plugins.dotnet.api.microsoft.VisualStudioProject;
import org.sonar.plugins.dotnet.api.microsoft.VisualStudioSolution;
import org.sonar.plugins.dotnet.api.utils.FileFinder;

import java.util.Collection;

/**
 * Sensors that extends this class get executed on every .NET sub-projects, but not on the root project (= the solution).
 */
public abstract class AbstractDotNetSensor implements Sensor {

  private static final Logger LOG = LoggerFactory.getLogger(AbstractDotNetSensor.class);

  public static final String MODE_SKIP = "skip";
  public static final String MODE_REUSE_REPORT = "reuseReport";

  private final MicrosoftWindowsEnvironment microsoftWindowsEnvironment;
  protected final String toolName;
  protected final String executionMode;

  /**
   * Creates an {@link AbstractDotNetSensor} that has a {@link MicrosoftWindowsEnvironment} reference.
   *
   * @param microsoftWindowsEnvironment
   *          the {@link MicrosoftWindowsEnvironment}
   */
  protected AbstractDotNetSensor(MicrosoftWindowsEnvironment microsoftWindowsEnvironment, String toolName, String executionMode) {
    this.microsoftWindowsEnvironment = microsoftWindowsEnvironment;
    this.toolName = toolName;
    this.executionMode = executionMode;
  }

  /**
   * {@inheritDoc}
   */
  public boolean shouldExecuteOnProject(Project project) {
    if (project.isRoot() || !isLanguageSupported(project.getLanguageKey())) {
      return false;
    }
    boolean skipMode = MODE_SKIP.equalsIgnoreCase(executionMode);
    if (skipMode) {
      LOG.info("{} plugin won't execute as it is set to 'skip' mode.", toolName);
      return false;
    }

    return true;
  }

  private boolean isLanguageSupported(String languageKey) {
    for (String key : getSupportedLanguages()) {
      if (key.equals(languageKey)) {
        return true;
      }
    }
    return false;
  }

  /**
   * Must return the list of supported languages.
   *
   * @return the keys of the supported languages.
   */
  public abstract String[] getSupportedLanguages();

  /**
   * {@inheritDoc}
   */
  public abstract void analyse(Project project, SensorContext context);

  /**
   * Returns the Sonar file representation ({@link File}) of the given file, if that file exists in the given project.
   *
   * @param file
   *          the real file
   * @param project
   *          the project
   * @return the Sonar resource if it exists in this project, or null if not.
   */
  public File fromIOFile(java.io.File file, Project project) {
    return File.fromIOFile(file, project);
  }

  /**
   * Returns the Visual Studio Project corresponding to the given Sonar Project.
   *
   * @param project
   *          the Sonar Project
   * @return the VS Project
   */
  protected VisualStudioProject getVSProject(Project project) {
    return microsoftWindowsEnvironment.getCurrentProject(project.getName());
  }

  /**
   * Is the current project a test project ?
   * @param project
   * @return
   */
  protected boolean isTestProject(Project project) {
    VisualStudioProject vsProject = getVSProject(project);
    final boolean result;
    if (vsProject == null) {
      // probably the root project, solution level
      result = false;
    } else {
      result = vsProject.isTest();
    }
    return result;
  }

  protected VisualStudioSolution getVSSolution() {
    return microsoftWindowsEnvironment.getCurrentSolution();
  }

  /**
   * Returns the {@link MicrosoftWindowsEnvironment} object.
   *
   * @return the {@link MicrosoftWindowsEnvironment}
   */
  protected MicrosoftWindowsEnvironment getMicrosoftWindowsEnvironment() {
    return microsoftWindowsEnvironment;
  }

  protected Collection<java.io.File> findFiles(Project project, String... queries) {
    VisualStudioSolution vsSolution = microsoftWindowsEnvironment.getCurrentSolution();
    VisualStudioProject vsProject = getVSProject(project);
    return FileFinder.findFiles(vsSolution, vsProject, queries);
  }

}
