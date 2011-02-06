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

package org.sonar.plugin.dotnet.coverage;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.plugin.dotnet.core.AbstractXmlParser;
import org.sonar.plugin.dotnet.coverage.model.FileCoverage;
import org.w3c.dom.Element;

@Deprecated
public abstract class AbstractParsingStrategy extends AbstractXmlParser {
  
  private final static Logger log = LoggerFactory.getLogger(AbstractParsingStrategy.class);

  private String filePath;
  private String methodPath;

  private String pointElement;
  private String countVisitsPointAttribute;
  private String startLinePointAttribute;
  private String endLinePointAttribute;
  private String fileIdPointAttribute;
  

  public String getFilePath() {
    return filePath;
  }

  public void setFilePath(String filePath) {
    this.filePath = filePath;
  }

  public String getMethodPath() {
    return methodPath;
  }

  public void setMethodPath(String methodPath) {
    this.methodPath = methodPath;
  }

  abstract String findAssemblyName(Element methodElement);
  
  /**
   * Find the file identifier of the source file containing a method
   * @param methodElement a dom element representing a c# method
   * @return null if no source file found. Otherwise the source 
   *              file identifier as an integer value
   */
  public Integer findFileId(Element methodElement) {
    String rawResult = evaluateAttribute(methodElement, ".//@"+fileIdPointAttribute);
    final Integer result;
    if (StringUtils.isEmpty(rawResult)) {
      result = null;
    } else {
      result = Integer.parseInt(rawResult); 
    }
    return result;
  }

  abstract boolean isCompatible(Element rootElement);

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

  public void handleMethodWithoutPoints(Element methodElement, FileCoverage fileCoverage) {
    // should not happen 
    // except when using partcover2.3 and 4
  }

}
