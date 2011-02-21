/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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

package org.apache.maven.dotnet;

import java.io.File;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

import org.apache.commons.lang.StringUtils;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioSolution;
import org.apache.maven.plugin.MojoExecutionException;
import org.apache.maven.plugin.MojoFailureException;
import org.apache.maven.plugin.logging.Log;

/**
 * Generates a quality report for a .Net project or solution using FxCop
 * 
 * @goal fxcop
 * @phase site
 * @description generates a FxCop report on a .Net project or solution
 * @author Jose CHILLAN Apr 9, 2009
 */
public class FxCopMojo extends AbstractCilRuleBasedMojo {

  public final static String DEFAULT_FX_COP_PROJECT = "default-rules.fxcop";

  /**
   * Name of the resource folder that contains the FxCop runtime
   */
  private final static String RESOURCE_DIR = "fxcop";
  /**
   * Name of the extracted folder that will contain the extracted FXCop.exe
   */
  private final static String EXPORT_PATH = "fxcop-runtime";

  /**
   * Location of the FxCop installation
   * 
   * @parameter expression="${fxcop.directory}"
   */
  private String fxCopDirectoryPath;
  
  private File fxCopDirectory;
  
  /**
   * Specifies additional directories to search for assembly dependencies. Useful
   * in particular when assembly bindings have been used. ';' is used as a 
   * delimiter between paths.
   * 
   * @parameter expression="${fxcop.additionalDirectories}"
   */
  private String fxCopAdditionalDirectoryPaths;

  /**
   * Name of the FxCop command line executable.
   * 
   * @parameter expression="${fxcop.executable}" default-value="FxCopCmd.exe"
   */
  private String fxCopExecutable = "FxCopCmd.exe";

  /**
   * Name of the FxCop configuration that contains the rules to use
   * 
   * @parameter  expression="${fxcop.config}"
   */
  private String fxCopConfigPath;
  
  private File fxCopConfigFile;

  /**
   * Name of the FxCop report file
   * 
   * @parameter alias="${fxCopReportName}" default-value="fxcop-report.xml"
   */
  private String fxCopReportName;
  
  /**
   * Name of the FxCop report file for the silverlight projects
   * 
   * @parameter alias="${silverlightFxCopReportName}" default-value="silverlight-fxcop-report.xml"
   */
  private String silverlightFxCopReportName;


  /**
   * Enable/disable the ignore generated code option
   * 
   * @parameter expression="${fxcop.ignore.generated.code}"
   *            alias="ignoreGeneratedCode" default-value="true"
   */
  private boolean ignoreGeneratedCode = true;

  private File executablePath;

  public FxCopMojo() {
  }

  /**
   * Launches the report for a solution
   * 
   * @param solution
   *          the solution to check
   * @throws MojoFailureException
   * @throws MojoExecutionException
   */
  @Override
  protected void executeSolution(VisualStudioSolution solution)
      throws MojoFailureException, MojoExecutionException {
    
    if (solution.isSilverlightUsed()) {
      List<File> slCheckedAssemblies = extractSilverlightAssemblies(solution);
      launchReport(slCheckedAssemblies, true, false);
    }
    List<File> checkedAssemblies = extractNonSilverlightAssemblies(solution);
    launchReport(checkedAssemblies, false, solution.isAspUsed());
  }

  /**
   * Launches the report for a project.
   * 
   * @param visualProject
   *          the project to execute
   * @throws MojoFailureException
   * @throws MojoExecutionException
   */
  @Override
  protected void executeProject(VisualStudioProject visualProject)
      throws MojoFailureException, MojoExecutionException {
    if (visualProject.isTest()) {
      getLog().info(
          "No FxCop report generated for the test project "
              + visualProject.getName());
      return;
    }
    File assembly = getGeneratedAssembly(visualProject);
    if (!assembly.exists()) {
      // No assembly found
      throw new MojoFailureException(
          "Cannot find the generated assembly to launch FxCop " + assembly);
    }
    launchReport(
        Collections.singletonList(assembly), 
        visualProject.isSilverlightProject(), 
        visualProject.isWebProject()
      );
  }

  /**
   * Launches the reporting for a list of assemblies
   * 
   * @param assemblies
   *          the assemblies to check
   * @param aspUsed TODO
   * @throws MojoExecutionException
   *           if an execution problem occurred
   * @throws MojoFailureException
   *           in case of execution failure
   */
  private void launchReport(List<File> assemblies, boolean silverlightUsed, boolean aspUsed)
      throws MojoExecutionException, MojoFailureException {
    Log log = getLog();
    if (assemblies.isEmpty()) {
      log.info("No assembly to check with FxCop");
      return;
    }
    prepareFxCopProject();
    log.info("Launching FxCop report for " + project.getName());
    // We retrieve the required files
    prepareExecutable();
    
    
    // We build the command arguments
    List<String> commandArguments = new ArrayList<String>();
    
    // things are a little bit different for silverlight
    final File reportFile;
    if (silverlightUsed) {
      reportFile = getReportFile(silverlightFxCopReportName);
      final File silverlightMscorlibLocation = getSilverlightMscorlibLocation();
      commandArguments.add("/d:" + toCommandPath(silverlightMscorlibLocation));
    } else {
      reportFile = getReportFile(fxCopReportName);
    }

    

    // We define the project file
    log.debug("- Project file : " + fxCopConfigFile);
    commandArguments.add("/p:" + toCommandPath(fxCopConfigFile));

    // Defines the report file
    log.debug("- Report file  : " + reportFile);
    commandArguments.add("/out:" + toCommandPath(reportFile));

    // Ignores the generated code is required
    if (ignoreGeneratedCode) {
      commandArguments.add("/ignoregeneratedcode");
    }

    // Put in verbose mode if required
    if (verbose) {
      commandArguments.add("/v");
    }
    // Add the assemblies to check
    log.debug("- Scanned assemblies :");
    for (File checkedAssembly : assemblies) {
      log.debug("   o " + checkedAssembly);
      commandArguments.add("/f:" + toCommandPath(checkedAssembly));
    }
    // Add additional directories
    if (!StringUtils.isEmpty(fxCopAdditionalDirectoryPaths)) {
      log.debug("- Additional directories :");
      String[] pathArray = StringUtils.split(fxCopAdditionalDirectoryPaths, ';');
      for (String path : pathArray) {
        File additionalDirectory = new File(path);
        log.debug("   o " + additionalDirectory);
        commandArguments.add("/d:" + toCommandPath(additionalDirectory));
      }
    }

    commandArguments.add("/gac");
    
    if (aspUsed) {
      commandArguments.add("/aspnet");
    }
    
    // We launch the command (and we accept the reference problems)
    launchCommand(executablePath, commandArguments, "FxCop", 0x200, false);
    log.info("FxCop report generated");
  }

  /**
   * Prepares the FxCop executable.
   * 
   * @throws MojoExecutionException
   */
  private void prepareExecutable() throws MojoExecutionException {
    if (StringUtils.isEmpty(fxCopDirectoryPath)) {
      fxCopDirectory = extractFolder(RESOURCE_DIR, EXPORT_PATH, "FxCop");
    } else {
      fxCopDirectory = new File(fxCopDirectoryPath);
    }
    executablePath = new File(fxCopDirectory, fxCopExecutable);
  }

  protected void prepareFxCopProject() throws MojoExecutionException {
    if (StringUtils.isEmpty(fxCopConfigPath)) {
      File reportDirectory = getReportDirectory();
      fxCopConfigFile = extractResource(reportDirectory,
          DEFAULT_FX_COP_PROJECT, DEFAULT_FX_COP_PROJECT, "fxcop project");
    } else {
      fxCopConfigFile = new File(fxCopConfigPath);
      if (!fxCopConfigFile.exists()) {
        throw new MojoExecutionException(
            "Could not find the fxcop project file: " + fxCopConfigFile);
      }
    }
    getLog().debug("Using FxCop project file: " + fxCopConfigFile);
  }

}