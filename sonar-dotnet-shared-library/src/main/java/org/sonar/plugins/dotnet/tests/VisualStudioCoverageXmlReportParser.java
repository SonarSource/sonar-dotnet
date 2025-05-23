/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
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
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package org.sonar.plugins.dotnet.tests;

import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import static org.sonarsource.dotnet.shared.CallableUtils.lazy;

public class VisualStudioCoverageXmlReportParser implements CoverageParser {

  private static final Logger LOG = LoggerFactory.getLogger(VisualStudioCoverageXmlReportParser.class);
  private final FileService fileService;

  public VisualStudioCoverageXmlReportParser(FileService fileService) {
    this.fileService = fileService;
  }

  @Override
  public void accept(File file, Coverage coverage) {
    LOG.debug("The current user dir is '{}'.", lazy(() -> System.getProperty("user.dir")));
    LOG.info("Parsing the Visual Studio coverage XML report {}", file.getAbsolutePath());
    new Parser(file, coverage).parse();
  }

  private class Parser {

    private final File file;
    private final Map<Integer, List<Integer>> coveredLines = new HashMap<>();
    private final Map<Integer, List<Integer>> uncoveredLines = new HashMap<>();
    private final Coverage coverage;

    Parser(File file, Coverage coverage) {
      this.file = file;
      this.coverage = coverage;
    }

    public void parse() {
      try (XmlParserHelper xmlParserHelper = new XmlParserHelper(file)) {
        checkRootTag(xmlParserHelper);
        dispatchTags(xmlParserHelper);
      } catch (IOException e) {
        throw new IllegalStateException("Unable to close report", e);
      }
    }

    private void dispatchTags(XmlParserHelper xmlParserHelper) {
      String tagName;
      while ((tagName = xmlParserHelper.nextStartTag()) != null) {
        if ("module".equals(tagName)) {
          handleModuleTag();
        } else if ("range".equals(tagName)) {
          handleRangeTag(xmlParserHelper);
        } else if ("source_file".equals(tagName)) {
          handleSourceFileTag(xmlParserHelper);
        }
      }
    }

    private void handleModuleTag() {
      coveredLines.clear();
      uncoveredLines.clear();
    }

    private void handleRangeTag(XmlParserHelper xmlParserHelper) {
      int source = xmlParserHelper.getRequiredIntAttribute("source_id");
      String covered = xmlParserHelper.getRequiredAttribute("covered");

      int line = xmlParserHelper.getRequiredIntAttribute("start_line");

      if ("yes".equals(covered) || "partial".equals(covered)) {
        coveredLines.putIfAbsent(source, new ArrayList<>());
        coveredLines.get(source).add(line);
      } else if ("no".equals(covered)) {
        uncoveredLines.putIfAbsent(source, new ArrayList<>());
        uncoveredLines.get(source).add(line);
      } else {
        throw xmlParserHelper.parseError("Unsupported \"covered\" value \"" + covered + "\", expected one of \"yes\", \"partial\" or \"no\"");
      }
    }

    private void handleSourceFileTag(XmlParserHelper xmlParserHelper) {
      int id = xmlParserHelper.getRequiredIntAttribute("id");
      String path = xmlParserHelper.getRequiredAttribute("path");

      String canonicalPath;
      try {
        canonicalPath = new File(path).getCanonicalPath();
      } catch (IOException e) {
        LOG.warn("Skipping the import of Visual Studio XML code coverage for the invalid file path: " + path
          + " at line " + xmlParserHelper.stream().getLocation().getLineNumber(), e);
        return;
      }

      if (!fileService.isSupportedAbsolute(canonicalPath)) {
        Optional<String> absolutePath = fileService.getAbsolutePath(path);
        if (absolutePath.isPresent()) {
          canonicalPath = absolutePath.get();
          LOG.debug("Found indexed file '{}' for coverage entry '{}'.", canonicalPath, path);
        } else {
          // debug logging should be done in the fileService
          return;
        }
      }

      if (coveredLines.containsKey(id)) {
        LOG.trace("Found covered lines for id '{}' for path '{}'", id, canonicalPath);

        for (Integer line : coveredLines.get(id)) {
          coverage.addHits(canonicalPath, line, 1);
        }
      }

      if (uncoveredLines.containsKey(id)) {
        LOG.trace("Found uncovered lines for id '{}' for path '{}'", id, canonicalPath);

        for (Integer line : uncoveredLines.get(id)) {
          coverage.addHits(canonicalPath, line, 0);
        }
      }
    }

    private void checkRootTag(XmlParserHelper xmlParserHelper) {
      xmlParserHelper.checkRootTag("results");
    }

  }

}
