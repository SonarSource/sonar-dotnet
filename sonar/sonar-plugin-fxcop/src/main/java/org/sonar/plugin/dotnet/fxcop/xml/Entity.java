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
 *
 */
package org.sonar.plugin.dotnet.fxcop.xml;

import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlType;

/**
 * A Project or Report for FxCop options.
 * @author Jose CHILLAN Jul 2, 2009
 */
@XmlType(name="Entity")
public class Entity
{
  @XmlAttribute(name="Status")
  private String status = "Active";

  @XmlAttribute(name="NewOnly")
  private String newOnly = "False";
  
  public Entity()
  {
  }

  
  /**
   * Returns the status.
   * 
   * @return The status to return.
   */
  public String getStatus()
  {
    return this.status;
  }

  
  /**
   * Sets the status.
   * 
   * @param status The status to set.
   */
  public void setStatus(String status)
  {
    this.status = status;
  }

  
  /**
   * Returns the newOnly.
   * 
   * @return The newOnly to return.
   */
  public String getNewOnly()
  {
    return this.newOnly;
  }

  
  /**
   * Sets the newOnly.
   * 
   * @param newOnly The newOnly to set.
   */
  public void setNewOnly(String newOnly)
  {
    this.newOnly = newOnly;
  }
  
  
}
