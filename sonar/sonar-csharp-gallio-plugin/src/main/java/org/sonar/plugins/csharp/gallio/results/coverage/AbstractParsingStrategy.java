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
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.findXMLEvent;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.isAStartElement;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.nextPosition;

import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;


import org.codehaus.staxmate.in.SMFilterFactory;
import org.codehaus.staxmate.in.SMInputCursor;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.plugins.csharp.gallio.results.coverage.model.CoveragePoint;
import org.sonar.plugins.csharp.gallio.results.coverage.model.FileCoverage;

import com.google.common.base.Predicate;
import com.google.common.collect.Maps;

public abstract class AbstractParsingStrategy implements CoverageResultParsingStrategy {

  protected Map<Integer, FileCoverage> sourceFilesById = new HashMap<Integer, FileCoverage>();

  private String pointElement;
  private String countVisitsPointAttribute;
  private String startLinePointAttribute;
  private String endLinePointAttribute;
  private String fileIdPointAttribute;
  private String moduleTag;
  private String fileTag;

  /**
   * Retrieve the source files
   * 
   * @param documentElements
   * @return a map containing the source files associated to their id
   */
  abstract Map<Integer, FileCoverage> findFiles(SMInputCursor documentElements);

  public void findPoints(SMInputCursor docsTag) {
    SMInputCursor classTags = descendantElements(docsTag);
    while (nextPosition(classTags) != null) {
      createProjects(classTags);
    }
  }

  protected CoveragePoint createPoint(SMInputCursor pointCursor) {

    CoveragePoint point = new CoveragePoint();
    int startLine = findAttributeIntValue(pointCursor, getStartLinePointAttribute());
    int endLine = findAttributeIntValue(pointCursor, getEndLinePointAttribute());

    point.setCountVisits(findAttributeIntValue(pointCursor, getCountVisitsPointAttribute()));
    point.setStartLine(startLine);
    point.setEndLine(endLine);

    return point;
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


      SMInputCursor pointTag = descendantElements(method);
      List<CoveragePoint> points = new ArrayList<CoveragePoint>();
      int fid = 0;

      while (nextPosition(pointTag) != null) {
        if (isAStartElement(pointTag) && (findAttributeValue(pointTag, getFileIdPointAttribute()) != null)) {
          CoveragePoint point = createPoint(pointTag);
          points.add(point);
          fid = findAttributeIntValue(pointTag, getFileIdPointAttribute());
        }
      }
      FileCoverage fileCoverage = sourceFilesById.get(Integer.valueOf(fid));
      fillFileCoverage(fileCoverage, points);
    } 
  }

  protected void fillFileCoverage(FileCoverage fileCoverage, List<CoveragePoint> points) {

    if (fileCoverage != null) {
      for (CoveragePoint point : points) {
        fileCoverage.addPoint(point);
      }
    }
  }

  public String getFileTag() {
    return fileTag;
  }

  public void setFileTag(String fileTag) {
    this.fileTag = fileTag;
  }

  public String getModuleTag() {
    return moduleTag;
  }

  public void setModuleTag(String moduleTag) {
    this.moduleTag = moduleTag;
  }

  public String getCountVisitsPointAttribute() {
    return countVisitsPointAttribute;
  }

  public void setCountVisitsPointAttribute(String countVisitsPointAttribute) {
    this.countVisitsPointAttribute = countVisitsPointAttribute;
  }

  public String getStartLinePointAttribute() {
    return startLinePointAttribute;
  }

  public void setStartLinePointAttribute(String startLinePointAttribute) {
    this.startLinePointAttribute = startLinePointAttribute;
  }

  public String getEndLinePointAttribute() {
    return endLinePointAttribute;
  }

  public void setEndLinePointAttribute(String endLinePointAttribute) {
    this.endLinePointAttribute = endLinePointAttribute;
  }

  public String getPointElement() {
    return pointElement;
  }

  public void setPointElement(String pointElement) {
    this.pointElement = pointElement;
  }

  public String getFileIdPointAttribute() {
    return fileIdPointAttribute;
  }

  public void setFileIdPointAttribute(String fileIdPointAttribute) {
    this.fileIdPointAttribute = fileIdPointAttribute;
  }

  public List<FileCoverage> parse(final SensorContext context, VisualStudioSolution solution, final Project sonarProject, SMInputCursor root) {

    SMInputCursor rootChildCursor = descendantElements(root);

    // Then all the indexed files are extracted
    sourceFilesById = findFiles(rootChildCursor);

    if (sourceFilesById.isEmpty()) {
      // no source, there is no point to parse further
      return Collections.EMPTY_LIST;
    }

    // filter files according to the exclusion patterns
    sourceFilesById = Maps.filterValues(sourceFilesById, new Predicate<FileCoverage>() {

      public boolean apply(FileCoverage input) {
        return context.isIndexed(org.sonar.api.resources.File.fromIOFile(input.getFile(), sonarProject), false);
      }
    });

    // We finally process the coverage details
    fillProjects(rootChildCursor);

    List<FileCoverage> sourceFiles = new ArrayList<FileCoverage>(sourceFilesById.values());
    return sourceFiles;
  }

  /**
   * Processes the details of the coverage
   * 
   * @param rootChildCursor
   *          cursor positioned to get the method elements
   */
  private void fillProjects(SMInputCursor rootChildCursor) {

    // Sets the cursor to the tags "Type" for PartCover and "Module" for NCover
    rootChildCursor.setFilter(SMFilterFactory.getElementOnlyFilter(getModuleTag()));
    do {
      if (findXMLEvent(rootChildCursor) != null) {

        findPoints(rootChildCursor);
      }
    } while (nextPosition(rootChildCursor) != null);
  }

  public void createProjects(SMInputCursor classElements) {
    SMInputCursor methodElements = descendantElements(classElements);
    while (nextPosition(methodElements) != null) {
      parseMethod(methodElements);
    }
  }

}