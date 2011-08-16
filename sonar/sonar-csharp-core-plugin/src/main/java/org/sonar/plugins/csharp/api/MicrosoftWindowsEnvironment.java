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

package org.sonar.plugins.csharp.api;

import java.io.File;
import java.util.Map;

import org.apache.commons.lang.StringUtils;
import org.sonar.api.BatchExtension;
import org.sonar.api.batch.InstantiationStrategy;
import org.sonar.api.utils.SonarException;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.squid.api.SourceFile;

import com.google.common.collect.Maps;

/**
 * Class used to share information, between C# plugins, about Windows and Visual Studio elements, such as:
 * <ul>
 * <li>the environment settings (.NET SDK directory for instance),</li>
 * <li>the current Visual Studio solution that is being analyzed.</li>
 * </ul>
 */
@InstantiationStrategy(InstantiationStrategy.PER_BATCH)
public class MicrosoftWindowsEnvironment implements BatchExtension {

  private CSharpConfiguration configuration;
  private boolean locked;
  // static configuration elements that are fed at the beginning of an analysis and that do not change afterwards
  private String dotnetVersion;
  private File dotnetSdkDirectory;
  private String silverlightVersion;
  private File silverlightDirectory;
  private VisualStudioSolution currentSolution;
  private Map<String, VisualStudioProject> projectsByName;
  private String workDir;
  // dynamic elements that will change during analysis
  private boolean testExecutionDone = false;

  public MicrosoftWindowsEnvironment() {
    this(null);
  }

  public MicrosoftWindowsEnvironment(CSharpConfiguration configuration) {
    this.configuration = configuration;
    projectsByName = Maps.newHashMap();
  }

  /**
   * After invoking this method, the {@link MicrosoftWindowsEnvironment} class won't be able to index files anymore: if
   * {@link #indexFile(SourceFile, File)} is called, a {@link IllegalStateException} will be thrown.
   */
  public void lock() {
    this.locked = true;
  }

  private void checkIfLocked() {
    if (locked) {
      throw new SonarException("Cannot override attributes that have already been assigned on MicrosoftWindowsEnvironment class.");
    }
  }

  /**
   * Returns the {@link VisualStudioProject} that is under analysis and which name is the given project name.
   * 
   * @return the current Visual Studio project
   */
  public VisualStudioProject getCurrentProject(String projectName) {
    return projectsByName.get(projectName);
  }

  /**
   * <b>Must not be used.</b>
   * 
   * @param currentSolution
   *          the currentSolution to set
   */
  public void setCurrentSolution(VisualStudioSolution currentSolution) {
    checkIfLocked();
    this.currentSolution = currentSolution;
    for (VisualStudioProject vsProject : currentSolution.getProjects()) {
      projectsByName.put(vsProject.getName(), vsProject);
    }
    if (configuration != null) {
      String sonarBranch = configuration.getString("sonar.branch", "");
      if ( !StringUtils.isEmpty(sonarBranch)) {
        // we also reference the projects with the name that Sonar gives when 'sonar.branch' is used
        for (VisualStudioProject vsProject : currentSolution.getProjects()) {
          projectsByName.put(vsProject.getName() + " " + sonarBranch, vsProject);
        }
      }
    }
  }

  /**
   * Returns the {@link VisualStudioSolution} that is under analysis
   * 
   * @return the current Visual Studio solution
   */
  public VisualStudioSolution getCurrentSolution() {
    return currentSolution;
  }

  /**
   * <b>Must not be used.</b>
   * 
   * @param dotnetVersion
   *          the dotnetVersion to set
   */
  public void setDotnetVersion(String dotnetVersion) {
    checkIfLocked();
    this.dotnetVersion = dotnetVersion;
  }

  /**
   * Returns the version of the .NET framework to use
   * 
   * @return the dotnetVersion
   */
  public String getDotnetVersion() {
    return dotnetVersion;
  }

  /**
   * <b>Must not be used.</b>
   * 
   * @param dotnetSdkDirectory
   *          the dotnetSdkDirectory to set
   */
  public void setDotnetSdkDirectory(File dotnetSdkDirectory) {
    checkIfLocked();
    this.dotnetSdkDirectory = dotnetSdkDirectory;
  }

  /**
   * Returns the path of the .NET SDK
   * 
   * @return the dotnetSdkDirectory
   */
  public File getDotnetSdkDirectory() {
    return dotnetSdkDirectory;
  }

  /**
   * Returns the version of Silverlight that must be used
   * 
   * @return the silverlightVersion
   */
  public String getSilverlightVersion() {
    return silverlightVersion;
  }

  /**
   * <b>Must not be used.</b>
   * 
   * @param silverlightVersion
   *          the silverlightVersion to set
   */
  public void setSilverlightVersion(String silverlightVersion) {
    checkIfLocked();
    this.silverlightVersion = silverlightVersion;
  }

  /**
   * Returns the path tof the Silverlight directory.
   * 
   * @return the silverlightDirectory
   */
  public File getSilverlightDirectory() {
    return silverlightDirectory;
  }

  /**
   * <b>Must not be used.</b>
   * 
   * @param silverlightDirectory
   *          the silverlightDirectory to set
   */
  public void setSilverlightDirectory(File silverlightDirectory) {
    checkIfLocked();
    this.silverlightDirectory = silverlightDirectory;
  }

  /**
   * Tells whether tests have already been executed or not.
   * 
   * @return true if tests have already been executed.
   */
  public boolean isTestExecutionDone() {
    return testExecutionDone;
  }

  /**
   * Call this method once the tests have been executed and their reports generated.
   */
  public void setTestExecutionDone() {
    this.testExecutionDone = true;
  }

  /**
   * Returns the working directory that must be used during the Sonar analysis. For example, it is "target/sonar" if Maven runner is used,
   * or ".sonar" if the Java runner is used.
   * 
   * @return the working directory
   */
  public String getWorkingDirectory() {
    return workDir;
  }

  /**
   * Sets the working directory used during the Sonar analysis.
   * 
   * @param workDir
   *          the working directory
   */
  public void setWorkingDirectory(String workDir) {
    this.workDir = workDir;
  }

}
