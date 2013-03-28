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
import org.sonar.api.utils.command.Command;
import org.sonar.plugins.dotnet.api.microsoft.VisualStudioProject;
import org.sonar.plugins.dotnet.api.microsoft.VisualStudioSolution;
import org.sonar.plugins.dotnet.api.tools.CilToolCommandBuilderSupport;

import java.io.File;
import java.util.Collection;

/**
 * Class used to build the command line to run the NDeps tool.
 */
public class NDepsCommandBuilder extends CilToolCommandBuilderSupport { // NOSONAR Not final, because can't be mocked otherwise

  private static final Logger LOG = LoggerFactory.getLogger(NDepsCommandBuilder.class);

  private String patterns;

  private String ignorableFields;

  private NDepsCommandBuilder() {
  }

  /**
   * Constructs a {@link NDepsCommandBuilder} object for the given Visual Studio solution.
   *
   * @param solution
   *          the solution to analyze
   *  @param project
   *          the VS project to analyze
   *
   * @return a DependencyParser builder for this solution
   */
  public static NDepsCommandBuilder createBuilder(VisualStudioSolution solution, VisualStudioProject project) {
    NDepsCommandBuilder builder = new NDepsCommandBuilder();
    builder.solution = solution;
    builder.vsProject = project;
    return builder;
  }

  /**
   * Transforms this command object into a Command object that can be passed to the CommandExecutor.
   *
   * @return the Command object that represents the command to launch.
   */
  public Command toCommand() throws NDepsException {
    // NDeps work only with a single assembly to scan
    Collection<File> assemblyToScanFiles = findAssembliesToScan();
    validate(assemblyToScanFiles);
    File assembly = (File) assemblyToScanFiles.toArray()[0];

    LOG.debug("- DependencyParser program    : " + executable);
    Command command = Command.create(executable.getAbsolutePath());

    LOG.debug("- Assembly            : " + assembly);
    command.addArgument("-a");
    command.addArgument(assembly.getAbsolutePath());

    LOG.debug("- Report file         : " + reportFile);
    command.addArgument("-o");
    command.addArgument(reportFile.getAbsolutePath());

    boolean testProject = vsProject.isTest();
    LOG.debug("- Design analysis         : " + !testProject);
    if (!testProject) {
      command.addArgument("-d");
      command.addArgument("yes");

      if (StringUtils.isNotEmpty(patterns)) {
        command.addArgument("-r");
        command.addArgument(patterns);
      }

      if (StringUtils.isNotEmpty(ignorableFields)) {
        command.addArgument("-i");
        command.addArgument(ignorableFields);
      }
    }

    return command;
  }

  @Override
  protected void validate(Collection<File> assemblyToScanFiles) {
    super.validate(assemblyToScanFiles);

    if (assemblyToScanFiles.size() != 1) {
      throw new IllegalStateException("NDeps support only one Assembly to scan. Project: " + vsProject.getName());
    }

    if (!((File) assemblyToScanFiles.toArray()[0]).exists()) {
      throw new IllegalStateException("Assembly to scan not found for project: " + vsProject.getName());
    }
  }

  public void setPatterns(String patterns) {
    this.patterns = patterns;
  }

  public void setIgnorableFields(String ignorableFields) {
    this.ignorableFields = ignorableFields;
  }
}
