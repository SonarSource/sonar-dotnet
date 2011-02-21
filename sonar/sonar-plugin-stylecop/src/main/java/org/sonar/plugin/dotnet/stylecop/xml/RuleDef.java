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
 * Created on Jul 21, 2009
 */
package org.sonar.plugin.dotnet.stylecop.xml;

import java.util.List;

import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlElementWrapper;
import javax.xml.bind.annotation.XmlType;

/**
 * The XML representation of a StlyeCop rule
 * 
 * @author Jose CHILLAN Apr 6, 2010
 */
@XmlType(name = "Rule")
public class RuleDef {
  @XmlAttribute(name = "Name")
  private String name;

  @XmlAttribute(name = "SonarPriority")
  private String priority = "minor";

  @XmlElementWrapper(name = "RuleSettings")
  @XmlElement(name = "BooleanProperty")
  private List<BooleanProperty> settings;

  /**
   * Constructs a @link{RuleDef}.
   */
  public RuleDef() {
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

  /**
   * Returns the settings.
   * 
   * @return The settings to return.
   */
  public List<BooleanProperty> getSettings() {
    return this.settings;
  }

  /**
   * Sets the settings.
   * 
   * @param settings
   *          The settings to set.
   */
  public void setSettings(List<BooleanProperty> settings) {
    this.settings = settings;
  }

  public String getPriority() {
    return priority;
  }

  public void setPriority(String priority) {
    this.priority = priority;
  }

}
