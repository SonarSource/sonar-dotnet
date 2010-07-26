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
 * Created on May 14, 2009
 */
package org.sonar.plugin.dotnet.coverage.model;

import java.io.File;
import java.util.HashMap;

/**
 * A FileCoverage.
 * 
 * @author Jose CHILLAN May 14, 2009
 */
public class FileCoverage extends CoverableSource
{
  private final File file;
  private String assemblyName;
  
  /**
   * Constructs a @link{FileCoverage}.
   */
  public FileCoverage(File file)
  {
    this.file = file;
    this.lines = new HashMap<Integer, SourceLine>();
  }

  /**
   * Returns the file.
   * 
   * @return The file to return.
   */
  public File getFile()
  {
    return this.file;
  }

  @Override
  public String toString()
  {
    return "File(name=" + file.getName() + ", assembly=" + assemblyName + ", coverage=" + getCoverage() + ", lines=" + countLines + ", covered=" + coveredLines + ")";
  }

  /**
   * Returns the assemblyName.
   * 
   * @return The assemblyName to return.
   */
  public String getAssemblyName()
  {
    return this.assemblyName;
  }

  
  /**
   * Sets the assemblyName.
   * 
   * @param assemblyName The assemblyName to set.
   */
  public void setAssemblyName(String assemblyName)
  {
    this.assemblyName = assemblyName;
  }
}
