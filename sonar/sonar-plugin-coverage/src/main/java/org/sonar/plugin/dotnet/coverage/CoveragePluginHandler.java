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
 * Created on May 14, 2009
 */
package org.sonar.plugin.dotnet.coverage;

import org.sonar.api.batch.maven.MavenPlugin;
import org.sonar.api.resources.Project;
import org.sonar.plugin.dotnet.core.AbstractDotNetMavenPluginHandler;

/**
 * Configures the maven coverage plugin.
 * 
 * @author Jose CHILLAN May 14, 2009
 */
public class CoveragePluginHandler extends AbstractDotNetMavenPluginHandler {

  /**
   * Constructs a @link{CoveragePluginHandler}.
   */
  public CoveragePluginHandler() {
  }

  @Override
  public void configure(Project project, MavenPlugin plugin) {
    super.configure(project, plugin);

    // We ignore the test failures in Sonar.
    plugin.setParameter("testFailureIgnore", "true");
    // injection of the default value in order to avoid strange behavior 
    // (i.e. gallio report erasing a source monitor report)
    plugin.setParameter("reportFileName", "gallio-report.xml");
  }

  /**
   * Launches the coverage goal.
   * 
   * @return
   */
  public String[] getGoals() {
    return new String[] { "coverage" };
  }

}
