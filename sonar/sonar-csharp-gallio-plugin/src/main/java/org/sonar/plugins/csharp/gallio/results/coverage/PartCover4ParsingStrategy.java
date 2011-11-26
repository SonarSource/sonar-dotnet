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

import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.descendantElements;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.findAttributeIntValue;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.findAttributeValue;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.isAStartElement;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.nextPosition;

import java.util.ArrayList;
import java.util.List;

import org.apache.commons.lang.StringUtils;
import org.codehaus.staxmate.in.SMInputCursor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.plugins.csharp.gallio.results.coverage.model.CoveragePoint;
import org.sonar.plugins.csharp.gallio.results.coverage.model.FileCoverage;

public class PartCover4ParsingStrategy extends PartCoverParsingStrategy {

  private static final Logger LOG = LoggerFactory.getLogger(PartCover4ParsingStrategy.class);

  public PartCover4ParsingStrategy() {
    setModuleTag("Type");
    setFileTag("File");
  }

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
  
  /**
   * Parse a method, retrieving all its points
   * 
   * @param method
   *          : method to parse
   * @param assemblyName
   *          : corresponding assembly name
   * @param sourceFilesById
   *          : map containing the source files
   */
  public void parseMethod(SMInputCursor method) {

    if (isAStartElement(method)) {

      String lineCount = findAttributeValue(method, "linecount");
      String temporaryFileId = findAttributeValue(method, "fid");
      boolean areUncoveredLines = (temporaryFileId != null);
      boolean methodWithPoints = false;

      SMInputCursor pointTag = descendantElements(method);
      List<CoveragePoint> points = new ArrayList<CoveragePoint>();
      int fid = 0;

      while (nextPosition(pointTag) != null) {
        methodWithPoints = true;
        if (isAStartElement(pointTag) && (findAttributeValue(pointTag, getFileIdPointAttribute()) != null)) {
          CoveragePoint point = createPoint(pointTag);
          points.add(point);
          fid = findAttributeIntValue(pointTag, getFileIdPointAttribute());
        }
      }
      final FileCoverage fileCoverage;
      if ( !methodWithPoints && areUncoveredLines) {
        fileCoverage = sourceFilesById.get(Integer.valueOf(temporaryFileId));
        handleMethodWithoutPoints(lineCount, fileCoverage);
      } else {
        fileCoverage = sourceFilesById.get(Integer.valueOf(fid));
      }
      fillFileCoverage(fileCoverage, points);
    } 
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
  public void findPoints(SMInputCursor docsTag) {
    createProjects(docsTag);
  }
}
