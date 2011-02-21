/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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

/*
 * Created on May 5, 2009
 *
 */
package org.sonar.plugin.dotnet.core;

import org.apache.commons.lang.StringUtils;
import org.sonar.api.batch.maven.DependsUponCustomRules;
import org.sonar.api.batch.maven.MavenPlugin;
import org.sonar.api.batch.maven.MavenPluginHandler;
import org.sonar.api.batch.maven.MavenUtils;
import org.sonar.api.resources.Project;

/**
 * A base class for the C# maven plugin handlers.
 * 
 * @author Jose CHILLAN May 5, 2009
 */
public abstract class AbstractDotNetMavenPluginHandler implements
    MavenPluginHandler, DependsUponCustomRules {
  public static final String GROUP_ID = MavenUtils.GROUP_ID_APACHE_MAVEN;
  public static final String ARTIFACT_ID = "maven-dotnet-plugin";

  /**
   * Version of the .Net plugin
   */
  private static final String DOTNET_PLUGIN_VERSION = "0.1";

  /**
   * Constructs a @link{AbstractDotNetMavenPluginHandler}.
   */
  public AbstractDotNetMavenPluginHandler() {
  }

  @Override
  public void configure(Project project, MavenPlugin plugin) {
    String[] excludedProjectNames 
      = project.getConfiguration().getStringArray("sonar.skippedModules");
    if (excludedProjectNames!=null) {
      String skippedProjectsParam = StringUtils.join(excludedProjectNames,',');
      plugin.setParameter("skippedProjects", skippedProjectsParam);
    }
  }

  public String getArtifactId() {
    return ARTIFACT_ID;
  }

  public String getGroupId() {
    return Constant.MAVEN_DOTNET_GROUP_ID;
  }

  public String getVersion() {
    return DOTNET_PLUGIN_VERSION;
  }

  public boolean isFixedVersion() {
    return false;
  }

}
