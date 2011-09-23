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

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.command.Command;
import org.sonar.dotnet.tools.commons.CilToolCommandBuilderSupport;
import org.sonar.dotnet.tools.commons.utils.FileFinder;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;


/**
 * Class used to build the command line to run FxCop.
 */
public class FxCopCommandBuilder extends CilToolCommandBuilderSupport { // NOSONAR Not final, because can't be mocked otherwise

  private static final Logger LOG = LoggerFactory.getLogger(FxCopCommandBuilder.class);
  private static final int DEFAULT_TIMEOUT = 10;
  private static final int MINUTES_TO_SECONDS = 60;
  
  private File silverlightFolder;
  private String[] assemblyDependencyDirectories = new String[] {};
  private boolean ignoreGeneratedCode;
  private int timeoutMinutes = DEFAULT_TIMEOUT;
  

  private FxCopCommandBuilder() {
  }

  /**
   * Constructs a {@link FxCopCommandBuilder} object for the given Visual Studio project.
   * @param solution 
   *          the current VS solution
   * @param project
   *          the VS project to analyze
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
   * Sets the Silverlight folder
   * 
   * @param silverlightFolder
   *          the Silverlight folder
   */
  public void setSilverlightFolder(File silverlightFolder) {
    this.silverlightFolder = silverlightFolder;
  }

  /**
   * Sets the assembly dependencies directories if needed.
   * 
   * @param assemblyDependencyDirectories
   *          the folders containing the dependencies
   * 
   */
  public void setAssemblyDependencyDirectories(String... assemblyDependencyDirectories) {
    this.assemblyDependencyDirectories = assemblyDependencyDirectories;
  }

  /**
   * Sets the parameter that allows to ignore generated code
   * 
   * @param ignoreGeneratedCode
   *          true to ignore generated code
   * 
   */
  public void setIgnoreGeneratedCode(boolean ignoreGeneratedCode) {
    this.ignoreGeneratedCode = ignoreGeneratedCode;
  }

  /**
   * Sets the timeout (in minutes) used for the FxCop plugin.
   * 
   * @param timeout
   *          the timeout
   */
  public void setTimeoutMinutes(int timeout) {
    this.timeoutMinutes = timeout;
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

    LOG.debug("- FxCop program         : " + executable);
    Command command = Command.create(executable.getAbsolutePath());

    LOG.debug("- Project file          : " + configFile);
    command.addArgument("/p:" + configFile.getAbsolutePath());

    LOG.debug("- Report file           : " + reportFile);
    command.addArgument("/out:" + reportFile.getAbsolutePath());

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
    return vsProject.isSilverlightProject();
  }

}
