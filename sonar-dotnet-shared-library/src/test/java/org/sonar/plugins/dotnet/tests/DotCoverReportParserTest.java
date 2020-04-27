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
import org.assertj.core.api.Assertions;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;

import java.io.File;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;

public class DotCoverReportParserTest {

  @Rule
  public LogTester logTester = new LogTester();

  @Rule
  public ExpectedException thrown = ExpectedException.none();
  private Predicate<String> alwaysTrue = s -> true;

  @Test
  public void no_title() {
    thrown.expect(IllegalArgumentException.class);
    thrown.expectMessage("The report contents does not match the following regular expression: .*?<title>(.*?)</title>.*");
    new DotCoverReportParser(alwaysTrue).accept(new File("src/test/resources/dotcover/no_title.html"), mock(Coverage.class));
  }

  @Test
  public void no_highlight() {
    thrown.expect(IllegalArgumentException.class);
    thrown.expectMessage("The report contents does not match the following regular expression: "
      + ".*<script type=\"text/javascript\">\\s*+highlightRanges\\(\\[(.*?)\\]\\);\\s*+</script>.*");
    new DotCoverReportParser(alwaysTrue).accept(new File("src/test/resources/dotcover/no_highlight.html"), mock(Coverage.class));
  }

  @Test
  public void valid() throws Exception {
    Coverage coverage = new Coverage();
    new DotCoverReportParser(alwaysTrue).accept(new File("src/test/resources/dotcover/valid.html"), coverage);

    String filePath = new File("mylibrary\\calc.cs").getCanonicalPath();
    assertThat(coverage.files()).containsOnly(filePath);

    assertThat(coverage.hits(filePath))
      .hasSize(16)
      .contains(
        Assertions.entry(12, 0),
        Assertions.entry(13, 0),
        Assertions.entry(14, 0),
        Assertions.entry(17, 1),
        Assertions.entry(18, 1),
        Assertions.entry(19, 1),
        Assertions.entry(22, 0),
        Assertions.entry(23, 0),
        Assertions.entry(24, 0),
        Assertions.entry(25, 0),
        Assertions.entry(26, 0),
        Assertions.entry(28, 0),
        Assertions.entry(29, 0),
        Assertions.entry(32, 0),
        Assertions.entry(33, 0),
        Assertions.entry(34, 0));

    assertThat(logTester.logs(LoggerLevel.INFO).get(0)).startsWith("Parsing the dotCover report ");
    assertThat(logTester.logs(LoggerLevel.TRACE).get(0))
      .startsWith("dotCover parser: found coverage for line '12', hits '0' when analyzing the path '");
  }

  @Test
  public void valid_with_multiple_sequence_points_per_line() throws Exception {
    Coverage coverage = new Coverage();
    new DotCoverReportParser(alwaysTrue).accept(new File("src/test/resources/dotcover/valid_multiple_sequence_points_per_line.html"), coverage);

    String filePath = new File("GetSet\\Bar.cs").getCanonicalPath();
    assertThat(coverage.files()).containsOnly(filePath);
    assertThat(coverage.hits(filePath))
      .hasSize(14)
      .containsOnly(
        // 2 hits - only one getter and one setter are covered
        Assertions.entry(7, 2),
        Assertions.entry(9, 1),
        // 2 hits - both get and set are covered
        Assertions.entry(11, 2),
        Assertions.entry(13, 1),
        Assertions.entry(16, 1),
        Assertions.entry(17, 3),
        Assertions.entry(21, 1),
        Assertions.entry(22, 1),
        Assertions.entry(24, 1),
        Assertions.entry(25, 1),
        Assertions.entry(31, 1),
        Assertions.entry(32, 1),
        Assertions.entry(33, 1),
        Assertions.entry(34, 1)
      );
  }

  @Test
  public void predicate_false() {
    Coverage coverage = new Coverage();
    Predicate<String> alwaysFalse = s -> false;
    new DotCoverReportParser(alwaysFalse).accept(new File("src/test/resources/dotcover/valid.html"), coverage);

    assertThat(coverage.files()).isEmpty();

    assertThat(logTester.logs(LoggerLevel.INFO).get(0)).startsWith("Parsing the dotCover report ");
    assertThat(logTester.logs(LoggerLevel.DEBUG).get(0))
      .startsWith("Skipping the import of dotCover code coverage for file '")
      .endsWith("' because it is not indexed or does not have the supported language.");
  }

  @Test
  public void should_not_fail_with_invalid_path() {
    new DotCoverReportParser(alwaysTrue).accept(new File("src/test/resources/dotcover/invalid_path.html"), mock(Coverage.class));
    assertThat(logTester.logs(LoggerLevel.INFO).get(0)).startsWith("Parsing the dotCover report ");
    assertThat(logTester.logs(LoggerLevel.DEBUG).get(0))
      .isEqualTo("Skipping the import of dotCover code coverage for the invalid file path: z:\\*\"?.cs.");
  }

}
