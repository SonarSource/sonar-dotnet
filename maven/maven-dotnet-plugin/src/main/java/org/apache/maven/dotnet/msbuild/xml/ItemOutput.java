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
 * An item output.
 * 
 * @author Jose CHILLAN Jan 14, 2010
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "Output", namespace = Constant.NAMESPACE)
public class ItemOutput {
  @XmlAttribute(name = "TaskParameter")
  private String taskParameter;

  @XmlAttribute(name = "ItemName")
  private String itemName;

  /**
   * Constructs a @link{ItemOutput}.
   */
  public ItemOutput() {
  }

  /**
   * Returns the taskParameter.
   * 
   * @return The taskParameter to return.
   */
  public String getTaskParameter() {
    return this.taskParameter;
  }

  /**
   * Sets the taskParameter.
   * 
   * @param taskParameter
   *          The taskParameter to set.
   */
  public void setTaskParameter(String taskParameter) {
    this.taskParameter = taskParameter;
  }

  /**
   * Returns the itemName.
   * 
   * @return The itemName to return.
   */
  public String getItemName() {
    return this.itemName;
  }

  /**
   * Sets the itemName.
   * 
   * @param itemName
   *          The itemName to set.
   */
  public void setItemName(String itemName) {
    this.itemName = itemName;
  }

}
