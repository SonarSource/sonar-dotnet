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
 * Created on Apr 28, 2009
 */
package org.sonar.plugin.dotnet.gallio;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

/**
 * A report for a unit test file.
 * 
 * @author Jose CHILLAN Apr 28, 2009
 */
public class UnitTestReport {
  private String assemblyName;
  private File sourceFile;
  private int errors = 0;
  private int skipped = 0;
  private int tests = 0;
  private int timeMS = 0;
  private int failures = 0;
  private int asserts;
  private List<TestCaseDetail> details;

  public UnitTestReport() {
    details = new ArrayList<TestCaseDetail>();
  }

  public int getErrors() {
    return errors;
  }

  public void setErrors(int errors) {
    this.errors = errors;
  }

  public int getSkipped() {
    return skipped;
  }

  public void setSkipped(int skipped) {
    this.skipped = skipped;
  }

  public int getTests() {
    return tests;
  }

  public void setTests(int tests) {
    this.tests = tests;
  }

  public int getTimeMS() {
    return timeMS;
  }

  public void setTimeMS(int timeMS) {
    this.timeMS = timeMS;
  }

  public int getFailures() {
    return failures;
  }

  public void setFailures(int failures) {
    this.failures = failures;
  }

  public List<TestCaseDetail> getDetails() {
    return details;
  }

  public void addDetail(TestCaseDetail detail) {
    this.details.add(detail);
    tests++;
    TestStatus status = detail.getStatus();
    switch (status) {
    case FAILED:
      failures++;
      break;
    case ERROR:
      errors++;
      break;
    case SKIPPED:case INCONCLUSIVE:
      skipped++;
      break;
    case SUCCESS:
      break;
    }
    
    // We complete the other indicators
    asserts += detail.getCountAsserts();
    timeMS += detail.getTimeMillis();
  }

  public int getAsserts() {
    return this.asserts;
  }

  public void setAsserts(int asserts) {
    this.asserts = asserts;
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
    return "Assembly=" + assemblyName + ", file:" + sourceFile + "(time="
        + timeMS / 1000. + "s, tests=" + tests + ", failures=" + failures
        + ", ignored=" + skipped + ", asserts=" + asserts + ")";
  }

  @Override
  public int hashCode() {
    final int prime = 31;
    int result = 1;
    result = prime * result
        + ((assemblyName == null) ? 0 : assemblyName.hashCode());
    result = prime * result + asserts;
    result = prime * result + errors;
    result = prime * result + failures;
    result = prime * result + skipped;
    result = prime * result
        + ((sourceFile == null) ? 0 : sourceFile.hashCode());
    result = prime * result + tests;
    result = prime * result + timeMS;
    return result;
  }

  @Override
  @SuppressWarnings("all")
  public boolean equals(Object obj) {
    if (this == obj)
      return true;
    if (obj == null)
      return false;
    if (getClass() != obj.getClass())
      return false;
    UnitTestReport other = (UnitTestReport) obj;
    if (assemblyName == null) {
      if (other.assemblyName != null)
        return false;
    } else if (!assemblyName.equals(other.assemblyName))
      return false;
    if (asserts != other.asserts)
      return false;
    if (errors != other.errors)
      return false;
    if (failures != other.failures)
      return false;
    if (skipped != other.skipped)
      return false;
    if (sourceFile == null) {
      if (other.sourceFile != null)
        return false;
    } else if (!sourceFile.equals(other.sourceFile))
      return false;
    if (tests != other.tests)
      return false;
    if (timeMS != other.timeMS)
      return false;
    return true;
  }
}
