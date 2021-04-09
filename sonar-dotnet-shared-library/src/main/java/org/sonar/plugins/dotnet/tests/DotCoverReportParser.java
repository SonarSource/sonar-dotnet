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
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import javax.annotation.Nullable;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

public class DotCoverReportParser implements CoverageParser {

  private static final String TITLE_START = "<title>";
  private static final String TITLE_END = "</title>";
  private static final String NO_TAG_MESSAGE = "The report does not contain a '%s' tag.";
  // the pattern for the information about a sequence point - (lineStart, columnStart, lineEnd, columnEnd, hits)
  private static final Pattern SEQUENCE_POINT = Pattern.compile("\\[(\\d++),\\d++,\\d++,\\d++,(\\d++)]");
  private static final String SEQUENCE_POINTS_GROUP_NAME = "SequencePoints";
  // the file coverage has a list of sequence points
  // we use SEQUENCE_POINT_PATTERN below to avoid non-determinism in the regular expression, due to the `[` and `]` characters appearing multiple times
  private static final Pattern FILE_COVERAGE = Pattern.compile(
    ".*<script type=\"text/javascript\">\\s*+highlightRanges\\(\\[(?<" + SEQUENCE_POINTS_GROUP_NAME + ">" + SEQUENCE_POINT + "(," + SEQUENCE_POINT + ")*)]\\);\\s*+</script>.*",
    Pattern.DOTALL);

  private static final Logger LOG = Loggers.get(DotCoverReportParser.class);
  private final FileService fileService;

  public DotCoverReportParser(FileService fileService) {
    this.fileService = fileService;
  }

  @Override
  public void accept(File file, Coverage coverage) {
    LOG.info("Parsing the dotCover report " + file.getAbsolutePath());
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
        LOG.debug("Skipping the import of dotCover code coverage for file '{}'"
          + " because it is not indexed or does not have the supported language.", fileCanonicalPath);
      }
    }

    @Nullable
    private String extractFileCanonicalPath(String contents) {
      String lowerCaseAbsolutePath = getTitlePath(contents);
      try {
        return new File(lowerCaseAbsolutePath).getCanonicalPath();
      } catch (IOException e) {
        LOG.debug("Skipping the import of dotCover code coverage for the invalid file path: " + lowerCaseAbsolutePath + ".", e);
        return null;
      }
    }

    private void collectCoverage(String fileCanonicalPath, String contents) {
      Matcher fileCoverageMatcher = FILE_COVERAGE.matcher(contents);
      if (!fileCoverageMatcher.matches()) {
        throw new IllegalArgumentException("The report contents does not match the following regular expression: " + FILE_COVERAGE.pattern());
      }

      String highlightedContents = fileCoverageMatcher.group(SEQUENCE_POINTS_GROUP_NAME);
      Matcher sequencePointsMatcher = SEQUENCE_POINT.matcher(highlightedContents);

      while (sequencePointsMatcher.find()) {
        int lineStart = Integer.parseInt(sequencePointsMatcher.group(1));
        int hits = Integer.parseInt(sequencePointsMatcher.group(2));

        coverage.addHits(fileCanonicalPath, lineStart, hits);

        LOG.trace("dotCover parser: found coverage for line '{}', hits '{}' when analyzing the path '{}'.",
          lineStart, hits, fileCanonicalPath);
      }
    }

    // fileContent could contain <title>foo</title> or </title>foo<title>
    private String getTitlePath(String fileContent) {
      int indexOfTitleStart = fileContent.indexOf(TITLE_START);
      if (indexOfTitleStart == -1) {
        throw new IllegalArgumentException(String.format(NO_TAG_MESSAGE, TITLE_START));
      }
      int indexOfTitleEnd = fileContent.indexOf(TITLE_END, indexOfTitleStart);
      if (indexOfTitleEnd == -1) {
        throw new IllegalArgumentException(String.format(NO_TAG_MESSAGE, TITLE_END));
      }
      return fileContent.substring(indexOfTitleStart + TITLE_START.length(), indexOfTitleEnd);
    }
  }
}
