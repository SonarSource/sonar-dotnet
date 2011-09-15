/*
 * .NET tools :: FxCop Runner
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
package org.sonar.dotnet.tools.fxcop;

import java.io.File;
import java.util.Collection;
import java.util.Set;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.command.Command;
import org.sonar.dotnet.tools.commons.utils.FileFinder;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;

import com.google.common.collect.Lists;

/**
 * Class used to build the command line to run FxCop.
 */
public class FxCopCommandBuilder { // NOSONAR Not final, because can't be mocked otherwise

  private static final Logger LOG = LoggerFactory.getLogger(FxCopCommandBuilder.class);
  private static final int DEFAULT_TIMEOUT = 10;
  private static final int MINUTES_TO_SECONDS = 60;

  private VisualStudioSolution solution;
  private VisualStudioProject vsProject;
  private File fxCopExecutable;
  private File fxCopConfigFile;
  private File fxCopReportFile;
  private File silverlightFolder;
  private String[] assembliesToScan = new String[] {};
  private String[] assemblyDependencyDirectories = new String[] {};
  private boolean ignoreGeneratedCode;
  private int timeoutMinutes = DEFAULT_TIMEOUT;
  private String buildConfigurations = "Debug";

  private FxCopCommandBuilder() {
  }

  /**
   * Constructs a {@link FxCopCommandBuilder} object for the given Visual Studio project.
   * @param solution 
   *          the current VS solution
   * @param project
   *          the VS project to analyse
   * 
   * @return a FxCop builder for this project
   */
  public static FxCopCommandBuilder createBuilder(VisualStudioSolution solution, VisualStudioProject project) {
    FxCopCommandBuilder builder = new FxCopCommandBuilder();
    builder.solution = solution;
    builder.vsProject = project;
    return builder;
  }

  /**
   * Sets the executable
   * 
   * @param fxCopExecutable
   *          the executable
   * @return the current builder
   */
  public FxCopCommandBuilder setExecutable(File fxCopExecutable) {
    this.fxCopExecutable = fxCopExecutable;
    return this;
  }

  /**
   * Sets FxCop configuration file that must be used to perform the analysis. It is mandatory.
   * 
   * @param fxCopConfigFile
   *          the file
   * @return the current builder
   */
  public FxCopCommandBuilder setConfigFile(File fxCopConfigFile) {
    this.fxCopConfigFile = fxCopConfigFile;
    return this;
  }

  /**
   * Sets the report file to generate
   * 
   * @param reportFile
   *          the report file
   * @return the current builder
   */
  public FxCopCommandBuilder setReportFile(File reportFile) {
    this.fxCopReportFile = reportFile;
    return this;
  }

  /**
   * Sets the Silverlight folder
   * 
   * @param silverlightFolder
   *          the Silverlight folder
   * @return the current builder
   */
  public FxCopCommandBuilder setSilverlightFolder(File silverlightFolder) {
    this.silverlightFolder = silverlightFolder;
    return this;
  }

  /**
   * Sets the assemblies to scan if the information should not be taken from the VS configuration files.
   * 
   * @param assembliesToScan
   *          the assemblies to scan
   * @return the current builder
   */
  public FxCopCommandBuilder setAssembliesToScan(String... assembliesToScan) {
    this.assembliesToScan = assembliesToScan;
    return this;
  }

  /**
   * Sets the assembly dependencies directories if needed.
   * 
   * @param assemblyDependencyDirectories
   *          the folders containing the dependencies
   * @return the current builder
   */
  public FxCopCommandBuilder setAssemblyDependencyDirectories(String... assemblyDependencyDirectories) {
    this.assemblyDependencyDirectories = assemblyDependencyDirectories;
    return this;
  }

  /**
   * Sets the parameter that allows to ignore generated code
   * 
   * @param ignoreGeneratedCode
   *          true to ignore generated code
   * @return the current builder
   */
  public FxCopCommandBuilder setIgnoreGeneratedCode(boolean ignoreGeneratedCode) {
    this.ignoreGeneratedCode = ignoreGeneratedCode;
    return this;
  }

  /**
   * Sets the timeout (in minutes) used for the FxCop plugin.
   * 
   * @param timeout
   *          the timeout
   * @return the current builder
   */
  public FxCopCommandBuilder setTimeoutMinutes(int timeout) {
    this.timeoutMinutes = timeout;
    return this;
  }

  /**
   * Sets the build configurations. By default, it is "Debug".
   * 
   * @param buildConfigurations
   *          the build configurations
   * @return the current builder
   */
  public FxCopCommandBuilder setBuildConfigurations(String buildConfigurations) {
    this.buildConfigurations = buildConfigurations;
    return this;
  }

  /**
   * Transforms this command object into a array of string that can be passed to the CommandExecutor.
   * 
   * @return the Command that represent the command to launch.
   */
  public Command toCommand() throws FxCopException {
    Collection<File> assemblyToScanFiles = findAssembliesToScan();
    Collection<File> assemblyDependencyDirectoriesFiles 
      = FileFinder.findDirectories(solution, vsProject, assemblyDependencyDirectories);
    validate(assemblyToScanFiles);

    LOG.debug("- FxCop program         : " + fxCopExecutable);
    Command command = Command.create(fxCopExecutable.getAbsolutePath());

    LOG.debug("- Project file          : " + fxCopConfigFile);
    command.addArgument("/p:" + fxCopConfigFile.getAbsolutePath());

    LOG.debug("- Report file           : " + fxCopReportFile);
    command.addArgument("/out:" + fxCopReportFile.getAbsolutePath());

    LOG.debug("- Scanned assemblies    :");
    for (File checkedAssembly : assemblyToScanFiles) {
      LOG.debug("   o " + checkedAssembly);
      command.addArgument("/f:" + checkedAssembly.getAbsolutePath());
    }

    LOG.debug("- Assembly dependencies :");
    for (File assemblyDependencyDir : assemblyDependencyDirectoriesFiles) {
      LOG.debug("   o " + assemblyDependencyDir);
      command.addArgument("/d:" + assemblyDependencyDir.getAbsolutePath());
    }

    if (isSilverlightUsed()) {
      if (silverlightFolder == null || !silverlightFolder.isDirectory()) {
        throw new FxCopException("The following Silverlight directory does not exist, please check your plugin configuration: "
            + (silverlightFolder == null ? "NULL" : silverlightFolder.getPath()));
      }
      LOG.debug("   o [Silverlight] " + silverlightFolder.getAbsolutePath());
      command.addArgument("/d:" + silverlightFolder.getAbsolutePath());
    }

    if (ignoreGeneratedCode) {
      LOG.debug("- Ignoring generated code");
      command.addArgument("/igc");
    }

    command.addArgument("/to:" + timeoutMinutes * MINUTES_TO_SECONDS);

    command.addArgument("/gac");

    if (isAspUsed()) {
      command.addArgument("/aspnet");
    }

    return command;
  }

  private boolean isAspUsed() {
    boolean isAspUsed = false;
    if (vsProject != null) {
      isAspUsed = vsProject.isWebProject();
    } else if (solution != null) {
      isAspUsed = solution.isAspUsed();
    }
    return isAspUsed;
  }

  private boolean isSilverlightUsed() {
    boolean isSilverlightUsed = vsProject.isSilverlightProject();
    return isSilverlightUsed;
  }

  private Collection<File> findAssembliesToScan() {
    final Collection<File> assemblyFiles;
    if (assembliesToScan.length == 0) {
      LOG.debug("No assembly specified: will look into 'csproj' files to find which should be analyzed.");
      assemblyFiles = Lists.newArrayList();
      addProjectAssembly(assemblyFiles, vsProject);
    } else {
      // Some assemblies have been specified: let's analyze them
      assemblyFiles = FileFinder.findFiles(solution, vsProject, assembliesToScan);
    }
    return assemblyFiles;
  }

  private void addProjectAssembly(Collection<File> assemblyFileList, VisualStudioProject visualStudioProject) {
    Set<File> assemblies = visualStudioProject.getGeneratedAssemblies(buildConfigurations);
    for (File assembly : assemblies) {
      if (assembly != null && assembly.isFile()) {
        LOG.debug(" - Found {}", assembly.getAbsolutePath());
        assemblyFileList.add(assembly);
      }
    }
  }

  private void validate(Collection<File> assemblyToScanFiles) {
    if (fxCopConfigFile == null || !fxCopConfigFile.exists()) {
      throw new IllegalStateException("The FxCop configuration file does not exist.");
    }
    if (assemblyToScanFiles.isEmpty()) {
      throw new IllegalStateException(
          "No assembly to scan. Please check your project's FxCop plugin configuration ('sonar.fxcop.assemblies' property).");
    }
  }

}
