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
import org.sonar.plugin.dotnet.coverage.model.FileCoverage;
import org.sonar.plugin.dotnet.coverage.model.ProjectCoverage;
import org.w3c.dom.Element;

@Deprecated
public class PartCover4ParsingStrategy extends PartCoverParsingStrategy {

  private final static Logger log = LoggerFactory
      .getLogger(PartCover2ParsingStrategy.class);

  public PartCover4ParsingStrategy() {
    setFilePath("/*/File");
    setMethodPath("/*/Type/Method");
  }

  @Override
  String findAssemblyName(Element methodElement) {
    Element typeElement = (Element) methodElement.getParentNode();
    String assemblyRef = typeElement.getAttribute("asmref");
    return evaluateAttribute(typeElement, "../Assembly[@id=\"" + assemblyRef
        + "\"]/@name");
  }

  @Override
  boolean isCompatible(Element element) {
    String version = element.getAttribute("version");
    // Evaluates the part cover version
    final boolean result;
    if (version.startsWith("4.")) {
      log.debug("Using PartCover 4 report format");
      result = true;
    } else if (StringUtils.isEmpty(version)
        && StringUtils.isEmpty(element.getAttribute("ver"))
        && !StringUtils.isEmpty(evaluateAttribute(element, "Assembly/@id"))) {
      log.debug("After guessing, using PartCover 4 report format");
      result = true;
    } else {
      log.debug("Not using PartCover 4 report format");
      result = false;
    }
    return result;
  }

  @Override
  public void handleMethodWithoutPoints(Element methodElement,
      FileCoverage fileCoverage) {
   
    String rawLineNumber = evaluateAttribute(methodElement, "@linecount");
    if (!StringUtils.isEmpty(rawLineNumber)) {
      fileCoverage.addUncoveredLines(Integer.parseInt(rawLineNumber));
    }
  }
  
 
}
