/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2021 SonarSource SA
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
import java.util.HashMap;
import java.util.Map;
import java.util.Optional;
import javax.annotation.Nullable;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

public class OpenCoverReportParser implements CoverageParser {

  private static final Logger LOG = Loggers.get(OpenCoverReportParser.class);
  private final FileService fileService;

  OpenCoverReportParser(FileService fileService) {
    this.fileService = fileService;
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
    // the key is the file ID, the value is the CoveredFile
    private final Map<String, CoveredFile> files = new HashMap<>();
    // the key is the file path
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
        } else if ("BranchPoint".equals(tagName)) {
          handleBranchPointTag(xmlParserHelper);
        }
      }
    }

    private void handleFileRef(XmlParserHelper xmlParserHelper) {
      this.fileRef = xmlParserHelper.getRequiredAttribute("uid");
    }

    private void handleFileTag(XmlParserHelper xmlParserHelper) {
      String uid = xmlParserHelper.getRequiredAttribute("uid");
      String pathInCoverageFile = xmlParserHelper.getRequiredAttribute("fullPath");

      String canonicalPath;
      try {
        canonicalPath = new File(pathInCoverageFile).getCanonicalPath();
      } catch (IOException e) {
        LOG.debug("Skipping the import of OpenCover code coverage for the invalid file path: " + pathInCoverageFile
          + " at line " + xmlParserHelper.stream().getLocation().getLineNumber(), e);
        return;
      }

      CoveredFile coveredFile;
      if (fileService.isSupportedAbsolute(canonicalPath)) {
        coveredFile = new CoveredFile(uid, pathInCoverageFile, canonicalPath);
      } else {
        // maybe it's a deterministic build file path
        Optional<String> optionalPath = fileService.getAbsolutePath(pathInCoverageFile);
        if (optionalPath.isPresent()) {
          coveredFile = new CoveredFile(uid, pathInCoverageFile, optionalPath.get());
          LOG.debug("Found indexed file '{}' for coverage entry '{}'.", coveredFile.supportedPath, coveredFile.originalPath);
        } else {
          coveredFile = new CoveredFile(uid, pathInCoverageFile, null);
        }
      }
      files.put(uid, coveredFile);
    }

    private void handleSequencePointTag(XmlParserHelper xmlParserHelper) {
      // Open Cover lacks model documentation but the details can be found in the source code:
      // https://github.com/OpenCover/opencover/blob/4.7.922/main/OpenCover.Framework/Model/SequencePoint.cs
      int line = xmlParserHelper.getRequiredIntAttribute("sl");
      int visitCount = xmlParserHelper.getRequiredIntAttribute("vc");

      String fileId = xmlParserHelper.getAttribute("fileid");
      if (fileId == null) {
        fileId = fileRef;
      }

      if (files.containsKey(fileId)) {
        CoveredFile coveredFile = files.get(fileId);

        if (coveredFile.isSupported()) {
          coverage.addHits(coveredFile.supportedPath, line, visitCount);

          LOG.trace("OpenCover parser: add hits for file {}, line '{}', visitCount '{}'.",
            coveredFile, line, visitCount);
        } else {
          LOG.debug("Skipping the file {}, line '{}', visitCount '{}' because file is not indexed or does not have the supported language.",
            coveredFile, line, visitCount);
        }
      } else {
        LOG.debug("OpenCover parser (handleSequencePointTag): the fileId '{}' key is not contained in files (entry for line '{}', visitCount '{}').",
          fileId, line, visitCount);
      }
    }

    private void handleBranchPointTag(XmlParserHelper xmlParserHelper) {
      String fileId = xmlParserHelper.getAttribute("fileid");
      if (fileId == null) {
        fileId = fileRef;
      }

      if (files.containsKey(fileId)) {
        CoveredFile coveredFile = files.get(fileId);

        int line = xmlParserHelper.getIntAttributeOrZero("sl");
        if (line == 0){
          LOG.warn("OpenCover parser: invalid start line for file {}.", coveredFile);
          return;
        }

        int offset = xmlParserHelper.getRequiredIntAttribute("offset");
        int offsetEnd = xmlParserHelper.getRequiredIntAttribute("offsetend");
        int path = xmlParserHelper.getRequiredIntAttribute("path");
        int visitCount = xmlParserHelper.getRequiredIntAttribute("vc");

        if (coveredFile.isSupported()) {
          coverage.add(new BranchPoint(coveredFile.supportedPath, line, offset, offsetEnd, path, visitCount));

          LOG.trace("OpenCover parser: add branch hits for file {}, line '{}', offset '{}', visitCount '{}'.",
            coveredFile, line, offset, visitCount);
        } else {
          LOG.debug("OpenCover parser: Skipping branch hits for file {}, line '{}', offset '{}', visitCount '{}' because file" +
              " is not indexed or does not have the supported language.",
            coveredFile, line, offset, visitCount);
        }
      } else {
        LOG.debug("OpenCover parser (handleBranchPointTag): the fileId '{}' key is not contained in files.", fileId);
      }
    }
  }

  /**
   * Represents a file mentioned in the OpenCover report list of files, which may or may not be supported by the FileService.
   * @see FileService
   */
  private class CoveredFile
  {
    // the file ID from the OpenCover report
    private String uid;
    // the path from the coverage file
    private String originalPath;
    // the path which is supported by the FileService
    private String supportedPath;

    private CoveredFile(String uid, String originalPath, @Nullable String supportedPath) {
      this.uid = uid;
      this.originalPath = originalPath;
      this.supportedPath = supportedPath;
    }

    /**
     * @return True if the file is supported by the FileService (which wraps the Scanner API).
     * @see FileService#isSupportedAbsolute
     */
    private boolean isSupported() {
      return supportedPath != null;
    }

    @Override
    public String toString() {
      return supportedPath == null
        ? String.format("(ID '%s', path '%s')", uid, originalPath)
        : String.format("(ID '%s', path '%s', indexed as '%s')", uid, originalPath, supportedPath);
    }
  }
}
