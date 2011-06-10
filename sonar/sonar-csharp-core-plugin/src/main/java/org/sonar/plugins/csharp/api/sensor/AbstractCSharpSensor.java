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

import org.sonar.api.batch.Sensor;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioProject;

/**
 * Abstract Sensor for C# plugins.
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
    return CSharpConstants.LANGUAGE_KEY.equals(project.getLanguageKey()) && !getVSProject(project).isTest();
  }

  /**
   * {@inheritDoc}
   */
  public abstract void analyse(Project project, SensorContext context);

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
   * Returns the {@link MicrosoftWindowsEnvironment} object.
   * 
   * @return the {@link MicrosoftWindowsEnvironment}
   */
  protected MicrosoftWindowsEnvironment getMicrosoftWindowsEnvironment() {
    return microsoftWindowsEnvironment;
  }

}
