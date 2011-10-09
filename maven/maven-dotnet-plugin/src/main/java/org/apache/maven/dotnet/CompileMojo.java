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

/*
 * Created on Apr 21, 2009
 */
package org.apache.maven.dotnet;

import java.io.File;
import java.util.ArrayList;
import java.util.List;
import org.apache.commons.lang.StringUtils;
import org.apache.maven.dotnet.commons.project.ArtifactType;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioSolution;
import org.apache.maven.plugin.MojoExecutionException;
import org.apache.maven.plugin.MojoFailureException;
import org.apache.maven.plugin.logging.Log;

/**
 * Builds a .Net project of solution using MSBuild
 * 
 * @goal compile
 * @phase compile
 * @description compiles a .Net project or solution
 * @author Jose CHILLAN Apr 9, 2009
 */
public class CompileMojo extends AbstractDotNetBuildMojo {
  /**
   * A flag to use to force the rebuild of the project or solution.
   * 
   * @parameter expression="${rebuild}" default-value="false"
   */
  private boolean rebuild;

  /**
   * A flag to disable the pre-build events during a compilation.
   * 
   * @parameter expression="${maven.dotnet.disable.prebuild.event}"
   *            default-value="false" alias="disablePreBuildEvent"
   */
  private boolean disablePreBuildEvent;

  /**
   * A flag to disable the post build events during a compilation.
   * 
   * @parameter expression="${maven.dotnet.disable.postbuild.event}"
   *            default-value="false" alias="disablePostBuildEvent"
   */
  private boolean disablePostBuildEvent;
  
  /**
   * The target platforms for the compilation. Comma may be used to specify several platforms.
   * This parameter is not mandatory.
   * 
   *  @parameter expression="${msbuild.platforms}"
   *            alias="${platforms}" default-value="Any CPU"
   * 
   */
  private String platforms; 
  
  /**
   * Builds a {@link CompileMojo}.
   */
  public CompileMojo() {
  }


    
  /**
   * Launches the compiling of a visual studio project.
   * 
   * @param visualProject
   *          the project to compile
   */
  @Override
  public void executeProject(VisualStudioProject visualProject)
      throws MojoExecutionException, MojoFailureException {
    // Cannot compile a web project alone
    if (visualProject.getType() != ArtifactType.WEB) {
      File csprojFile = visualProject.getProjectFile();
      launchBuild(csprojFile);
    }
  }

  /**
   * Launches the compiling of a visual studio solution.
   * 
   * @param visualSolution
   *          the solution to compile
   */
  @Override
  public void executeSolution(VisualStudioSolution visualSolution)
      throws MojoExecutionException, MojoFailureException {
    File solutionFile = visualSolution.getSolutionFile();
    launchBuild(solutionFile);
  }

  /**
   * Launches the build of a project or solution
   * 
   * @param file
   * @throws MojoExecutionException
   * @throws MojoFailureException
   */
  public void launchBuild(File file) throws MojoExecutionException,
      MojoFailureException {
    File executable = getMsBuildCommand();

    List<String> configurations = getBuildConfigurations();
    for (String configuration : configurations) {

      List<String> arguments = new ArrayList<String>();
      arguments.add(toCommandPath(file));
      // Activates the build or rebuild
      if (rebuild) {
        arguments.add("/t:Rebuild");
      } else {
        arguments.add("/t:Build");
      }
      
      if (parallelBuild) {
        arguments.add("/m");
      }

      // Manages the disabled events
      if (disablePostBuildEvent) {
        // Disable the post build events if required
        arguments.add("/p:PostBuildEvent=\"\"");
        // arguments.add("/p:PostBuildEventUseInBuild=false");
      }
      if (disablePreBuildEvent) {
        // Disable the pre-build events if required
        // arguments.add("/p:PreBuildEventUseInBuild=false");
        arguments.add("/p:PreBuildEvent=\"\"");
      }

      // For VS 2010 uncomment those lines
      // if (disablePreLinkEvent)
      // {
      // Disable the pre-link events if required
      // arguments.add("/p:PreLinkEventUseInBuild=false");
      // }

      // Case of disabled debug symbols
      if (!generatePdb) {
        arguments.add("/p:DebugSymbols=false");
        arguments.add("/p:DebugType=None");
      }

      // Adds the configuration
      arguments.add("/p:Configuration=" + configuration);

      String[] platformArray = StringUtils.split(platforms, ",;");
      for (String platform : platformArray) {
        List<String> platformArguments = new ArrayList<String>(arguments);
        
        platformArguments.add("/p:Platform=" + platform ); 
        
        Log log = getLog();
        log.info("Launching the build of " + file);
        log.debug(" - Tool Version  : " + toolVersion);
        log.debug(" - MsBuild exe   : " + executable);
        log.debug(" - Configuration : " + configuration
            + (rebuild ? " (force rebuild)" : ""));

        // We launch the compile command (the logs are put in debug because they
        // may be verbose)
        launchCommand(executable, platformArguments, "build", 0);
        log.info("Build of " + solutionName + " in configuration "
            + configuration + " terminated with SUCCESS!");
      }
      
      
      
    }
  }
}
