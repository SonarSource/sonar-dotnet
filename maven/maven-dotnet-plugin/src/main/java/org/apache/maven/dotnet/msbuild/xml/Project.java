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
 * Created on Jan 14, 2010
 */
package org.apache.maven.dotnet.msbuild.xml;

import java.util.ArrayList;
import java.util.List;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;

/**
 * A Project.
 * 
 * @author Jose CHILLAN Jan 14, 2010
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlRootElement(name = "Project", namespace = Constant.NAMESPACE)
@XmlType(name = "Project")
public class Project
{
  @XmlAttribute(name = "ToolsVersion")
  private String          toolsVersion = "3.5";

  @XmlAttribute(name = "DefaultTargets")
  private String          defaultTargets;

  @XmlElement(name="PropertyGroup")
  private PropertyGroup propertyGroup;
  
  @XmlElement(name="UsingTask")
  private UsingTask usingTask;
  
  @XmlElement(type=ItemGroup.class, name = "ItemGroup")
  private List<ItemGroup> itemGroups;

  @XmlElement(type = Target.class, name = "Target")
  private List<Target>    targets;

  /**
   * Constructs a @link{Project}.
   */
  public Project()
  {
    this.itemGroups = new ArrayList<ItemGroup>();
    this.targets = new ArrayList<Target>();
  }

  /**
   * Returns the toolsVersion.
   * 
   * @return The toolsVersion to return.
   */
  public String getToolsVersion()
  {
    return this.toolsVersion;
  }

  /**
   * Sets the toolsVersion.
   * 
   * @param toolsVersion The toolsVersion to set.
   */
  public void setToolsVersion(String toolsVersion)
  {
    this.toolsVersion = toolsVersion;
  }

  /**
   * Returns the defaultTargets.
   * 
   * @return The defaultTargets to return.
   */
  public String getDefaultTargets()
  {
    return this.defaultTargets;
  }

  /**
   * Sets the defaultTargets.
   * 
   * @param defaultTargets The defaultTargets to set.
   */
  public void setDefaultTargets(String defaultTargets)
  {
    this.defaultTargets = defaultTargets;
  }

  /**
   * Returns the targets.
   * 
   * @return The targets to return.
   */
  public List<Target> getTargets()
  {
    return this.targets;
  }

  /**
   * Sets the targets.
   * 
   * @param targets The targets to set.
   */
  public void setTargets(List<Target> targets)
  {
    this.targets = targets;
  }

  /**
   * Returns the itemGroups.
   * 
   * @return The itemGroups to return.
   */
  public List<ItemGroup> getItemGroups()
  {
    return this.itemGroups;
  }

  /**
   * Sets the itemGroups.
   * 
   * @param itemGroups The itemGroups to set.
   */
  public void setItemGroups(List<ItemGroup> itemGroups)
  {
    this.itemGroups = itemGroups;
  }

  public void addTarget(Target target)
  {
    this.targets.add(target);
  }

  public void addItem(ItemGroup group)
  {
    this.itemGroups.add(group);
  }

  
  /**
   * Returns the propertyGroup.
   * 
   * @return The propertyGroup to return.
   */
  public PropertyGroup getPropertyGroup()
  {
    return this.propertyGroup;
  }

  
  /**
   * Sets the propertyGroup.
   * 
   * @param propertyGroup The propertyGroup to set.
   */
  public void setPropertyGroup(PropertyGroup propertyGroup)
  {
    this.propertyGroup = propertyGroup;
  }

  
  /**
   * Returns the usingTask.
   * 
   * @return The usingTask to return.
   */
  public UsingTask getUsingTask()
  {
    return this.usingTask;
  }

  
  /**
   * Sets the usingTask.
   * 
   * @param usingTask The usingTask to set.
   */
  public void setUsingTask(UsingTask usingTask)
  {
    this.usingTask = usingTask;
  }
}
