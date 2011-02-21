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


package org.sonar.plugin.dotnet.gendarme;

import java.text.ParseException;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.ParsingUtils;

/**
 * This class represents the location of a defect, 
 * i.e. a source file path and a line number
 * 
 * @author Alexandre Victoor
 */
public class DefectLocation {
  
  private final static Logger log 
    = LoggerFactory.getLogger(DefectLocation.class);
  
  private String path;
  private Integer lineNumber;
  
  private DefectLocation() {
    // use factory method
  }
  
  /**
   * Factory method that takes in input a location 
   * element value from a gendarme xml report.
   * @param source  the source string from the gendarme report 
   * @return        a defect location object containing the parsing result
   */
  public static DefectLocation parse(String source) {
    final DefectLocation result = new DefectLocation();

    final int lineNumberPosition = source.lastIndexOf('(');
    if (lineNumberPosition != -1 && source.endsWith(")")) {
      result.path = source.substring(0, lineNumberPosition);
      String lineNumberInfo 
        = source.substring(lineNumberPosition + 1, source.length() - 1);
      if (StringUtils.contains(lineNumberInfo, ',')) {
        // something like "123,34"
        result.lineNumber 
          = safeParseInt(StringUtils.substringBefore(lineNumberInfo, ","));
      } else {
        // something like "~123"
        result.lineNumber 
          = safeParseInt(lineNumberInfo.substring(1));
      }
    } else {
      result.path = source;
    }
    return result;
  }
  
  private static Integer safeParseInt(String lineStr) {
    if (StringUtils.isBlank(lineStr)) {
      log.debug("Empty string to parse to int");
      return null;
    }
    try {
      return (int) ParsingUtils.parseNumber(lineStr);
    } catch (ParseException ignore) {
      log.info("Int parsing error", ignore);
      return null;
    }
  }
  
  public String getPath() {
    return path;
  }
 
  public Integer getLineNumber() {
    return lineNumber;
  }

}
