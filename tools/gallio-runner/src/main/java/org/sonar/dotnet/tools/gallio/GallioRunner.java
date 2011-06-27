/*
 * .NET tools :: Gallio Runner
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
package org.sonar.dotnet.tools.gallio;

import java.io.File;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.command.CommandExecutor;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioSolution;

/**
 * Class that runs the Gallio program.
 */
public class GallioRunner { // NOSONAR : can't mock it otherwise

  private static final Logger LOG = LoggerFactory.getLogger(GallioRunner.class);

  private static final long MINUTES_TO_MILLISECONDS = 60000;

  private File gallioExecutable;
  private boolean ignoreTestFailures;

  private GallioRunner() {
  }

  /**
   * Creates a new {@link GallioRunner} object for the given executable file. If the executable file does not exist, then the embedded one
   * will be used.
   * 
   * @param gallioPath
   *          the full path of Gallio executable. For instance: "C:/Program Files/Gallio/bin/Gallio.Echo.exe".
   * @param ignoreTestFailures
   *          set to true if the execution of Gallio should not throw an exception in case of test failures
   */
  public static GallioRunner create(String gallioPath, boolean ignoreTestFailures) {
    GallioRunner runner = new GallioRunner();
    runner.gallioExecutable = new File(gallioPath);
    runner.ignoreTestFailures = ignoreTestFailures;
    return runner;
  }

  /**
   * Creates a pre-configured {@link GallioCommandBuilder} that needs to be completed before running the
   * {@link #execute(GallioCommandBuilder, int)} method.
   * 
   * @param solution
   *          the solution to analyse
   * @return the command to complete.
   */
  public GallioCommandBuilder createCommandBuilder(VisualStudioSolution solution) {
    GallioCommandBuilder builder = GallioCommandBuilder.createBuilder(solution);
    builder.setExecutable(gallioExecutable);
    return builder;
  }

  /**
   * Executes the given Gallio command.
   * 
   * @param gallioCommandBuilder
   *          the gallioCommandBuilder
   * @param timeoutMinutes
   *          the timeout for the command
   * @throws GallioException
   *           if Gallio fails to execute
   */
  public void execute(GallioCommandBuilder gallioCommandBuilder, int timeoutMinutes) throws GallioException {
    LOG.debug("Executing Gallio program...");
    int exitCode = CommandExecutor.create().execute(gallioCommandBuilder.toCommand(), timeoutMinutes * MINUTES_TO_MILLISECONDS);
    if (exitCode != 0 && exitCode != 16) {
      if (exitCode == 1 && ignoreTestFailures) {
        return;
      }
      throw new GallioException(exitCode);
    }
  }
}
