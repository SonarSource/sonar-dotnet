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
 * Created on Apr 7, 2009
 */
package org.apache.maven.dotnet.metrics.xml;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;

@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "export")
public class Export
{
  @XmlElement(name = "export_file")
  private String file;
  @XmlElement(name = "export_type")
  private String type;
  @XmlElement(name = "export_option")
  private String[] options;

  public Export()
  {
    this.options = new String[]{"1", "3"};
  }

  /**
   * Returns the file.
   * 
   * @return The file to return.
   */
  public String getFile()
  {
    return this.file;
  }

  /**
   * Sets the file.
   * 
   * @param file The file to set.
   */
  public void setFile(String file)
  {
    this.file = file;
  }

  /**
   * Returns the type.
   * 
   * @return The type to return.
   */
  public String getType()
  {
    return this.type;
  }

  /**
   * Sets the type.
   * 
   * @param type The type to set.
   */
  public void setType(String type)
  {
    this.type = type;
  }

  /**
   * Returns the optins.
   * 
   * @return The option
   */
  public String[] getOptions()
  {
    return this.options;
  }

  /**
   * Sets the options.
   * 
   * @param options The options to set.
   */
  public void setFormat(String[] options)
  {
    this.options = options;
  }

}