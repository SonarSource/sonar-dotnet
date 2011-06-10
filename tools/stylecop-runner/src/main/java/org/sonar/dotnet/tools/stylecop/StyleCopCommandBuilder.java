/*
 * .NET tools :: StyleCop Runner
 * Copyright (C) 2011 Jose Chillan, Alexandre Victoor and SonarSource
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

package org.sonar.dotnet.tools.stylecop;

import java.io.File;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.command.Command;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioProject;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioSolution;

/**
 * Class used to build the command line to run StyleCop.
 */
public final class StyleCopCommandBuilder {

  private static final Logger LOG = LoggerFactory.getLogger(StyleCopCommandBuilder.class);

  private VisualStudioSolution solution;
  private VisualStudioProject vsProject;
  private File styleCopConfigFile;
  private File styleCopReportFile;
  private File dotnetSdkDirectory;
  private File styleCopFolder;

  private StyleCopCommandBuilder() {
  }

  /**
   * Constructs a {@link StyleCopCommandBuilder} object for the given Visual Studio solution.
   * 
   * @param solution
   *          the solution to analyse
   * @return a StyleCop builder for this solution
   */
  public static StyleCopCommandBuilder createBuilder(VisualStudioSolution solution) {
    StyleCopCommandBuilder builder = new StyleCopCommandBuilder();
    builder.solution = solution;
    return builder;
  }

  /**
   * Constructs a {@link StyleCopCommandBuilder} object for the given Visual Studio project.
   * 
   * @param solution
   *          the solution that contains the VS project
   * @param project
   *          the VS project to analyse
   * @return a StyleCop builder for this project
   */
  public static StyleCopCommandBuilder createBuilder(VisualStudioSolution solution, VisualStudioProject project) {
    StyleCopCommandBuilder builder = createBuilder(solution);
    builder.vsProject = project;
    return builder;
  }

  /**
   * Sets StyleCop configuration file that must be used to perform the analysis. It is mandatory.
   * 
   * @param styleCopConfigFile
   *          the file
   * @return the current builder
   */
  public StyleCopCommandBuilder setConfigFile(File styleCopConfigFile) {
    this.styleCopConfigFile = styleCopConfigFile;
    return this;
  }

  /**
   * Sets the report file to generate
   * 
   * @param reportFile
   *          the report file
   * @return the current builder
   */
  public StyleCopCommandBuilder setReportFile(File reportFile) {
    this.styleCopReportFile = reportFile;
    return this;
  }

  /**
   * Sets the directory where MSBuild.exe is.
   * 
   * @param dotnetSdkDirectory
   *          the directory
   * @return the current builder
   */
  protected StyleCopCommandBuilder setDotnetSdkDirectory(File dotnetSdkDirectory) {
    this.dotnetSdkDirectory = dotnetSdkDirectory;
    return this;
  }

  /**
   * Sets the directory where StyleCop is installed.
   * 
   * @param styleCopFolder
   *          the install folder
   * @return the current builder
   */
  protected StyleCopCommandBuilder setStyleCopFolder(File styleCopFolder) {
    this.styleCopFolder = styleCopFolder;
    return this;
  }

  /**
   * Transforms this command object into a array of string that can be passed to the CommandExecutor, and generates the required MSBuild
   * file to execute StyleCop.
   * 
   * @return the Command that represent the command to launch.
   */
  public Command toCommand() {
    validate();
    MsBuildFileGenerator msBuildFileGenerator = new MsBuildFileGenerator(solution, styleCopConfigFile, styleCopReportFile, styleCopFolder);
    File msBuildFile = msBuildFileGenerator.generateFile(styleCopReportFile.getParentFile(), vsProject);

    LOG.debug("- MSBuild path          : " + dotnetSdkDirectory.getAbsolutePath());
    Command command = Command.create(new File(dotnetSdkDirectory, "MSBuild.exe").getAbsolutePath());

    LOG.debug("- Application Root      : " + solution.getSolutionDir().getAbsolutePath());
    command.addArgument("/p:AppRoot=" + solution.getSolutionDir().getAbsolutePath());

    LOG.debug("- Target to run         : StyleCopLaunch");
    command.addArgument("/target:StyleCopLaunch");

    command.addArgument("/noconsolelogger");

    LOG.debug("- MSBuild file          : " + msBuildFile.getAbsolutePath());
    command.addArgument(msBuildFile.getAbsolutePath());

    return command;
  }

  private void validate() {
    if (styleCopConfigFile == null || !styleCopConfigFile.exists()) {
      throw new IllegalStateException("The StyleCop configuration file does not exist.");
    }
  }

}
