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
 * Created on Jan 14, 2010
 *
 */
package org.apache.maven.dotnet.msbuild.xml;

import java.util.ArrayList;
import java.util.List;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;

/**
 * A ItemGroup.
 * 
 * @author Jose CHILLAN Jan 14, 2010
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ItemGroup", namespace = Constant.NAMESPACE)
public class ItemGroup {

  @XmlElement(type = ProjectItem.class, name = "Project")
  private List<ProjectItem> projects;

  @XmlElement(type = ProjectItem.class, name = "CSFile")
  private List<ProjectItem> csFiles;

  /**
   * Constructs a @link{ItemGroup}.
   */
  public ItemGroup() {
    this.projects = new ArrayList<ProjectItem>();
    this.csFiles = new ArrayList<ProjectItem>();
  }

  /**
   * Returns the projects.
   * 
   * @return The projects to return.
   */
  public List<ProjectItem> getProjects() {
    return this.projects;
  }

  /**
   * Sets the projects.
   * 
   * @param projects
   *          The projects to set.
   */
  public void setProjects(List<ProjectItem> projects) {
    this.projects = projects;
  }

  public void addProject(String projectPath) {
    ProjectItem project = new ProjectItem();
    project.setInclude(projectPath);
    this.projects.add(project);

  }

  public List<ProjectItem> getCsFiles() {
    return csFiles;
  }

  public void setCsFiles(List<ProjectItem> csFiles) {
    this.csFiles = csFiles;
  }

  public void addCsFiles(String path) {
    ProjectItem project = new ProjectItem();
    project.setInclude(path);
    this.csFiles.add(project);

  }
}
