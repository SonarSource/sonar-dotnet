/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
 *
 * Sonar is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * Sonar is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Sonar; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

package org.sonar.plugin.dotnet.cpd;

import static org.sonar.plugin.dotnet.cpd.Constants.*;

import org.apache.commons.configuration.Configuration;
import org.sonar.api.batch.maven.MavenPlugin;
import org.sonar.api.resources.Project;
import org.sonar.plugin.dotnet.core.AbstractDotNetMavenPluginHandler;

/**
 * handler used to launch the cpd mojo of the dotnet plugin 
 * 
 * @author Alexandre Victoor
 */
public class DotnetCpdPluginHandler extends AbstractDotNetMavenPluginHandler {
  
  private Configuration configuration;

  public DotnetCpdPluginHandler(Configuration configuration) {
    this.configuration = configuration;
  }


  public String[] getGoals() {
    return new String[] { "cpd" };
  }

  @Override
  public void configure(Project project, MavenPlugin plugin) {
    super.configure(project, plugin);
    plugin.setParameter(
      "minimumTokens",
      configuration.getString(
        CPD_MINIMUM_TOKENS_PROPERTY,
        Integer.toString(CPD_MINIMUM_TOKENS_DEFAULT_VALUE)
      )
    );
  }
  
}
