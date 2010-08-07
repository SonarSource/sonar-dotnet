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
 * Created on Jun 18, 2009
 */
package org.apache.maven.dotnet;

import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.Set;

import org.apache.maven.dotnet.commons.project.ArtifactType;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioSolution;
import org.apache.maven.plugin.MojoExecutionException;
import org.apache.maven.plugin.MojoFailureException;
import org.apache.maven.plugin.logging.Log;
import org.codehaus.plexus.util.FileUtils;

/**
 * Cleans the build objects generated for a .Net project or solution
 * 
 * @goal clean
 * @phase clean
 * @description clean the build object of a .Net project or solution
 * @author Jose CHILLAN Apr 9, 2009
 */
public class CleanMojo extends AbstractDotNetBuildMojo {

  @Override
  protected void executeProject(VisualStudioProject visualProject)
      throws MojoExecutionException, MojoFailureException {
    // Cannot clean a web project alone
    if (visualProject.getType() != ArtifactType.WEB) {
      File csprojFile = visualProject.getProjectFile();
      launchClean(csprojFile);
    }
  }

  @Override
  protected void executeSolution(VisualStudioSolution visualSolution)
      throws MojoExecutionException, MojoFailureException {
    File solutionFile = visualSolution.getSolutionFile();
    launchClean(solutionFile);

  }

  /**
   * Launches the cleaning of a project or solution
   * 
   * @param file
   *          the project or solution file
   * @throws MojoExecutionException
   * @throws MojoFailureException
   */
  public void launchClean(File file) throws MojoExecutionException,
      MojoFailureException {
    File executable = getMsBuildCommand();
    if (!executable.exists()) {
      throw new MojoExecutionException(
          "Could not find the MSBuild executable for the version "
              + toolVersion
              + ". Please "
              + "ensure you have properly defined the properties 'dotnet.2.0.sdk.dir' or 'dotnet.3.5.sdk.dir'");
    }

    Log log = getLog();
    log.info("Launching the cleaning of " + file);
    log.debug(" - Tool Version  : " + toolVersion);
    log.debug(" - MsBuild exe   : " + executable);

    // ASP.NET precompiled directory clean-up
    List<VisualStudioProject> visualStudioProjects = getVisualSolution()
        .getProjects();
    for (VisualStudioProject visualStudioProject : visualStudioProjects) {
      if (visualStudioProject.isWebProject()) {
        log.info("Cleaning precompiled asp.net dlls for project "
            + visualStudioProject);
        File precompilationDirectory = visualStudioProject
            .getWebPrecompilationDirectory();
        try {
          FileUtils.cleanDirectory(precompilationDirectory);
        } catch (IOException e) {
          throw new MojoExecutionException("error while cleaning web project "
              + visualStudioProject, e);
        }
      }
    }

    // We clean all configurations
    List<String> configurations = getBuildConfigurations();
    for (String configuration : configurations) {
      // Launches the clean for each configuration
      List<String> arguments = new ArrayList<String>();
      arguments.add(toCommandPath(file));
      arguments.add("/t:Clean");
      arguments.add("/p:Configuration=" + configuration);

      // We launch the compile command (the logs are put in debug because they
      // may be verbose)
      launchCommand(executable, arguments, "clean", 0, true);
    }

    log.info("Cleaning done!");
  }
}
