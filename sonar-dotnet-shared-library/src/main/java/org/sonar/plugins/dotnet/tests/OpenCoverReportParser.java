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

import java.util.function.Predicate;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

import java.io.File;
import java.io.IOException;
import java.util.HashMap;
import java.util.Map;

public class OpenCoverReportParser implements CoverageParser {

  private static final Logger LOG = Loggers.get(OpenCoverReportParser.class);
  private final Predicate<String> isSupported;

  OpenCoverReportParser(Predicate<String> isSupported) {
    this.isSupported = isSupported;
  }

  @Override
  public void accept(File file, Coverage coverage) {
    LOG.debug("The current user dir is '{}'.", System.getProperty("user.dir"));
    LOG.info("Parsing the OpenCover report " + file.getAbsolutePath());
    new Parser(file, coverage).parse();
  }

  private class Parser {

    private final File file;
    private final Coverage coverage;
    // the key is the file ID, the value is the file path
    private final Map<String, String> files = new HashMap<>();
    // the key is the file path
    private final Map<String, DetailedLineCoverage> detailedLineCoveragePerFile = new HashMap<>();
    private String fileRef;

    Parser(File file, Coverage coverage) {
      this.file = file;
      this.coverage = coverage;
    }

    public void parse() {
      try (XmlParserHelper xmlParserHelper = new XmlParserHelper(file)) {
        xmlParserHelper.checkRootTag("CoverageSession");
        dispatchTags(xmlParserHelper);
      } catch (IOException e) {
        throw new IllegalStateException("Unable to close report", e);
      }
    }

    private void dispatchTags(XmlParserHelper xmlParserHelper) {
      String tagName;
      while ((tagName = xmlParserHelper.nextStartTag()) != null) {
        if ("File".equals(tagName)) {
          handleFileTag(xmlParserHelper);
        } else if ("FileRef".equals(tagName)) {
          handleFileRef(xmlParserHelper);
        } else if ("SequencePoint".equals(tagName)) {
          handleSequencePointTag(xmlParserHelper);
        }
      }
      // some lines can contain multiple sequence points, which need separate processing
      for (DetailedLineCoverage detailedLineCoverage : detailedLineCoveragePerFile.values()) {
        detailedLineCoverage.transferData(coverage);
      }
    }

    private void handleFileRef(XmlParserHelper xmlParserHelper) {
      this.fileRef = xmlParserHelper.getRequiredAttribute("uid");
    }

    private void handleFileTag(XmlParserHelper xmlParserHelper) {
      String uid = xmlParserHelper.getRequiredAttribute("uid");
      String fullPath = xmlParserHelper.getRequiredAttribute("fullPath");

      try {
        files.put(uid, new File(fullPath).getCanonicalPath());
      } catch (IOException e) {
        LOG.debug("Skipping the import of OpenCover code coverage for the invalid file path: " + fullPath
          + " at line " + xmlParserHelper.stream().getLocation().getLineNumber(), e);
      }
    }

    private void handleSequencePointTag(XmlParserHelper xmlParserHelper) {
      // Open Cover lacks model documentation but the details can be found in the source code:
      // https://github.com/OpenCover/opencover/blob/4.7.922/main/OpenCover.Framework/Model/SequencePoint.cs
      int line = xmlParserHelper.getRequiredIntAttribute("sl");
      int endLine = xmlParserHelper.getRequiredIntAttribute("el");
      int startColumn = xmlParserHelper.getRequiredIntAttribute("sc");
      int endColumn = xmlParserHelper.getRequiredIntAttribute("ec");
      int visitCount = xmlParserHelper.getRequiredIntAttribute("vc");
      int branchExitsCount = xmlParserHelper.getIntAttributeOrZero("bec");
      int branchExitsVisit = xmlParserHelper.getIntAttributeOrZero("bev");

      String fileId = xmlParserHelper.getAttribute("fileid");
      if (fileId == null) {
        fileId = fileRef;
      }

      if (files.containsKey(fileId)) {
        String filePath = files.get(fileId);

        if (isSupported.test(filePath)) {
          LOG.trace("OpenCover parser: add hits for fileId '{}', filePath '{}', line '{}', visitCount '{}'",
            fileId, filePath, line, visitCount);

          // Branch exit count is 0 for all the lines which don't have branches.
          if (branchExitsCount > 0){
            coverage.addBranchCoverage(filePath, new BranchCoverage(line, branchExitsCount, branchExitsVisit));
          }

          coverage.addHits(filePath, line, visitCount);

          // we only want to track coverage of sequence points that are present on a single line
          if (line == endLine) {
            String sequencePointIdentifier = String.format("%d-%d-%d-%d", line, endLine, startColumn, endColumn);
            detailedLineCoveragePerFile.putIfAbsent(filePath, new DetailedLineCoverage(filePath));
            DetailedLineCoverage detailedLineCoverage = detailedLineCoveragePerFile.get(filePath);
            detailedLineCoverage.addCoverageForLineAndSequencePoint(line, sequencePointIdentifier, visitCount);
          }
        } else {
          LOG.debug("Skipping the fileId '{}', line '{}', vc '{}' because file '{}'" +
              " is not indexed or does not have the supported language.",
            fileId, line, visitCount, filePath);
        }
      } else {
        LOG.debug("OpenCover parser: the filePath '{}' key is not contained in files", fileId);
      }
    }

  }

  /**
   * Detailed line coverage for a file: the number of visits for each sequence point on the line.
   */
  private static class DetailedLineCoverage {

    String filePath;

    // The key is the line number.
    // The value is the aggregated coverage for each sequence point on the line.
    private Map<Integer, SequencePointsCoverage> sequencePointCoveragePerLine;

    DetailedLineCoverage(String filePath) {
      this.filePath = filePath;
      this.sequencePointCoveragePerLine = new HashMap<>();
    }

    /**
     * @param lineId - line number
     * @param sequencePointIdentifier - identifier for the sequence point - must be unique inside the file
     * @param hits - number of hits (visit count)
     */
    void addCoverageForLineAndSequencePoint(Integer lineId, String sequencePointIdentifier, int hits) {
      sequencePointCoveragePerLine.putIfAbsent(lineId, new SequencePointsCoverage());
      SequencePointsCoverage sequencePointsCoverage = sequencePointCoveragePerLine.get(lineId);
      sequencePointsCoverage.addHit(sequencePointIdentifier, hits);
    }

    /**
     * Transfers the coverage data to the Coverage object.
     * @param coverage - the object which centralizes coverage information.
     */
    void transferData(Coverage coverage) {
      LOG.trace("Found coverage information about '{}' lines having multiple sequence points for file '{}'",
        sequencePointCoveragePerLine.size(), filePath);

      for (Map.Entry<Integer, SequencePointsCoverage> lineSequencePointCoverage : sequencePointCoveragePerLine.entrySet()) {

        Integer lineId = lineSequencePointCoverage.getKey();
        SequencePointsCoverage sequencePointsCoverage = lineSequencePointCoverage.getValue();

        // where there are multiple sequence points on a line, use branch coverage
        if (sequencePointsCoverage.size() > 1) {
          int coveredSequencePointsOnLine = (int) sequencePointsCoverage.countCoveredSequencePoints();
          int totalSequencePointsOnLine = sequencePointsCoverage.size();
          // this is not really branch coverage, but using this API we can highlight lines with partial coverage
          coverage.addBranchCoverage(filePath, new BranchCoverage(lineId, totalSequencePointsOnLine, coveredSequencePointsOnLine));
        }
      }
    }

    // Holds sequence point coverage information for a line.
    private static class SequencePointsCoverage {
      // the key is the sequence point identifier
      // the value is the number of hits (visit counts)
      private Map<String, Integer> hitsPerSequencePoint;

      private SequencePointsCoverage() {
        hitsPerSequencePoint = new HashMap<>();
      }

      int size() {
        return hitsPerSequencePoint.size();
      }

      long countCoveredSequencePoints() {
        return hitsPerSequencePoint.values().stream().filter(hits -> hits > 0).count();
      }

      void addHit(String methodName, int hits) {
        hitsPerSequencePoint.putIfAbsent(methodName, 0);
        Integer oldVal = hitsPerSequencePoint.get(methodName);
        hitsPerSequencePoint.put(methodName, oldVal + hits);
      }
    }
  }
}
