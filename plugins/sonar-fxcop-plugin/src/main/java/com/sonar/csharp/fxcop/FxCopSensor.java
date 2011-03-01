/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.fxcop;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.Sensor;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;

import com.sonar.plugins.csharp.api.tree.CSharpResourcesBridge;

/**
 * Collects the FXCop reporting into sonar.
 */
public class FxCopSensor implements Sensor {

  private final static Logger log = LoggerFactory.getLogger(FxCopSensor.class);

  public FxCopSensor() {
  }

  /**
   * {@inheritDoc}
   */
  public void analyse(Project project, SensorContext context) {

    log.info("----------> Executin FxCop...");
    Resource<?> file = CSharpResourcesBridge.getInstance().getFromTypeName("Example.Core", "Money");
    log.info("----------> " + file.getKey());

  }

  public boolean shouldExecuteOnProject(Project project) {
    return project.getLanguageKey().equals("cs");
  }

}