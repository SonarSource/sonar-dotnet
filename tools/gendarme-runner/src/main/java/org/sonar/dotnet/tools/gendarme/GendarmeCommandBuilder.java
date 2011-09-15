/*
 * .NET tools :: Gendarme Runner
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
package org.sonar.dotnet.tools.gendarme;

import java.io.File;
import java.io.IOException;
import java.util.Collection;
import java.util.Set;

import org.apache.commons.io.FileUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.command.Command;
import org.sonar.dotnet.tools.commons.utils.FileFinder;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;

import com.google.common.collect.Lists;

/**
 * Class used to build the command line to run Gendarme.
 */
public final class GendarmeCommandBuilder {

  private static final Logger LOG = LoggerFactory.getLogger(GendarmeCommandBuilder.class);

  private VisualStudioSolution solution;
  private VisualStudioProject vsProject;
  private File gendarmeExecutable;
  private File silverlightFolder;
  private String gendarmeConfidence = "normal+";
  private String gendarmeSeverity = "all";
  private File gendarmeConfigFile;
  private File gendarmeReportFile;
  private String buildConfigurations = "Debug";
  private String[] assembliesToScan = new String[] {};

  private GendarmeCommandBuilder() {
  }

  /**
   * Constructs a {@link GendarmeCommandBuilder} object for the given Visual Studio solution.
   * 
   * @param solution
   *          the solution to analyse
   *  @param project
   *          the VS project to analyse
   *          
   * @return a Gendarme builder for this solution
   */
  public static GendarmeCommandBuilder createBuilder(VisualStudioSolution solution, VisualStudioProject project) {
    GendarmeCommandBuilder builder = new GendarmeCommandBuilder();
    builder.solution = solution;
    builder.vsProject = project;
    return builder;
  }

  /**
   * Sets the executable
   * 
   * @param gendarmeExecutable
   *          the executable
   * @return the current builder
   */
  public GendarmeCommandBuilder setExecutable(File gendarmeExecutable) {
    this.gendarmeExecutable = gendarmeExecutable;
    return this;
  }

  /**
   * Sets the configuration file to use
   * 
   * @param gendarmeConfigFile
   *          the config file
   * @return the current builder
   */
  public GendarmeCommandBuilder setConfigFile(File gendarmeConfigFile) {
    this.gendarmeConfigFile = gendarmeConfigFile;
    return this;
  }

  /**
   * Sets the report file to generate
   * 
   * @param reportFile
   *          the report file
   * @return the current builder
   */
  public GendarmeCommandBuilder setReportFile(File reportFile) {
    this.gendarmeReportFile = reportFile;
    return this;
  }

  /**
   * Sets the Silverlight folder
   * 
   * @param silverlightFolder
   *          the Silverlight folder
   * @return the current builder
   */
  public GendarmeCommandBuilder setSilverlightFolder(File silverlightFolder) {
    this.silverlightFolder = silverlightFolder;
    return this;
  }

  /**
   * Sets the Gendarme confidence level. By default "normal+" if nothing is specified.
   * 
   * @param gendarmeConfidence
   *          the confidence level
   * @return the current builder
   */
  public GendarmeCommandBuilder setConfidence(String gendarmeConfidence) {
    this.gendarmeConfidence = gendarmeConfidence;
    return this;
  }

  /**
   * Sets the Gendarme severity level. By default "all" if nothing is specified.
   * 
   * @param gendarmeSeverity
   *          the severity level
   * @return the current builder
   */
  public GendarmeCommandBuilder setSeverity(String gendarmeSeverity) {
    this.gendarmeSeverity = gendarmeSeverity;
    return this;
  }

  /**
   * Sets the build configurations. By default, it is "Debug".
   * 
   * @param buildConfigurations
   *          the build configurations
   * @return the current builder
   */
  public GendarmeCommandBuilder setBuildConfigurations(String buildConfigurations) {
    this.buildConfigurations = buildConfigurations;
    return this;
  }
  
  /**
   * Sets the assemblies to scan if the information should not be taken from the VS configuration files.
   * 
   * @param assembliesToScan
   *          the assemblies to scan
   * @return the current builder
   */
  public GendarmeCommandBuilder setAssembliesToScan(String... assembliesToScan) {
    this.assembliesToScan = assembliesToScan;
    return this;
  }
  

  protected String getBuildConfigurations() {
    return buildConfigurations;
  }

  /**
   * Transforms this command object into a Command object that can be passed to the CommandExecutor.
   * 
   * @return the Command object that represents the command to launch.
   */
  public Command toCommand() throws GendarmeException {
    Collection<File> assemblyToScanFiles = findAssembliesToScan();
    validate(assemblyToScanFiles);

    LOG.debug("- Gendarme program    : " + gendarmeExecutable);
    Command command = Command.create(gendarmeExecutable.getAbsolutePath());

    LOG.debug("- Config file         : " + gendarmeConfigFile);
    command.addArgument("--config");
    command.addArgument(gendarmeConfigFile.getAbsolutePath());

    LOG.debug("- Report file         : " + gendarmeReportFile);
    command.addArgument("--xml");
    command.addArgument(gendarmeReportFile.getAbsolutePath());

    LOG.debug("- Quiet output");
    command.addArgument("--quiet");

    LOG.debug("- Confidence          : " + gendarmeConfidence);
    command.addArgument("--confidence");
    command.addArgument(gendarmeConfidence);

    LOG.debug("- Severity            : all");
    command.addArgument("--severity");
    command.addArgument(gendarmeSeverity);

    LOG.debug("- Scanned assemblies  :");
    for (File checkedAssembly : assemblyToScanFiles) {
      LOG.debug("   o " + checkedAssembly);
      command.addArgument(checkedAssembly.getAbsolutePath());
    }

    if (vsProject != null && vsProject.isSilverlightProject()) {
      copySilverlightAssembly();
    }

    return command;
  }

  protected void copySilverlightAssembly() throws GendarmeException {
    File silverlightAssembly = new File(silverlightFolder, "mscorlib.dll");
    if (silverlightAssembly == null || !silverlightAssembly.isFile()) {
      throw new GendarmeException("Could not find Silverlight Mscorlib.dll assembly. Please check your settings.");
    }
    File destinationDirectory = vsProject.getArtifactDirectory(buildConfigurations);
    if (destinationDirectory == null) {
      throw new GendarmeException("Impossible to copy Silverlight Mscorlib.dll as there is no existing artifact "
          + "directory for the build configuration: " + buildConfigurations);
    }
    try {
      LOG.debug("Copy Silverlight Mscorlib.dll file ");
      FileUtils.copyFileToDirectory(silverlightAssembly, destinationDirectory);
    } catch (IOException e) {
      throw new GendarmeException("Cannot copy Silverlight 'mscorlib.dll' file to " + destinationDirectory, e);
    }
  }

  private Collection<File> findAssembliesToScan() {
    final Collection<File> assemblyFiles;
    if (assembliesToScan.length == 0) {
      LOG.debug("No assembly specified: will look into 'csproj' files to find which should be analyzed.");
      assemblyFiles = Lists.newArrayList();
      addProjectAssembly(assemblyFiles, vsProject);
    } else {
      // Some assemblies have been specified: let's analyze them
      assemblyFiles = FileFinder.findFiles(solution, vsProject, assembliesToScan);
    }
    return assemblyFiles;
  }

  private void addProjectAssembly(Collection<File> assemblyFileList, VisualStudioProject visualStudioProject) {
    Set<File> assemblies = visualStudioProject.getGeneratedAssemblies(buildConfigurations);
    for (File assembly : assemblies) {
      if (assembly != null && assembly.isFile()) {
        LOG.debug(" - Found {}", assembly.getAbsolutePath());
        assemblyFileList.add(assembly);
      }
    }
  }
  
  

  protected void validate(Collection<File> assemblyToScanFiles) {
    if (gendarmeConfigFile == null || !gendarmeConfigFile.exists()) {
      throw new IllegalStateException("The Gendarme configuration file does not exist.");
    }
    if (assemblyToScanFiles.isEmpty()) {
      throw new IllegalStateException("No assembly to scan. Please check your project's Gendarme plugin configuration.");
    }
  }
}
