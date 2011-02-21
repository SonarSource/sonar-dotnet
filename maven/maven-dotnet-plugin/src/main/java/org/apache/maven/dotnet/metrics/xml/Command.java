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
 * Created on Apr 7, 2009
 */
package org.apache.maven.dotnet.metrics.xml;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;

/**
 * A command to generate XML for a SourceMonitor command file.
 * 
 * @author Jose CHILLAN Apr 9, 2009
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "command")
public class Command {
  @XmlElement(name = "project_file")
  private String projectFile;
  @XmlElement(name = "parse_utf8_files")
  private boolean parseUtf8;
  @XmlElement(name = "project_language")
  private String projectLanguage;
  @XmlElement(name = "source_directory")
  private String sourceDirectory;
  @XmlElement(name = "checkpoint_name")
  private String checkPointName;

  @XmlElement(name = "checkpoint_date")
  private String checkPointDate;

  @XmlElement(name = "file_extensions")
  private String fileExtensions;
  @XmlElement(name = "include_subdirectories")
  private boolean includeSubdirectories;
  @XmlElement(name = "ignore_headers_footers")
  private boolean ignoreHeaderFooters;

  @XmlElement(name = "export")
  private Export export;

  @XmlElement(name = "source_subdirectory_list")
  private SourceSubdirectoryList subdirectoryList;

  public Command() {
    // Parses the utf8 by default
    this.parseUtf8 = true;
  }

  /**
   * Returns the projectFile.
   * 
   * @return The projectFile to return.
   */
  public String getProjectFile() {
    return this.projectFile;
  }

  /**
   * Sets the projectFile.
   * 
   * @param projectFile
   *          The projectFile to set.
   */
  public void setProjectFile(String projectFile) {
    this.projectFile = projectFile;
  }

  /**
   * Returns the projectLanguage.
   * 
   * @return The projectLanguage to return.
   */
  public String getProjectLanguage() {
    return this.projectLanguage;
  }

  /**
   * Sets the projectLanguage.
   * 
   * @param projectLanguage
   *          The projectLanguage to set.
   */
  public void setProjectLanguage(String projectLanguage) {
    this.projectLanguage = projectLanguage;
  }

  /**
   * Returns the sourceDirectory.
   * 
   * @return The sourceDirectory to return.
   */
  public String getSourceDirectory() {
    return this.sourceDirectory;
  }

  /**
   * Sets the sourceDirectory.
   * 
   * @param sourceDirectory
   *          The sourceDirectory to set.
   */
  public void setSourceDirectory(String sourceDirectory) {
    this.sourceDirectory = sourceDirectory;
  }

  /**
   * Returns the checkPointName.
   * 
   * @return The checkPointName to return.
   */
  public String getCheckPointName() {
    return this.checkPointName;
  }

  /**
   * Sets the checkPointName.
   * 
   * @param checkPointName
   *          The checkPointName to set.
   */
  public void setCheckPointName(String checkPointName) {
    this.checkPointName = checkPointName;
  }

  /**
   * Returns the checkPointDate.
   * 
   * @return The checkPointDate to return.
   */
  public String getCheckPointDate() {
    return this.checkPointDate;
  }

  /**
   * Sets the checkPointDate.
   * 
   * @param checkPointDate
   *          The checkPointDate to set.
   */
  public void setCheckPointDate(String checkPointDate) {
    this.checkPointDate = checkPointDate;
  }

  /**
   * Returns the fileExtensions.
   * 
   * @return The fileExtensions to return.
   */
  public String getFileExtensions() {
    return this.fileExtensions;
  }

  /**
   * Sets the fileExtensions.
   * 
   * @param fileExtensions
   *          The fileExtensions to set.
   */
  public void setFileExtensions(String fileExtensions) {
    this.fileExtensions = fileExtensions;
  }

  /**
   * Returns the includeSubdirectories.
   * 
   * @return The includeSubdirectories to return.
   */
  public boolean isIncludeSubdirectories() {
    return this.includeSubdirectories;
  }

  /**
   * Sets the includeSubdirectories.
   * 
   * @param includeSubdirectories
   *          The includeSubdirectories to set.
   */
  public void setIncludeSubdirectories(boolean includeSubdirectories) {
    this.includeSubdirectories = includeSubdirectories;
  }

  /**
   * Returns the ignoreHeaderFooters.
   * 
   * @return The ignoreHeaderFooters to return.
   */
  public boolean isIgnoreHeaderFooters() {
    return this.ignoreHeaderFooters;
  }

  /**
   * Sets the ignoreHeaderFooters.
   * 
   * @param ignoreHeaderFooters
   *          The ignoreHeaderFooters to set.
   */
  public void setIgnoreHeaderFooters(boolean ignoreHeaderFooters) {
    this.ignoreHeaderFooters = ignoreHeaderFooters;
  }

  /**
   * Returns the export.
   * 
   * @return The export to return.
   */
  public Export getExport() {
    return this.export;
  }

  /**
   * Sets the export.
   * 
   * @param export
   *          The export to set.
   */
  public void setExport(Export export) {
    this.export = export;
  }

  /**
   * Returns the subdirectoryList.
   * 
   * @return The subdirectoryList to return.
   */
  public SourceSubdirectoryList getSubdirectoryList() {
    return this.subdirectoryList;
  }

  /**
   * Sets the subdirectoryList.
   * 
   * @param subdirectoryList
   *          The subdirectoryList to set.
   */
  public void setSubdirectoryList(SourceSubdirectoryList subdirectoryList) {
    this.subdirectoryList = subdirectoryList;
  }

  /**
   * Returns the parseUtf8.
   * 
   * @return The parseUtf8 to return.
   */
  public boolean isParseUtf8() {
    return this.parseUtf8;
  }

  /**
   * Sets the parseUtf8.
   * 
   * @param parseUtf8
   *          The parseUtf8 to set.
   */
  public void setParseUtf8(boolean parseUtf8) {
    this.parseUtf8 = parseUtf8;
  }

}
