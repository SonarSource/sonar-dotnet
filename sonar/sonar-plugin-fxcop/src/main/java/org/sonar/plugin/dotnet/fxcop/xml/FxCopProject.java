/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexandre.victoor@codehaus.org
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
 * Created on Jul 2, 2009
 */
package org.sonar.plugin.dotnet.fxcop.xml;

import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;

@XmlRootElement(name = "FxCopProject")
@XmlType(name = "FxCopProject")
public class FxCopProject
{
  @XmlAttribute(name = "Version")
  private String         version = "1.36";

  @XmlAttribute(name = "Name")
  private String         name;

  @XmlElement(name = "ProjectOptions")
  private ProjectOptions projectOptions;

  @XmlElement(name = "Rules")
  private RuleSet        rules;

  /**
   * Constructs a @link{FxCopReport}.
   */
  public FxCopProject()
  {
  }

  /**
   * Returns the projectOptions.
   * 
   * @return The projectOptions to return.
   */
  public ProjectOptions getProjectOptions()
  {
    return this.projectOptions;
  }

  /**
   * Sets the projectOptions.
   * 
   * @param projectOptions The projectOptions to set.
   */
  public void setProjectOptions(ProjectOptions projectOptions)
  {
    this.projectOptions = projectOptions;
  }

  /**
   * Returns the rules.
   * 
   * @return The rules to return.
   */
  public RuleSet getRules()
  {
    return this.rules;
  }

  /**
   * Sets the rules.
   * 
   * @param rules The rules to set.
   */
  public void setRules(RuleSet rules)
  {
    this.rules = rules;
  }

  /**
   * Returns the version.
   * 
   * @return The version to return.
   */
  public String getVersion()
  {
    return this.version;
  }

  /**
   * Sets the version.
   * 
   * @param version The version to set.
   */
  public void setVersion(String version)
  {
    this.version = version;
  }

  /**
   * Returns the name.
   * 
   * @return The name to return.
   */
  public String getName()
  {
    return this.name;
  }

  /**
   * Sets the name.
   * 
   * @param name The name to set.
   */
  public void setName(String name)
  {
    this.name = name;
  }
}
