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
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;

/**
 * A CreateItem.
 * 
 * @author Jose CHILLAN Jan 14, 2010
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "CreateItem", namespace=Constant.NAMESPACE)
public class CreateItem
{
  @XmlAttribute(name = "Include")
  private String     include;

  @XmlElement(name = "Output")
  private ItemOutput output;

  /**
   * Constructs a @link{CreateItem}.
   */
  public CreateItem()
  {
  }

  /**
   * Returns the include.
   * 
   * @return The include to return.
   */
  public String getInclude()
  {
    return this.include;
  }

  /**
   * Sets the include.
   * 
   * @param include The include to set.
   */
  public void setInclude(String include)
  {
    this.include = include;
  }

  /**
   * Returns the output.
   * 
   * @return The output to return.
   */
  public ItemOutput getOutput()
  {
    return this.output;
  }

  /**
   * Sets the output.
   * 
   * @param output The output to set.
   */
  public void setOutput(ItemOutput output)
  {
    this.output = output;
  }

}
