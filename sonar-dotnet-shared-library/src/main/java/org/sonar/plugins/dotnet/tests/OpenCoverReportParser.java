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
    private final Map<String, String> files = new HashMap<>();
    private final Coverage coverage;
    private final Map<String, LineComplexCoverage> complexCoveragePerFile;
    private String fileRef;

    Parser(File file, Coverage coverage) {
      this.file = file;
      this.coverage = coverage;
      this.complexCoveragePerFile = new HashMap<>();
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
          handleSegmentPointTag(xmlParserHelper);
        }
      }
      // treat the cases of multiple element coverage per file
      for (LineComplexCoverage complexCoverage : complexCoveragePerFile.values()) {
        complexCoverage.transferData(coverage);
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

    private void handleSegmentPointTag(XmlParserHelper xmlParserHelper) {
      // Open Cover lacks model documentation but the details can be found in the source code:
      // https://github.com/OpenCover/opencover/blob/4.7.922/main/OpenCover.Framework/Model/SequencePoint.cs
      int line = xmlParserHelper.getRequiredIntAttribute("sl");
      int endLine = xmlParserHelper.getRequiredIntAttribute("el");
      int startColumn = xmlParserHelper.getRequiredIntAttribute("sc");
      int endColumn = xmlParserHelper.getRequiredIntAttribute("ec");
      int vc = xmlParserHelper.getRequiredIntAttribute("vc");
      int branchExitsCount = xmlParserHelper.getIntAttributeOrZero("bec");
      int branchExitsVisit = xmlParserHelper.getIntAttributeOrZero("bev");

      String fileId = xmlParserHelper.getAttribute("fileid");
      if (fileId == null) {
        fileId = fileRef;
      }

      if (files.containsKey(fileId)) {
        String identifiedFile = files.get(fileId);

        if (isSupported.test(identifiedFile)) {
          LOG.trace("OpenCover parser: add hits for fileId '{}', line '{}', vc '{}'", fileId, line, vc);

          // Branch exit count is 0 for all the lines which don't have branches.
          if (branchExitsCount > 0){
            coverage.addBranchCoverage(identifiedFile, new BranchCoverage(line, branchExitsCount, branchExitsVisit));
          }

          coverage.addHits(identifiedFile, line, vc);

          if (line == endLine) {
            String elementIdentifier = String.format("%d-%d-%d-%d", line, endLine, startColumn, endColumn);
            complexCoveragePerFile.putIfAbsent(identifiedFile, new LineComplexCoverage(identifiedFile));
            LineComplexCoverage lineComplexCoverage = complexCoveragePerFile.get(identifiedFile);
            lineComplexCoverage.addCoverageForLineAndMethod(line, elementIdentifier, vc);
          }
        } else {
          LOG.debug("Skipping the fileId '{}', line '{}', vc '{}' because file '{}'" +
              " is not indexed or does not have the supported language.",
            fileId, line, vc, identifiedFile);
        }
      } else {
        LOG.debug("OpenCover parser: the fileId '{}' key is not contained in files", fileId);
      }
    }

  }

  /**
   * This class is useful when on a single line there are multiple methods. Some may be covered, some not
   * (e.g. an auto-implemented property can have a covered getter and un-covered setter)
   *
   * So this class holds information about methods which are declared on a single line.
   */
  private static class LineComplexCoverage {

    String fileId;

    // the key is the line ID
    // the value is a list of method hits
    private Map<Integer, MethodHits> methodHitsPerLine;

    LineComplexCoverage(String fileId) {
      this.fileId = fileId;
      this.methodHitsPerLine = new HashMap<>();
    }

    void addCoverageForLineAndMethod(Integer lineId, String methodName, int hits) {
      methodHitsPerLine.putIfAbsent(lineId, new MethodHits());
      MethodHits methodHits = methodHitsPerLine.get(lineId);
      methodHits.addHit(methodName, hits);
    }

    /**
     * Transfers the coverage data to the Coverage object.
     * @param coverage - the object which centralizes coverage information.
     */
    void transferData(Coverage coverage) {
      LOG.trace("Found coverage information about '{}' lines having single-line method declarations for file '{}'",
        methodHitsPerLine.size(), fileId);

      for (Map.Entry<Integer, MethodHits> lineMethodHits : methodHitsPerLine.entrySet()) {

        Integer lineId = lineMethodHits.getKey();
        MethodHits methodHits = lineMethodHits.getValue();
        // where we have both covered and uncovered, use branch coverage
        // e.g. auto-implemented property with getter and setter
        if (methodHits.size() > 1) {
          int coveredMethods = methodHits.countMethodsWithCoverage();
          // this is not really branch coverage, but it's better than nothing
          coverage.addBranchCoverage(fileId, new BranchCoverage(lineId, methodHits.size(), coveredMethods));
        }
      }
    }

    private static class MethodHits {
      private Map<String, Integer> methodHits;

      private MethodHits() {
        methodHits = new HashMap<>();
      }

      int size() {
        return methodHits.size();
      }

      int countMethodsWithCoverage() {
        int number = 0;
        for (Integer hits : methodHits.values()) {
          if (hits > 0) {
            number++;
          }
        }
        return number;
      }

      void addHit(String methodName, int hits) {
        methodHits.putIfAbsent(methodName, 0);
        Integer oldVal = methodHits.get(methodName);
        methodHits.put(methodName, oldVal + hits);
      }
    }
  }
}
