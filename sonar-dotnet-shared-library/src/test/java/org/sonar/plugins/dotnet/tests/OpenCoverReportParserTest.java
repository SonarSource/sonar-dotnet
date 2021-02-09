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
import java.util.List;
import java.util.Optional;
import org.assertj.core.api.Assertions;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class OpenCoverReportParserTest {

  @Rule
  public LogTester logTester = new LogTester();

  @Rule
  public ExpectedException thrown = ExpectedException.none();
  private FileService alwaysTrue;
  private FileService alwaysFalseAndEmpty;

  @Before
  public void prepare() {
    alwaysTrue = mock(FileService.class);
    when(alwaysTrue.isSupportedAbsolute(anyString())).thenReturn(true);
    when(alwaysTrue.getAbsolutePath(anyString())).thenThrow(new UnsupportedOperationException("Should not call this"));
    alwaysFalseAndEmpty = mock(FileService.class);
    when(alwaysFalseAndEmpty.isSupportedAbsolute(anyString())).thenReturn(false);
    when(alwaysFalseAndEmpty.getAbsolutePath(anyString())).thenReturn(Optional.empty());
  }

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

    // FIXME the test case may be wrong https://github.com/SonarSource/sonar-dotnet/issues/4038
    assertThat(logTester.logs(LoggerLevel.WARN)).hasSize(6);
    assertThat(logTester.logs(LoggerLevel.WARN).get(0))
      .startsWith("OpenCover parser: invalid start line for file (ID '3', path 'MyLibrary\\Adder.cs', indexed as");
  }

  @Test
  public void valid_with_no_absolute_path_no_deterministic_build_path() throws Exception {
    Coverage coverage = new Coverage();

    new OpenCoverReportParser(alwaysFalseAndEmpty).accept(new File("src/test/resources/opencover/valid.xml"), coverage);

    assertThat(coverage.files()).isEmpty();
    assertThat(coverage.hits(new File("MyLibrary\\Adder.cs").getCanonicalPath())).isEmpty();

    assertThat(logTester.logs(LoggerLevel.INFO).get(0)).startsWith("Parsing the OpenCover report ");
    assertThat(logTester.logs(LoggerLevel.DEBUG).get(0)).startsWith("The current user dir is ");

    // FIXME the test case may be wrong https://github.com/SonarSource/sonar-dotnet/issues/4038
    assertThat(logTester.logs(LoggerLevel.WARN))
      .hasSize(6)
      .contains("OpenCover parser: invalid start line for file (ID '3', path 'MyLibrary\\Adder.cs', NO INDEXED PATH).");
  }

  @Test
  public void valid_with_no_absolute_path_deterministic_build_path_found() {
    Coverage coverage = new Coverage();
    FileService mockFileService = mock(FileService.class);
    when(mockFileService.isSupportedAbsolute(anyString())).thenReturn(false);
    String resolvedDeterministicPath = "/test/file/Calc.cs";
    when(mockFileService.getAbsolutePath("MyLibrary\\Adder.cs")).thenReturn(Optional.of(resolvedDeterministicPath));
    new OpenCoverReportParser(mockFileService).accept(new File("src/test/resources/opencover/valid.xml"), coverage);

    assertThat(coverage.files()).hasSize(1);
    assertThat(coverage.hits(resolvedDeterministicPath))
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

    assertThat(logTester.logs(LoggerLevel.INFO).get(0)).startsWith("Parsing the OpenCover report ");
    List<String> debugLogs = logTester.logs(LoggerLevel.DEBUG);
    assertThat(debugLogs)
      .hasSize(13)
      .contains(
      "CoveredFile created: (ID '3', path 'MyLibrary\\Adder.cs', indexed as '/test/file/Calc.cs').",
      "CoveredFile created: (ID '1', path 'MyLibraryNUnitTest\\AdderNUnitTest.cs', NO INDEXED PATH).",
      "CoveredFile created: (ID '4', path 'MyLibrary\\Multiplier.cs', NO INDEXED PATH)."
    );

    // FIXME the test case may be wrong https://github.com/SonarSource/sonar-dotnet/issues/4038
    assertThat(logTester.logs(LoggerLevel.WARN)).hasSize(6);
  }

  @Test
  public void valid_with_deterministic_source_path_returns_found_path() {
    Coverage coverage = new Coverage();
    FileService mockFileService = mock(FileService.class);
    when(mockFileService.isSupportedAbsolute(anyString())).thenReturn(false);
    String testAbsolutePath = "/full/path/to/Foo.cs";
    when(mockFileService.getAbsolutePath("/_/CoverageWithDeterministicSourcePaths/CoverageWithDeterministicSourcePaths/Foo.cs")).thenReturn(Optional.of(testAbsolutePath));
    new OpenCoverReportParser(mockFileService).accept(new File("src/test/resources/opencover/deterministic_source_paths.xml"), coverage);

    assertThat(coverage.files()).hasSize(1);
    assertThat(coverage.hits(testAbsolutePath))
      .hasSize(6)
      .containsOnly(
        Assertions.entry(6, 1),
        Assertions.entry(7, 1),
        Assertions.entry(8, 1),
        Assertions.entry(11, 0),
        Assertions.entry(12, 0),
        Assertions.entry(13, 0));
    assertThat(coverage.getBranchCoverage(testAbsolutePath))
      .hasSize(1)
      .containsExactly(new BranchCoverage(7, 2, 1));

    assertThat(logTester.logs(LoggerLevel.WARN)).isEmpty();
    assertThat(logTester.logs(LoggerLevel.INFO).get(0)).startsWith("Parsing the OpenCover report ");
    List<String> debugLogs = logTester.logs(LoggerLevel.DEBUG);
    assertThat(debugLogs)
      .hasSize(7)
      .contains("CoveredFile created: (ID '5', path '/_/CoverageWithDeterministicSourcePaths/CoverageWithDeterministicSourcePaths/Foo.cs', indexed as '/full/path/to/Foo.cs').");
    assertThat(logTester.logs(LoggerLevel.TRACE))
      .hasSize(8)
      .containsExactlyInAnyOrder(
        "OpenCover parser: add hits for file (ID '5', path '/_/CoverageWithDeterministicSourcePaths/CoverageWithDeterministicSourcePaths/Foo.cs', indexed as '/full/path/to/Foo.cs'), line '6', visitCount '1'.",
        "OpenCover parser: add hits for file (ID '5', path '/_/CoverageWithDeterministicSourcePaths/CoverageWithDeterministicSourcePaths/Foo.cs', indexed as '/full/path/to/Foo.cs'), line '7', visitCount '1'.",
        "OpenCover parser: add hits for file (ID '5', path '/_/CoverageWithDeterministicSourcePaths/CoverageWithDeterministicSourcePaths/Foo.cs', indexed as '/full/path/to/Foo.cs'), line '8', visitCount '1'.",
        "OpenCover parser: add branch hits for file (ID '5', path '/_/CoverageWithDeterministicSourcePaths/CoverageWithDeterministicSourcePaths/Foo.cs', indexed as '/full/path/to/Foo.cs'), line '7', offset '3', visitCount '0'.",
        "OpenCover parser: add branch hits for file (ID '5', path '/_/CoverageWithDeterministicSourcePaths/CoverageWithDeterministicSourcePaths/Foo.cs', indexed as '/full/path/to/Foo.cs'), line '7', offset '3', visitCount '1'.",
        "OpenCover parser: add hits for file (ID '5', path '/_/CoverageWithDeterministicSourcePaths/CoverageWithDeterministicSourcePaths/Foo.cs', indexed as '/full/path/to/Foo.cs'), line '11', visitCount '0'.",
        "OpenCover parser: add hits for file (ID '5', path '/_/CoverageWithDeterministicSourcePaths/CoverageWithDeterministicSourcePaths/Foo.cs', indexed as '/full/path/to/Foo.cs'), line '12', visitCount '0'.",
        "OpenCover parser: add hits for file (ID '5', path '/_/CoverageWithDeterministicSourcePaths/CoverageWithDeterministicSourcePaths/Foo.cs', indexed as '/full/path/to/Foo.cs'), line '13', visitCount '0'."
      );
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

    assertThat(coverage.getBranchCoverage(filePath))
      .hasSize(5)
      .contains(
        //  if (x == 0 || y < 0)
        new BranchCoverage(9, 4 , 2),

        // _ = y == 0 || z == 0;
        new BranchCoverage(12, 2 , 1),

        // _ = x == y || y == z;
        new BranchCoverage(13, 2, 1),

        // _ = y == 0 || z == 0; _ = x == y || y == z;
        new BranchCoverage(14, 4, 2),

        // return x < 2
        //    ? y < 3
        //        ? z < 1
        //            ? 1
        //            : 2
        //        : 3
        //    : 4;
        new BranchCoverage(18, 6, 3));
  }

  @Test
  public void branchCoverage_codeFile_analyzedByMultipleProjects() throws Exception {
    Coverage coverage = new Coverage();
    String filePath = new File("BranchCoverage3296\\Code\\ValueProvider.cs").getCanonicalPath();

    new OpenCoverReportParser(alwaysTrue).accept(new File("src/test/resources/opencover/code_tested_by_multiple_projects.xml"), coverage);
    assertThat(coverage.files()).containsOnly(filePath);

    assertThat(coverage.getBranchCoverage(filePath)).containsOnly(new BranchCoverage(5, 2, 2));
  }

  @Test
  public void branchCoverage_multipleCodePaths_analyzedByMultipleProjects() throws Exception {
    Coverage coverage = new Coverage();
    String filePath = new File("SwitchCoverage\\SwitchCoverage\\Foo.cs").getCanonicalPath();

    OpenCoverReportParser parser = new OpenCoverReportParser(alwaysTrue);

    parser.accept(new File("src/test/resources/opencover/switch_expression_multiple_test_projects_1.xml"), coverage);
    parser.accept(new File("src/test/resources/opencover/switch_expression_multiple_test_projects_2.xml"), coverage);

    assertThat(coverage.files()).contains(filePath);
    assertThat(coverage.hits(filePath))
      .hasSize(1)
      .contains(Assertions.entry(8, 4));

    assertThat(coverage.getBranchCoverage(filePath))
      .hasSize(1)
      // the switch expression gets transformed to a more complex IL representation, hence 8 conditions
      .contains(new BranchCoverage(8, 8 , 6));
  }

  @Test
  public void branchCoverage_codeFile_unsupportedFile() throws Exception {
    Coverage coverage = new Coverage();
    String filePath = new File("BranchCoverage3296\\Code\\ValueProvider.cs").getCanonicalPath();

    // Notice we pass "alwaysFalseAndEmpty" as a predicate
    new OpenCoverReportParser(alwaysFalseAndEmpty).accept(new File("src/test/resources/opencover/code_tested_by_multiple_projects.xml"), coverage);
    assertThat(coverage.files()).isEmpty();
    assertThat(coverage.getBranchCoverage(filePath)).isEmpty();
    assertThat(logTester.logs(LoggerLevel.DEBUG))
      .hasSize(9)
      // 6 logs below
      // The other logs contain system-dependants paths (e.g. "The current user dir is ...", "CoveredFile created: ...")
      .contains(
        // these are not ordered
        "Skipping the file (ID '1', path 'BranchCoverage3296\\Code\\ValueProvider.cs', NO INDEXED PATH), line '5', visitCount '1' because file is not indexed or does not have the supported language.",
        "Skipping the file (ID '2', path 'BranchCoverage3296\\Code\\ValueProvider.cs', NO INDEXED PATH), line '5', visitCount '1' because file is not indexed or does not have the supported language.",
        "OpenCover parser: Skipping branch hits for file (ID '2', path 'BranchCoverage3296\\Code\\ValueProvider.cs', NO INDEXED PATH), line '5', offset '1', visitCount '1' because file is not indexed or does not have the supported language.",
        "OpenCover parser: Skipping branch hits for file (ID '2', path 'BranchCoverage3296\\Code\\ValueProvider.cs', NO INDEXED PATH), line '5', offset '1', visitCount '0' because file is not indexed or does not have the supported language.",
        "OpenCover parser: Skipping branch hits for file (ID '1', path 'BranchCoverage3296\\Code\\ValueProvider.cs', NO INDEXED PATH), line '5', offset '1', visitCount '0' because file is not indexed or does not have the supported language.",
        "OpenCover parser: Skipping branch hits for file (ID '1', path 'BranchCoverage3296\\Code\\ValueProvider.cs', NO INDEXED PATH), line '5', offset '1', visitCount '1' because file is not indexed or does not have the supported language.");
  }

  @Test
  public void branchCoverage_invalidFileId() throws Exception {
    Coverage coverage = new Coverage();
    String filePath = new File("BranchCoverage3296\\Code\\ValueProvider.cs").getCanonicalPath();

    new OpenCoverReportParser(alwaysTrue).accept(new File("src/test/resources/opencover/invalid_file_id.xml"), coverage);
    assertThat(coverage.files()).containsOnly(filePath);

    assertThat(coverage.getBranchCoverage(filePath)).isEmpty();
    List<String> debugLogs = logTester.logs(LoggerLevel.DEBUG);
    assertThat(debugLogs)
      .hasSize(7)
      // 4 logs below
      // The other logs contain system-dependants paths (e.g. "The current user dir is ...", "CoveredFile created: ...")
      .contains(
        "OpenCover parser (handleBranchPointTag): the fileId '3' key is not contained in files.",
        "OpenCover parser (handleBranchPointTag): the fileId '4' key is not contained in files.",
        "OpenCover parser (handleBranchPointTag): the fileId '3' key is not contained in files.",
        "OpenCover parser (handleBranchPointTag): the fileId '3' key is not contained in files.");
  }

  @Test
  public void branchCoverage_getter_setter_multiple_sequence_points_per_line() throws Exception {
    Coverage coverage = new Coverage();
    new OpenCoverReportParser(alwaysTrue).accept(new File("src/test/resources/opencover/valid_case_multiple_sequence_points_per_line.xml"), coverage);

    String barFilePath = new File("GetSet\\Bar.cs").getCanonicalPath();
    assertThat(coverage.files()).containsOnly(
      barFilePath,
      new File("GetSet\\FooCallsBar.cs").getCanonicalPath(),
      new File("GetSetTests\\BarTests.cs").getCanonicalPath()
    );
    assertThat(coverage.hits(barFilePath))
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

      assertThat(coverage.getBranchCoverage(barFilePath))
        .hasSize(1)
        .containsOnly(new BranchCoverage(17, 2, 1)); // line 17: ArrowMethod

      List<String> traceLogs = logTester.logs(LoggerLevel.TRACE);
      assertThat(traceLogs).hasSize(34);
  }

  @Test
  public void log_unsupported_file_extension() {
    Coverage coverage = new Coverage();

    // to easily check the logs (it has only one coverage entry)
    new OpenCoverReportParser(alwaysFalseAndEmpty).accept(new File("src/test/resources/opencover/one_class.xml"), coverage);

    assertThat(coverage.files()).isEmpty();

    List<String> debugLogs = logTester.logs(LoggerLevel.DEBUG);
    assertThat(debugLogs.get(0)).startsWith("The current user dir is '");
    assertThat(debugLogs.stream().skip(1))
      .containsExactlyInAnyOrder(
        "CoveredFile created: (ID '1', path 'MyLibraryNUnitTest\\AdderNUnitTest.cs', NO INDEXED PATH).",
        "Skipping the file (ID '1', path 'MyLibraryNUnitTest\\AdderNUnitTest.cs', NO INDEXED PATH), line '16', visitCount '1' because file is not indexed or does not have the supported language.",
        "Skipping the file (ID '1', path 'MyLibraryNUnitTest\\AdderNUnitTest.cs', NO INDEXED PATH), line '17', visitCount '1' because file is not indexed or does not have the supported language.",
        "Skipping the file (ID '1', path 'MyLibraryNUnitTest\\AdderNUnitTest.cs', NO INDEXED PATH), line '18', visitCount '1' because file is not indexed or does not have the supported language."
      );
  }

  @Test
  public void should_not_fail_with_invalid_path() {
    new OpenCoverReportParser(alwaysTrue).accept(new File("src/test/resources/opencover/invalid_path.xml"), mock(Coverage.class));
    List<String> debugLogs = logTester.logs(LoggerLevel.DEBUG);
    assertThat(debugLogs.get(0)).startsWith("The current user dir is '");
    assertThat(debugLogs.get(1)).startsWith("Skipping the import of OpenCover code coverage for the invalid file path: z:\\*\"?.cs at line 150");
    assertThat(debugLogs.get(1)).startsWith("Skipping the import of OpenCover code coverage for the invalid file path: z:\\*\"?.cs at line 150");
    assertThat(debugLogs.get(8)).startsWith("CoveredFile created: (ID '3', path 'MyLibrary\\Adder.cs', indexed as");
    assertThat(debugLogs.get(9)).startsWith("CoveredFile created: (ID '4', path 'MyLibrary\\Multiplier.cs', indexed as");
    assertThat(debugLogs).contains(
      "OpenCover parser (handleSequencePointTag): the fileId '1' key is not contained in files (entry for line '16', visitCount '1').",
      "OpenCover parser (handleSequencePointTag): the fileId '1' key is not contained in files (entry for line '17', visitCount '1').",
      "OpenCover parser (handleSequencePointTag): the fileId '1' key is not contained in files (entry for line '18', visitCount '1').",
      "OpenCover parser (handleSequencePointTag): the fileId '1' key is not contained in files (entry for line '22', visitCount '1').",
      "OpenCover parser (handleSequencePointTag): the fileId '1' key is not contained in files (entry for line '23', visitCount '1').",
      "OpenCover parser (handleSequencePointTag): the fileId '1' key is not contained in files (entry for line '24', visitCount '1')."
    );
  }

}
