/*
 * .NET tools :: DependencyParser Runner
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
package org.sonar.dotnet.tools.dependencyparser;

import java.io.File;
import java.io.IOException;
import java.net.URL;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.command.CommandExecutor;
import org.sonar.dotnet.tools.commons.utils.ZipUtils;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;

/**
 * Class that runs the DependencyParser program.
 */
public class DependencyParserRunner { // NOSONAR : can't mock it otherwise

  private static final Logger LOG = LoggerFactory.getLogger(DependencyParserRunner.class);

  private static final String DEPENDENCYPARSER_EXECUTABLE = "DependencyParser.exe";
  private static final long MINUTES_TO_MILLISECONDS = 60000;
  private static final String EMBEDDED_VERSION = "1.1";

  private File dependencyParserExecutable;

  private DependencyParserRunner() {
  }

  /**
   * Creates a new {@link DependencyParserRunner} object for the given executable file. If the executable file does not exist, then the embedded one
   * will be used.
   * 
   * @param dependencyParserPath
   *          the full path of the gendarme install directory. For instance: "C:/Program Files/DependencyParser". May be null: in this
   *          case, the embedded DependencyParser executable will be used.
   * @param tempFolder
   *          the temporary folder where the embedded DependencyParser executable will be copied if the dependencyParserPath does not point to a valid
   *          executable
   */
  public static DependencyParserRunner create(String dependencyParserPath, String tempFolder) throws DependencyParserException {
    DependencyParserRunner runner = new DependencyParserRunner();

    File executable = new File(dependencyParserPath, DEPENDENCYPARSER_EXECUTABLE);
    if (!executable.exists() || !executable.isFile()) {
      LOG.info("DependencyParser executable not found: '{}'. The embedded version ({}) will be used instead.", executable.getAbsolutePath(),
          EMBEDDED_VERSION);
      executable = new File(tempFolder, "dependencyparser-" + EMBEDDED_VERSION + "/" + DEPENDENCYPARSER_EXECUTABLE);
      if (!executable.isFile()) {
        LOG.info("Extracting DependencyParser binaries in {}", tempFolder);
        extractDependencyParserBinaries(tempFolder);
      }
    }
    runner.dependencyParserExecutable = executable;

    return runner;
  }

  protected static void extractDependencyParserBinaries(String tempFolder) throws DependencyParserException {
    try {
      URL executableURL = DependencyParserRunner.class.getResource("/dependencyparser-" + EMBEDDED_VERSION);
      ZipUtils.extractArchiveFolderIntoDirectory(StringUtils.substringBefore(executableURL.getFile(), "!").substring(5), "dependencyparser-"
          + EMBEDDED_VERSION, tempFolder);
    } catch (IOException e) {
      throw new DependencyParserException("Could not extract the embedded DependencyParser executable: " + e.getMessage(), e);
    }
  }

  /**
   * Creates a pre-configured {@link DependencyParserCommandBuilder} that needs to be completed before running the {@link #execute(DependencyParserCommandBuilder, int)} method.
   * 
   * @param solution
   *          the solution to analyse
   * @param project
   *          the VS project to analyse
   * @return the command to complete.
   */
  public DependencyParserCommandBuilder createCommandBuilder(VisualStudioSolution solution, VisualStudioProject project) {
    DependencyParserCommandBuilder builder = DependencyParserCommandBuilder.createBuilder(solution, project);
    builder.setExecutable(dependencyParserExecutable);
    return builder;
  }

  /**
   * Executes the given DependencyParser command.
   * 
   * @param dependencyParserCommandBuilder
   *          the dependencyParserCommandBuilder
   * @param timeoutMinutes
   *          the timeout for the command
   * @throws DependencyParserException
   *           if DependencyParser fails to execute
   */
  public void execute(DependencyParserCommandBuilder dependencyParserCommandBuilder, int timeoutMinutes) throws DependencyParserException {
    LOG.debug("Executing Gendarme program...");

    int exitCode = CommandExecutor.create().execute(dependencyParserCommandBuilder.toCommand(), timeoutMinutes * MINUTES_TO_MILLISECONDS);
    if (exitCode != 0) {
      throw new DependencyParserException(exitCode);
    }
  }

}
