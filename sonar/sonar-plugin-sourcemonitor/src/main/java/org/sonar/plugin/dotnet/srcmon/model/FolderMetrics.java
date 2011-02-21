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
 * Created on May 19, 2009
 */
package org.sonar.plugin.dotnet.srcmon.model;

import java.io.File;

import org.apache.maven.dotnet.commons.project.VisualStudioProject;

/**
 * Metrics corresponding to a specified source folder.
 * 
 * @author Jose CHILLAN May 19, 2009
 */
public class FolderMetrics extends AbstractMeterable {
  private VisualStudioProject project;
  private File directory;
  private String projectFolder;

  /**
   * Constructs a @link{NamespaceMetrics}.
   * 
   * @param project
   * @param directory
   * @param projectFolder
   */
  public FolderMetrics(VisualStudioProject project, File directory,
      String projectFolder) {
    super();
    this.project = project;
    this.directory = directory;
    this.projectFolder = projectFolder;
  }

  /**
   * Returns the project.
   * 
   * @return The project to return.
   */
  public VisualStudioProject getProject() {
    return this.project;
  }

  /**
   * Sets the project.
   * 
   * @param project
   *          The project to set.
   */
  public void setProject(VisualStudioProject project) {
    this.project = project;
  }

  /**
   * Returns the directory.
   * 
   * @return The directory to return.
   */
  public File getDirectory() {
    return this.directory;
  }

  /**
   * Sets the directory.
   * 
   * @param directory
   *          The directory to set.
   */
  public void setDirectory(File directory) {
    this.directory = directory;
  }

  /**
   * Returns the projectFolder.
   * 
   * @return The projectFolder to return.
   */
  public String getProjectFolder() {
    return this.projectFolder;
  }

  /**
   * Sets the projectFolder.
   * 
   * @param projectFolder
   *          The projectFolder to set.
   */
  public void setProjectFolder(String projectFolder) {
    this.projectFolder = projectFolder;
  }
}
