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

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.profiles.ProfileExporter;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;


public abstract class AbstractRuleBasedCSharpSensor extends AbstractRegularCSharpSensor {
  
  private static final Logger LOG = LoggerFactory.getLogger(AbstractRuleBasedCSharpSensor.class);
  
  protected final RulesProfile rulesProfile;
  private final ProfileExporter profileExporter;

  protected AbstractRuleBasedCSharpSensor(CSharpConfiguration configuration, RulesProfile rulesProfile, ProfileExporter profileExporter, MicrosoftWindowsEnvironment microsoftWindowsEnvironment, String toolName, String executionMode) {
    super(configuration, microsoftWindowsEnvironment, toolName, executionMode);
    this.rulesProfile = rulesProfile;
    this.profileExporter = profileExporter;
  }
  
  /**
   * {@inheritDoc}
   */
  public boolean shouldExecuteOnProject(Project project) {
    return super.shouldExecuteOnProject(project) && (isToolEnableInProfile());
  }
  
  private boolean isToolEnableInProfile() {
    boolean result = !rulesProfile.getActiveRulesByRepository(profileExporter.getKey()).isEmpty();
    if (!result) {
      LOG.warn("/!\\ SKIP "+ toolName + " analysis: no rule defined for " + toolName + " in the \"{}\" profile.", rulesProfile.getName());
    }
    return result;
  }
}
