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

import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.findAttributeIntValue;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.findAttributeValue;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.findElementName;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.findNextElementName;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.isAStartElement;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.nextPosition;

import java.io.File;
import java.io.IOException;
import java.util.HashMap;
import java.util.Map;

import org.codehaus.staxmate.in.SMInputCursor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.plugin.dotnet.core.SonarPluginException;
import org.sonar.plugin.dotnet.coverage.model.FileCoverage;

public abstract class PartCoverParsingStrategy extends AbstractParsingStrategy {

  private final static Logger log = LoggerFactory.getLogger(PartCoverParsingStrategy.class);


  public PartCoverParsingStrategy() {
    setPointElement("pt");
    setFileIdPointAttribute("fid");
    setCountVisitsPointAttribute("visit");
    setStartLinePointAttribute("sl");
    setEndLinePointAttribute("el");
  }

  @Override
  public Map<Integer, FileCoverage> findFiles(SMInputCursor docsTag) {
    Map<Integer, FileCoverage> files = new HashMap<Integer, FileCoverage>();
    try{
      while( !( this.getFileTag() ).equals( findNextElementName(docsTag) ) ){
        log.debug("Retrieving files");
      }
      while( ( this.getFileTag() ).equals( findElementName(docsTag) ) ){
        if(isAStartElement(docsTag)){
          log.debug( findAttributeValue(docsTag, "url") );
          File document = new File( findAttributeValue(docsTag, "url") );
          String path = document.getPath();
          if( "None".equals(path) || path == null ){
            log.debug("Method coverage data not attached to any file");
          }
          else{
            files.put( findAttributeIntValue(docsTag, "id"),
                new FileCoverage( document.getCanonicalFile() ) ); 
            log.debug("A sourceFile has been added");
          }
        }
        nextPosition(docsTag);
      }
    }catch(IOException e){
      throw new SonarPluginException("Unable to get canonicalFile from the created document while parsing PartCover XML result", e);
    }
    return files;
  }

}
