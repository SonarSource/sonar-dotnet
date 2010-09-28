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

/*
 * Created on May 19, 2009
 */
package org.sonar.plugin.dotnet.stylecop;

import static org.sonar.plugin.dotnet.stylecop.Constants.*;

import java.io.File;
import java.io.IOException;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.maven.MavenPlugin;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.utils.ServerHttpClient;
import org.sonar.plugin.dotnet.core.AbstractDotNetMavenPluginHandler;

/**
 * Configures the launch of the maven style cop plugin.
 * 
 * @author Jose CHILLAN May 19, 2009
 */
public class StyleCopPluginHandler extends AbstractDotNetMavenPluginHandler {
  
  private final static Logger log = LoggerFactory.getLogger(StyleCopPluginHandler.class);
  
  public static final String STYLE_COP_FILE = "sonar.StyleCop";

  private RulesProfile rulesProfile;
  private StyleCopRuleRepository rulesRepository;

  /**
   * Constructs a @link{StyleCopPluginHandler}.
   * 
   * @param rulesProfile
   * @param rulesRepository
   * @param serverHttpClient
   * @param exclusions
   */
  public StyleCopPluginHandler(RulesProfile rulesProfile,
      StyleCopRuleRepository rulesRepository, ServerHttpClient serverHttpClient) {
    super();
    this.rulesProfile = rulesProfile;
    this.rulesRepository = rulesRepository;
  }

  public String[] getGoals() {
    return new String[] { "stylecop" };
  }

  /**
   * @param pom
   * @param plugin
   */
  @Override
  public void configure(Project project, MavenPlugin plugin) {
    try {
      super.configure(project, plugin);
      generateConfigurationFile(project, plugin);
      configureParameters(plugin);
      plugin.setParameter("styleCopReportName", STYLECOP_REPORT_NAME);
    } catch (IOException e) {
      log.debug("Unexpected error during config phase", e);
    }
  }

  /**
   * Extracts the configuration file to use and binds the FXCop Mojo to it.
   * 
   * @param pom
   * @param plugin
   * @throws IOException
   */
  private void generateConfigurationFile(Project project, MavenPlugin plugin)
      throws IOException {
    String styleCopConfiguration = rulesRepository
        .exportConfiguration(rulesProfile);
    File configFile = project.getFileSystem().writeToWorkingDirectory(
        styleCopConfiguration, STYLE_COP_FILE);
    // Defines the configuration file
    plugin.setParameter("styleCopConfigFile", configFile.getAbsolutePath());
  }

  public void configureParameters(MavenPlugin plugin) {
    // Nothing yet
  }
}
