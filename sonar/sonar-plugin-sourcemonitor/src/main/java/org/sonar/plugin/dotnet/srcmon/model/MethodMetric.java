/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

/*
 * Created on May 5, 2009
 */
package org.sonar.plugin.dotnet.srcmon.model;

import java.io.File;

/**
 * Metrics assoicated to a class method.
 * 
 * @author Jose CHILLAN Apr 6, 2010
 */
public class MethodMetric {
  private File file;
  private String className;
  private String methodName;
  private int methodLine;
  private int complexity;
  private int countStatements;
  private int maximumDepth;
  private int countCalls;
  private boolean isAccessor;

  /**
   * Constructs a @link{MethodMetric}.
   */
  public MethodMetric() {
    super();
  }

  /**
   * Returns the file.
   * 
   * @return The file to return.
   */
  public File getFile() {
    return this.file;
  }

  /**
   * Sets the file.
   * 
   * @param file
   *          The file to set.
   */
  public void setFile(File file) {
    this.file = file;
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
   * Returns the methodLine.
   * 
   * @return The methodLine to return.
   */
  public int getMethodLine() {
    return this.methodLine;
  }

  /**
   * Sets the methodLine.
   * 
   * @param methodLine
   *          The methodLine to set.
   */
  public void setMethodLine(int methodLine) {
    this.methodLine = methodLine;
  }

  /**
   * Returns the complexity.
   * 
   * @return The complexity to return.
   */
  public int getComplexity() {
    return this.complexity;
  }

  /**
   * Sets the complexity.
   * 
   * @param complexity
   *          The complexity to set.
   */
  public void setComplexity(int complexity) {
    this.complexity = complexity;
  }

  /**
   * Returns the countStatements.
   * 
   * @return The countStatements to return.
   */
  public int getCountStatements() {
    return this.countStatements;
  }

  /**
   * Sets the countStatements.
   * 
   * @param countStatements
   *          The countStatements to set.
   */
  public void setCountStatements(int countStatements) {
    this.countStatements = countStatements;
  }

  /**
   * Returns the maximumDepth.
   * 
   * @return The maximumDepth to return.
   */
  public int getMaximumDepth() {
    return this.maximumDepth;
  }

  /**
   * Sets the maximumDepth.
   * 
   * @param maximumDepth
   *          The maximumDepth to set.
   */
  public void setMaximumDepth(int maximumDepth) {
    this.maximumDepth = maximumDepth;
  }

  /**
   * Returns the countCalls.
   * 
   * @return The countCalls to return.
   */
  public int getCountCalls() {
    return this.countCalls;
  }

  /**
   * Sets the countCalls.
   * 
   * @param countCalls
   *          The countCalls to set.
   */
  public void setCountCalls(int countCalls) {
    this.countCalls = countCalls;
  }

  /**
   * Returns the accessor.
   * 
   * @return The accessor to return.
   */
  public boolean isAccessor() {
    return this.isAccessor;
  }

  /**
   * Sets the accessor flag.
   * 
   * @param accessor
   *          the accessor flag to set
   */
  public void setAccessor(boolean accessor) {
    this.isAccessor = accessor;
  }

  @Override
  public String toString() {
    return "method(class=" + className + ", name=" + methodName + ", line="
        + methodLine + ", complexity=" + complexity + ")";
  }
}
