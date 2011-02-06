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

import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.findAttributeValue;

import org.codehaus.staxmate.in.SMInputCursor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.plugin.dotnet.coverage.model.FileCoverage;

public abstract class PartCover2ParsingStrategy extends PartCoverParsingStrategy{

  private final static Logger log = LoggerFactory
  .getLogger(PartCover2ParsingStrategy.class);

  protected PartCover2ParsingStrategy() {
    super();
  }

  @Override
  public String findAssemblyName(SMInputCursor typeTag) {
    return findAttributeValue(typeTag, "asm");
  }

  @Override
  public void saveAssemblyNamesById(SMInputCursor docsTag) {
    log.debug("Creating a map with assembly names would be useless with PartCover 2.x version");
  }

  @Override
  public void handleMethodWithoutPoints(String lineCount, FileCoverage fileCoverage) {
    log.debug("Unused method for PartCover 2.x");  
  }

  @Override
  public void saveId(String id) {
    log.debug("Unused method for PartCover 2.x");  
  }
  
  @Override
  public String getAssemblyReference(){
    log.debug("Unused method for PartCover 2.x, return \"\" to avoid null pointer");
    return "";
  }

  @Override
  public void setAssemblyReference(String asmRef) {
    log.debug("Unused method for PartCover 2.x"); 
  }
  
}
