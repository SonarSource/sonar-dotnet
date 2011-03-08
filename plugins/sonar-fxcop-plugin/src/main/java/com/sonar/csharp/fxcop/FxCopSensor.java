/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.fxcop;

import static com.sonar.csharp.fxcop.Constants.FXCOP_REPORT_XML;
import static com.sonar.csharp.fxcop.Constants.SL_FXCOP_REPORT_XML;

import java.io.File;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.Sensor;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.batch.maven.DependsUponMavenPlugin;
import org.sonar.api.batch.maven.MavenPluginHandler;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.api.rules.RuleFinder;

import com.sonar.csharp.fxcop.maven.FxCopPluginHandler;
import com.sonar.csharp.fxcop.results.FxCopResultParser;
import com.sonar.plugins.csharp.api.tree.CSharpResourcesBridge;

/**
 * Collects the FXCop reporting into sonar.
 */
public class FxCopSensor implements Sensor, DependsUponMavenPlugin {

  private final static Logger log = LoggerFactory.getLogger(FxCopSensor.class);
  private ProjectFileSystem fileSystem;
  private RuleFinder ruleFinder;
  private FxCopPluginHandler pluginHandler;

  public FxCopSensor(ProjectFileSystem fileSystem, RuleFinder ruleFinder, FxCopPluginHandler pluginHandler) {
    this.fileSystem = fileSystem;
    this.ruleFinder = ruleFinder;
    this.pluginHandler = pluginHandler;
  }

  public boolean shouldExecuteOnProject(Project project) {
    return project.getLanguageKey().equals("cs");
  }

  public MavenPluginHandler getMavenPluginHandler(Project project) {
    // TODO: must not use the Maven plugin anymore, but launch the analysis manually
    return pluginHandler;
  }

  /**
   * {@inheritDoc}
   */
  public void analyse(Project project, SensorContext context) {
    final String[] reportFileNames = new String[] { FXCOP_REPORT_XML, SL_FXCOP_REPORT_XML };
    File dir = project.getFileSystem().getBuildDir();

    for (String reportFileName : reportFileNames) {
      File report = new File(dir, reportFileName);
      if (report.exists()) {
        log.info("FxCop report found at location {}", report);
        FxCopResultParser parser = new FxCopResultParser(project, context, ruleFinder, CSharpResourcesBridge.getInstance());
        parser.setEncoding(fileSystem.getSourceCharset());
        parser.parse(report);
      } else {
        log.info("No FxCop report found for path {}", report);
      }
    }
  }

}