/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.fxcop.runner;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

import org.apache.commons.configuration.Configuration;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.resources.ProjectFileSystem;

import com.google.common.collect.Lists;
import com.sonar.csharp.fxcop.FxCopConstants;

/**
 * Class used to build the command line to run FxCop.
 */
public class FxCopCommand {

  private static final Logger log = LoggerFactory.getLogger(FxCopCommand.class);

  private ProjectFileSystem fileSystem;
  private int timeoutMinutes = FxCopConstants.TIMEOUT_MINUTES_DEFVALUE;
  private String fxCopCommand;
  private File fxCopConfigFile;
  private String[] assembliesToScan;
  private List<File> assemblyFilesToScan;
  private File reportFile;

  /**
   * Constructs a {@link FxCopCommand} object.
   * 
   * @param configuration
   *          FxCop configuration elements
   * @param fileSystem
   *          the file system of the project
   */
  public FxCopCommand(Configuration configuration, ProjectFileSystem fileSystem) {
    this.fileSystem = fileSystem;
    this.timeoutMinutes = configuration.getInt(FxCopConstants.TIMEOUT_MINUTES_KEY, FxCopConstants.TIMEOUT_MINUTES_DEFVALUE);
    this.fxCopCommand = configuration.getString(FxCopConstants.EXECUTABLE_KEY, FxCopConstants.EXECUTABLE_DEFVALUE);
    this.assembliesToScan = configuration.getStringArray(FxCopConstants.ASSEMBLIES_TO_SCAN_KEY);
    reportFile = new File(fileSystem.getBuildDir(), FxCopConstants.FXCOP_REPORT_XML);
  }

  /**
   * Sets FxCop configuration file that must be used to perform the analysis. It is mandatory.
   * 
   * @param fxCopConfigFile
   *          the file
   */
  public void setFxCopConfigFile(File fxCopConfigFile) {
    this.fxCopConfigFile = fxCopConfigFile;
  }

  /**
   * Returns the timeout used for the FxCop plugin.
   * 
   * @return the timeout
   */
  public int getTimeoutMinutes() {
    return timeoutMinutes;
  }

  /**
   * Transforms this command object into a array of string that can be passed to the CommandExecutor.
   * 
   * @return the array of strings that represent the command to launch.
   */
  public String[] toArray() {
    assemblyFilesToScan = getAssembliesToScan();
    validate();

    List<String> command = new ArrayList<String>();

    log.debug("- FxCop program      : " + fxCopCommand);
    command.add(fxCopCommand);

    log.debug("- Project file       : " + fxCopConfigFile);
    command.add("/p:" + fxCopConfigFile.getAbsolutePath());

    log.debug("- Report file        : " + reportFile);
    command.add("/out:" + reportFile.getAbsolutePath());

    log.debug("- Scanned assemblies :");
    for (File checkedAssembly : assemblyFilesToScan) {
      log.debug("   o " + checkedAssembly);
      command.add("/f:" + checkedAssembly.getAbsolutePath());
    }

    command.add("/gac");

    return command.toArray(new String[command.size()]);
  }

  private List<File> getAssembliesToScan() {
    List<File> assemblies = Lists.newArrayList();
    File basedir = fileSystem.getBasedir();
    for (int i = 0; i < assembliesToScan.length; i++) {
      String assemblyPath = assembliesToScan[i].trim();
      File assembly = new File(basedir, assemblyPath);
      if (assembly == null || !assembly.exists()) {
        log.warn("The following assembly is supposed to be analyzed, but it can't be found: " + assemblyPath);
      } else {
        assemblies.add(assembly);
      }
    }
    return assemblies;
  }

  private void validate() {
    if (fxCopConfigFile == null || !fxCopConfigFile.exists()) {
      throw new IllegalStateException("The FxCop configuration file does not exist.");
    }
    if (assemblyFilesToScan.isEmpty()) {
      throw new IllegalStateException(
          "No assembly to scan. Please check your project's FxCop plugin configuration ('sonar.fxcop.assemblies' property).");
    }
  }
}
