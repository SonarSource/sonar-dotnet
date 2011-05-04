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
 * Created on Apr 16, 2009
 */
package org.apache.maven.dotnet.commons.project;

import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Set;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * A visual studio solution model.
 * 
 * @author Jose CHILLAN Apr 16, 2009
 */
public class VisualStudioSolution {
  
  private final static Logger log = LoggerFactory.getLogger(VisualStudioSolution.class);
  
  private File solutionFile;
  private File solutionDir;
  private String name;
  private List<VisualStudioProject> projects;
  private List<String> buildConfigurations;

  public VisualStudioSolution(File solutionFile,
      List<VisualStudioProject> projects) {
    this.solutionFile = solutionFile;
    this.solutionDir = solutionFile.getParentFile();
    this.projects = projects;
    initializeFileAssociations();
    
    removeAssemblyNameDuplicates();
    
  }

  private void removeAssemblyNameDuplicates() {
    Map<String, VisualStudioProject> projectMap = new HashMap<String, VisualStudioProject>();
    for (VisualStudioProject project : projects) {
      String assemblyName = project.getAssemblyName();
      if (projectMap.containsKey(assemblyName)) {
        int i = 1;
        String newAssemblyName = assemblyName;
        do {
          i++;
          newAssemblyName = assemblyName + "_" + i;
        } while(projectMap.containsKey(newAssemblyName));
        project.setAssemblyName(newAssemblyName);
        projectMap.put(newAssemblyName, project);
      } else {
        projectMap.put(assemblyName, project);
      }
    }
  }

  /**
   * Clean-up file/project associations in order
   * to avoid having the same file in several projects.
   */
  private void initializeFileAssociations() {
    Set<File> csFiles = new HashSet<File>();
    for (VisualStudioProject project : projects) {
      Set<File> projectFiles = project.getSourceFileMap().keySet();
      Set<File> projectFilesToRemove = new HashSet<File>(); 
      for (File file : projectFiles) {
        if (getProjectByLocation(file)==null) {
          projectFilesToRemove.add(file);
        }
      }
      // remove files not present in the project directory
      projectFiles.removeAll(projectFilesToRemove); 
      
      // remove files present in other projects
      projectFiles.removeAll(csFiles);
      
      csFiles.addAll(projectFiles);
    }
  }

  /**
   * Gets the project a cs file belongs to.
   * 
   * @param file
   * @return the project contains the file, or <code>null</code> if none is
   *         matching
   */
  public VisualStudioProject getProject(File file) {
    for (VisualStudioProject project : projects) {
      if (project.contains(file)) {
        return project;
      }
    }
    return null;
  }

  /**
   * Gets the project whose base directory contains the file/directory.
   * 
   * @param file
   *          the file to look for
   * @return the associated project, or <code>null</code> if none is matching
   */
  public VisualStudioProject getProjectByLocation(File file) {
    String canonicalPath;
    try {
      canonicalPath = file.getCanonicalPath();
      for (VisualStudioProject project : projects) {
        File directory = project.getDirectory();
        String projectFolderPath = directory.getPath();
        if (canonicalPath.startsWith(projectFolderPath)) {
          if (project.isParentDirectoryOf(file)) {
            return project;
          }
        }
      }
    } catch (IOException e) {
      log.debug("getProjectByLocation i/o exception", e);
    }

    return null;
  }

  /**
   * Returns the solutionFile.
   * 
   * @return The solutionFile to return.
   */
  public File getSolutionFile() {
    return this.solutionFile;
  }

  /**
   * Returns the solutionDir.
   * 
   * @return The solutionDir to return.
   */
  public File getSolutionDir() {
    return this.solutionDir;
  }

  /**
   * Gets a project by its assembly name.
   * 
   * @param assemblyName
   *          the name of the assembly
   * @return the project, or <code>null</code> if not found
   */
  public VisualStudioProject getProject(String assemblyName) {
    VisualStudioProject result = null;
    for (VisualStudioProject project : projects) {
      if (assemblyName.equalsIgnoreCase(project.getAssemblyName())) {
        result = project;
        break;
      }
    }
    if (result == null) {
      // perhaps a web project
      for (VisualStudioProject project : projects) {
        if (project instanceof WebVisualStudioProject) {
          WebVisualStudioProject webProject = (WebVisualStudioProject) project;
          if (webProject.getWebAssemblyNames().contains(assemblyName)) {
            result = project;
            break;
          }
        }
      }
    }
    return result;
  }

  /**
   * Returns the projects.
   * 
   * @return The projects to return.
   */
  public List<VisualStudioProject> getProjects() {
    return this.projects;
  }

  /**
   * Returns the test projects.
   * 
   * @return The projects to return.
   */
  public List<VisualStudioProject> getTestProjects() {
    List<VisualStudioProject> result = new ArrayList<VisualStudioProject>();
    for (VisualStudioProject visualStudioProject : projects) {
      if (visualStudioProject.isTest()) {
        result.add(visualStudioProject);
      }
    }
    return result;
  }
  
  /**
   * Iterate through all the projects of the solution 
   * seeking for silverlight applications
   * @return  true if a silverlight application is found
   */
  public boolean isSilverlightUsed() {
    final Iterator<VisualStudioProject> projectIterator = projects.iterator();
    boolean silverlightFound = false;
    while (projectIterator.hasNext() && !silverlightFound) {
      silverlightFound = projectIterator.next().isSilverlightProject();
    }
    return silverlightFound;
  }
  
  /**
   * Iterate through all the projects of the solution 
   * seeking for asp.net applications
   * @return  true if an asp.net project is found
   */
  public boolean isAspUsed() {
    final Iterator<VisualStudioProject> projectIterator = projects.iterator();
    boolean aspFound = false;
    while (projectIterator.hasNext() && !aspFound) {
      aspFound = projectIterator.next().isWebProject();
    }
    return aspFound;
  }

  /**
   * Returns the name.
   * 
   * @return The name to return.
   */
  public String getName() {
    return this.name;
  }

  /**
   * Sets the name.
   * 
   * @param name
   *          The name to set.
   */
  public void setName(String name) {
    this.name = name;
  }

  
  public List<String> getBuildConfigurations() {
    return buildConfigurations;
  }

  
  public void setBuildConfigurations(List<String> buildConfigurations) {
    this.buildConfigurations = buildConfigurations;
  }
  
  @Override
  public String toString() {
    return "Solution(path=" + solutionFile + ")";
  }

}
