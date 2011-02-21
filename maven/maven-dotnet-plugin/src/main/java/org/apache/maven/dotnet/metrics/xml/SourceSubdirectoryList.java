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
 * Created on Apr 9, 2009
 */
package org.apache.maven.dotnet.metrics.xml;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;

@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "source_subdirectory_list")
public class SourceSubdirectoryList {
  @XmlElement(name = "exclude_subdirectories")
  private boolean excludeSubdirectories;
  @XmlElement(name = "source_subtree")
  private String[] sourceSubTree;
  @XmlElement(name = "source_subdirectory")
  private String[] sourceSubdirectory;

  public SourceSubdirectoryList() {
  }

  /**
   * Returns the excludeSubdirectories.
   * 
   * @return The excludeSubdirectories to return.
   */
  public boolean isExcludeSubdirectories() {
    return this.excludeSubdirectories;
  }

  /**
   * Sets the excludeSubdirectories.
   * 
   * @param excludeSubdirectories
   *          The excludeSubdirectories to set.
   */
  public void setExcludeSubdirectories(boolean excludeSubdirectories) {
    this.excludeSubdirectories = excludeSubdirectories;
  }

  /**
   * Returns the sourceSubTree.
   * 
   * @return The sourceSubTree to return.
   */
  public String[] getSourceSubTree() {
    return this.sourceSubTree;
  }

  /**
   * Sets the sourceSubTree.
   * 
   * @param sourceSubTree
   *          The sourceSubTree to set.
   */
  public void setSourceSubTree(String[] sourceSubTree) {
    this.sourceSubTree = sourceSubTree;
  }

  /**
   * Returns the sourceSubdirectory.
   * 
   * @return The sourceSubdirectory to return.
   */
  public String[] getSourceSubdirectory() {
    return this.sourceSubdirectory;
  }

  /**
   * Sets the sourceSubdirectory.
   * 
   * @param sourceSubdirectory
   *          The sourceSubdirectory to set.
   */
  public void setSourceSubdirectory(String[] sourceSubdirectory) {
    this.sourceSubdirectory = sourceSubdirectory;
  }

}
