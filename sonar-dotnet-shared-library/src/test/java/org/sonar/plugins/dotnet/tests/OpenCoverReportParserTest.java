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

import java.util.List;
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

public class OpenCoverReportParserTest {

  @Rule
  public LogTester logTester = new LogTester();

  @Rule
  public ExpectedException thrown = ExpectedException.none();
  private Predicate<String> alwaysTrue = s -> true;

  @Test
  public void invalid_root() {
    thrown.expectMessage("<CoverageSession>");
    new OpenCoverReportParser(alwaysTrue).accept(new File("src/test/resources/opencover/invalid_root.xml"), mock(Coverage.class));
  }

  @Test
  public void missing_start_line() {
    thrown.expectMessage("Missing attribute \"sl\" in element <SequencePoint>");
    thrown.expectMessage("missing_start_line.xml at line 27");
    new OpenCoverReportParser(alwaysTrue).accept(new File("src/test/resources/opencover/missing_start_line.xml"), mock(Coverage.class));
  }

  @Test
  public void wrong_start_line() {
    thrown.expectMessage("Expected an integer instead of \"foo\" for the attribute \"sl\"");
    thrown.expectMessage("wrong_start_line.xml at line 27");
    new OpenCoverReportParser(alwaysTrue).accept(new File("src/test/resources/opencover/wrong_start_line.xml"), mock(Coverage.class));
  }

  @Test
  public void non_existing_file() {
    thrown.expectMessage("non_existing_file.xml");
    new OpenCoverReportParser(alwaysTrue).accept(new File("src/test/resources/opencover/non_existing_file.xml"), mock(Coverage.class));
  }

  @Test
  public void valid() throws Exception {
    Coverage coverage = new Coverage();
    new OpenCoverReportParser(alwaysTrue).accept(new File("src/test/resources/opencover/valid.xml"), coverage);

    assertThat(coverage.files()).containsOnly(
      new File("MyLibraryNUnitTest\\AdderNUnitTest.cs").getCanonicalPath(),
      new File("MyLibrary\\Adder.cs").getCanonicalPath(),
      new File("MyLibrary\\Multiplier.cs").getCanonicalPath());

    assertThat(coverage.hits(new File("MyLibrary\\Adder.cs").getCanonicalPath()))
      .hasSize(15)
      .contains(
        Assertions.entry(11, 2),
        Assertions.entry(12, 2),
        Assertions.entry(13, 0),
        Assertions.entry(14, 0),
        Assertions.entry(15, 0),
        Assertions.entry(18, 2),
        Assertions.entry(22, 6),
        Assertions.entry(26, 2),
        Assertions.entry(27, 2),
        Assertions.entry(30, 4),
        Assertions.entry(31, 4),
        Assertions.entry(32, 4),
        Assertions.entry(35, 2),
        Assertions.entry(36, 2),
        Assertions.entry(37, 2));

    assertThat(coverage.hits(new File("MyLibrary\\Multiplier.cs").getCanonicalPath()))
      .hasSize(3)
      .contains(
        Assertions.entry(11, 0),
        Assertions.entry(12, 0),
        Assertions.entry(13, 0));

    List<String> debugLogs = logTester.logs(LoggerLevel.DEBUG);
    assertThat(debugLogs.get(0)).startsWith("The current user dir is '");
  }

  @Test
  public void branchCoverage() throws Exception {
    Coverage coverage = new Coverage();
    String filePath = new File("D:\\git\\BranchCoveragePoc\\BranchCoveragePoc\\Calculator.cs").getCanonicalPath();

    new OpenCoverReportParser(alwaysTrue).accept(new File("src/test/resources/opencover/coverage_branches.xml"), coverage);

    assertThat(coverage.files()).containsOnly(filePath);
    assertThat(coverage.hits(filePath))
      .hasSize(9)
      .contains(
        Assertions.entry(8, 1),
        Assertions.entry(9, 1),
        Assertions.entry(10, 1),
        Assertions.entry(12, 1),
        Assertions.entry(13, 1),
        Assertions.entry(14, 2),
        Assertions.entry(16, 1),
        Assertions.entry(18, 1),
        Assertions.entry(25, 1));

    assertThat(coverage.getBranchCoverage(filePath).size()).isEqualTo(5);

    assertThat(coverage.getBranchCoverage(filePath).get(0)).isEqualTo(new BranchCoverage(9, 4 , 2));
    assertThat(coverage.getBranchCoverage(filePath).get(1)).isEqualTo(new BranchCoverage(12, 2 , 1));
    assertThat(coverage.getBranchCoverage(filePath).get(2)).isEqualTo(new BranchCoverage(13, 2, 1));
    assertThat(coverage.getBranchCoverage(filePath).get(3)).isEqualTo(new BranchCoverage(14, 6, 4));
    assertThat(coverage.getBranchCoverage(filePath).get(4)).isEqualTo(new BranchCoverage(18, 6, 3));
  }

    @Test
  public void branchCoverage_getter_setter_multiple_sequence_points_per_line() throws Exception {
    Coverage coverage = new Coverage();
    String filePath = new File("GetSet\\Bar.cs").getCanonicalPath();

    new OpenCoverReportParser(alwaysTrue).accept(new File("src/test/resources/opencover/valid_complex_case.xml"), coverage);

    assertThat(coverage.files()).containsOnly(
      filePath,
      new File("GetSet\\FooCallsBar.cs").getCanonicalPath(),
      new File("GetSetTests\\BarTests.cs").getCanonicalPath()
    );
    assertThat(coverage.hits(filePath))
      .hasSize(10)
      .contains(
        // 2 hits from tests for Bar, 1 hit from tests for FooCallsBar
        Assertions.entry(11, 3),
        Assertions.entry(13, 1),
        // 2 hits from tests for Bar, 1 hit from tests for FooCallsBar
        Assertions.entry(15, 3),
        Assertions.entry(17, 1),
        Assertions.entry(20, 1),
        Assertions.entry(21, 3),
        Assertions.entry(25, 1),
        Assertions.entry(26, 1),
        Assertions.entry(28, 1),
        Assertions.entry(29, 1));

      // the unreachable code is taken into consideration by the coverage tool
      assertThat(coverage.getBranchCoverage(filePath))
        .hasSize(5)
        .containsOnly(
          // line 11: CoveredGet , UncoveredProperty and CoveredSet on the same line
          new BranchCoverage(11, 6, 2),

          // line 13: CoveredGetOnSecondLine
          new BranchCoverage(13, 2, 1),

          // line 15: CoveredProperty
          new BranchCoverage(15, 2, 2),

          // line 17: ArrowMethod
          new BranchCoverage(17, 2, 1),

          // line 21: first line inside BodyMethod - 3 statements (what is after 'goto' is ignored)
          new BranchCoverage(21, 3, 3)
        );

      List<String> traceLogs = logTester.logs(LoggerLevel.TRACE);
      assertThat(traceLogs).hasSize(35);

      String logLine = traceLogs.get(32);
      assertThat(logLine)
        .startsWith("Found coverage information about '10' lines having multiple sequence points for file '")
        .endsWith("Bar.cs'");
  }

  @Test
  public void log_unsupported_file_extension() {
    Coverage coverage = new Coverage();
    Predicate<String> alwaysFalse = s -> false;
    // to easily check the logs (it has only one coverage entry)
    new OpenCoverReportParser(alwaysFalse).accept(new File("src/test/resources/opencover/one_class.xml"), coverage);

    assertThat(coverage.files()).isEmpty();

    List<String> debugLogs = logTester.logs(LoggerLevel.DEBUG);
    assertThat(debugLogs.get(0)).startsWith("The current user dir is '");
    assertThat(debugLogs.get(1))
      .startsWith("Skipping the fileId '1', line '16', vc '1' because file '")
      .endsWith("\\MyLibraryNUnitTest\\AdderNUnitTest.cs' is not indexed or does not have the supported language.");
  }

  @Test
  public void should_not_fail_with_invalid_path() {
    new OpenCoverReportParser(alwaysTrue).accept(new File("src/test/resources/opencover/invalid_path.xml"), mock(Coverage.class));
    List<String> debugLogs = logTester.logs(LoggerLevel.DEBUG);
    assertThat(debugLogs.get(0)).startsWith("The current user dir is '");
    assertThat(debugLogs.get(1)).startsWith("Skipping the import of OpenCover code coverage for the invalid file path: z:\\*\"?.cs at line 150");
  }

}
