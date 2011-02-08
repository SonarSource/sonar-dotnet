/*
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

package org.sonar.plugin.dotnet.coverage.stax;

import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.descendantElements;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.findAttributeIntValue;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.findAttributeValue;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.isAStartElement;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.nextPosition;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import org.codehaus.staxmate.in.SMInputCursor;
import org.sonar.plugin.dotnet.core.AbstractXmlParser;
import org.sonar.plugin.dotnet.coverage.model.CoveragePoint;
import org.sonar.plugin.dotnet.coverage.model.FileCoverage;

public abstract class AbstractParsingStrategy extends AbstractXmlParser {

  private String pointElement;
  private String countVisitsPointAttribute;
  private String startLinePointAttribute;
  private String endLinePointAttribute;
  private String fileIdPointAttribute;
  private String moduleTag;
  private String fileTag;
  protected String assemblyReference;


  /**
   * Retrieve associated assembly name, depending on the report structure
   * 
   * @param docTag 
   * @return the assembly name
   */
  abstract String findAssemblyName(SMInputCursor docTag);

  /**
   * Due to a special output format of PartCover4, we need to put the assembly name
   * in order to retrieve them later. This method is only used with PartCover4.
   * 
   * @param docsTag 
   */
  abstract void saveAssemblyNamesById(SMInputCursor docsTag);

  /**
   * Due to a special output format of PartCover4, we need to put the id associated
   * to the assembly in order to retrieve the assembly name.
   * This method is only used with PartCover4.
   * 
   * @param assemblyId
   */
  abstract void saveId(String assemblyId);

  /**
   * Help to retrieve from which version the xml is coming from and choose the right strategy
   * 
   * @param rootCursor
   * @return true if the strategy matches the version, else false
   */
  abstract boolean isCompatible(SMInputCursor rootCursor);

  /**
   * Retrieve the source files
   * 
   * @param documentElements
   * @return a map containing the source files associated to their id
   */
  abstract Map<Integer, FileCoverage> findFiles(SMInputCursor documentElements);

  /**
   * Retrieve Assembly Reference to associate it with the right assembly.
   * This method is only used with PartCover4.
   * 
   * @return the reference to the assembly
   */
  abstract String getAssemblyReference();

  /**
   * Save the reference to the assembly.
   * This method is only used with PartCover4.
   * 
   * @param assemblyReference
   */
  abstract void setAssemblyReference(String assemblyReference);

  public void findPoints(String assemblyName, SMInputCursor docsTag,
      PointParserCallback callback) {
    SMInputCursor classTags = descendantElements(docsTag);
    while(nextPosition(classTags) != null) {
      callback.createProjects(assemblyName, classTags);
    }
  }

  private CoveragePoint createPoint(SMInputCursor pointCursor) {

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
   * @param method : method to parse
   * @param assemblyName : corresponding assembly name
   * @param sourceFilesById : map containing the source files
   */
  public final FileCoverage parseMethod(SMInputCursor method, String assemblyName, Map<Integer, FileCoverage> sourceFilesById) {

    final FileCoverage fileCoverage;
    if(isAStartElement(method)){

      initializeVariables(method);

      SMInputCursor pointTag = descendantElements(method);
      List<CoveragePoint> points = new ArrayList<CoveragePoint>();
      int fid = 0;

      while(nextPosition(pointTag) != null){
        setMethodWithPointsToTrue();
        if(isAStartElement(pointTag) && (findAttributeValue(pointTag, getFileIdPointAttribute()) != null)){
          CoveragePoint point = createPoint(pointTag);
          points.add(point);
          fid = findAttributeIntValue(pointTag, getFileIdPointAttribute());
        }
      }
      fileCoverage = createFileCoverage(sourceFilesById, fid);
      fillFileCoverage(assemblyName, fileCoverage, points);
    } else {
      fileCoverage = null;
    }

    return fileCoverage;
  }

  /**
   * Initialize variables used by PartCover 4
   * @param method
   */
  protected void initializeVariables(SMInputCursor method){};

  /**
   * Used by PartCover 4 to decide if the method has uncovered lines
   */
  protected void setMethodWithPointsToTrue(){};

  /**
   * Retrieve the fileCoverage to be filled, this method is overrided in the PartCover 4 strategy
   * @param sourceFilesById
   * @param fileCoverage
   * @param fid
   * @return the fileCoverage
   */
  protected FileCoverage createFileCoverage(Map<Integer, FileCoverage> sourceFilesById, int fid){
    return sourceFilesById.get(Integer.valueOf(fid));
  }

  protected void fillFileCoverage(String assemblyName, FileCoverage fileCoverage,
      List<CoveragePoint> points) {

    if(fileCoverage != null){
      for(CoveragePoint point : points){
        fileCoverage.addPoint(point);
      }
      fileCoverage.setAssemblyName(assemblyName);
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

}
