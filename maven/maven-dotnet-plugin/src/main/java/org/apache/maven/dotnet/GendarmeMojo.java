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
import java.io.IOException;
import java.text.MessageFormat;
import java.util.ArrayList;
import java.util.List;
import java.util.Set;

import org.apache.commons.lang.StringUtils;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioSolution;
import org.apache.maven.plugin.MojoExecutionException;
import org.apache.maven.plugin.MojoFailureException;
import org.apache.maven.plugin.logging.Log;
import org.codehaus.plexus.util.FileUtils;

/**
 * Generates a quality report for a .Net project or solution using mono gendarme
 * 
 * @goal gendarme
 * @phase site
 * @description generates a mono-gendarme report on a .Net project or solution
 * @author Alexandre Victoor
 * 
 */
public class GendarmeMojo extends AbstractCilRuleBasedMojo {

  private static final String MSCORLIB_DLL = "mscorlib.dll";

  /**
   * Name of the resource folder that contains the mono gendarme runtime
   */
  private final static String RESOURCE_DIR = "gendarme";
  /**
   * Name of the extracted folder that will contain the extracted gendarme.exe
   */
  private final static String EXPORT_PATH = "gendarme-runtime";

  /**
   * Location of the mono gendarme installation
   * 
   * @parameter expression="${gendarme.directory}"
   */
  private String gendarmeDirectoryPath;


  /**
   * Name of the mono gendarme command line executable.
   * 
   * @parameter expression="${gendarme.executable}" default-value="gendarme.exe"
   */
  private String gendarmeExecutable = "gendarme.exe";

  /**
   * Pattern name of the gendarme report files.
   * One file per project of the solution.
   * The pattern syntax is the good old java.text.MessageFormat one.
   * {0} is the project/assembly name.
   * 
   * @parameter alias="${gendarmeReportName}"
   *            default-value="gendarme-report-{0}.xml"
   */
  private String gendarmeReportName;


  /**
   * Path to the gendarme config file that specifies rule settings
   * 
   * @parameter expression="${gendarme.config}"
   */
  private String gendarmeConfigFile;
  
  /**
   * Path to the gendarme ignore config file
   * 
   * @parameter expression="${gendarme.ignore}"
   */
  private String gendarmeIgnoreFile;
  
  /**
   * Filter defects for the specified confidence levels.
   * confidence [all | [[low | normal | high | total][+|-]],...
   * 
   * @parameter expression="${gendarme.confidence}" default-value="normal+"
   */
  private String confidence;
  
  /**
   * Filter defects for the specified severity levels.
   * severity [all | [[audit | low | medium | high | critical][+|-]]],...
   * 
   * @parameter expression="${gendarme.severity}" default-value="all"
   */
  private String severity;


  /**
   * The gendarme.exe file 
   */
  private File executableFile;

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
    
    // We retrieve the required executable files
    prepareExecutable();
    
    List<VisualStudioProject> projects = solution.getProjects();
    for (VisualStudioProject project : projects) {
      if (!project.isTest()) {
        launchReport(project);
      }
    }
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
          "No gendarme report generated for the test project "
              + visualProject.getName());
      return;
    }
    File assembly = visualProject.getArtifact(buildConfigurations);
    if (!assembly.exists()) {
      // No assembly found
      throw new MojoFailureException(
          "Cannot find the generated assembly to launch gendarme " + assembly);
    }
    
    // We retrieve the required executable files
    prepareExecutable();
    
    launchReport(visualProject);
  }
  
  /**
   * Launches the reporting for a list of assemblies
   * 
   * @param assemblies
   *          the assemblies to check
   * @param silverlightUsed
   *          flag that indicates if silverlight is used in one of the assemblies
   * @throws MojoExecutionException
   *           if an execution problem occurred
   * @throws MojoFailureException
   *           in case of execution failure
   */
  private void launchReport(VisualStudioProject visualProject)
      throws MojoExecutionException, MojoFailureException {
    final Log log = getLog();
    
    Set<File> assemblies = visualProject.getGeneratedAssemblies(buildConfigurations);
    
    if (assemblies.isEmpty()) {
      log.info("No assembly to check with Gendarme");
      return;
    }
    
    if (visualProject.isSilverlightProject()) {
      // mscorlib.dll need to be in the same directory
      // of one of the analyzed assemblies. We take
      // the first one of the list
      final File destinationDirectory 
        = visualProject.getArtifactDirectory(buildConfigurations);
      final File silverlightMscorlibLocation = getSilverlightMscorlibLocation();
      
      try {
        File mscorlibFile = new File(silverlightMscorlibLocation, MSCORLIB_DLL); //NOSONAR field written by plexus/maven
        FileUtils.copyFileToDirectory(mscorlibFile, destinationDirectory);
      } catch (IOException e) {
        log.error(e);
        throw new MojoFailureException(
            "Cannot copy custom mscorlib.dll file to " + destinationDirectory
           );
      }
    }

    final String projectGendarmeReportName 
      = MessageFormat.format(gendarmeReportName, visualProject.getName());
 
    final File reportFile = getReportFile(projectGendarmeReportName);  //NOSONAR field written by plexus/maven

    // We build the command arguments
    final List<String> commandArguments = new ArrayList<String>();

    // Defines the report file
    log.debug("- Report file  : " + reportFile);
    commandArguments.add("--xml");
    commandArguments.add(toCommandPath(reportFile));

    if (StringUtils.isNotEmpty(gendarmeConfigFile)) {
      commandArguments.add("--config");
      commandArguments.add(gendarmeConfigFile);
    }
    
    if (StringUtils.isNotEmpty(gendarmeIgnoreFile)) {
      commandArguments.add("--ignore");
      commandArguments.add(gendarmeIgnoreFile);
    }

    // Put in verbose mode if required
    if (verbose) {
      commandArguments.add("--v");
    }
    
    // put the severity level param
    commandArguments.add("--severity");
    commandArguments.add(severity);
    
    // put the confidence level param
    commandArguments.add("--confidence");
    commandArguments.add(confidence);
    
    
    // Add the assemblies to check
    log.debug("- Scanned assemblies :");
    for (File checkedAssembly : assemblies) {
      log.debug("   o " + checkedAssembly);
      commandArguments.add(toCommandPath(checkedAssembly));
    }

    launchCommand(executableFile, commandArguments, "gendarme", 1);
    log.info("gendarme report generated");
    
    // clean up needed
    if (visualProject.isSilverlightProject()) {
      final File destinationDirectory 
        = visualProject.getArtifactDirectory(buildConfigurations);
      new File(destinationDirectory, MSCORLIB_DLL).delete();
    }
  }

  /**
   * Launches the reporting for a list of assemblies
   * 
   * @param assemblies
   *          the assemblies to check
   * @param silverlightUsed
   *          flag that indicates if silverlight is used in one of the assemblies
   * @throws MojoExecutionException
   *           if an execution problem occurred
   * @throws MojoFailureException
   *           in case of execution failure
   */
  private void launchReport(List<File> assemblies, boolean silverlightUsed)
      throws MojoExecutionException, MojoFailureException {
    final Log log = getLog();
    if (assemblies.isEmpty()) {
      log.info("No assembly to check with Gendarme");
      return;
    }
    
    if (silverlightUsed) {
      // mscorlib.dll need to be in the same directory
      // of one of the analyzed assemblies. We take
      // the first one of the list
      final File destinationDirectory 
        = assemblies.get(0).getParentFile();
      final File silverlightMscorlibLocation = getSilverlightMscorlibLocation();
      
      try {
        File mscorlibFile = new File(silverlightMscorlibLocation, MSCORLIB_DLL); //NOSONAR field written by plexus/maven
        FileUtils.copyFileToDirectory(mscorlibFile, destinationDirectory);
      } catch (IOException e) {
        log.error(e);
        throw new MojoFailureException(
            "Cannot copy custom mscorlib.dll file to " + destinationDirectory
           );
      }
    }
    

    // We retrieve the required files
    prepareExecutable();

    File reportFile = getReportFile(gendarmeReportName);  //NOSONAR field written by plexus/maven

    // We build the command arguments
    List<String> commandArguments = new ArrayList<String>();

    // Defines the report file
    log.debug("- Report file  : " + reportFile);
    commandArguments.add("--xml");
    commandArguments.add(toCommandPath(reportFile));

    if (StringUtils.isNotEmpty(gendarmeConfigFile)) {
      commandArguments.add("--config");
      commandArguments.add(gendarmeConfigFile);
    }

    // Put in verbose mode if required
    if (verbose) {
      commandArguments.add("--v");
    }
    
    // put the severity level param
    commandArguments.add("--severity");
    commandArguments.add(severity);
    
    // put the confidence level param
    commandArguments.add("--confidence");
    commandArguments.add(confidence);
    
    
    // Add the assemblies to check
    log.debug("- Scanned assemblies :");
    for (File checkedAssembly : assemblies) {
      log.debug("   o " + checkedAssembly);
      commandArguments.add(toCommandPath(checkedAssembly));
    }

    launchCommand(executableFile, commandArguments, "gendarme", 1);
    log.info("gendarme report generated");
    
    // clean up needed
    if (silverlightUsed) {
      File destinationDirectory = assemblies.get(0).getParentFile();
      new File(destinationDirectory, MSCORLIB_DLL).delete();
    }
  }

  /**
   * Prepares the Gendarme executable.
   * 
   * @throws MojoExecutionException
   */
  private void prepareExecutable() throws MojoExecutionException {
    final File gendarmeDirectory; 
    if (StringUtils.isEmpty(gendarmeDirectoryPath)) {
      gendarmeDirectory = extractFolder(RESOURCE_DIR, EXPORT_PATH, "Gendarme");
    } else{
      gendarmeDirectory = new File(gendarmeDirectoryPath);
    }
    executableFile = new File(gendarmeDirectory, gendarmeExecutable);
  }

}
