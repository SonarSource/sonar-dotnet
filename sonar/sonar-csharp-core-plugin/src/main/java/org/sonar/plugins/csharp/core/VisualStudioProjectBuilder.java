/*
 * Sonar C# Plugin :: Core
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
package org.sonar.plugins.csharp.core;

import java.io.File;
import java.io.IOException;
import java.util.Collection;
import java.util.Properties;

import org.apache.commons.io.FileUtils;
import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.bootstrap.ProjectBuilder;
import org.sonar.api.batch.bootstrap.ProjectDefinition;
import org.sonar.api.batch.bootstrap.ProjectReactor;
import org.sonar.api.utils.SonarException;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.api.DotNetToolsException;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.visualstudio.ModelFactory;
import org.sonar.plugins.csharp.api.visualstudio.SourceFile;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioProject;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioSolution;

/**
 * Project Builder created and executed once per build to override the project definition, based on the Visual Studio files found in the
 * sources.
 */
public class VisualStudioProjectBuilder extends ProjectBuilder {

  private static final Logger LOG = LoggerFactory.getLogger(VisualStudioProjectBuilder.class);

  private CSharpConfiguration configuration;
  private MicrosoftWindowsEnvironment microsoftWindowsEnvironment;

  /**
   * Creates a new {@link VisualStudioProjectBuilder}
   * 
   * @param reactor
   *          the reactor
   * @param configuration
   *          the shared C# configuration
   * @param microsoftWindowsEnvironment
   *          the shared Microsoft Windows Environment
   */
  public VisualStudioProjectBuilder(ProjectReactor reactor, CSharpConfiguration configuration,
      MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    super(reactor);
    this.configuration = configuration;
    this.microsoftWindowsEnvironment = microsoftWindowsEnvironment;
  }

  @Override
  protected void build(ProjectReactor reactor) {
    if ("cs".equals(configuration.getString("sonar.language", ""))) {
      LOG.debug("Executing VisualStudioProjectBuilder");
      ProjectDefinition root = reactor.getRoot();

      // First, read all the plugin configuration details related to MS Windows
      retrieveMicrosoftWindowsEnvironmentConfig();

      // Then create the Visual Studio Solution object from the ".sln" file
      createVisualStudioSolution(root.getBaseDir());

      // And finally create the Sonar projects definition
      createMultiProjectStructure(root);

      // lock the MicrosoftWindowsEnvironment object so that nobody can modify it afterwards
      microsoftWindowsEnvironment.lock();
    }
  }

  private void createMultiProjectStructure(ProjectDefinition root) {
    VisualStudioSolution currentSolution = microsoftWindowsEnvironment.getCurrentSolution();
    root.resetSourceDirs();
    LOG.debug("- Root Project: {}", root.getName());
    String workDir = root.getWorkDir().getAbsolutePath().substring(root.getBaseDir().getAbsolutePath().length() + 1);
    microsoftWindowsEnvironment.setWorkingDirectory(workDir);
    Properties rootProps = enhanceRootProperties(root);

    for (VisualStudioProject vsProject : currentSolution.getProjects()) {
      ProjectDefinition subProject = ProjectDefinition.create((Properties) rootProps.clone()).setBaseDir(vsProject.getDirectory())
          .setWorkDir(new File(vsProject.getDirectory(), workDir))
          .setKey(StringUtils.substringBefore(root.getKey(), ":") + ":" + StringUtils.deleteWhitespace(vsProject.getName()))
          .setVersion(root.getVersion()).setName(vsProject.getName()).addContainerExtension(microsoftWindowsEnvironment);

      if (vsProject.isTest()) {
        subProject.setTestDirs(".");
        for (SourceFile sourceFile : vsProject.getSourceFiles()) {
          subProject.addTestFiles(sourceFile.getFile());
        }
      } else {
        subProject.setSourceDirs(".");
        for (SourceFile sourceFile : vsProject.getSourceFiles()) {
          subProject.addSourceFiles(sourceFile.getFile());
        }
      }

      LOG.debug("  - Adding Sub Project => {}", subProject.getName());
      root.addSubProject(subProject);
    }
  }

  protected Properties enhanceRootProperties(ProjectDefinition root) {
    Properties props = root.getProperties();
    // Handling encoding
    if (StringUtils.isBlank(props.getProperty("sonar.sourceEncoding"))) {
      LOG.info("'sonar.sourceEncoding' has not been defined: setting it to default value 'UTF-8'.");
      props.setProperty("sonar.sourceEncoding", "UTF-8");
    }
    // Handling exclusions
    if (configuration.getBoolean(CSharpConstants.EXCLUDE_GENERATED_CODE_KEY, CSharpConstants.EXCLUDE_GENERATED_CODE_DEFVALUE)) {
      String exclusions = props.getProperty("sonar.exclusions", "");
      StringBuilder newExclusions = new StringBuilder(exclusions);
      if ( !StringUtils.isEmpty(exclusions)) {
        newExclusions.append(",");
      }
      newExclusions.append(CSharpConstants.DEFAULT_FILES_TO_EXCLUDE);
      props.setProperty("sonar.exclusions", newExclusions.toString());
    }
    return props;
  }

  private void retrieveMicrosoftWindowsEnvironmentConfig() {
    // .NET version
    String dotnetVersion = configuration.getString(CSharpConstants.DOTNET_VERSION_KEY, CSharpConstants.DOTNET_VERSION_DEFVALUE);
    microsoftWindowsEnvironment.setDotnetVersion(dotnetVersion);
    // .NET SDK folder
    File dotnetSdkDirectory = new File(configuration.getString(CSharpConstants.getDotnetSdkDirKey(dotnetVersion),
        CSharpConstants.getDotnetSdkDirDefaultValue(dotnetVersion)));
    if ( !dotnetSdkDirectory.isDirectory()) {
      throw new SonarException("The following .NET SDK directory does not exist, please check your plugin configuration: "
          + dotnetSdkDirectory.getPath());
    } else {
      microsoftWindowsEnvironment.setDotnetSdkDirectory(dotnetSdkDirectory);
    }
    // Silverlight version
    String silverlightVersion = configuration.getString(CSharpConstants.SILVERLIGHT_VERSION_KEY,
        CSharpConstants.SILVERLIGHT_VERSION_DEFVALUE);
    microsoftWindowsEnvironment.setSilverlightVersion(silverlightVersion);
    // Silverlight folder
    File silverlightDirectory = new File(configuration.getString(CSharpConstants.getSilverlightDirKey(silverlightVersion),
        CSharpConstants.getSilverlightDirDefaultValue(silverlightVersion)));
    if ( !silverlightDirectory.isDirectory()) {
    	throw new SonarException("The following silverlight SDK directory does not exist, please check your plugin configuration: "
    	          + silverlightDirectory.getPath());
    }
    microsoftWindowsEnvironment.setSilverlightDirectory(silverlightDirectory);
  }

  private void createVisualStudioSolution(File baseDir) {
    File slnFile = findSlnFile(baseDir);
    if (slnFile == null) {
      throw new SonarException("No valid '.sln' file could be found. Please read the previous log messages to know more.");
    }
    LOG.info("The following 'sln' file has been found and will be used: " + slnFile.getAbsolutePath());

    try {
      ModelFactory.setTestProjectNamePattern(configuration.getString(CSharpConstants.TEST_PROJET_PATTERN_KEY,
          CSharpConstants.TEST_PROJET_PATTERN_DEFVALUE));
      VisualStudioSolution solution = ModelFactory.getSolution(slnFile);
      microsoftWindowsEnvironment.setCurrentSolution(solution);
    } catch (IOException e) {
      throw new SonarException("Error occured while reading Visual Studio files.", e);
    } catch (DotNetToolsException e) {
      throw new SonarException("Error occured while reading Visual Studio files.", e);
    }
  }

  private File findSlnFile(File baseDir) {
    String slnFilePath = configuration.getString(CSharpConstants.SOLUTION_FILE_KEY, CSharpConstants.SOLUTION_FILE_DEFVALUE);
    final File slnFile;
    if (StringUtils.isEmpty(slnFilePath)) {
      LOG.info("No '.sln' file found or specified: trying to find one...");
      slnFile = searchForSlnFile(baseDir);	
    } else {
      final File confSlnFile = new File(baseDir, slnFilePath);
      if ( confSlnFile.isFile()) {
    	  slnFile = confSlnFile;
      } else {
    	slnFile = null;  
        LOG.warn("The specified '.sln' path does not point to an existing file: " + confSlnFile.getAbsolutePath());
      }
    }
    return slnFile;
  }

  private File searchForSlnFile(File baseDir) {
    File slnFile = null;
    @SuppressWarnings("unchecked")
    Collection<File> foundSlnFiles = FileUtils.listFiles(baseDir, new String[] { "sln" }, false);
    if (foundSlnFiles.isEmpty()) {
      LOG.warn("No '.sln' file specified, and none found at the root of the project: " + baseDir.getAbsolutePath());
    } else if (foundSlnFiles.size() > 1) {
      LOG.warn("More than one '.sln' file found at the root of the project: please tell which one to use via the configuration ("
          + CSharpConstants.SOLUTION_FILE_KEY + ").");
    } else {
      slnFile = foundSlnFiles.iterator().next();
    }
    return slnFile;
  }

}
