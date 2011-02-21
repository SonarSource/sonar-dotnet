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

package org.sonar.plugin.dotnet.coverage.stax;

import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.findAttributeValue;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.findElementName;

import org.codehaus.staxmate.in.SMInputCursor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;


public class PartCover23ParsingStrategy extends PartCover2ParsingStrategy {

  private final static Logger log = LoggerFactory
  .getLogger(PartCover23ParsingStrategy.class);
  
  public PartCover23ParsingStrategy() {
    setModuleTag("Type");
    setFileTag("File");
  }

  @Override
  public boolean isCompatible(SMInputCursor rootCursor) {
    boolean result = false;
    String version = findAttributeValue(rootCursor, "ver");
    log.debug("Visible version is : {}", version);
    if (version != null){
      if (version.startsWith("2.3")) {
        log.debug("Using PartCover 2.3 report format");
        result = true;
      } else {
        log.debug("Not using PartCover 2.3 report format");
        result = false;
      }
    }
    else{
      if(findAttributeValue(rootCursor, "exitCode") == null && "PartCoverReport".equals(findElementName(rootCursor))){
        log.debug("Using PartCover 2.3 report format");
        result = true;
      }
      else {
        log.debug("Not using PartCover 2.3 report format");
        result = false;
      }
    }
    return result;
  }
  
}
