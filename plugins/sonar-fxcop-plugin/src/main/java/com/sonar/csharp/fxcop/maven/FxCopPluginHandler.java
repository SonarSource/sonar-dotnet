/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

/*
 * Created on May 7, 2009
 */
package com.sonar.csharp.fxcop.maven;

import java.io.File;
import java.io.IOException;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.maven.DependsUponCustomRules;
import org.sonar.api.batch.maven.MavenPlugin;
import org.sonar.api.batch.maven.MavenPluginHandler;
import org.sonar.api.batch.maven.MavenUtils;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;

import com.sonar.csharp.fxcop.rules.FxCopRuleRepository;

/**
 * Generates the configuration of FXCop goal for Sonar.
 */
public class FxCopPluginHandler implements MavenPluginHandler, DependsUponCustomRules {

  private final static Logger log = LoggerFactory.getLogger(FxCopPluginHandler.class);

  private static final String FX_COP_FILE = "sonar.FxCop";

  public static final String GROUP_ID = MavenUtils.GROUP_ID_APACHE_MAVEN;
  public static final String ARTIFACT_ID = "maven-dotnet-plugin";
  private static final String DOTNET_PLUGIN_VERSION = "0.5";

  private RulesProfile rulesProfile;
  private FxCopRuleRepository rulesRepository;

  /**
   * Constructs a @link{FxCopPluginHandler}.
   */
  public FxCopPluginHandler(RulesProfile rulesProfile, FxCopRuleRepository fxCopRulesRepository) {
    this.rulesProfile = rulesProfile;
    this.rulesRepository = fxCopRulesRepository;
  }

  public void configure(Project project, MavenPlugin plugin) {
    String[] excludedProjectNames = project.getConfiguration().getStringArray("sonar.skippedModules");
    if (excludedProjectNames != null) {
      String skippedProjectsParam = StringUtils.join(excludedProjectNames, ',');
      plugin.setParameter("skippedProjects", skippedProjectsParam);
    }
    try {
      generateConfigurationFile(project, plugin);
    } catch (IOException e) {
      log.error("Error while generating fxcop conf file", e);
    }
  }

  public String getArtifactId() {
    return ARTIFACT_ID;
  }

  public String getGroupId() {
    return "org.codehaus.sonar-plugins.dotnet";
  }

  public String getVersion() {
    return DOTNET_PLUGIN_VERSION;
  }

  public boolean isFixedVersion() {
    return false;
  }

  public String[] getGoals() {
    return new String[] { "fxcop" };
  }

  /**
   * Extracts the configuration file to use and binds the FXCop Mojo to it.
   * 
   * @param pom
   * @param plugin
   * @throws IOException
   */
  private void generateConfigurationFile(Project project, MavenPlugin plugin) throws IOException {
    String fxCopConfiguration = rulesRepository.exportConfiguration(rulesProfile);
    File configFile = project.getFileSystem().writeToWorkingDirectory(fxCopConfiguration, FX_COP_FILE);
    // Defines the configuration file
    plugin.setParameter("fxCopConfigPath", configFile.getAbsolutePath());
  }

}
