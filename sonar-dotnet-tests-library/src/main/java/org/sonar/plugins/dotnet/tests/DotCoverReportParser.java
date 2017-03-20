/*
 * SonarQube .NET Tests Library
 * Copyright (C) 2014-2017 SonarSource SA
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

import com.google.common.base.Preconditions;
import com.google.common.base.Throwables;
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

  private static final Logger LOG = Loggers.get(DotCoverReportParser.class);

  @Override
  public void accept(File file, Coverage coverage) {
    LOG.info("Parsing the dotCover report " + file.getAbsolutePath());
    new Parser(file, coverage).parse();
  }

  private static class Parser {

    private static final Pattern TITLE_PATTERN = Pattern.compile(".*?<title>(.*?)</title>.*", Pattern.DOTALL);
    private static final Pattern COVERED_LINES_PATTERN_1 = Pattern.compile(
      ".*<script type=\"text/javascript\">\\s*+highlightRanges\\(\\[(.*?)\\]\\);\\s*+</script>.*",
      Pattern.DOTALL);
    private static final Pattern COVERED_LINES_PATTERN_2 = Pattern.compile("\\[(\\d++),\\d++,\\d++,\\d++,(\\d++)\\]");

    private final File file;
    private final Coverage coverage;

    public Parser(File file, Coverage coverage) {
      this.file = file;
      this.coverage = coverage;
    }

    public void parse() {
      String contents;
      try {
        contents = new String(Files.readAllBytes(file.toPath()), StandardCharsets.UTF_8);
      } catch (IOException e) {
        throw Throwables.propagate(e);
      }

      String fileCanonicalPath = extractFileCanonicalPath(contents);
      if (fileCanonicalPath != null) {
        collectCoverage(fileCanonicalPath, contents);
      }
    }

    @Nullable
    private static String extractFileCanonicalPath(String contents) {
      Matcher matcher = TITLE_PATTERN.matcher(contents);
      checkMatches(matcher);

      String lowerCaseAbsolutePath = matcher.group(1);

      try {
        return new File(lowerCaseAbsolutePath).getCanonicalPath();
      } catch (IOException e) {
        LOG.debug("Skipping the import of dotCover code coverage for the invalid file path: " + lowerCaseAbsolutePath, e);
        return null;
      }
    }

    private void collectCoverage(String fileCanonicalPath, String contents) {
      Matcher matcher = COVERED_LINES_PATTERN_1.matcher(contents);
      checkMatches(matcher);
      String highlightedContents = matcher.group(1);

      matcher = COVERED_LINES_PATTERN_2.matcher(highlightedContents);

      while (matcher.find()) {
        int line = Integer.parseInt(matcher.group(1));
        int hits = Integer.parseInt(matcher.group(2));
        coverage.addHits(fileCanonicalPath, line, hits);
      }
    }

    private static void checkMatches(Matcher matcher) {
      Preconditions.checkArgument(matcher.matches(), "The report contents does not match the following regular expression: " + matcher.pattern().pattern());
    }

  }

}
