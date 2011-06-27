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
import java.util.List;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.command.Command;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioProject;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioSolution;

import com.google.common.collect.Lists;

/**
 * Class used to build the command line to run Gallio.
 */
public final class GallioCommandBuilder {

  private static final Logger LOG = LoggerFactory.getLogger(GallioCommandBuilder.class);

  private VisualStudioSolution solution;
  private File gallioExecutable;
  private File gallioReportFile;
  private String filter;
  private String buildConfigurations = "Debug";

  private GallioCommandBuilder() {
  }

  /**
   * Constructs a {@link GallioCommandBuilder} object for the given Visual Studio solution.
   * 
   * @param solution
   *          the solution to analyse
   * @return a Gallio builder for this solution
   */
  public static GallioCommandBuilder createBuilder(VisualStudioSolution solution) {
    GallioCommandBuilder builder = new GallioCommandBuilder();
    builder.solution = solution;
    return builder;
  }

  /**
   * Sets the install dir for Gallio
   * 
   * @param gallioExecutable
   *          the executable
   * @return the current builder
   */
  public GallioCommandBuilder setExecutable(File gallioExecutable) {
    this.gallioExecutable = gallioExecutable;
    return this;
  }

  /**
   * Sets the report file to generate
   * 
   * @param reportFile
   *          the report file
   * @return the current builder
   */
  public GallioCommandBuilder setReportFile(File reportFile) {
    this.gallioReportFile = reportFile;
    return this;
  }

  /**
   * Sets Gallio test filter. <br/>
   * This can be used to execute only a specific test category (i.e. CategotyName:unit to consider only tests from the 'unit' category)
   * 
   * @param gallioFilter
   *          the filter for Gallio
   * @return the current builder
   */
  public GallioCommandBuilder setFilter(String gallioFilter) {
    this.filter = gallioFilter;
    return this;
  }

  /**
   * Sets the build configurations. By default, it is "Debug".
   * 
   * @param buildConfigurations
   *          the build configurations
   * @return the current builder
   */
  public GallioCommandBuilder setBuildConfigurations(String buildConfigurations) {
    this.buildConfigurations = buildConfigurations;
    return this;
  }

  /**
   * Transforms this command object into a Command object that can be passed to the CommandExecutor.
   * 
   * @return the Command object that represents the command to launch.
   */
  public Command toCommand() throws GallioException {
    List<File> testAssemblies = findTestAssemblies();
    validate(testAssemblies);

    LOG.debug("- Gallio folder : " + gallioExecutable);
    Command command = Command.create(gallioExecutable.getAbsolutePath());

    LOG.debug("- Runner              : IsolatedProcess");
    command.addArgument("/r:IsolatedProcess");

    File reportDirectory = gallioReportFile.getParentFile();
    LOG.debug("- Report directory    : " + reportDirectory.getAbsolutePath());
    command.addArgument("/report-directory:" + reportDirectory.getAbsolutePath());

    String reportName = trimFileReportName();
    LOG.debug("- Report file         : " + reportName);
    command.addArgument("/report-name-format:" + reportName);

    command.addArgument("/report-type:Xml");

    if (StringUtils.isNotEmpty(filter)) {
      LOG.debug("- Filter              : " + filter);
      command.addArgument("/f:" + filter);
    }

    LOG.debug("- Test assemblies     :");
    for (File testAssembly : testAssemblies) {
      LOG.debug("   o " + testAssembly);
      command.addArgument(testAssembly.getAbsolutePath());
    }

    return command;
  }

  private String trimFileReportName() {
    String reportName = gallioReportFile.getName();
    if (reportName.toLowerCase().endsWith(".xml")) {
      // We remove the terminal .xml that will be added by the Gallio runner
      reportName = reportName.substring(0, reportName.length() - 4);
    }
    return reportName;
  }

  protected List<File> findTestAssemblies() throws GallioException {
    List<File> assemblyFileList = Lists.newArrayList();
    if (solution != null) {
      for (VisualStudioProject visualStudioProject : solution.getTestProjects()) {
        addAssembly(assemblyFileList, visualStudioProject);
      }
    } else {
      throw new GallioException("No .NET solution or project has been given to the Gallio command builder.");
    }
    return assemblyFileList;
  }

  protected void addAssembly(List<File> assemblyFileList, VisualStudioProject visualStudioProject) {
    File assembly = visualStudioProject.getArtifact(buildConfigurations);
    if (assembly != null && assembly.isFile()) {
      LOG.debug(" - Found {}", assembly.getAbsolutePath());
      assemblyFileList.add(assembly);
    }
  }

  protected void validate(List<File> testAssemblies) throws GallioException {
    if (gallioExecutable == null || !gallioExecutable.isFile()) {
      throw new GallioException("Gallio executable cannot be found at the following location:" + gallioExecutable);
    }
    if (gallioReportFile == null) {
      throw new GallioException("Gallio report file has not been specified.");
    }
    if (testAssemblies.isEmpty()) {
      throw new GallioException("No test assembly was found. Please check your project's Gallio plugin configuration.");
    }
  }
}
