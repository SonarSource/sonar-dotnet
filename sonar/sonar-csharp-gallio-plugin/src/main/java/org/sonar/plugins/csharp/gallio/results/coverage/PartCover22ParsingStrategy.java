/*
 * Sonar C# Plugin :: Gallio
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
package org.sonar.plugins.csharp.gallio.results.coverage;

import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.findAttributeValue;

import org.codehaus.staxmate.in.SMInputCursor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class PartCover22ParsingStrategy extends PartCover2ParsingStrategy {

  private static final Logger LOG = LoggerFactory.getLogger(PartCover22ParsingStrategy.class);

  public PartCover22ParsingStrategy() {
    setModuleTag("type");
    setFileTag("file");
  }

  public boolean isCompatible(SMInputCursor rootCursor) {
    boolean result = false;
    String version = findAttributeValue(rootCursor, "ver");
    LOG.debug("Visible version is : {}", version);
    if (version != null) {
      if (version.startsWith("2.2")) {
        LOG.debug("Using PartCover 2.2 report format");
        result = true;
      } else {
        LOG.debug("Not using PartCover 2.2 report format2");
        result = false;
      }
    } else {
      if (findAttributeValue(rootCursor, "exitCode") != null) {
        LOG.debug("Using PartCover 2.2 report format");
        result = true;
      } else {
        LOG.debug("Not using PartCover 2.2 report format2");
        result = false;
      }
    }
    return result;
  }
}
