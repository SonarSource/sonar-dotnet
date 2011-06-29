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

import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.findAttributeIntValue;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.findAttributeValue;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.findElementName;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.findNextElementName;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.isAStartElement;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.nextPosition;

import java.io.File;
import java.io.IOException;
import java.util.HashMap;
import java.util.Map;

import org.codehaus.staxmate.in.SMInputCursor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.SonarException;
import org.sonar.plugins.csharp.gallio.results.coverage.model.FileCoverage;

public abstract class PartCoverParsingStrategy extends AbstractParsingStrategy {

  private final static Logger LOG = LoggerFactory.getLogger(PartCoverParsingStrategy.class);

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
    try {
      while ( !(this.getFileTag()).equals(findNextElementName(docsTag))) {
        LOG.debug("Retrieving files");
      }
      while ((this.getFileTag()).equals(findElementName(docsTag))) {
        if (isAStartElement(docsTag)) {
          LOG.debug(findAttributeValue(docsTag, "url"));
          File document = new File(findAttributeValue(docsTag, "url"));
          String path = document.getPath();
          if ("None".equals(path) || path == null) {
            LOG.debug("Method coverage data not attached to any file");
          } else {
            files.put(findAttributeIntValue(docsTag, "id"), new FileCoverage(document.getCanonicalFile()));
            LOG.debug("A sourceFile has been added");
          }
        }
        nextPosition(docsTag);
      }
    } catch (IOException e) {
      throw new SonarException("Unable to get canonicalFile from the created document while parsing PartCover XML result", e);
    }
    return files;
  }

}
