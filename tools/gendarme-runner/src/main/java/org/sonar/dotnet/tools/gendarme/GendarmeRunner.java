/*
 * .NET tools :: Gendarme Runner
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
package org.sonar.dotnet.tools.gendarme;

import java.io.File;
import java.io.IOException;
import java.net.URL;

import org.apache.commons.io.FileUtils;
import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.command.CommandExecutor;
import org.sonar.dotnet.tools.commons.utils.ZipUtils;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;

/**
 * Class that runs the Gendarme program.
 */
public class GendarmeRunner { // NOSONAR : can't mock it otherwise

  private static final Logger LOG = LoggerFactory.getLogger(GendarmeRunner.class);

  private static final String GENDARME_EXECUTABLE = "gendarme.exe";
  private static final long MINUTES_TO_MILLISECONDS = 60000;
  private static final String EMBEDDED_VERSION = "2.10";

  private File gendarmeExecutable;
  private VisualStudioProject vsProject;

  private GendarmeRunner() {
  }

  /**
   * Creates a new {@link GendarmeRunner} object for the given executable file. If the executable file does not exist, then the embedded one
   * will be used.
   * 
   * @param gendarmePath
   *          the full path of the gendarme install directory. For instance: "C:/Program Files/gendarme-2.10-bin". May be null: in this
   *          case, the embedded Gendarme executable will be used.
   * @param tempFolder
   *          the temporary folder where the embedded Gendarme executable will be copied if the gendarmePath does not point to a valid
   *          executable
   */
  public static GendarmeRunner create(String gendarmePath, String tempFolder) throws GendarmeException {
    GendarmeRunner runner = new GendarmeRunner();

    File executable = new File(gendarmePath, GENDARME_EXECUTABLE);
    if ( !executable.exists() || !executable.isFile()) {
      LOG.info("Gendarme executable not found: '{}'. The embedded version ({}) will be used instead.", executable.getAbsolutePath(),
          EMBEDDED_VERSION);
      executable = new File(tempFolder, "gendarme-" + EMBEDDED_VERSION + "-bin/" + GENDARME_EXECUTABLE);
      if ( !executable.isFile()) {
        LOG.info("Extracting Gendarme binaries in {}", tempFolder);
        extractGendarmeBinaries(tempFolder);
      }
    }
    runner.gendarmeExecutable = executable;

    return runner;
  }

  protected static void extractGendarmeBinaries(String tempFolder) throws GendarmeException {
    try {
      URL executableURL = GendarmeRunner.class.getResource("/gendarme-" + EMBEDDED_VERSION + "-bin");
      ZipUtils.extractArchiveFolderIntoDirectory(StringUtils.substringBefore(executableURL.getFile(), "!").substring(5), "gendarme-"
          + EMBEDDED_VERSION + "-bin", tempFolder);
    } catch (IOException e) {
      throw new GendarmeException("Could not extract the embedded Gendarme executable: " + e.getMessage(), e);
    }
  }

  /**
   * Creates a pre-configured {@link GendarmeCommandBuilder} that needs to be completed before running the
   * {@link #execute(GendarmeCommandBuilder, int)} method.
   * 
   * @param solution
   *          the solution to analyse
   * @return the command to complete.
   */
  public GendarmeCommandBuilder createCommandBuilder(VisualStudioSolution solution) {
    GendarmeCommandBuilder builder = GendarmeCommandBuilder.createBuilder(solution);
    builder.setExecutable(gendarmeExecutable);
    return builder;
  }

  /**
   * Creates a pre-configured {@link GendarmeCommandBuilder} that needs to be completed before running the
   * {@link #execute(GendarmeCommandBuilder, int)} method.
   * 
   * @param project
   *          the VS project to analyse
   * @return the command to complete.
   */
  public GendarmeCommandBuilder createCommandBuilder(VisualStudioProject project) {
    this.vsProject = project;
    GendarmeCommandBuilder builder = GendarmeCommandBuilder.createBuilder(project);
    builder.setExecutable(gendarmeExecutable);
    return builder;
  }

  /**
   * Executes the given Gendarme command.
   * 
   * @param gendarmeCommandBuilder
   *          the gendarmeCommandBuilder
   * @param timeoutMinutes
   *          the timeout for the command
   * @throws GendarmeException
   *           if Gendarme fails to execute
   */
  public void execute(GendarmeCommandBuilder gendarmeCommandBuilder, int timeoutMinutes) throws GendarmeException {
    LOG.debug("Executing Gendarme program...");
    try {
      int exitCode = CommandExecutor.create().execute(gendarmeCommandBuilder.toCommand(), timeoutMinutes * MINUTES_TO_MILLISECONDS);
      // Gendarme returns 1 when the analysis is successful but contains violations, so 1 is valid
      if (exitCode != 0 && exitCode != 1) {
        throw new GendarmeException(exitCode);
      }
    } finally {
      cleanupFiles(gendarmeCommandBuilder.getBuildConfigurations());
    }
  }

  protected void cleanupFiles(String buildConfigurations) {
    if (vsProject != null && vsProject.isSilverlightProject()) {
      // need to remove the Silverlight mscorlib.dll
      LOG.debug("Delete Silverlight Mscorlib.dll file");
      FileUtils.deleteQuietly(new File(vsProject.getArtifactDirectory(buildConfigurations), "mscorlib.dll"));
    }
  }

}
