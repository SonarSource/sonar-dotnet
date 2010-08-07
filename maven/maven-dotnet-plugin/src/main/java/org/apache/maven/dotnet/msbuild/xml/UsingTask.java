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
 */
package org.apache.maven.dotnet.msbuild.xml;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlType;

/**
 * A UsingTask.
 * 
 * @author Jose CHILLAN Jan 14, 2010
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "UsingTask", namespace = Constant.NAMESPACE)
public class UsingTask {
  @XmlAttribute(name = "AssemblyFile")
  private String assemblyFile;

  @XmlAttribute(name = "TaskName")
  private String taskName;

  /**
   * Constructs a @link{UsingTask}.
   */
  public UsingTask() {
  }

  /**
   * Returns the assemblyFile.
   * 
   * @return The assemblyFile to return.
   */
  public String getAssemblyFile() {
    return this.assemblyFile;
  }

  /**
   * Sets the assemblyFile.
   * 
   * @param assemblyFile
   *          The assemblyFile to set.
   */
  public void setAssemblyFile(String assemblyFile) {
    this.assemblyFile = assemblyFile;
  }

  /**
   * Returns the taskName.
   * 
   * @return The taskName to return.
   */
  public String getTaskName() {
    return this.taskName;
  }

  /**
   * Sets the taskName.
   * 
   * @param taskName
   *          The taskName to set.
   */
  public void setTaskName(String taskName) {
    this.taskName = taskName;
  }

}
