/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2020 SonarSource SA
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
import java.util.function.Predicate;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

public class VisualStudioCoverageXmlReportParser implements CoverageParser {

  private static final Logger LOG = Loggers.get(VisualStudioCoverageXmlReportParser.class);
  private final Predicate<String> isSupported;

  VisualStudioCoverageXmlReportParser(Predicate<String> isSupported) {
    this.isSupported = isSupported;
  }

  @Override
  public void accept(File file, Coverage coverage) {
    LOG.debug("The current user dir is '{}'.", System.getProperty("user.dir"));
    LOG.info("Parsing the Visual Studio coverage XML report " + file.getAbsolutePath());
    Parser parser = new Parser(file, coverage);
    parser.parse();
  }

  private class Parser {

    private final File file;
    /**
     * The key is the file ID.
     * The value is the line coverage information for that file.
     */
    private final Map<Integer, LineCoverage> lineCoveragePerFile = new HashMap<>();
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
      lineCoveragePerFile.clear();
    }

    /**
     * The Range tags contains the coverage information per line.
     * Important: some lines can be both covered and uncovered,
     * e.g. automatic property with covered getter and uncovered setter.
     */
    private void handleRangeTag(XmlParserHelper xmlParserHelper) {
      int source = xmlParserHelper.getRequiredIntAttribute("source_id");
      String covered = xmlParserHelper.getRequiredAttribute("covered");

      int line = xmlParserHelper.getRequiredIntAttribute("start_line");

      lineCoveragePerFile.putIfAbsent(source, new LineCoverage(source));
      LineCoverage lineCoverage = lineCoveragePerFile.get(source);
      if ("yes".equals(covered) || "partial".equals(covered)) {
        lineCoverage.addCoverageForLine(line, true);
      } else if ("no".equals(covered)) {
        lineCoverage.addCoverageForLine(line, false);
      } else {
        throw xmlParserHelper.parseError("Unsupported \"covered\" value \"" + covered + "\", expected one of \"yes\", \"partial\" or \"no\"");
      }
    }

    /**
     * After parsing the Range tags, the source file tags are iterated and
     * the coverage gets transmitted to the scanner API.
     */
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

      if (!isSupported.test(canonicalPath)) {
        LOG.debug("Skipping file with path '{}' because it is not indexed or does not have the supported language.", canonicalPath);
        return;
      }

      LineCoverage fileCoverage = lineCoveragePerFile.get(id);
      if (fileCoverage != null)
      {
        fileCoverage.transferData(coverage, canonicalPath);
      }
    }

    private void checkRootTag(XmlParserHelper xmlParserHelper) {
      xmlParserHelper.checkRootTag("results");
    }
  }

  private static class LineCoverage {

    int fileId;

    // the key is the line ID
    // the values are a list of booleans, where
    // - 'True' means the line is covered or partially covered
    // - 'False' means the line is not covered
    private Map<Integer, List<Boolean>> coveragePerLine;

    LineCoverage(int fileId) {
      this.fileId = fileId;
      this.coveragePerLine = new HashMap<>();
    }

    void addCoverageForLine(Integer lineId, Boolean isCovered) {
      coveragePerLine.putIfAbsent(lineId, new ArrayList<>());
      coveragePerLine.get(lineId).add(isCovered);
    }

    /**
     * Transfers the coverage data to the Coverage object.
     * @param coverage - the object which centralizes coverage information.
     * @param canonicalFilePath - the path of the file for which this class contains coverage data.
     */
    void transferData(Coverage coverage, String canonicalFilePath) {
      if (!coveragePerLine.isEmpty()) {
        LOG.trace("Found coverage information about '{}' lines for file id '{}' , path '{}'",
          coveragePerLine.size(), fileId, canonicalFilePath);
      }

      for (Map.Entry<Integer, List<Boolean>> lineCoverage : coveragePerLine.entrySet()) {

        Integer lineId = lineCoverage.getKey();
        Iterable<Boolean> coverageValues = lineCoverage.getValue();
        int visits = 0;
        int entryCount = 0;
        // process line coverage
        for (Boolean value : coverageValues) {
          int visit = value ? 1 : 0;
          coverage.addHits(canonicalFilePath, lineId, visit);
          visits += visit;
          entryCount++;
        }
        // where we have both covered and uncovered, use branch coverage
        // e.g. auto-implemented property with getter and setter
        if (entryCount > 1) {
          // this is not really branch coverage, but it's better than nothing
          coverage.addBranchCoverage(canonicalFilePath, new BranchCoverage(lineId, entryCount, visits));
        }
      }
    }
  }

}
