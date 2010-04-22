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
 * Created on Jul 21, 2009
 *
 */
package org.sonar.plugin.dotnet.stylecop.xml;

import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlType;
import javax.xml.bind.annotation.XmlValue;


/**
 * A boolean property for StyleCop configuration.
 * @author Jose CHILLAN Jul 21, 2009
 */
@XmlType(name="BooleanProperty")
public class BooleanProperty
{
  @XmlAttribute(name="Name")
  private String name;
  
  @XmlValue
  private String value;

  
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

  
  /**
   * Returns the value.
   * 
   * @return The value to return.
   */
  public String getValue()
  {
    return this.value;
  }

  
  /**
   * Sets the value.
   * 
   * @param value The value to set.
   */
  public void setValue(String value)
  {
    this.value = value;
  }
  
  
}
