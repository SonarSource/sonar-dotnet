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
 *
 */
package org.apache.maven.dotnet.msbuild.xml;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlType;

/**
 * A StyleCopTask.
 * 
 * @author Jose CHILLAN Jan 14, 2010
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "StyleCopTask", namespace = Constant.NAMESPACE)
public class StyleCopTask {
  @XmlAttribute(name = "ProjectFullPath")
  private String fullPath;
  @XmlAttribute(name = "SourceFiles")
  private String sourceFiles;
  @XmlAttribute(name = "ForceFullAnalysis")
  private String forceFullAnalysis;
  @XmlAttribute(name = "TreatErrorsAsWarnings")
  private String treatErrorAsWarnings;
  @XmlAttribute(name = "OutputFile")
  private String outputFile;
  @XmlAttribute(name = "OverrideSettingsFile")
  private String settingsFile;
  @XmlAttribute(name = "MaxViolationCount")
  private int maxViolationCount;

  /**
   * Constructs a @link{StyleCopTask}.
   */
  public StyleCopTask() {
    this.forceFullAnalysis = "true";
    this.treatErrorAsWarnings = "true";
    this.maxViolationCount = -1;
  }

  /**
   * Returns the fullPath.
   * 
   * @return The fullPath to return.
   */
  public String getFullPath() {
    return this.fullPath;
  }

  /**
   * Sets the fullPath.
   * 
   * @param fullPath
   *          The fullPath to set.
   */
  public void setFullPath(String fullPath) {
    this.fullPath = fullPath;
  }

  /**
   * Returns the sourceFiles.
   * 
   * @return The sourceFiles to return.
   */
  public String getSourceFiles() {
    return this.sourceFiles;
  }

  /**
   * Sets the sourceFiles.
   * 
   * @param sourceFiles
   *          The sourceFiles to set.
   */
  public void setSourceFiles(String sourceFiles) {
    this.sourceFiles = sourceFiles;
  }

  /**
   * Returns the forceFullAnalysis.
   * 
   * @return The forceFullAnalysis to return.
   */
  public String getForceFullAnalysis() {
    return this.forceFullAnalysis;
  }

  /**
   * Sets the forceFullAnalysis.
   * 
   * @param forceFullAnalysis
   *          The forceFullAnalysis to set.
   */
  public void setForceFullAnalysis(String forceFullAnalysis) {
    this.forceFullAnalysis = forceFullAnalysis;
  }

  /**
   * Returns the treatErrorAsWarnings.
   * 
   * @return The treatErrorAsWarnings to return.
   */
  public String getTreatErrorAsWarnings() {
    return this.treatErrorAsWarnings;
  }

  /**
   * Sets the treatErrorAsWarnings.
   * 
   * @param treatErrorAsWarnings
   *          The treatErrorAsWarnings to set.
   */
  public void setTreatErrorAsWarnings(String treatErrorAsWarnings) {
    this.treatErrorAsWarnings = treatErrorAsWarnings;
  }

  /**
   * Returns the outputFile.
   * 
   * @return The outputFile to return.
   */
  public String getOutputFile() {
    return this.outputFile;
  }

  /**
   * Sets the outputFile.
   * 
   * @param outputFile
   *          The outputFile to set.
   */
  public void setOutputFile(String outputFile) {
    this.outputFile = outputFile;
  }

  /**
   * Returns the settingsFile.
   * 
   * @return The settingsFile to return.
   */
  public String getSettingsFile() {
    return this.settingsFile;
  }

  /**
   * Sets the settingsFile.
   * 
   * @param settingsFile
   *          The settingsFile to set.
   */
  public void setSettingsFile(String settingsFile) {
    this.settingsFile = settingsFile;
  }

  /**
   * Returns the maxViolationCount.
   * 
   * @return The maxViolationCount to return.
   */
  public int getMaxViolationCount() {
    return this.maxViolationCount;
  }

  /**
   * Sets the maxViolationCount.
   * 
   * @param maxViolationCount
   *          The maxViolationCount to set.
   */
  public void setMaxViolationCount(int maxViolationCount) {
    this.maxViolationCount = maxViolationCount;
  }

}
