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

import org.sonar.api.resources.Project;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;

/**
 * Abstract Sensor for C# plugins that will be executed on every sub-project that is not a test project.
 */
public abstract class AbstractRegularCSharpSensor extends AbstractCSharpSensor {
 
  /**
   * Creates an {@link AbstractRegularCSharpSensor} that has a {@link MicrosoftWindowsEnvironment} reference.
   * 
   * @param microsoftWindowsEnvironment
   *          the {@link MicrosoftWindowsEnvironment}
   */
  protected AbstractRegularCSharpSensor(MicrosoftWindowsEnvironment microsoftWindowsEnvironment, String toolName, String executionMode) {
    super(microsoftWindowsEnvironment, toolName, executionMode);
  }
  
  /**
   * TODO remove ASAP after having updated the squid plugin
   * @param microsoftWindowsEnvironment
   */
  protected AbstractRegularCSharpSensor(MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    super(microsoftWindowsEnvironment,  "Squid C#", "");
  }

  /**
   * {@inheritDoc}
   */
  public boolean shouldExecuteOnProject(Project project) {
    return !isTestProject(project) && super.shouldExecuteOnProject(project) ;
  }

}
