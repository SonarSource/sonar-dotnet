/*
 * .NET tools :: NDeps Runner
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
package org.sonar.dotnet.tools.ndeps;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.SonarException;
import org.sonar.api.utils.command.CommandExecutor;
import org.sonar.plugins.dotnet.api.microsoft.VisualStudioProject;
import org.sonar.plugins.dotnet.api.microsoft.VisualStudioSolution;
import org.sonar.plugins.dotnet.api.utils.ZipUtils;

import java.io.File;
import java.io.IOException;
import java.net.URL;

/**
 * Class that runs the NDeps program.
 */
public class NDepsRunner { // NOSONAR : can't mock it otherwise

  private static final Logger LOG = LoggerFactory.getLogger(NDepsRunner.class);

  private static final String DEPENDENCYPARSER_EXECUTABLE = "DependencyParser.exe";
  private static final long MINUTES_TO_MILLISECONDS = 60000;
  private static final String EMBEDDED_VERSION = "1.1";

  private File nDepsExecutable;

  private NDepsRunner() {
  }

  /**
   * Creates a new {@link NDepsRunner} object for the given executable file. If the executable file does not exist, then the embedded one
   * will be used.
   * 
   * @param nDepsPath
   *          the full path of NDeps install directory. For instance: "C:/Program Files/NDeps". May be null: in this
   *          case, the embedded NDeps executable will be used.
   * @param tempFolder
   *          the temporary folder where the embedded NDeps executable will be copied if the nDepsPath does not point to a valid
   *          executable
   */
  public static NDepsRunner create(String nDepsPath, String tempFolder) throws NDepsException {
    NDepsRunner runner = new NDepsRunner();

    File executable = new File(nDepsPath, DEPENDENCYPARSER_EXECUTABLE);
    if (!executable.exists() || !executable.isFile()) {
      LOG.info("NDeps executable not found: '{}'. The embedded version ({}) will be used instead.", executable.getAbsolutePath(),
          EMBEDDED_VERSION);
      executable = new File(tempFolder, "NDeps-" + EMBEDDED_VERSION + "/" + DEPENDENCYPARSER_EXECUTABLE);
      if (!executable.isFile()) {
        LOG.info("Extracting NDeps binaries in {}", tempFolder);
        extractNDepsBinaries(tempFolder);
      }
    }
    runner.nDepsExecutable = executable;

    return runner;
  }

  protected static void extractNDepsBinaries(String tempFolder) throws NDepsException {
    try {
      URL executableURL = NDepsRunner.class.getResource("/NDeps-" + EMBEDDED_VERSION);
      ZipUtils.extractArchiveFolderIntoDirectory(StringUtils.substringBefore(executableURL.getFile(), "!").substring(5), "NDeps-"
        + EMBEDDED_VERSION, tempFolder);
    } catch (IOException e) {
      throw new SonarException("Could not extract the embedded NDeps executable", e);
    }
  }

  /**
   * Creates a pre-configured {@link NDepsCommandBuilder} that needs to be completed before running the {@link #execute(NDepsCommandBuilder, int)} method.
   * 
   * @param solution
   *          the solution to analyse
   * @param project
   *          the VS project to analyse
   * @return the command to complete.
   */
  public NDepsCommandBuilder createCommandBuilder(VisualStudioSolution solution, VisualStudioProject project) {
    NDepsCommandBuilder builder = NDepsCommandBuilder.createBuilder(solution, project);
    builder.setExecutable(nDepsExecutable);
    return builder;
  }

  /**
   * Executes the given NDeps command.
   * 
   * @param nDepsCommandBuilder
   *          the NDepsCommandBuilder
   * @param timeoutMinutes
   *          the timeout for the command
   * @throws NDepsException
   *           if NDeps fails to execute
   */
  public void execute(NDepsCommandBuilder nDepsCommandBuilder, int timeoutMinutes) throws NDepsException {
    LOG.debug("Executing NDeps program...");

    int exitCode = CommandExecutor.create().execute(nDepsCommandBuilder.toCommand(), timeoutMinutes * MINUTES_TO_MILLISECONDS);
    if (exitCode != 0) {
      throw NDepsException.createFromCode(exitCode);
    }
  }

}
