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
 * Created on May 7, 2009
 */
package org.sonar.plugin.dotnet.fxcop;

/**
 * Definition of a FXCop rule.
 * @author Jose CHILLAN Feb 16, 2010
 */
public class FxCopRule
{
  private String  category;
  private String  name;
  private boolean enabled;
  private String  fileName;

  /**
   * Constructs a @link{FxCopRule}.
   */
  public FxCopRule()
  {
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

  /**
   * Returns the enabled.
   * 
   * @return The enabled to return.
   */
  public boolean isEnabled()
  {
    return this.enabled;
  }

  /**
   * Sets the enabled.
   * 
   * @param enabled The enabled to set.
   */
  public void setEnabled(boolean enabled)
  {
    this.enabled = enabled;
  }

  @Override
  public String toString()
  {
    return "FxCopRule(name=" + name + ", enabled=" + enabled + ", category=" + category + ")";
  }

  /**
   * Returns the category.
   * 
   * @return The category to return.
   */
  public String getCategory()
  {
    return this.category;
  }

  /**
   * Sets the category.
   * 
   * @param category The category to set.
   */
  public void setCategory(String category)
  {
    this.category = category;
  }

  
  /**
   * Returns the fileName.
   * 
   * @return The fileName to return.
   */
  public String getFileName()
  {
    return this.fileName;
  }

  
  /**
   * Sets the fileName.
   * 
   * @param fileName The fileName to set.
   */
  public void setFileName(String fileName)
  {
    this.fileName = fileName;
  }
}
