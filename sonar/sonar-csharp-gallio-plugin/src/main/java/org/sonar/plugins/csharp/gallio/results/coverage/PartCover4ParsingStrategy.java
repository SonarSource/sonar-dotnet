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
package org.sonar.plugins.csharp.gallio.results.coverage;

import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.advanceCursor;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.findAttributeValue;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.findElementName;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.isAStartElement;

import java.util.HashMap;
import java.util.Map;

import org.apache.commons.lang.StringUtils;
import org.codehaus.staxmate.in.SMInputCursor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.plugins.csharp.gallio.results.coverage.model.FileCoverage;

public class PartCover4ParsingStrategy extends PartCoverParsingStrategy {

  private final static Logger LOG = LoggerFactory.getLogger(PartCover4ParsingStrategy.class);

  private Map<String, String> assemblyNamesById;
  private String id;
  private String lineCount;
  private String temporaryFileId;
  private boolean areUncoveredLines;
  private boolean methodWithPoints;

  public PartCover4ParsingStrategy() {
    setModuleTag("Type");
    setFileTag("File");
    setAssemblyReference("asmref");
  }

  @Override
  public String findAssemblyName(SMInputCursor typeTag) {
    return getAssemblyNamesById().get(getId());
  }

  @Override
  public boolean isCompatible(SMInputCursor rootCursor) {
    boolean result = false;
    String version = findAttributeValue(rootCursor, "version");
    if (version != null) {
      if (version.startsWith("4.")) {
        LOG.debug("Using PartCover 4 report format");
        result = true;
      } else {
        LOG.debug("Not using PartCover 4 report format");
        result = false;
      }
    } else if (findAttributeValue(rootCursor, "date") != null) {
      LOG.debug("Guessing PartCover 4 report format with the date Tag");
      result = true;
    }
    return result;
  }

  @Override
  public void saveAssemblyNamesById(SMInputCursor docsTag) {
    this.assemblyNamesById = new HashMap<String, String>();
    while ("Assembly".equals(findElementName(docsTag))) {
      if (isAStartElement(docsTag)) {
        String name = findAttributeValue(docsTag, "name");
        String assemblyId = findAttributeValue(docsTag, "id");
        LOG.debug("Adding assembly {} with its id ({}) in the map", name, assemblyId);
        assemblyNamesById.put(assemblyId, name);
      }
      advanceCursor(docsTag);
    }
    setAssemblyNamesById(assemblyNamesById);
  }

  @Override
  protected void initializeVariables(SMInputCursor method) {
    this.lineCount = findAttributeValue(method, "linecount");
    this.temporaryFileId = findAttributeValue(method, "fid");
    this.areUncoveredLines = (temporaryFileId != null);
    this.methodWithPoints = false;
  }

  @Override
  protected void setMethodWithPointsToTrue() {
    this.methodWithPoints = true;
  }

  @Override
  protected FileCoverage createFileCoverage(Map<Integer, FileCoverage> sourceFilesById, int fid) {
    FileCoverage fileCoverage = null;
    if ( !this.methodWithPoints && this.areUncoveredLines) {
      fileCoverage = sourceFilesById.get(Integer.valueOf(this.temporaryFileId));
      handleMethodWithoutPoints(this.lineCount, fileCoverage);
    } else {
      fileCoverage = sourceFilesById.get(Integer.valueOf(fid));
    }
    return fileCoverage;
  }

  /**
   * This method is used by PartCover4 to take uncovered lines into account
   * 
   * @param lineCount
   * @param fileCoverage
   */
  private void handleMethodWithoutPoints(String lineCount, FileCoverage fileCoverage) {
    if ( !StringUtils.isEmpty(lineCount)) {
      fileCoverage.addUncoveredLines(Integer.parseInt(lineCount));
    }
  }

  @Override
  public void findPoints(String assemblyName, SMInputCursor docsTag, PointParserCallback callback) {
    callback.createProjects(assemblyName, docsTag);
  }

  @Override
  public String getAssemblyReference() {
    return assemblyReference;
  }

  @Override
  public void setAssemblyReference(String asmRef) {
    this.assemblyReference = asmRef;
  }

  public Map<String, String> getAssemblyNamesById() {
    return assemblyNamesById;
  }

  private void setAssemblyNamesById(Map<String, String> assemblyNamesById) {
    this.assemblyNamesById = assemblyNamesById;
  }

  private String getId() {
    return id;
  }

  public void saveId(String id) {
    this.id = id;
  }

}
