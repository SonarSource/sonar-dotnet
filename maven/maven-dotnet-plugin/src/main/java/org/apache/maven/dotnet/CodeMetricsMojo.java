/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexandre.victoor@codehaus.org
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

/*
 * Copyright 2001-2005 The Apache Software Foundation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may
 * obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */

import java.io.File;
import java.io.IOException;
import java.net.URI;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioSolution;
import org.apache.maven.plugin.MojoExecutionException;
import org.apache.maven.plugin.MojoFailureException;

/**
 * Goal that generate the C# metrics
 * 
 * @goal metrics
 * @phase site
 * @description generate a metrics report on a C# project or solution
 */
public class CodeMetricsMojo
  extends AbstractDotNetMojo
{
  /**
   * Name of the resource folder that contains the StyleCop exe
   */
  private final static String RESOURCE_DIR = "metrics";
  /**
   * Name of the folder that will contain the extracted StyleCop exe
   */
  private final static String EXPORT_PATH  = "sourcemonitor-runtime";

  /**
   * Location of the Source Monitor installation
   * 
   * @parameter expression="${sourcemonitor.directory}"
   */
  private File                sourceMonitorDirectory;

  /**
   * Simple name of the Source Monitor executable
   * 
   * @parameter expression="${metrics.source.monitor.executable}" default-value="SourceMonitor.exe"
   */
  private String              sourceMonitorExecutable;

  /**
   * List of the sub-trees of the project to excluded from the analysis
   * 
   * @parameter alias=${excludeSubTree}"
   */
  private String              excludedSubTree;

  /**
   * List of the excluded extensions for the metrics. For example "*.Designer.cs" which is the default value
   * 
   * @parameter alias=${excludedExtensions}"
   */
  private String[] excludedExtensions = {"*.Designer.cs"};
  
  /**
   * Name of the generated metric file.
   * 
   * @parameter alias="${metricsReportFileName}" default-value="metrics-report.xml"
   */
  private String              reportFileName;


  /**
   * Executes the reporting for a solution
   * 
   * @param solution the solution to report
   */
  @Override
  protected void executeSolution(VisualStudioSolution solution) throws MojoExecutionException, MojoFailureException
  {
    SrcMonCommandGenerator generator = new SrcMonCommandGenerator();
    generator.setExcludedExtensions(Arrays.asList(excludedExtensions));
    String version = project.getVersion();
    generator.setCheckPointName(version);
    File outputFile = getReportFile(reportFileName);
    File projectFile = getReportFile(this.reportFileName + ".smp");

    // This is the directory where the command file will be generated
    File workDirectory = getReportDirectory();

    // We delete the files before generation
    deleteFiles(outputFile, projectFile);

    // The command generator is populated
    generator.setGeneratedFile(outputFile.toString());
    generator.setWorkDirectory(workDirectory);
    File projectDir = project.getBasedir();
    generator.setSourcePath(projectDir.toString());
    File sourceMonitorExe = getExecutable();
    generator.setSourceMonitorPath(sourceMonitorExe.toString());
    generator.setProjectFile(projectFile.toString());
    
    // We exclude all the test projects
    List<VisualStudioProject> testProjects = solution.getTestProjects();
    File solutionDir = solution.getSolutionDir();
    for (VisualStudioProject visualStudioProject : testProjects)
    {
      File directory = visualStudioProject.getDirectory();
      try
      {
        URI directoryUri = directory.getCanonicalFile().toURI();
        URI solutionUri = solutionDir.getCanonicalFile().toURI();
        String relativePath = solutionUri.relativize(directoryUri).getPath();
        generator.addExcludedDirectory(relativePath);
      }
      catch (IOException e)
      {
        getLog().debug("Could not compute the relative path for the project " + visualStudioProject);
      }
    }
    
    getLog().info("Launching metrics generation for solution " + solution.getName());
    try
    {
      File commandFile = generator.generateCommandFile();
      List<String> arguments = new ArrayList<String>();
      arguments.add("/C");
      arguments.add(toCommandPath(commandFile));
      launchCommand(sourceMonitorExe, arguments, "Metrics", 0, debug);
      getLog().info("Metrics generated!");
    }
    catch (Exception e)
    {
      throw new MojoExecutionException("Could not execute source monitor", e);
    }

  }

  /**
   * @param visualProject
   * @throws MojoFailureException
   * @throws MojoExecutionException
   */
  @Override
  protected void executeProject(VisualStudioProject visualProject) throws MojoFailureException, MojoExecutionException
  {
    SrcMonCommandGenerator generator = new SrcMonCommandGenerator();
    String version = project.getVersion();
    generator.setCheckPointName(version);
    File outputFile = getReportFile(reportFileName);
    File projectFile = getReportFile(this.reportFileName + ".smp");

    // We delete the files before generation
    deleteFiles(outputFile, projectFile);

    // The generator is populated
    generator.setGeneratedFile(outputFile.toString());
    File projectDir = project.getBasedir();
    generator.setSourcePath(projectDir.toString());
    File sourceMonitorExe = getExecutable();
    generator.setSourceMonitorPath(sourceMonitorExe.toString());
    generator.setProjectFile(projectFile.toString());
    getLog().info("Launching metrics generation for project " + visualProject.getName());
    try
    {
      generator.launch();
    }
    catch (Exception e)
    {
      throw new MojoExecutionException("Could not execute source monitor", e);
    }
  }

  /**
   * @throws MojoFailureException
   * @throws MojoExecutionException
   */
  private File getExecutable() throws MojoFailureException, MojoExecutionException
  {
    if (sourceMonitorDirectory == null)
    {
      // We extract the sourcemonitor.exe if the path is not defined
      sourceMonitorDirectory = extractFolder(RESOURCE_DIR, EXPORT_PATH, "SourceMonitor");
    }
    
    File executable = new File(sourceMonitorDirectory, sourceMonitorExecutable);
    if (!executable.exists())
    {
      getLog().error("Cannot find the SourceMonitor executable :" + executable);
      getLog()
              .error("Please ensure that SourceMonitor is well installed and that the property "
                     + "source.monitor.directory is correctly defined or that the source.monitor.executable is correct in your settings.xml");
      throw new MojoFailureException("Cannot find the SourceMonitor directory");
    }
    return executable;
  }

}
