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

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.w3c.dom.Element;

public class PartCover2ParsingStrategy extends PartCoverParsingStrategy {

  private final static Logger log = LoggerFactory
      .getLogger(PartCover2ParsingStrategy.class);

  private String partcoverExactVersion;

  protected PartCover2ParsingStrategy() {
    super();
  }

  @Override
  String findAssemblyName(Element methodElement) {
    Element typeElement = (Element) methodElement.getParentNode();
    return typeElement.getAttribute("asm");
  }

  @Override
  boolean isCompatible(Element element) {
    String version = element.getAttribute("ver");
    // Evaluates the part cover version
    final boolean result;
    if (version.startsWith(partcoverExactVersion)) {
      log.debug("Using PartCover " + partcoverExactVersion + " report format");
      result = true;
    } else {
      log.debug("Not using PartCover " + partcoverExactVersion
          + " report format");
      result = false;
    }
    return result;
  }

  public void setPartcoverExactVersion(String partcoverExactVersion) {
    this.partcoverExactVersion = partcoverExactVersion;
  }

}
