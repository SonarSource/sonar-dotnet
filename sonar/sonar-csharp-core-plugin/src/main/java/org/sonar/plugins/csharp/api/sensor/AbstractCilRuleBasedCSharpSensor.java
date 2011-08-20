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
import java.util.Set;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.resources.Project;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;

/**
 * Support classes for sensor of tools inspecting compiled code.
 * @author Alexandre Victoor
 *
 */
public abstract class AbstractCilRuleBasedCSharpSensor extends AbstractRegularCSharpSensor {
  
  private static final Logger LOG = LoggerFactory.getLogger(AbstractCilRuleBasedCSharpSensor.class);

  protected final CSharpConfiguration configuration;
  private final String toolName;

  protected AbstractCilRuleBasedCSharpSensor(MicrosoftWindowsEnvironment microsoftWindowsEnvironment, CSharpConfiguration configuration, String toolName) {
    super(microsoftWindowsEnvironment);
    this.configuration = configuration;
    this.toolName = toolName;
  }

  /**
   * {@inheritDoc}
   */
  public boolean shouldExecuteOnProject(Project project) {
    final boolean result;
    if (super.shouldExecuteOnProject(project)) {
      final String buildConfigurations = configuration.getString(CSharpConstants.BUILD_CONFIGURATIONS_KEY,
          CSharpConstants.BUILD_CONFIGURATIONS_DEFVALUE);
      final VisualStudioProject visualProject = getVSProject(project);
      final Set<File> assemblies = visualProject.getGeneratedAssemblies(buildConfigurations);
      if (assemblies.isEmpty()) {
        LOG.info("No assembly to check with " + toolName);
        result = false;
      } else {
        result = true;
      }
    } else {
      result = false;
    }
    return result;
  }
}
