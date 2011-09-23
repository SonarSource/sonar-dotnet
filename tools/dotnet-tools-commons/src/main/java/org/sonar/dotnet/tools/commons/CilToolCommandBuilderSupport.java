/*
 * .NET tools :: Commons
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

package org.sonar.dotnet.tools.commons;

import java.io.File;
import java.util.Collection;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.command.Command;
import org.sonar.dotnet.tools.commons.utils.FileFinder;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;



/**
 * Support class for the cil based rule engines
 * 
 * @author Alexandre Victoor
 *
 */
public abstract class CilToolCommandBuilderSupport {
  
  private static final Logger LOG = LoggerFactory.getLogger(CilToolCommandBuilderSupport.class);
  
  protected VisualStudioSolution solution;
  protected VisualStudioProject vsProject;
  protected File configFile;
  protected File executable;
  protected String buildConfigurations = "Debug";
  protected File reportFile;

  protected String[] assembliesToScan = new String[] {};
  
  
  public abstract Command toCommand() throws Exception;
  
  /**
   * Set the assemblies to scan if the information should not be taken from the VS configuration files.
   * 
   * @param assembliesToScan
   *          the assemblies to scan
   *
   */
  public void setAssembliesToScan(String... assembliesToScan) {
    this.assembliesToScan = assembliesToScan;
  }
  
  /**
   * Set the build configurations. By default, it is "Debug".
   * 
   * @param buildConfigurations
   *          the build configurations
   *          
   */
  public void setBuildConfigurations(String buildConfigurations) {
    this.buildConfigurations = buildConfigurations;
  }
  
  /**
   * Set the configuration file that must be used to perform the analysis. It is mandatory.
   * 
   * @param configFile
   *          the file
   * @return the current builder
   */
  public void setConfigFile(File configFile) {
    this.configFile = configFile;
  }
  
  /**
   * Sets the executable
   * 
   * @param executable
   *          the executable
   * 
   */
  public void setExecutable(File fxCopExecutable) {
    this.executable = fxCopExecutable;
  }
  
  /**
   * Sets the report file to generate
   * 
   * @param reportFile
   *          the report file
   * 
   */
  public void setReportFile(File reportFile) {
    this.reportFile = reportFile; 
  }
  
  protected Collection<File> findAssembliesToScan() {
    Collection<File> assemblyFiles;
    
    if (assembliesToScan.length == 0) {
      LOG.debug("No assembly specified: will look into 'csproj' files to find which should be analyzed.");
      assemblyFiles = vsProject.getGeneratedAssemblies(buildConfigurations);
    } else {
      // Some assemblies have been specified: let's analyze them
      assemblyFiles = FileFinder.findFiles(solution, vsProject, assembliesToScan);
      if (assemblyFiles.isEmpty()) {
        LOG.warn("No assembly found using patterns " + StringUtils.join(assembliesToScan,','));
        LOG.warn("Fallback to 'csproj' files to find which should be analyzed.");
        assemblyFiles = vsProject.getGeneratedAssemblies(buildConfigurations);
      }
    }
    return assemblyFiles;
  }
  
  protected void validate(Collection<File> assemblyToScanFiles) {
    if (configFile == null || !configFile.exists()) {
      throw new IllegalStateException("The configuration file does not exist.");
    }
    if (assemblyToScanFiles.isEmpty()) {
      throw new IllegalStateException(
          "No assembly to scan. Please check your project's FxCop plugin configuration ('sonar.dotnet.assemblies' property).");
    }
  }
}
