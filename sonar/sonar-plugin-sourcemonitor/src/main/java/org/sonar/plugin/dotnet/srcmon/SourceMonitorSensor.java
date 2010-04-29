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
 * Created on May 5, 2009
 */
package org.sonar.plugin.dotnet.srcmon;

import java.io.File;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.apache.commons.lang.StringUtils;
import org.apache.maven.dotnet.commons.project.DotNetProjectException;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioSolution;
import org.jfree.util.Log;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.batch.maven.MavenPluginHandler;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Measure;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.plugin.dotnet.core.AbstractDotnetSensor;
import org.sonar.plugin.dotnet.core.project.VisualUtils;
import org.sonar.plugin.dotnet.core.resource.CLRAssembly;
import org.sonar.plugin.dotnet.core.resource.CSharpFile;
import org.sonar.plugin.dotnet.core.resource.CSharpFolder;
import org.sonar.plugin.dotnet.srcmon.model.FileMetrics;
import org.sonar.plugin.dotnet.srcmon.model.FolderMetrics;
import org.sonar.plugin.dotnet.srcmon.model.ProjectMetrics;
import org.sonar.plugin.dotnet.srcmon.model.SolutionMetrics;
import org.sonar.plugin.dotnet.srcmon.model.SourceMetric;

/**
 * Extracts the Source Monitor generated metrics and stores them into sonar.
 * 
 * @author Jose CHILLAN May 5, 2009
 */
public class SourceMonitorSensor extends AbstractDotnetSensor
{
  private final static Logger         log = LoggerFactory.getLogger(SourceMonitorSensor.class);

  private Map<String, ProjectMetrics> projects;
  private Map<File, FolderMetrics>    directories;
  private SolutionMetrics             solutionMetrics;
  private SourceMonitorPluginHandler  pluginHandler;
  private VisualStudioSolution        solution;

  public SourceMonitorSensor(SourceMonitorPluginHandler pluginHandler)
  {
    this.projects = new HashMap<String, ProjectMetrics>();
    this.directories = new HashMap<File, FolderMetrics>();
    this.solutionMetrics = new SolutionMetrics();
    this.pluginHandler = pluginHandler;
  }

  /**
   * Collects the project metrics.
   * 
   * @param project
   * @param context
   */
  @Override
  public void analyse(Project project, SensorContext context)
  {
    File report = findReport(project, SourceMonitorPlugin.SOURCE_MONITOR_REPORT);
    SourceMonitorResultParser parser = new SourceMonitorResultParser();
    File basedir = project.getFileSystem().getBasedir();
    List<FileMetrics> files = parser.parse(basedir, report);

    try
    {
      solution = VisualUtils.getSolution(project);
    }
    catch (DotNetProjectException e)
    {
      Log.warn("Could not find a solution for project " + project);
      return;
    }

    collectFileMeasures(project, files, context);
  }

  /**
   * Collects the metrics for a file.
   * 
   * @param pom
   * @param files
   * @param context
   */
  private void collectFileMeasures(Project project, List<FileMetrics> files, SensorContext context)
  {
    // We collect all the files
    for (int idxFile = 0; idxFile < files.size(); idxFile++)
    {
      FileMetrics file = files.get(idxFile);
      processFile(project, context, file);
    }

    storeFolderMetrics(project, context);
    storeProjectMetrics(project, context);
    storeSolutionMetrics(project, context);
  }

  /**
   * Stores the metrics at the solution level.
   * 
   * @param project
   * @param context
   */
  private void storeSolutionMetrics(Project project, SensorContext context)
  {
    int countLines = solutionMetrics.getCountLines();
    int ncloc = countLines
                - solutionMetrics.getCommentLines()
                - solutionMetrics.getDocumentationLines()
                - solutionMetrics.getCountBlankLines();

    context.saveMeasure(CoreMetrics.LINES, (double) countLines);
    // Non comment line of codes (according to its definition for SONAR)
    context.saveMeasure(CoreMetrics.NCLOC, (double) ncloc);

    context.saveMeasure(CoreMetrics.COMMENT_LINES, (double) solutionMetrics.getCommentLines());
    if (ncloc != 0)
    {
      double percentageComments = solutionMetrics.getCommentLines() * 100. / ncloc;
      double percentageDocumentation = (solutionMetrics.getDocumentationLines() * 100.0 / ncloc);
      context.saveMeasure(CoreMetrics.PUBLIC_DOCUMENTED_API_DENSITY, percentageDocumentation);
      context.saveMeasure(CoreMetrics.COMMENT_LINES_DENSITY, percentageComments);
    }

    context.saveMeasure(DotnetSourceMetrics.COUNT_STATEMENTS, (double) solutionMetrics.getCountStatements());
    context.saveMeasure(CoreMetrics.PUBLIC_API, (double) solutionMetrics.getDocumentationLines());
    context.saveMeasure(CoreMetrics.FUNCTIONS, (double) solutionMetrics.getCountMethods());
    context.saveMeasure(CoreMetrics.ACCESSORS, (double) solutionMetrics.getCountAccessors());
    context.saveMeasure(CoreMetrics.STATEMENTS, (double) solutionMetrics.getCountStatements());

    context.saveMeasure(CoreMetrics.CLASSES, (double) solutionMetrics.getCountClasses());
    // We load the complexity
    context.saveMeasure(CoreMetrics.COMPLEXITY, (double) solutionMetrics.getComplexity());
    context.saveMeasure(CoreMetrics.FUNCTION_COMPLEXITY, solutionMetrics.getAverageComplexity());
    context.saveMeasure(CoreMetrics.PACKAGES, (double) solutionMetrics.getCountNamespaces());
    
    // Distributions
    String classDistrib = solutionMetrics.getClassComplexityDistribution();
    String methodDistrib = solutionMetrics.getMethodComplexityDistribution();
    Measure classDistribMeasure = new Measure(CoreMetrics.CLASS_COMPLEXITY_DISTRIBUTION, classDistrib);
    Measure methodDistribMeasure = new Measure(CoreMetrics.FUNCTION_COMPLEXITY_DISTRIBUTION, methodDistrib);
    context.saveMeasure(classDistribMeasure);
    context.saveMeasure(methodDistribMeasure);
  }

  private void storeProjectMetrics(Project project, SensorContext context)
  {
    for (ProjectMetrics metrics : projects.values())
    {
      String assemblyName = metrics.getAssemblyName();
      CLRAssembly resource = CLRAssembly.fromName(project, assemblyName);
      storeMetrics(project, context, metrics, resource);
      context.saveMeasure(resource, CoreMetrics.PACKAGES, (double) metrics.getCountNamespaces());
      
      // Adds distributions
      String classDistrib = solutionMetrics.getClassComplexityDistribution();
      String methodDistrib = solutionMetrics.getMethodComplexityDistribution();
      Measure classDistribMeasure = new Measure(CoreMetrics.CLASS_COMPLEXITY_DISTRIBUTION, classDistrib);
      Measure methodDistribMeasure = new Measure(CoreMetrics.FUNCTION_COMPLEXITY_DISTRIBUTION, methodDistrib);
      context.saveMeasure(resource, classDistribMeasure);
      context.saveMeasure(resource, methodDistribMeasure);

    }
  }

  /**
   * Stores the metrics for a folder.
   * 
   * @param context
   */
  private void storeFolderMetrics(Project project, SensorContext context)
  {
    for (FolderMetrics metrics : directories.values())
    {
      File directory = metrics.getDirectory();

      CSharpFolder folderResource = CSharpFolder.fromDirectory(project, directory);
      // We store the metrics for each folder
      storeMetrics(project, context, metrics, folderResource);
      
      // Adds distributions
      String classDistrib = solutionMetrics.getClassComplexityDistribution();
      String methodDistrib = solutionMetrics.getMethodComplexityDistribution();
      Measure classDistribMeasure = new Measure(CoreMetrics.CLASS_COMPLEXITY_DISTRIBUTION, classDistrib);
      Measure methodDistribMeasure = new Measure(CoreMetrics.FUNCTION_COMPLEXITY_DISTRIBUTION, methodDistrib);
      context.saveMeasure(folderResource, classDistribMeasure);
      context.saveMeasure(folderResource, methodDistribMeasure);

    }
  }

  /**
   * Stores the metrics for a resource
   * 
   * @param context
   * @param metric
   * @param resource
   */
  private void storeMetrics(Project project, SensorContext context, SourceMetric metric, Resource<?> resource)
  {
    int countLines = metric.getCountLines();
    int ncloc = countLines - metric.getCommentLines() - metric.getDocumentationLines() - metric.getCountBlankLines();

    context.saveMeasure(resource, CoreMetrics.LINES, (double) countLines);
    // Non comment line of codes (according to its definition for SONAR)
    context.saveMeasure(resource, CoreMetrics.NCLOC, (double) ncloc);

    context.saveMeasure(resource, DotnetSourceMetrics.COUNT_STATEMENTS, (double) metric.getCountStatements());
    context.saveMeasure(resource, CoreMetrics.COMMENT_LINES, (double) metric.getCommentLines());

    // We compute the densities of comments and documentation only if there is some code
    if (ncloc != 0)
    {
      double percentageComments = metric.getCommentLines() * 100.0 / ncloc;
      double percentageDocumentation = (metric.getDocumentationLines() * 100.0 / ncloc);
      context.saveMeasure(resource, CoreMetrics.COMMENT_LINES_DENSITY, percentageComments);
      context.saveMeasure(resource, CoreMetrics.PUBLIC_DOCUMENTED_API_DENSITY, percentageDocumentation);
    }
    context.saveMeasure(resource, DotnetSourceMetrics.DOCUMENTATION_LINES, (double) metric.getDocumentationLines());
    context.saveMeasure(resource, CoreMetrics.FUNCTIONS, (double) metric.getCountMethods());
    context.saveMeasure(resource, CoreMetrics.ACCESSORS, (double) metric.getCountAccessors());
    context.saveMeasure(resource, CoreMetrics.STATEMENTS, (double) metric.getCountStatements());

    context.saveMeasure(resource, CoreMetrics.CLASSES, (double) metric.getCountClasses());
    // We load the complexity
    context.saveMeasure(resource, CoreMetrics.COMPLEXITY, (double) metric.getComplexity());
    context.saveMeasure(resource, CoreMetrics.FUNCTION_COMPLEXITY, metric.getAverageComplexity());
  }

  private void processFile(Project project, SensorContext context, FileMetrics fileMetric)
  {
    String assemblyName = "";

    // We count the file in the solution
    solutionMetrics.addFile(fileMetric);

    File filePath = fileMetric.getSourcePath();
    VisualStudioProject visualProject = solution.getProject(filePath);
    // We combine the files by assembly
    if (visualProject == null)
    {
      log.warn("Could not find a project for the file : " + filePath);
      return;
    }
    
    // We populate the project specific data
    assemblyName = visualProject.getAssemblyName();
    ProjectMetrics projectMetrics = projects.get(assemblyName);
    if (projectMetrics == null)
    {
      projectMetrics = new ProjectMetrics(assemblyName);
      projects.put(assemblyName, projectMetrics);
    }
    projectMetrics.addFile(fileMetric);

    File sourceDirectory = filePath.getParentFile();
    String projectPath = visualProject.getDirectory().getPath();
    String folder = StringUtils.removeStart(sourceDirectory.getPath(), projectPath);

    FolderMetrics folderMetrics = directories.get(sourceDirectory);
    if (folderMetrics == null)
    {
      folderMetrics = new FolderMetrics(visualProject, sourceDirectory, folder);
      directories.put(sourceDirectory, folderMetrics);
    }
    folderMetrics.addFile(fileMetric);

    // We Store the file results
    CSharpFile sourceFile = CSharpFile.from(project, filePath, false);
    storeMetrics(project, context, fileMetric, sourceFile);
  }

  /**
   * @param project
   * @return
   */
  @Override
  public MavenPluginHandler getMavenPluginHandler(Project project)
  {
    return pluginHandler;
  }

}