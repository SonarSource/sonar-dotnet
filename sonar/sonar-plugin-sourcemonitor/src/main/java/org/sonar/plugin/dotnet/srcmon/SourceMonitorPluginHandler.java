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
 */
package org.sonar.plugin.dotnet.srcmon;

import org.sonar.api.batch.maven.MavenPlugin;
import org.sonar.api.resources.Project;
import org.sonar.plugin.dotnet.core.AbstractDotNetMavenPluginHandler;

/**
 * A plugin that generates the execution of the Source Monitor metrics.
 * 
 * @author Jose CHILLAN May 5, 2009
 */
public class SourceMonitorPluginHandler extends
    AbstractDotNetMavenPluginHandler {

  @Override
  public void configure(Project project, MavenPlugin plugin) {
      super.configure(project, plugin);
      plugin.setParameter("metricsReportFileName", SourceMonitorPlugin.SOURCE_MONITOR_REPORT);
  }

  public String[] getGoals() {
    return new String[] { "metrics" };
  }

}
