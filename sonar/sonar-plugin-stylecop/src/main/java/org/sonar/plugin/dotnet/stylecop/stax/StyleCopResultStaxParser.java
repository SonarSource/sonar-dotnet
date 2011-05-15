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

package org.sonar.plugin.dotnet.stylecop.stax;

import static org.sonar.plugin.dotnet.core.StaxHelper.advanceCursor;
import static org.sonar.plugin.dotnet.core.StaxHelper.descendantSpecifiedElements;
import static org.sonar.plugin.dotnet.core.StaxHelper.findAttributeValue;
import static org.sonar.plugin.dotnet.core.StaxHelper.isAStartElement;
import static org.sonar.plugin.dotnet.core.StaxHelper.nextPosition;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamException;

import org.codehaus.staxmate.SMInputFactory;
import org.codehaus.staxmate.in.SMHierarchicCursor;
import org.codehaus.staxmate.in.SMInputCursor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.BatchExtension;
import org.sonar.plugin.dotnet.core.SonarPluginException;
import org.sonar.plugin.dotnet.stylecop.StyleCopViolation;


public class StyleCopResultStaxParser implements BatchExtension {
  
  private final static Logger log = LoggerFactory.getLogger(StyleCopResultStaxParser.class);

  /**
   * Parses a StyleCop report.
   * 
   * @param reportFile
   * @return violations returned by the StyleCop report
   */
  public List<StyleCopViolation> parse(File reportFile) {

    List<StyleCopViolation> violationsToReturn = new ArrayList<StyleCopViolation>();
    try{
      log.debug("Start parsing...");
      SMInputFactory inf = new SMInputFactory(XMLInputFactory.newInstance());
      SMHierarchicCursor rootCursor = inf.rootElementCursor(reportFile);
      advanceCursor(rootCursor);    
      SMInputCursor violations = descendantSpecifiedElements(rootCursor, "Violation");

      while (nextPosition(violations) != null && isAStartElement(violations)) {

        String lineNumber = findAttributeValue(violations, "LineNumber");
        String filePath = findAttributeValue(violations, "Source");
        String key = findAttributeValue(violations, "Rule");
        String message = violations.collectDescendantText();

        violationsToReturn.add( new StyleCopViolation(lineNumber, filePath, key, message) );
      }
      return violationsToReturn;

    }catch(XMLStreamException e){
      throw new SonarPluginException("Error while parsing StyleCop report", e);
    }
  }



}
