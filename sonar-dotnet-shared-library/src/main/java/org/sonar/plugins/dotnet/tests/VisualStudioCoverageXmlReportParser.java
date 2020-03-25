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
     * The outer map key is the file ID.
     * The inner map key is the line number.
     * The inner map values are a list of Boolean - true when the line is covered, false when it is not.
     */
    private final Map<Integer, Map<Integer, List<Boolean>>> coveragePerLinePerFile = new HashMap<>();
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
      coveragePerLinePerFile.clear();
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

      coveragePerLinePerFile.putIfAbsent(source, new HashMap<>());
      Map<Integer, List<Boolean>> lineCoverage = coveragePerLinePerFile.get(source);
      lineCoverage.putIfAbsent(line, new ArrayList<>());
      if ("yes".equals(covered) || "partial".equals(covered)) {
        lineCoverage.get(line).add(true);
      } else if ("no".equals(covered)) {
        lineCoverage.get(line).add(false);
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

      Map<Integer, List<Boolean>> fileCoverage = coveragePerLinePerFile.get(id);
      if (!fileCoverage.isEmpty()) {
        LOG.trace("Found coverage information about '{}' lines for file id '{}' , path '{}'",
          fileCoverage.size(), id, canonicalPath);
      }

      processLineCoverage(canonicalPath, fileCoverage);
    }

    /**
     * Iterates over the line coverage metrics of the given file and calls the scanner Coverage API.
     * @param canonicalFilePath - the path of the file
     * @param fileCoverage - the key is the line number inside the file, the value is a list of coverage data
     *                     for the line - true if covered, false if not covered
     */
    private void processLineCoverage(String canonicalFilePath, Map<Integer, List<Boolean>> fileCoverage) {
      for (Map.Entry<Integer, List<Boolean>> lineCoverage : fileCoverage.entrySet()) {

        Integer lineId = lineCoverage.getKey();

        List<Boolean> coverageValues = lineCoverage.getValue();
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

    private void checkRootTag(XmlParserHelper xmlParserHelper) {
      xmlParserHelper.checkRootTag("results");
    }
  }

}
