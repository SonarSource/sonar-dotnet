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
    // the key is the file ID, the value is the file path (which can be missing if the file is not found on disk)
    private final Map<String, Optional<String>> files = new HashMap<>();
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

      if (fileService.isSupportedAbsolute(canonicalPath)) {
        files.put(uid, Optional.of(canonicalPath));
      } else {
        // maybe it's a deterministic build file path
        Optional<String> optionalPath = fileService.getAbsolutePath(pathInCoverageFile);
        files.put(uid, optionalPath);
        optionalPath.ifPresent(path -> LOG.debug("Found indexed file '{}' for coverage entry '{}'.", path, pathInCoverageFile));
      }
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
        Optional<String> optionalPath = files.get(fileId);

        if (optionalPath.isPresent()) {
          String absoluteFilePath = optionalPath.get();
          coverage.addHits(absoluteFilePath, line, visitCount);

          LOG.trace("OpenCover parser: add hits for fileId '{}', filePath '{}', line '{}', visitCount '{}'",
            fileId, absoluteFilePath, line, visitCount);
        } else {
          LOG.debug("Skipping the fileId '{}', line '{}', visitCount '{}' because file is not indexed or does not have the supported language.",
            fileId, line, visitCount);
        }
      } else {
        LOG.debug("OpenCover parser: the fileId '{}' key is not contained in files", fileId);
      }
    }

    private void handleBranchPointTag(XmlParserHelper xmlParserHelper) {
      String fileId = xmlParserHelper.getAttribute("fileid");
      if (fileId == null) {
        fileId = fileRef;
      }

      if (files.containsKey(fileId)) {
        int line = xmlParserHelper.getIntAttributeOrZero("sl");
        if (line == 0){
          LOG.warn("OpenCover parser: invalid start line for {}", fileId);
          return;
        }

        int offset = xmlParserHelper.getRequiredIntAttribute("offset");
        int offsetEnd = xmlParserHelper.getRequiredIntAttribute("offsetend");
        int path = xmlParserHelper.getRequiredIntAttribute("path");
        int visitCount = xmlParserHelper.getRequiredIntAttribute("vc");

        Optional<String> optionalPath = files.get(fileId);
        if (optionalPath.isPresent()) {
          coverage.add(new BranchPoint(optionalPath.get(), line, offset, offsetEnd, path, visitCount));

          LOG.trace("OpenCover parser: add branch hits for fileId '{}', filePath '{}', line '{}', offset '{}', visitCount '{}'",
            fileId, optionalPath.get(), line, offset, visitCount);
        } else {
          LOG.debug("OpenCover parser: Skipping branch hits for fileId '{}', line '{}', offset '{}', visitCount '{}' because file" +
              " is not indexed or does not have the supported language.",
            fileId, line, offset, visitCount);
        }
      } else {
        LOG.debug("OpenCover parser: the fileId '{}' key is not contained in files", fileId);
      }
    }
  }
}
