/*
 * Sonar C# Plugin :: Gallio
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
 * Created on Apr 28, 2009
 */
package org.sonar.plugins.csharp.gallio.results.execution.model;

import java.io.File;

import org.apache.commons.lang.StringEscapeUtils;
import org.apache.commons.lang.StringUtils;

/**
 * Details for a test case.
 * 
 * @author Jose CHILLAN Jun 16, 2009
 */
public class TestCaseDetail {

  private String name;
  private TestStatus status;
  private String stackTrace;
  private String errorMessage;
  private int timeMillis = 0;
  private int countAsserts;
  private File sourceFile;
  private String assemblyName;

  /**
   * Constructs an empty @link{TestCaseDetail}.
   */
  public TestCaseDetail() {

  }

  public String getName() {
    return name;
  }

  public void setName(String name) {
    this.name = name;
  }

  public String getStackTrace() {
    return stackTrace;
  }

  public String getFormatedStackTrace() {
    return StringEscapeUtils.escapeXml(stackTrace);
  }

  public void setStackTrace(String stackTrace) {
    this.stackTrace = stackTrace;
  }

  public String getErrorMessage() {
    return errorMessage;
  }

  public String getFormatedErrorMessage() {
    return StringEscapeUtils.escapeXml(StringUtils.remove(errorMessage, "\n\t"));
  }

  public void setErrorMessage(String errorMessage) {
    this.errorMessage = errorMessage;
  }

  public int getTimeMillis() {
    return timeMillis;
  }

  public void setTimeMillis(int timeMS) {
    this.timeMillis = timeMS;
  }

  public int getCountAsserts() {
    return this.countAsserts;
  }

  public void setCountAsserts(int countAsserts) {
    this.countAsserts = countAsserts;
  }

  public String getAssemblyName() {
    return assemblyName;
  }

  @Override
  public String toString() {
    return "Test " + name + "(" + status + ", time=" + timeMillis * 0.001 + ")";
  }

  /**
   * Returns the status.
   * 
   * @return The status to return.
   */
  public TestStatus getStatus() {
    return this.status;
  }

  /**
   * Sets the status.
   * 
   * @param status
   *          The status to set.
   */
  public void setStatus(TestStatus status) {
    this.status = status;
  }

  /**
   * Returns the testFile.
   * 
   * @return The testFile to return.
   */
  public File getSourceFile() {
    return this.sourceFile;
  }

  /**
   * Sets the testFile.
   * 
   * @param testFile
   *          The testFile to set.
   */
  public void setSourceFile(File testFile) {
    this.sourceFile = testFile;
  }

  public String createSourceKey() {
    String path = this.sourceFile.getPath();
    return ("[" + this.assemblyName + "]" + path);
  }

  public void merge(TestDescription description) {
    this.sourceFile = description.getSourceFile();
    this.name = description.getMethodName();
    if (description.getAssemblyName() == null) {
      this.assemblyName = "AssemblyNotFound";
    } else {
      this.assemblyName = description.getAssemblyName();
    }
  }

}
