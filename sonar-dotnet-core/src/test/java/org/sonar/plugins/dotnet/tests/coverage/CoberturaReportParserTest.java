/*
 * SonarSource :: .NET :: Core
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
import java.util.List;
import java.util.Optional;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.slf4j.event.Level;
import org.sonar.api.testfixtures.log.LogTester;
import org.sonar.plugins.dotnet.tests.FileService;
import static org.assertj.core.api.Assertions.assertThat;
import static org.junit.Assert.assertThrows;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.ArgumentMatchers.contains;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

public class CoberturaReportParserTest {

  @Rule
  public LogTester logTester = new LogTester();

  private FileService alwaysTrue;
  private FileService alwaysFalseAndEmpty;

  @Before
  public void prepare() {
    logTester.setLevel(Level.TRACE);
    alwaysTrue = mock(FileService.class);
    when(alwaysTrue.isSupportedAbsolute(anyString())).thenReturn(true);
    when(alwaysTrue.getAbsolutePath(anyString())).thenThrow(new UnsupportedOperationException("Should not call this"));
    alwaysFalseAndEmpty = mock(FileService.class);
    when(alwaysFalseAndEmpty.isSupportedAbsolute(anyString())).thenReturn(false);
    when(alwaysFalseAndEmpty.getAbsolutePath(anyString())).thenReturn(Optional.empty());
  }

  @Test
  public void invalid_root() {
    var exception = assertThrows(RuntimeException.class,
      () -> parseCoverage("invalid_root.xml"));
    assertThat(exception).hasMessageContaining("<coverage>");
  }

  @Test
  public void non_existing_file() {
    var exception = assertThrows(RuntimeException.class,
      () -> parseCoverage("non_existing_file.xml"));
    assertThat(exception).hasMessageContaining("non_existing_file.xml");
  }

  @Test
  public void valid_empty() {
    Coverage coverage = parseCoverage("valid_empty.xml");
    assertThat(coverage.files()).isEmpty();
  }

  @Test
  public void absolute_path_no_sources_resolves_file() {
    parseCoverage("absolute_path_no_sources.xml");
    verify(alwaysTrue).isSupportedAbsolute(contains("Class1.cs"));
    assertThat(logTester.logs(Level.DEBUG)).anyMatch(log -> log.contains("CoveredFile created") && log.contains("indexed as"));
  }

  @Test
  public void absolute_path_with_sources_ignores_sources() {
    parseCoverage("absolute_path_with_sources.xml");
    verify(alwaysTrue).isSupportedAbsolute(contains("Class1.cs"));
    assertThat(logTester.logs(Level.DEBUG))
      .anyMatch(log -> log.contains("found source"))
      .anyMatch(log -> log.contains("CoveredFile created") && log.contains("indexed as"));
  }

  @Test
  public void relative_path_with_sources_uses_first_matching_source() {
    FileService selectiveService = mock(FileService.class);
    when(selectiveService.isSupportedAbsolute(anyString())).thenReturn(false);
    when(selectiveService.isSupportedAbsolute(contains("SecondSource"))).thenReturn(true);
    when(selectiveService.getAbsolutePath(anyString())).thenReturn(Optional.empty());

    parseCoverage("relative_path_with_sources.xml", selectiveService);
    // Verify both sources were evaluated: FirstSource was tried and rejected, then SecondSource matched
    verify(selectiveService, times(2)).isSupportedAbsolute(anyString());
    assertThat(logTester.logs(Level.DEBUG)).anyMatch(log ->
      log.contains("CoveredFile created") && log.contains("SecondSource") && log.contains("indexed as"));
  }

  @Test
  public void relative_path_with_sources_no_match_skips_file() {
    parseCoverage("relative_path_with_sources.xml", alwaysFalseAndEmpty);
    assertThat(logTester.logs(Level.DEBUG)).anyMatch(log -> log.contains("could not resolve relative filename"));
  }

  @Test
  public void relative_path_no_sources_skips_file() {
    parseCoverage("relative_path_no_sources.xml");
    assertThat(logTester.logs(Level.DEBUG)).anyMatch(log -> log.contains("could not resolve relative filename"));
  }

  @Test
  public void multiple_classes_same_filename_resolved_once() {
    parseCoverage("multiple_classes_same_filename.xml");
    verify(alwaysTrue, times(1)).isSupportedAbsolute(anyString());
    long coveredFileLogCount = logTester.logs(Level.DEBUG).stream()
      .filter(log -> log.contains("CoveredFile created"))
      .count();
    assertThat(coveredFileLogCount).isEqualTo(1);
  }

  @Test
  public void empty_source_tag_is_ignored() {
    parseCoverage("empty_source_tag.xml");
    assertThat(logTester.logs(Level.DEBUG)).noneMatch(log -> log.contains("found source"));
    verify(alwaysTrue).isSupportedAbsolute(anyString());
  }

  @Test
  public void should_not_fail_with_invalid_path() {
    parseCoverage("invalid_path.xml");
    assertThat(logTester.logs(Level.DEBUG)).anyMatch(log -> log.contains("Skipping the import of Cobertura code coverage for the invalid file path"));
  }

  @Test
  public void source_with_nested_elements_does_not_fail() {
    parseCoverage("source_with_nested_elements.xml");
    assertThat(logTester.logs(Level.DEBUG)).anyMatch(log -> log.contains("failed to read <source> element text"));
    verify(alwaysTrue).isSupportedAbsolute(anyString());
  }

  @Test
  public void deterministic_build_path_fallback() {
    FileService deterministicService = mock(FileService.class);
    when(deterministicService.isSupportedAbsolute(anyString())).thenReturn(false);
    when(deterministicService.getAbsolutePath(anyString())).thenReturn(Optional.of("C:\\resolved\\Lib\\Class1.cs"));

    parseCoverage("absolute_path_no_sources.xml", deterministicService);
    verify(deterministicService).getAbsolutePath(anyString());
    assertThat(logTester.logs(Level.DEBUG)).anyMatch(log -> log.contains("CoveredFile created") && log.contains("indexed as"));
  }

  @Test
  public void line_coverage_adds_hits() {
    Coverage coverage = parseCoverage("line_coverage.xml");
    assertThat(coverage.files()).hasSize(2);
    String file1 = getFilePath(coverage, "Class1");
    assertThat(coverage.hits(file1))
      .hasSize(3)
      .containsEntry(10, 6)   // hits=3 from method + hits=3 from class = 6
      .containsEntry(11, 0)   // hits=0 from method only
      .containsEntry(15, 1);  // hits=1 from class only
    String file2 = getFilePath(coverage, "Class2");
    assertThat(coverage.hits(file2))
      .hasSize(2)
      .containsEntry(5, 3)    // hits=2 from method + hits=1 from class = 3
      .containsEntry(20, 4);  // hits=4 from class only
  }

  @Test
  public void line_coverage_unresolved_file_skips_lines() {
    Coverage coverage = parseCoverage("line_coverage_unresolved_file.xml");
    // Relative path with no sources — file can't resolve, lines are skipped
    assertThat(coverage.files()).isEmpty();
  }

  @Test
  public void branch_jump_conditions_creates_two_condition_data_per_condition() {
    Coverage coverage = parseCoverage("branch_jump_conditions.xml");
    String filePath = getFilePath(coverage, "Class1");
    List<ConditionData> conditions = getConditions(coverage, filePath);
    assertThat(conditions)
      .hasSize(12)
      .allMatch(c -> "cobertura".equals(c.getFormat()));
    // condition 24: 100% -> path 0 hit=1, path 1 hit=1
    List<ConditionData> cond24 = conditions.stream().filter(c -> c.getLocationStart() == 24).toList();
    assertThat(cond24).hasSize(4); // 2 paths x 2 occurrences
    assertThat(cond24.stream().filter(c -> c.getPath() == 0).allMatch(c -> c.getHits() == 1)).isTrue();
    assertThat(cond24.stream().filter(c -> c.getPath() == 1).allMatch(c -> c.getHits() == 1)).isTrue();
    // condition 36: 50% -> path 0 hit=1, path 1 hit=0
    List<ConditionData> cond36 = conditions.stream().filter(c -> c.getLocationStart() == 36).toList();
    assertThat(cond36).hasSize(4);
    assertThat(cond36.stream().filter(c -> c.getPath() == 0).allMatch(c -> c.getHits() == 1)).isTrue();
    assertThat(cond36.stream().filter(c -> c.getPath() == 1).allMatch(c -> c.getHits() == 0)).isTrue();
    // condition 48: 0% -> both paths hit=0
    List<ConditionData> cond48 = conditions.stream().filter(c -> c.getLocationStart() == 48).toList();
    assertThat(cond48)
      .hasSize(4)
      .allMatch(c -> c.getHits() == 0);
    // line coverage still recorded
    assertThat(coverage.hits(filePath)).containsEntry(10, 4).containsEntry(11, 2);
    assertThat(logTester.logs(Level.TRACE)).anyMatch(log -> log.contains("add jump branch conditions"));
  }

  @Test
  public void branch_switch_condition_creates_n_condition_data() {
    Coverage coverage = parseCoverage("branch_switch_condition.xml");
    String filePath = getFilePath(coverage, "Class1");
    // Switch with condition-coverage="33% (2/6)" appears twice (method + class)
    List<ConditionData> conditions = getConditions(coverage, filePath);
    // 6 paths * 2 occurrences = 12
    assertThat(conditions)
      .hasSize(12)
      .allMatch(c -> "cobertura".equals(c.getFormat()))
      .allMatch(c -> c.getLocationStart() == 48)
      .allMatch(c -> c.getLocationEnd() == 0);
    // First 2 of 6 paths have hits (per occurrence)
    List<ConditionData> firstOccurrence = conditions.stream().limit(6).toList();
    assertThat(firstOccurrence.stream().filter(c -> c.getPath() < 2).allMatch(c -> c.getHits() == 1)).isTrue();
    assertThat(firstOccurrence.stream().filter(c -> c.getPath() >= 2).allMatch(c -> c.getHits() == 0)).isTrue();
    // getBranchCoverage aggregated result: 6 total, 2 covered
    assertThat(coverage.getBranchCoverage(filePath))
      .containsExactly(new BranchCoverage(42, 6, 2));
    assertThat(logTester.logs(Level.TRACE)).anyMatch(log -> log.contains("add 6 switch branch conditions"));
  }

  @Test
  public void branch_no_conditions_creates_unmergeable_condition_data() {
    Coverage coverage = parseCoverage("branch_no_conditions.xml");
    String filePath = getFilePath(coverage, "Class1");
    List<ConditionData> conditions = getConditions(coverage, filePath);
    // condition-coverage="50% (2/4)" -> 4 ConditionData
    assertThat(conditions)
      .hasSize(4)
      .allMatch(c -> "unmergeable".equals(c.getFormat()))
      .allMatch(c -> c.getStartLine() == 42);
    // First 2 have hits, last 2 don't
    assertThat(conditions.stream().filter(c -> c.getPath() < 2).allMatch(c -> c.getHits() == 1)).isTrue();
    assertThat(conditions.stream().filter(c -> c.getPath() >= 2).allMatch(c -> c.getHits() == 0)).isTrue();
    // locationStart equals path index for unmergeable
    for (ConditionData c : conditions) {
      assertThat(c.getLocationStart()).isEqualTo(c.getPath());
    }
    // line coverage recorded
    assertThat(coverage.hits(filePath)).containsEntry(42, 2).containsEntry(43, 0);
    assertThat(logTester.logs(Level.TRACE)).anyMatch(log -> log.contains("add 4 unmergeable branch conditions"));
  }

  @Test
  public void branch_mixed_jump_and_switch_conditions() {
    Coverage coverage = parseCoverage("branch_mixed_conditions.xml");
    // Class1: 1 jump + 1 switch, condition-coverage="50% (3/6)"
    // Jump condition 24 (100%): 2 ConditionData, both hit. Consumes 2 branches, 2 covered.
    // Switch condition 48: remainingTotal=6-2=4, remainingCovered=max(0,3-2)=1 -> 4 ConditionData, 1 hit.
    String class1Path = getFilePath(coverage, "Class1");
    List<ConditionData> class1Conditions = getConditions(coverage, class1Path);
    assertThat(class1Conditions)
      .hasSize(6)
      .allMatch(c -> "cobertura".equals(c.getFormat()));
    List<ConditionData> jump24 = class1Conditions.stream().filter(c -> c.getLocationStart() == 24).toList();
    assertThat(jump24)
      .hasSize(2)
      .allMatch(c -> c.getHits() == 1);
    List<ConditionData> switch48 = class1Conditions.stream().filter(c -> c.getLocationStart() == 48).toList();
    assertThat(switch48).hasSize(4);
    assertThat(switch48.stream().filter(c -> c.getPath() == 0).allMatch(c -> c.getHits() == 1)).isTrue();
    assertThat(switch48.stream().filter(c -> c.getPath() >= 1).allMatch(c -> c.getHits() == 0)).isTrue();
    // Class2: 2 jumps + 1 switch, condition-coverage="50% (5/10)"
    // Jump 0 (100%): 2 entries, both hit. Consumes 2 branches, 2 covered.
    // Jump 1 (50%):  2 entries, path0 hit, path1 not. Consumes 2 branches, 1 covered.
    // Switch 2: remainingTotal=10-4=6, remainingCovered=max(0,5-3)=2 -> 6 entries, 2 hit.
    String class2Path = getFilePath(coverage, "Class2");
    List<ConditionData> class2Conditions = getConditions(coverage, class2Path);
    assertThat(class2Conditions)
      .hasSize(10)
      .allMatch(c -> "cobertura".equals(c.getFormat()));

    List<ConditionData> jump0 = class2Conditions.stream().filter(c -> c.getLocationStart() == 0).toList();
    assertThat(jump0)
      .hasSize(2)
      .allMatch(c -> c.getHits() == 1);

    List<ConditionData> jump1 = class2Conditions.stream().filter(c -> c.getLocationStart() == 1).toList();
    assertThat(jump1).hasSize(2);
    assertThat(jump1.stream().filter(c -> c.getPath() == 0).allMatch(c -> c.getHits() == 1)).isTrue();
    assertThat(jump1.stream().filter(c -> c.getPath() == 1).allMatch(c -> c.getHits() == 0)).isTrue();

    List<ConditionData> switch2 = class2Conditions.stream().filter(c -> c.getLocationStart() == 2).toList();
    assertThat(switch2).hasSize(6);
    assertThat(switch2.stream().filter(c -> c.getPath() < 2).allMatch(c -> c.getHits() == 1)).isTrue();
    assertThat(switch2.stream().filter(c -> c.getPath() >= 2).allMatch(c -> c.getHits() == 0)).isTrue();
  }

  @Test
  public void branch_malformed_condition_coverage_treats_as_zero_percent() {
    Coverage coverage = parseCoverage("branch_malformed_condition_coverage.xml");
    String filePath = getFilePath(coverage, "Class1");
    List<ConditionData> conditions = getConditions(coverage, filePath);
    assertThat(conditions)
      .hasSize(2)
      .allMatch(c -> c.getHits() == 0);
    assertThat(logTester.logs(Level.DEBUG)).anyMatch(log -> log.contains("could not parse coverage percentage"));
  }

  @Test
  public void branch_missing_condition_coverage_attribute_skips_branch_data() {
    Coverage coverage = parseCoverage("branch_no_condition_coverage_attr.xml");
    String filePath = getFilePath(coverage, "Class1");
    assertThat(getConditions(coverage, filePath)).isEmpty();
    assertThat(coverage.hits(filePath)).containsEntry(10, 3).containsEntry(11, 1);
  }

  @Test
  public void branch_unresolved_file_skips_condition_data() {
    Coverage coverage = parseCoverage("branch_unresolved_file.xml");
    assertThat(coverage.files()).isEmpty();
    assertThat(coverage.getConditionData()).isEmpty();
  }

  private Coverage parseCoverage(String reportFileName) {
    return parseCoverage(reportFileName, alwaysTrue);
  }

  private Coverage parseCoverage(String reportFileName, FileService fileService) {
    Coverage coverage = new Coverage();
    new CoberturaReportParser(fileService).accept(new File("src/test/resources/cobertura/" + reportFileName), coverage);
    return coverage;
  }

  private String getFilePath(Coverage coverage, String className) {
    return coverage.files().stream()
      .filter(f -> f.contains(className))
      .findFirst()
      .orElseThrow();
  }

  private List<ConditionData> getConditions(Coverage coverage, String filePath) {
    return coverage.getConditionData().stream()
      .filter(c -> c.getFilePath().equals(filePath))
      .toList();
  }
}
