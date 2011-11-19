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

import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.advanceCursor;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.descendantElements;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.findAttributeIntValue;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.findAttributeValue;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.findElementName;
import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.nextPosition;

import java.io.File;
import java.io.IOException;
import java.util.HashMap;
import java.util.Map;

import org.codehaus.staxmate.in.SMFilterFactory;
import org.codehaus.staxmate.in.SMInputCursor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.SonarException;
import org.sonar.plugins.csharp.gallio.results.coverage.model.FileCoverage;

public class NCover3ParsingStrategy extends AbstractParsingStrategy {

  private static final Logger LOG = LoggerFactory.getLogger(NCover3ParsingStrategy.class);

  public NCover3ParsingStrategy() {
    setPointElement("seqpnt");
    setFileIdPointAttribute("doc");
    setCountVisitsPointAttribute("vc");
    setStartLinePointAttribute("l");
    setEndLinePointAttribute("el");
    setModuleTag("module");
  }

  @Override
  public String findAssemblyName(SMInputCursor docTag) {
    return findAttributeValue(docTag, "assembly");
  }

  @Override
  public Map<Integer, FileCoverage> findFiles(SMInputCursor docsTag) {
    Map<Integer, FileCoverage> files = new HashMap<Integer, FileCoverage>();
    try {
      docsTag.setFilter(SMFilterFactory.getElementOnlyFilter("documents"));
      nextPosition(docsTag);
      SMInputCursor doc = descendantElements(docsTag);
      while (nextPosition(doc) != null) {
        File document = new File(findAttributeValue(doc, "url"));
        String path = document.getPath();
        LOG.debug(path);
        if ("None".equals(path) || path == null) {
          LOG.debug("Method coverage data not attached to any file");
        } else {
          files.put(findAttributeIntValue(doc, "id"), new FileCoverage(document.getCanonicalFile()));
        }
        advanceCursor(doc);
      }
    } catch (IOException e) {
      throw new SonarException("Unable to get canonicalFile from the created document while parsing NCover3 XML result", e);
    }
    return files;
  }

  public boolean isCompatible(SMInputCursor rootCursor) {
    return "coverage".equals(findElementName(rootCursor));
  }

  @Override
  public void saveAssemblyNamesById(SMInputCursor docsTag) {
    LOG.debug("Unused method for NCover 3");
  }

  @Override
  public void saveId(String id) {
    LOG.debug("Unused method for NCover 3");
  }

  @Override
  public String getAssemblyReference() {
    LOG.debug("Unused method for NCover3, return \"\" to avoid null pointer");
    return "";
  }

  @Override
  public void setAssemblyReference(String asmRef) {
    LOG.debug("Unused method for NCover3");
  }

}
