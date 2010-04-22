/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexandre.victoor@codehaus.org
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
/*
 * Created on May 7, 2009
 */
package org.sonar.plugin.dotnet.fxcop;

import java.io.File;
import java.io.IOException;

import org.sonar.api.batch.maven.MavenPlugin;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.plugin.dotnet.core.AbstractDotNetMavenPluginHandler;

/**
 * Generates the configuration of FXCop goal for Sonar.
 * 
 * @author Jose CHILLAN May 7, 2009
 */
public class FxCopPluginHandler
  extends AbstractDotNetMavenPluginHandler
{
  private static final String FX_COP_FILE   = "sonar.FxCop";
  private static final String FX_COP_REPORT = "fxcop-report.xml";

  private RulesProfile        rulesProfile;
  private FxCopRuleRepository rulesRepository;

  /**
   * Constructs a @link{FxCopPluginHandler}.
   */
  public FxCopPluginHandler(RulesProfile rulesProfile, FxCopRuleRepository fxCopRulesRepository)
  {
    this.rulesProfile = rulesProfile;
    this.rulesRepository = fxCopRulesRepository;
  }

  public String[] getGoals()
  {
    return new String[] { "fxcop"
    };
  }

  @Override
  public void configure(Project project, MavenPlugin plugin)
  {
    try
    {
      super.configure(project, plugin);
      generateConfigurationFile(project, plugin);
      configureParameters(plugin);
      plugin.setParameter("fxCopReportName", FX_COP_REPORT);
    }
    catch (IOException e)
    {
    }
  }

  /**
   * Extracts the configuration file to use and binds the FXCop Mojo to it.
   * 
   * @param pom
   * @param plugin
   * @throws IOException
   */
  private void generateConfigurationFile(Project project, MavenPlugin plugin) throws IOException
  {
    String fxCopConfiguration = rulesRepository.exportConfiguration(rulesProfile);
    File configFile = project.getFileSystem().writeToWorkingDirectory(fxCopConfiguration, FX_COP_FILE);
    // Defines the configuration file
    plugin.setParameter("fxCopConfigFile", configFile.getAbsolutePath());
  }

  public void configureParameters(MavenPlugin plugin)
  {
    // Nothing yet
  }
}
