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
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;

/**
 * A PropertyGroup.
 * 
 * @author Jose CHILLAN Jan 14, 2010
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "PropertyGroup", namespace = Constant.NAMESPACE)
public class PropertyGroup {
  @XmlElement(name = "ProjectRoot")
  private String projectRoot;

  @XmlElement(name = "StyleCopRoot")
  private String styleCopRoot;

  /**
   * Constructs a @link{PropertyGroup}.
   */
  public PropertyGroup() {
  }

  /**
   * Returns the projectRoot.
   * 
   * @return The projectRoot to return.
   */
  public String getProjectRoot() {
    return this.projectRoot;
  }

  /**
   * Sets the projectRoot.
   * 
   * @param projectRoot
   *          The projectRoot to set.
   */
  public void setProjectRoot(String projectRoot) {
    this.projectRoot = projectRoot;
  }

  /**
   * Returns the styleCopRoot.
   * 
   * @return The styleCopRoot to return.
   */
  public String getStyleCopRoot() {
    return this.styleCopRoot;
  }

  /**
   * Sets the styleCopRoot.
   * 
   * @param styleCopRoot
   *          The styleCopRoot to set.
   */
  public void setStyleCopRoot(String styleCopRoot) {
    this.styleCopRoot = styleCopRoot;
  }

}
