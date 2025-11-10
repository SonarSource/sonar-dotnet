/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonar.plugins.dotnet.tests.coverage;

import java.io.File;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import javax.annotation.Nullable;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.plugins.dotnet.tests.FileService;

public class DotCoverReportParser implements CoverageParser {

  private static final String TITLE_START = "<title>";
  private static final String HIGHLIGHT_RANGES_START = "highlightRanges([";
  // the pattern for the information about a sequence point - [lineStart, columnStart, lineEnd, columnEnd, hits]
  private static final Pattern SEQUENCE_POINT = Pattern.compile("\\[(\\d++),\\d++,\\d++,\\d++,(\\d++)]");
  private static final Logger LOG = LoggerFactory.getLogger(DotCoverReportParser.class);
  private final FileService fileService;

  public DotCoverReportParser(FileService fileService) {
    this.fileService = fileService;
  }

  @Override
  public void accept(File file, Coverage coverage) {
    LOG.info("Parsing the dotCover report {}", file.getAbsolutePath());
    new Parser(file, coverage).parse();
  }

  private class Parser {

    private final File file;
    private final Coverage coverage;

    Parser(File file, Coverage coverage) {
      this.file = file;
      this.coverage = coverage;
    }

    public void parse() {
      String contents;
      try {
        contents = new String(Files.readAllBytes(file.toPath()), StandardCharsets.UTF_8);
      } catch (IOException e) {
        throw new IllegalStateException(e);
      }

      String fileCanonicalPath = extractFileCanonicalPath(contents);
      if (fileCanonicalPath != null && fileService.isSupportedAbsolute(fileCanonicalPath)) {
        collectCoverage(fileCanonicalPath, contents);
      } else {
        LOG.debug("Skipping the import of dotCover code coverage for file '{}' because it is not indexed or"
            + " does not have the supported language. " + VERIFY_SONARPROJECTPROPERTIES_MESSAGE,
          fileCanonicalPath);
      }
    }

    @Nullable
    private String extractFileCanonicalPath(String contents) {
      int indexOfTitleStart = getIndexOf(contents, TITLE_START, 0);
      int indexOfTitleEnd = getIndexOf(contents, "</title>", indexOfTitleStart);
      String lowerCaseAbsolutePath = contents.substring(indexOfTitleStart + TITLE_START.length(), indexOfTitleEnd);
      try {
        return new File(lowerCaseAbsolutePath).getCanonicalPath();
      } catch (IOException e) {
        LOG.debug("Skipping the import of dotCover code coverage for the invalid file path: " + lowerCaseAbsolutePath + ".", e);
        return null;
      }
    }

    private void collectCoverage(String fileCanonicalPath, String contents) {
      int indexOfScript = getIndexOf(contents, "<script type=\"text/javascript\">", 0);
      int indexOfRanges = getIndexOf(contents, HIGHLIGHT_RANGES_START, indexOfScript);
      Matcher sequencePointsMatcher = SEQUENCE_POINT.matcher(contents.substring(indexOfRanges + HIGHLIGHT_RANGES_START.length()));

      while (sequencePointsMatcher.find()) {
        int lineStart = Integer.parseInt(sequencePointsMatcher.group(1));
        int hits = Integer.parseInt(sequencePointsMatcher.group(2));
        coverage.addHits(fileCanonicalPath, lineStart, hits);

        LOG.trace("dotCover parser: found coverage for line '{}', hits '{}' when analyzing the path '{}'.", lineStart, hits, fileCanonicalPath);
      }
    }

    private int getIndexOf(String fileContent, String part, int startIndex) {
      int index = fileContent.indexOf(part, startIndex);
      if (index == -1) {
        throw new IllegalArgumentException("The report does not contain expected '" + part + "'.");
      }
      return index;
    }
  }
}
