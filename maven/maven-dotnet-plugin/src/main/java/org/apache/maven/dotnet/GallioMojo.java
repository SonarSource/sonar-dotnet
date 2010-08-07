/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
 *
 * Sonar is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * Sonar is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Sonar; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

/*
 * Created on May 28, 2009
 */
package org.apache.maven.dotnet;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

import org.apache.commons.lang.StringUtils;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioSolution;
import org.apache.maven.plugin.MojoExecutionException;
import org.apache.maven.plugin.MojoFailureException;
import org.apache.maven.plugin.logging.Log;
import org.codehaus.plexus.util.cli.Commandline;

/**
 * Launches the unit tests for a .Net solution or a test project using Gallio
 * 
 * @goal test
 * @phase test
 * @description Maven Mojo that executes unit tests with Gallio, that is multi
 *              test framework compliant
 * @author Jose CHILLAN Apr 9, 2009
 */
public class GallioMojo extends AbstractUnitTestMojo

{
  /**
   * The Gallio exit code when no test has been found.
   */
  private static final int GALLIO_NO_TEST_EXIT_CODE = 16;

  /**
   * Name of the resource folder that contains the Gallio.exe
   */
  static final String RESOURCE_DIR = "gallio";
  /**
   * Name of the folder that will contain the extracted Gallio.exe
   */
  static final String EXPORT_PATH = "gallio-runtime";
  /**
   * Location of the Gallio installation
   * 
   * @parameter expression="${gallio.directory}"
   */
  private File gallioDirectory;

  /**
   * Name of the Gallio executable file
   * 
   * @parameter alias="gallioExecutable" default-value="Gallio.Echo.exe"
   */
  private String gallioExecutable;

  /**
   * Name of the Gallio runner to use
   * 
   * @parameter expression="${gallio.runner}" alias="gallioRunner"
   *            default-value="IsolatedProcess"
   */
  protected String gallioRunner;

  /**
   * The output file for Gallio.
   * 
   * @parameter alias="reportFileName" default-value="gallio-report.xml"
   */
  private String reportFileName;

  /**
   * Set this to 'true' to bypass unit tests entirely. Its use is NOT
   * RECOMMENDED, especially if you enable it using the "maven.test.skip"
   * property, because maven.test.skip disables both running the tests and
   * compiling the tests. Consider using the skipTests parameter instead.
   * 
   * @parameter expression="${maven.test.skip}"
   */
  protected boolean skip;

  /**
   * Set this to 'true' to skip running tests, but still compile them. Its use
   * is NOT RECOMMENDED, but quite convenient on occasion.
   * 
   * @parameter expression="${skipTests}"
   */
  protected boolean skipTests;

  /**
   * Set this to true to ignore a failure during testing. Its use is NOT
   * RECOMMENDED, but quite convenient on occasion.
   * 
   * @parameter expression="${maven.test.failure.ignore}" default-value="false"
   */
  protected boolean testFailureIgnore = false;

  /**
   * Optional test filter for gallio. This can be used to execute only a
   * specific test category (i.e. CategotyName:unit to consider only tests from
   * the 'unit' category)
   * 
   * @parameter expression="${gallio.filter}"
   */
  protected String filter;

  private File reportFile;

  private File gallioExe;

  @Override
  protected void executeProject(VisualStudioProject visualProject)
      throws MojoExecutionException, MojoFailureException {
    throw new MojoFailureException(
        "Unit tests are not supported for a single project");
  }

  @Override
  protected void executeSolution(VisualStudioSolution visualSolution)
      throws MojoExecutionException, MojoFailureException {
    if (skip || skipTests) {
      getLog().info("Tests are skipped.");
      return;
    }

    List<File> testAssemblies = extractTestAssemblies(visualSolution);
    // We launch the tests
    if (!testAssemblies.isEmpty()) {
      launchTests(visualSolution, testAssemblies);
    } else {
      getLog().info(
          "Found no test assembly to launch in the solution "
              + visualSolution.getName());
    }
  }

  /**
   * @param fullPaths
   * @throws MojoExecutionException
   * @throws MojoFailureException
   */
  private int launchTests(VisualStudioSolution visualSolution,
      List<File> testedAssemblies) throws MojoExecutionException,
      MojoFailureException {
    Log log = getLog();

    // We log the command
    boolean throwsFailure = !testFailureIgnore;

    // The command line to execute if generated
    Commandline commandLine = generateTestCommand(visualSolution,
        testedAssemblies);

    String commandName = getCommandName();
    int result = launchCommand(commandLine, commandName, 16, false,
        throwsFailure);
    if (result == GALLIO_NO_TEST_EXIT_CODE) {
      log.warn("No test has been found in assemblies : " + testedAssemblies);
    } else if ((result != 0) && testFailureIgnore) {
      // If the failure are not stopping the execution, we just display a
      // message
      log.error("There are test failures!\n\nPlease refer to "
          + getReportDirectory() + " for test results details");
    }
    return result;
  }

  protected String getCommandName() {
    return "Gallio tests";
  }

  /**
   * Generates the command line to launch.
   * 
   * @param solution
   *          the enclosing solution
   * @param testAssemblies
   *          the test assemblies
   * @return the command line to run
   * @throws MojoExecutionException
   * @throws MojoFailureException
   */
  protected Commandline generateTestCommand(VisualStudioSolution solution,
      List<File> testAssemblies) throws MojoExecutionException,
      MojoFailureException {
    File gallioExe = getGallioExe();
    List<String> arguments = generateGallioArgs(testAssemblies);
    logCommand(testAssemblies);
    Commandline commandLine = generateCommandLine(gallioExe, arguments);
    return commandLine;
  }

  /**
   * Just logs the command
   * 
   * @param testedAssemblies
   * @throws MojoExecutionException
   */
  protected void logCommand(List<File> testedAssemblies)
      throws MojoExecutionException {
    Log log = getLog();
    log.info("Launching Gallio");
    if (log.isDebugEnabled()) {
      File gallioExe = getGallioExe();
      log.debug("Parameters of the Gallio execution");
      log.debug(" - Gallio             : " + gallioExe);
      log.debug(" - Test assemblies    : " + testedAssemblies);
      log.debug(" - Report file        : " + reportFile);
      log.debug(" - Report type        : Xml");
    }
  }

  /**
   * @param testedAssemblies
   * @return
   */
  protected List<String> generateGallioArgs(List<File> testedAssemblies) {
    List<String> arguments = new ArrayList<String>();

    List<String> fullPaths = new ArrayList<String>();
    // We transform the list of files into a list of path
    for (File testedAssembly : testedAssemblies) {
      fullPaths.add(toCommandPath(testedAssembly));
    }
    reportFile = getReportFile(reportFileName);

    if (StringUtils.isNotEmpty(filter)) {
      arguments.add("/f:" + filter);
    }

    // Defines the runner(default is Local)
    // Note that Local is mandatory to use partcover (maybe we can build a
    // Partcover runner ???)
    arguments.add("/r:" + gallioRunner);

    arguments.add("/report-directory:" + reportFile.getParent());
    String reportName = reportFile.getName();
    if (reportName.toLowerCase().endsWith(".xml")) {
      // We remove the terminal .xml that will be added by the Gallio runner
      reportName = reportName.substring(0, reportName.length() - 4);
    }
    arguments.add("/report-name-format:" + reportName);
    arguments.add("/report-type:Xml");
    arguments.addAll(fullPaths);
    return arguments;
  }

  /**
   * Gets the Gallio executable.
   * 
   * @return the Gallio executable path
   * @throws MojoExecutionException
   */
  protected File getGallioExe() throws MojoExecutionException {
    if (gallioDirectory == null) {
      gallioDirectory = extractFolder(RESOURCE_DIR, EXPORT_PATH, "Gallio");
      gallioExe = new File(gallioDirectory, gallioExecutable);
    } else if (gallioExe == null) {
      gallioExe = new File(gallioDirectory, "bin/" + gallioExecutable);
    }
    return gallioExe;
  }

}