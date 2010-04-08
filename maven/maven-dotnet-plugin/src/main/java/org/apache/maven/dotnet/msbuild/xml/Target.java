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

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;

/**
 * A Target.
 * 
 * @author Jose CHILLAN Jan 14, 2010
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Target", namespace = Constant.NAMESPACE)
public class Target
{
  @XmlAttribute(name="Name")
  private String       name;
  
  @XmlElement(name = "CreateItem")
  private CreateItem   item;

  @XmlElement(name = "StyleCopTask")
  private StyleCopTask styleCopTask;

  /**
   * Constructs a @link{Target}.
   */
  public Target()
  {
  }

  /**
   * Returns the item.
   * 
   * @return The item to return.
   */
  public CreateItem getItem()
  {
    return this.item;
  }

  /**
   * Sets the item.
   * 
   * @param item The item to set.
   */
  public void setItem(CreateItem item)
  {
    this.item = item;
  }

  /**
   * Returns the styleCopTask.
   * 
   * @return The styleCopTask to return.
   */
  public StyleCopTask getStyleCopTask()
  {
    return this.styleCopTask;
  }

  /**
   * Sets the styleCopTask.
   * 
   * @param styleCopTask The styleCopTask to set.
   */
  public void setStyleCopTask(StyleCopTask styleCopTask)
  {
    this.styleCopTask = styleCopTask;
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
