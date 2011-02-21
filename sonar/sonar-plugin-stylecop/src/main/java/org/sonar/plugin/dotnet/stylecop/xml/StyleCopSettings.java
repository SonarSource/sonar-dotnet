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
 * Created on Jul 23, 2009
 */
package org.sonar.plugin.dotnet.stylecop.xml;

import java.util.List;

import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlElementWrapper;
import javax.xml.bind.annotation.XmlRootElement;

/**
 * The XML representation of StyleCop settings.
 * 
 * @author Jose CHILLAN Apr 6, 2010
 */
@XmlRootElement(name = "StyleCopSettings")
public class StyleCopSettings {
  @XmlAttribute(name = "Version")
  private String version = "4.3";

  @XmlElementWrapper(name = "Analyzers")
  @XmlElement(name = "Analyzer")
  private List<Analyzer> analizers;

  /**
   * Constructs a @link{StyleCopSettings}.
   */
  public StyleCopSettings() {
  }

  /**
   * Returns the version.
   * 
   * @return The version to return.
   */
  public String getVersion() {
    return this.version;
  }

  /**
   * Sets the version.
   * 
   * @param version
   *          The version to set.
   */
  public void setVersion(String version) {
    this.version = version;
  }

  /**
   * Returns the analizers.
   * 
   * @return The analizers to return.
   */
  public List<Analyzer> getAnalizers() {
    return this.analizers;
  }

  /**
   * Sets the analizers.
   * 
   * @param analizers
   *          The analizers to set.
   */
  public void setAnalizers(List<Analyzer> analizers) {
    this.analizers = analizers;
  }

}
