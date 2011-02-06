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

import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.advanceCursor;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.findAttributeValue;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.findElementName;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.isAStartElement;

import java.util.HashMap;
import java.util.Map;

import org.apache.commons.lang.StringUtils;
import org.codehaus.staxmate.in.SMInputCursor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.plugin.dotnet.coverage.model.FileCoverage;

public class PartCover4ParsingStrategy extends PartCoverParsingStrategy {

  private final static Logger log = LoggerFactory
  .getLogger(PartCover4ParsingStrategy.class);

  private Map<String, String> assemblyNamesById;
  private String id;

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
    if (version != null){
      if (version.startsWith("4.")) {
        log.debug("Using PartCover 4 report format");
        result = true;
      } else {
        log.debug("Not using PartCover 4 report format");
        result = false;
      }
    }
    else if(findAttributeValue(rootCursor, "date") != null){
      log.debug("Guessing PartCover 4 report format with the date Tag");
      result = true;
    }
    return result;
  }

  @Override
  public void saveAssemblyNamesById(SMInputCursor docsTag) {
    Map<String, String> assemblyNamesById = new HashMap<String, String>();
    while( "Assembly".equals( findElementName(docsTag) ) ) {
      if( isAStartElement(docsTag) ){
        String name = findAttributeValue(docsTag, "name");
        String assemblyId = findAttributeValue(docsTag, "id");
        log.debug("Adding assembly {} with its id ({}) in the map", name, assemblyId);
        assemblyNamesById.put(assemblyId, name);
      }
      advanceCursor(docsTag);
    }
    setAssemblyNamesById(assemblyNamesById);
  }

  @Override
  public void handleMethodWithoutPoints(String lineCount,
      FileCoverage fileCoverage) {
    if (!StringUtils.isEmpty(lineCount)) {
      fileCoverage.addUncoveredLines(Integer.parseInt(lineCount));
    }
  }

  @Override
  public void findPoints(String assemblyName, SMInputCursor docsTag,
      PointParserCallback callback) {
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

  public String getId() {
    return id;
  }

  public void saveId(String id) {
    this.id = id;
  }
  
}
