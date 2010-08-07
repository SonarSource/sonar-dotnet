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
 * Created on Jun 4, 2009
 */
package org.sonar.plugin.dotnet.gallio;

import java.io.File;

import org.apache.commons.lang.builder.ToStringBuilder;
import org.apache.commons.lang.builder.ToStringStyle;

/**
 * Full description of a unit test.
 * 
 * @author Jose CHILLAN Jun 4, 2009
 */
public class TestDescription {
  private String assemblyName;
  private String namespace;
  private String className;
  private String methodName;
  private int line;
  private File sourceFile;

  /**
   * Constructs a @link{TestDescription}.
   */
  public TestDescription() {
  }

  /**
   * Returns the assemblyName.
   * 
   * @return The assemblyName to return.
   */
  public String getAssemblyName() {
    return this.assemblyName;
  }

  /**
   * Sets the assemblyName.
   * 
   * @param assemblyName
   *          The assemblyName to set.
   */
  public void setAssemblyName(String assemblyName) {
    this.assemblyName = assemblyName;
  }

  /**
   * Returns the namespace.
   * 
   * @return The namespace to return.
   */
  public String getNamespace() {
    return this.namespace;
  }

  /**
   * Sets the namespace.
   * 
   * @param namespace
   *          The namespace to set.
   */
  public void setNamespace(String namespace) {
    this.namespace = namespace;
  }

  /**
   * Returns the className.
   * 
   * @return The className to return.
   */
  public String getClassName() {
    return this.className;
  }

  /**
   * Sets the className.
   * 
   * @param className
   *          The className to set.
   */
  public void setClassName(String className) {
    this.className = className;
  }

  /**
   * Returns the methodName.
   * 
   * @return The methodName to return.
   */
  public String getMethodName() {
    return this.methodName;
  }

  /**
   * Sets the methodName.
   * 
   * @param methodName
   *          The methodName to set.
   */
  public void setMethodName(String methodName) {
    this.methodName = methodName;
  }

  /**
   * Returns the line.
   * 
   * @return The line to return.
   */
  public int getLine() {
    return this.line;
  }

  /**
   * Sets the line.
   * 
   * @param line
   *          The line to set.
   */
  public void setLine(int line) {
    this.line = line;
  }

  /**
   * Returns the sourceFile.
   * 
   * @return The sourceFile to return.
   */
  public File getSourceFile() {
    return this.sourceFile;
  }

  /**
   * Sets the sourceFile.
   * 
   * @param sourceFile
   *          The sourceFile to set.
   */
  public void setSourceFile(File sourceFile) {
    this.sourceFile = sourceFile;
  }

  @Override
  public String toString() {
    return ToStringBuilder.reflectionToString(this,
        ToStringStyle.SHORT_PREFIX_STYLE);
  }
}
