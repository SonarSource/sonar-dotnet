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

import java.util.Collection;

import org.sonar.api.batch.Sensor;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.File;
import org.sonar.api.resources.Project;
import org.sonar.dotnet.tools.commons.utils.FileFinder;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;

/**
 * This sensor gets executed on every C# sub-projects, but not on the root project (= the solution). <br/>
 * <br/>
 * Super class of {@link AbstractRegularCSharpSensor} and {@link AbstractTestCSharpSensor}.
 */
public abstract class AbstractCSharpSensor implements Sensor {

  private MicrosoftWindowsEnvironment microsoftWindowsEnvironment;

  /**
   * Creates an {@link AbstractCSharpSensor} that has a {@link MicrosoftWindowsEnvironment} reference.
   * 
   * @param microsoftWindowsEnvironment
   *          the {@link MicrosoftWindowsEnvironment}
   */
  protected AbstractCSharpSensor(MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    this.microsoftWindowsEnvironment = microsoftWindowsEnvironment;
  }

  /**
   * {@inheritDoc}
   */
  public boolean shouldExecuteOnProject(Project project) {
    if (project.isRoot()) {
      return false;
    }
    return CSharpConstants.LANGUAGE_KEY.equals(project.getLanguageKey());
  }

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
