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
    var parser = new CoberturaReportParser(alwaysTrue);
    var exception = assertThrows(RuntimeException.class,
      () -> parser.accept(new File("src/test/resources/cobertura/invalid_root.xml"), mock(Coverage.class)));
    assertThat(exception).hasMessageContaining("<coverage>");
  }

  @Test
  public void non_existing_file() {
    var parser = new CoberturaReportParser(alwaysTrue);
    var exception = assertThrows(RuntimeException.class,
      () -> parser.accept(new File("src/test/resources/cobertura/non_existing_file.xml"), mock(Coverage.class)));
    assertThat(exception).hasMessageContaining("non_existing_file.xml");
  }

  @Test
  public void valid_empty() {
    Coverage coverage = new Coverage();
    new CoberturaReportParser(alwaysTrue).accept(new File("src/test/resources/cobertura/valid_empty.xml"), coverage);
    assertThat(coverage.files()).isEmpty();
  }

  @Test
  public void absolute_path_no_sources_resolves_file() {
    Coverage coverage = new Coverage();
    new CoberturaReportParser(alwaysTrue).accept(new File("src/test/resources/cobertura/absolute_path_no_sources.xml"), coverage);
    verify(alwaysTrue).isSupportedAbsolute(contains("Class1.cs"));
    assertThat(logTester.logs(Level.DEBUG)).anyMatch(log -> log.contains("CoveredFile created") && log.contains("indexed as"));
  }

  @Test
  public void absolute_path_with_sources_ignores_sources() {
    Coverage coverage = new Coverage();
    new CoberturaReportParser(alwaysTrue).accept(new File("src/test/resources/cobertura/absolute_path_with_sources.xml"), coverage);
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
    Coverage coverage = new Coverage();

    new CoberturaReportParser(selectiveService).accept(new File("src/test/resources/cobertura/relative_path_with_sources.xml"), coverage);
    // Verify both sources were evaluated: FirstSource was tried and rejected, then SecondSource matched
    verify(selectiveService, times(2)).isSupportedAbsolute(anyString());
    assertThat(logTester.logs(Level.DEBUG)).anyMatch(log ->
      log.contains("CoveredFile created") && log.contains("SecondSource") && log.contains("indexed as"));
  }

  @Test
  public void relative_path_with_sources_no_match_skips_file() {
    Coverage coverage = new Coverage();
    new CoberturaReportParser(alwaysFalseAndEmpty).accept(new File("src/test/resources/cobertura/relative_path_with_sources.xml"), coverage);

    assertThat(logTester.logs(Level.DEBUG)).anyMatch(log -> log.contains("could not resolve relative filename"));
  }

  @Test
  public void relative_path_no_sources_skips_file() {
    Coverage coverage = new Coverage();
    new CoberturaReportParser(alwaysTrue).accept(new File("src/test/resources/cobertura/relative_path_no_sources.xml"), coverage);

    assertThat(logTester.logs(Level.DEBUG)).anyMatch(log -> log.contains("could not resolve relative filename"));
  }

  @Test
  public void multiple_classes_same_filename_resolved_once() {
    Coverage coverage = new Coverage();
    new CoberturaReportParser(alwaysTrue).accept(new File("src/test/resources/cobertura/multiple_classes_same_filename.xml"), coverage);

    verify(alwaysTrue, times(1)).isSupportedAbsolute(anyString());
    long coveredFileLogCount = logTester.logs(Level.DEBUG).stream()
      .filter(log -> log.contains("CoveredFile created"))
      .count();
    assertThat(coveredFileLogCount).isEqualTo(1);
  }

  @Test
  public void empty_source_tag_is_ignored() {
    Coverage coverage = new Coverage();
    new CoberturaReportParser(alwaysTrue).accept(new File("src/test/resources/cobertura/empty_source_tag.xml"), coverage);

    assertThat(logTester.logs(Level.DEBUG)).noneMatch(log -> log.contains("found source"));
    verify(alwaysTrue).isSupportedAbsolute(anyString());
  }

  @Test
  public void should_not_fail_with_invalid_path() {
    Coverage coverage = new Coverage();
    new CoberturaReportParser(alwaysTrue).accept(new File("src/test/resources/cobertura/invalid_path.xml"), coverage);

    assertThat(logTester.logs(Level.DEBUG)).anyMatch(log -> log.contains("Skipping the import of Cobertura code coverage for the invalid file path"));
  }

  @Test
  public void source_with_nested_elements_does_not_fail() {
    Coverage coverage = new Coverage();
    new CoberturaReportParser(alwaysTrue).accept(new File("src/test/resources/cobertura/source_with_nested_elements.xml"), coverage);

    assertThat(logTester.logs(Level.DEBUG)).anyMatch(log -> log.contains("failed to read <source> element text"));
    verify(alwaysTrue).isSupportedAbsolute(anyString());
  }

  @Test
  public void deterministic_build_path_fallback() {
    FileService deterministicService = mock(FileService.class);
    when(deterministicService.isSupportedAbsolute(anyString())).thenReturn(false);
    when(deterministicService.getAbsolutePath(anyString())).thenReturn(Optional.of("C:\\resolved\\Lib\\Class1.cs"));

    Coverage coverage = new Coverage();
    new CoberturaReportParser(deterministicService).accept(new File("src/test/resources/cobertura/absolute_path_no_sources.xml"), coverage);

    verify(deterministicService).getAbsolutePath(anyString());
    assertThat(logTester.logs(Level.DEBUG)).anyMatch(log -> log.contains("CoveredFile created") && log.contains("indexed as"));
  }

  @Test
  public void line_coverage_adds_hits() {
    Coverage coverage = new Coverage();
    new CoberturaReportParser(alwaysTrue).accept(new File("src/test/resources/cobertura/line_coverage.xml"), coverage);

    assertThat(coverage.files()).hasSize(2);

    String file1 = coverage.files().stream().filter(f -> f.contains("Class1")).findFirst().orElseThrow();
    assertThat(coverage.hits(file1))
      .hasSize(3)
      .containsEntry(10, 6)   // hits=3 from method + hits=3 from class = 6
      .containsEntry(11, 0)   // hits=0 from method only
      .containsEntry(15, 1);  // hits=1 from class only

    String file2 = coverage.files().stream().filter(f -> f.contains("Class2")).findFirst().orElseThrow();
    assertThat(coverage.hits(file2))
      .hasSize(2)
      .containsEntry(5, 3)    // hits=2 from method + hits=1 from class = 3
      .containsEntry(20, 4);  // hits=4 from class only
  }

  @Test
  public void line_coverage_unresolved_file_skips_lines() {
    Coverage coverage = new Coverage();
    new CoberturaReportParser(alwaysTrue).accept(new File("src/test/resources/cobertura/line_coverage_unresolved_file.xml"), coverage);

    // Relative path with no sources — file can't resolve, lines are skipped
    assertThat(coverage.files()).isEmpty();
  }
}
