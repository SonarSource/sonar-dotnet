/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.fxcop.runner;

import java.io.File;

import org.apache.commons.configuration.Configuration;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.BatchExtension;
import org.sonar.api.resources.ProjectFileSystem;

/**
 * Class that runs the FxCop program.
 */
public class FxCopRunner implements BatchExtension {

  private static final Logger LOG = LoggerFactory.getLogger(FxCopRunner.class);

  private FxCopCommand command;

  /**
   * Constructs a {@link FxCopRunner}.
   * 
   * @param configuration
   *          FxCop configuration elements
   * @param fileSystem
   *          the file system of the project
   */
  public FxCopRunner(Configuration configuration, ProjectFileSystem projectFileSystem) {
    this.command = new FxCopCommand(configuration, projectFileSystem);
  }

  /**
   * Launches the FxCop program.
   * 
   * @param fxCopConfigFile
   *          the FxCop config file to use
   */
  public void execute(File fxCopConfigFile) {
    LOG.debug("Executing FxCop program");
    command.setFxCopConfigFile(fxCopConfigFile);
    new CommandExecutor().execute(command.toArray(), command.getTimeoutMinutes() * 60);
  }

}
