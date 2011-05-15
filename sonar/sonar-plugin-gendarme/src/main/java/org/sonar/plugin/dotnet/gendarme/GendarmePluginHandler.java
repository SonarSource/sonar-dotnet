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
 * Created on May 7, 2009
 */
package org.sonar.plugin.dotnet.gendarme;

import static org.sonar.plugin.dotnet.gendarme.Constants.*;

import java.io.File;
import java.io.IOException;
import java.util.List;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.maven.MavenPlugin;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.rules.ActiveRule;
import org.sonar.plugin.dotnet.core.AbstractDotNetMavenPluginHandler;

/**
 * Generates the configuration of gendarme goal for Sonar.
 * 
 * @author Jose CHILLAN May 7, 2009
 */
public class GendarmePluginHandler extends AbstractDotNetMavenPluginHandler {

  private final static Logger log = LoggerFactory
      .getLogger(GendarmePluginHandler.class);

  private final static String GENDARME_FILE = "sonar.Gendarme";
  private final static String GENDARME_REPORT = Constants.GENDARME_REPORT_XML;

  private RulesProfile rulesProfile;
  private GendarmeRuleRepository rulesRepository;

  /**
   * Constructs a @link{GendarmePluginHandler}.
   */
  public GendarmePluginHandler(RulesProfile rulesProfile,
      GendarmeRuleRepository gendarmeRulesRepository) {
    this.rulesProfile = rulesProfile;
    this.rulesRepository = gendarmeRulesRepository;
  }

  public String[] getGoals() {
    return new String[] { "gendarme" };
  }

  @Override
  public void configure(Project project, MavenPlugin plugin) {
    try {
      super.configure(project, plugin);
      generateConfigurationFile(project, plugin);
      plugin.setParameter("gendarmeReportName", GENDARME_REPORT);
    } catch (IOException e) {
      throw new RuntimeException(e);
    }
    
    String confidence 
      = project.getConfiguration().getString(GENDARME_CONFIDENCE_KEY);
    
    if (StringUtils.isNotEmpty(confidence)) {
      plugin.setParameter("confidence", confidence);
    }
    
    String severity 
      = project.getConfiguration().getString(GENDARME_SEVERITY_KEY);
  
    if (StringUtils.isNotEmpty(severity)) {
      plugin.setParameter("severity", severity);
    }
    
  }

  /**
   * Extracts the configuration file to use and binds the gendarme Mojo to it.
   * 
   * @param pom
   * @param plugin
   * @throws IOException
   */
  private void generateConfigurationFile(Project project, MavenPlugin plugin)
    throws IOException {
    List<ActiveRule> activeRules = rulesProfile
        .getActiveRulesByPlugin(GendarmePlugin.KEY);
    if (activeRules == null || activeRules.isEmpty()) {
      log.error("Warning, no configuration for Mono Gendarme");
    } else {
      String gendarmeConfiguration = rulesRepository
          .exportConfiguration(rulesProfile);
      File configFile = project.getFileSystem().writeToWorkingDirectory(
          gendarmeConfiguration, GENDARME_FILE);
      // Defines the configuration file
      plugin.setParameter("gendarmeConfigFile", configFile.getAbsolutePath());
    }
  }
}
