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

package org.apache.maven.dotnet;

import java.io.File;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

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
public class FxCopMojo extends AbstractDotNetMojo {

	public final static String DEFAULT_FX_COP_PROJECT = "default-rules.fxcop";
	public final static String DEFAULT_FX_COP_REPORT_NAME = "fxcop-report.xml";

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
	private File fxCopDirectory;

	/**
	 * Name of the FxCop command line executable.
	 * 
	 * @parameter expression="${fxcop.executable}" default-value="FxCopCmd.exe"
	 */
	private String fxCopExecutable = "FxCopCmd.exe";

	/**
	 * Name of the FxCop configuration that contains the rules to use
	 * 
	 * @parameter alias="${fxCopConfigFile}"
	 */
	File fxCopConfigFile;

	/**
	 * Name of the FxCop report file
	 * 
	 * @parameter alias="${fxCopReportName}" default-value="fxcop-report.xml"
	 */
	private String fxCopReportName = DEFAULT_FX_COP_REPORT_NAME;

	/**
	 * A set of dll name patterns to include in the analysis
	 * 
	 * @parameter alias="includes"
	 */
	private String[] includes = { "target/Debug/**/*.dll" };

	/**
	 * A set of dll name patterns to exclude from the analysis
	 * 
	 * @parameter alias="excludes"
	 */
	private String[] excludes = { "**/obj/**/*" };

	/**
	 * Enable/disable the verbose mode for FxCop
	 * 
	 * @parameter expression="${verbose}"
	 */
	private boolean verbose;

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
		List<File> checkedAssemblies = extractAssemblies(solution);
		launchReport(checkedAssemblies);
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
		launchReport(Collections.singletonList(assembly));
	}

	/**
	 * Launches the reporting for a list of assemblies
	 * 
	 * @param assemblies
	 *          the assemblies to check
	 * @throws MojoExecutionException
	 *           if an execution problem occurred
	 * @throws MojoFailureException
	 *           in case of execution failure
	 */
	protected void launchReport(List<File> assemblies)
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
		File reportFile = getReportFile(fxCopReportName, DEFAULT_FX_COP_REPORT_NAME);

		// We build the command arguments
		List<String> commandArguments = new ArrayList<String>();

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

		commandArguments.add("/gac");
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
		if (fxCopDirectory == null) {
			fxCopDirectory = extractFolder(RESOURCE_DIR, EXPORT_PATH, "FxCop");
		}
		executablePath = new File(fxCopDirectory, fxCopExecutable);
	}

	protected void prepareFxCopProject() throws MojoExecutionException {
		if (fxCopConfigFile == null) {
			File reportDirectory = getReportDirectory();
			fxCopConfigFile = extractResource(reportDirectory,
			    DEFAULT_FX_COP_PROJECT, DEFAULT_FX_COP_PROJECT, "fxcop project");
		} else {
			if (!fxCopConfigFile.exists()) {
				throw new MojoExecutionException(
				    "Could not find the fxcop project file: " + fxCopConfigFile);
			}
		}
		getLog().debug("Using FxCop project file: " + fxCopConfigFile);
	}

}