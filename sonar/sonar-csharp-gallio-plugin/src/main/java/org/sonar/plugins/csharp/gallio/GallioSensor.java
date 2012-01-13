/*
 * Sonar C# Plugin :: Gallio
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
package org.sonar.plugins.csharp.gallio;

import java.io.File;
import java.util.Collection;
import java.util.Collections;
import java.util.List;
import java.util.Set;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.DependedUpon;
import org.sonar.api.batch.DependsUpon;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.api.utils.SonarException;
import org.sonar.dotnet.tools.commons.utils.FileFinder;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.dotnet.tools.gallio.GallioCommandBuilder;
import org.sonar.dotnet.tools.gallio.GallioException;
import org.sonar.dotnet.tools.gallio.GallioRunner;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.sensor.AbstractCSharpSensor;

import com.google.common.collect.Lists;
import com.google.common.collect.Sets;

/**
 * Executes Gallio only once in the Solution directory to generate test execution and coverage reports.
 */
@DependsUpon(CSharpConstants.CSHARP_CORE_EXECUTED)
@DependedUpon(GallioConstants.BARRIER_GALLIO_EXECUTED)
public class GallioSensor extends AbstractCSharpSensor {

  private static final Logger LOG = LoggerFactory.getLogger(GallioSensor.class);

  private CSharpConfiguration configuration;

  /**
   * Constructs a {@link GallioSensor}.
   * 
   * @param fileSystem
   * @param configuration
   * @param microsoftWindowsEnvironment
   */
  public GallioSensor(CSharpConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    super(microsoftWindowsEnvironment, "Gallio", configuration.getString(GallioConstants.MODE, ""));
    this.configuration = configuration;
  }

  /**
   * {@inheritDoc}
   */
  public boolean shouldExecuteOnProject(Project project) {
    if (MODE_REUSE_REPORT.equals(executionMode)) {
      LOG.info("Gallio won't execute as it is set to 'reuseReport' mode.");
      return false;
    }
    if (getMicrosoftWindowsEnvironment().isTestExecutionDone()) {
      LOG.info("Gallio won't execute as test execution has already been done.");
      return false;
    }
    if (getMicrosoftWindowsEnvironment().getCurrentSolution() != null
        && getMicrosoftWindowsEnvironment().getCurrentSolution().getTestProjects().isEmpty()) {
      LOG.info("Gallio won't execute as there are no test projects.");
      return false;
    }

    return super.shouldExecuteOnProject(project);
  }
  
  
  private void addAssembly(Collection<File> assemblyFileList, VisualStudioProject visualStudioProject) {
    String buildConfigurations = configuration.getString(CSharpConstants.BUILD_CONFIGURATIONS_KEY,
        CSharpConstants.BUILD_CONFIGURATIONS_DEFVALUE);
    File assembly = visualStudioProject.getArtifact(buildConfigurations);
    if (assembly != null && assembly.isFile()) {
      assemblyFileList.add(assembly);
    }
  }
  
  private List<File> findTestAssemblies(VisualStudioSolution solution, String[] testAssemblyPatterns) throws GallioException {
    Set<File> assemblyFiles = Sets.newHashSet();
    if (solution != null) {
      if (testAssemblyPatterns.length == 0) {
        for (VisualStudioProject visualStudioProject : solution.getTestProjects()) {
          addAssembly(assemblyFiles, visualStudioProject);
        }
      } else {
        for (VisualStudioProject visualStudioProject : solution.getTestProjects()) {
          Collection<File> projectTestAssemblies 
            = FileFinder.findFiles(solution, visualStudioProject, testAssemblyPatterns);
          if (projectTestAssemblies.isEmpty()) {
            addAssembly(assemblyFiles, visualStudioProject);
          } else {
            assemblyFiles.addAll(projectTestAssemblies);
          }
        }
      }
      
    } else {
      throw new GallioException("No .NET solution or project has been given to the Gallio command builder.");
    }
    return Lists.newArrayList(assemblyFiles);
  }

  @Override
  public void analyse(Project project, SensorContext context) {
    
    VisualStudioSolution solution = getMicrosoftWindowsEnvironment().getCurrentSolution();
    
    File workDir = 
      new File(solution.getSolutionDir(), getMicrosoftWindowsEnvironment().getWorkingDirectory());
    if ( !workDir.exists()) {
      workDir.mkdirs();
    }
    
    String[] testAssemblyPatterns = configuration.getStringArray(GallioConstants.TEST_ASSEMBLIES_KEY);
    boolean safeMode = configuration.getBoolean(GallioConstants.SAFE_MODE, false);
    int timeout = configuration.getInt(GallioConstants.TIMEOUT_MINUTES_KEY, GallioConstants.TIMEOUT_MINUTES_DEFVALUE);
    
    try {
      
      List<File> testAssemblies = findTestAssemblies(solution, testAssemblyPatterns);
      
      if (safeMode) {
        for (File assembly : testAssemblies) {
          File gallioReportFile = new File(workDir, assembly.getName() + "." + GallioConstants.GALLIO_REPORT_XML);
          File coverageReportFile = new File(workDir, assembly.getName() + "." + GallioConstants.GALLIO_COVERAGE_REPORT_XML);
          GallioRunner runner = createRunner(project, context, workDir);
          GallioCommandBuilder builder = createBuilder(runner, Collections.singletonList(assembly), gallioReportFile, coverageReportFile);
          runner.execute(builder, timeout);
        }
      } else {
        File gallioReportFile = new File(workDir, GallioConstants.GALLIO_REPORT_XML);
        File coverageReportFile = new File(workDir, GallioConstants.GALLIO_COVERAGE_REPORT_XML);
        GallioRunner runner = createRunner(project, context, workDir);
        GallioCommandBuilder builder = createBuilder(runner, testAssemblies, gallioReportFile, coverageReportFile);
        runner.execute(builder, timeout);
      }
    } catch (GallioException e) {
      throw new SonarException("Gallio execution failed.", e);
    }

    // tell that tests were executed so that no other project tries to launch them a second time
    getMicrosoftWindowsEnvironment().setTestExecutionDone();
    
  }
  
  private GallioRunner createRunner(Project project, SensorContext context, File workDir) {
    // create runner
    File gallioInstallDir = new File(configuration.getString(GallioConstants.INSTALL_FOLDER_KEY, GallioConstants.INSTALL_FOLDER_DEFVALUE)); 
    GallioRunner runner = GallioRunner.create(gallioInstallDir.getAbsolutePath(), workDir.getAbsolutePath(), true);
    return runner;
  }
  
  private GallioCommandBuilder createBuilder(GallioRunner runner, List<File> testAssemblies, File gallioReportFile, File coverageReportFile) {
    GallioCommandBuilder builder = runner.createCommandBuilder(getMicrosoftWindowsEnvironment().getCurrentSolution());

    // Add info for Gallio execution
    builder.setReportFile(gallioReportFile);
    builder.setFilter(configuration.getString(GallioConstants.FILTER_KEY, GallioConstants.FILTER_DEFVALUE));

    builder.setGallioRunnerType(configuration.getString(GallioConstants.RUNNER_TYPE_KEY, null));
    builder.setTestAssemblies(testAssemblies);
    //builder.setTestAssemblyPatterns(configuration.getStringArray(GallioConstants.TEST_ASSEMBLIES_KEY));

    // Add info for coverage execution
    builder.setCoverageReportFile(coverageReportFile);
    builder.setCoverageTool(configuration.getString(GallioConstants.COVERAGE_TOOL_KEY, GallioConstants.COVERAGE_TOOL_DEFVALUE));
    builder.setCoverageExcludes(configuration
        .getString(GallioConstants.COVERAGE_EXCLUDES_KEY, GallioConstants.COVERAGE_EXCLUDES_DEFVALUE));
    builder.setPartCoverInstallDirectory(new File(configuration.getString(GallioConstants.PART_COVER_INSTALL_KEY,
        GallioConstants.PART_COVER_INSTALL_DEFVALUE)));
    builder.setOpenCoverInstallDirectory(new File(configuration.getString(GallioConstants.OPEN_COVER_INSTALL_KEY,
        GallioConstants.OPEN_COVER_INSTALL_DEFVALUE)));
    /*builder.setBuildConfigurations(configuration.getString(CSharpConstants.BUILD_CONFIGURATIONS_KEY,
        CSharpConstants.BUILD_CONFIGURATIONS_DEFVALUE));*/

    return builder;
  }

}