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

import java.util.Map;
import org.junit.Test;

import static org.assertj.core.api.Assertions.assertThat;

public class CoverageTest {

  @Test
  public void test() {
    Coverage coverage = new Coverage();
    assertThat(coverage.files()).isEmpty();
    assertThat(coverage.hits("foo.txt")).isEmpty();

    coverage.addHits("foo.txt", 42, 1);
    assertThat(coverage.files()).containsExactlyInAnyOrder("foo.txt");
    assertThat(coverage.hits("foo.txt")).isEqualTo(Map.of(42, 1));

    coverage.addHits("foo.txt", 42, 3);
    assertThat(coverage.files()).containsExactlyInAnyOrder("foo.txt");
    assertThat(coverage.hits("foo.txt")).isEqualTo(Map.of(42, 4));

    coverage.addHits("foo.txt", 1234, 11);
    assertThat(coverage.files()).containsExactlyInAnyOrder("foo.txt");
    assertThat(coverage.hits("foo.txt")).isEqualTo(Map.of(42, 4, 1234, 11));

    coverage.addHits("bar.txt", 1, 2);
    assertThat(coverage.files()).containsExactlyInAnyOrder("foo.txt", "bar.txt");
    assertThat(coverage.hits("foo.txt")).isEqualTo(Map.of(42, 4, 1234, 11));
    assertThat(coverage.hits("bar.txt")).isEqualTo(Map.of(1, 2));

    Coverage other = new Coverage();

    coverage.mergeWith(other);
    assertThat(coverage.files()).containsExactlyInAnyOrder("foo.txt", "bar.txt");
    assertThat(coverage.hits("foo.txt")).isEqualTo(Map.of(42, 4, 1234, 11));
    assertThat(coverage.hits("bar.txt")).isEqualTo(Map.of(1, 2));

    other.addHits("baz.txt", 2, 7);
    assertThat(other.files()).containsExactlyInAnyOrder("baz.txt");
    assertThat(other.hits("baz.txt")).isEqualTo(Map.of(2, 7));

    coverage.mergeWith(other);
    assertThat(other.files()).containsExactlyInAnyOrder("baz.txt");
    assertThat(other.hits("baz.txt")).isEqualTo(Map.of(2, 7));
    assertThat(coverage.files()).containsExactlyInAnyOrder("foo.txt", "bar.txt", "baz.txt");
    assertThat(coverage.hits("foo.txt")).isEqualTo(Map.of(42, 4, 1234, 11));
    assertThat(coverage.hits("bar.txt")).isEqualTo(Map.of(1, 2));
    assertThat(coverage.hits("baz.txt")).isEqualTo(Map.of(2, 7));
  }

  @Test
  public void givenEmptyListOfConditionDatas_getBranchCoverage_returnsEmpty() {
    Coverage sut = new Coverage();

    assertThat(sut.getBranchCoverage("fileName")).isEmpty();
  }

  @Test
  public void givenSingleConditionData_getBranchCoverage_returnsEmpty() {
    final String filePath = "filePath";

    Coverage sut = new Coverage();
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 1, new ConditionData.Location(2, 3), 4, 5, "coverageIdentifier"));

    // Normally this case should not happen but if we have only one branch point
    // we should not report coverage as line coverage is already covering that.
    assertThat(sut.getBranchCoverage(filePath)).isEmpty();
  }

  @Test
  public void givenSingleConditionDataPerFile_getBranchCoverage_returnsEmpty() {
    final String firstPath = "firstPath";
    final String secondPath = "secondPath";
    final String coverageIdentifier = "coverageIdentifier";

    Coverage sut = new Coverage();
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,firstPath, 1, new ConditionData.Location(2, 3), 4, 5, coverageIdentifier));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,secondPath, 1, new ConditionData.Location(6, 7), 8, 9, coverageIdentifier));

    // Normally this case should not happen but if we have only one branch point
    // we should not report coverage as line coverage is already covering that.
    assertThat(sut.getBranchCoverage(firstPath)).isEmpty();
    assertThat(sut.getBranchCoverage(secondPath)).isEmpty();
  }

  @Test
  public void givenSingleConditionDataPerLine_getBranchCoverage_returnsEmpty() {
    final String filePath = "filePath";
    final String coverageIdentifier = "coverageIdentifier";

    Coverage sut = new Coverage();
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 1, new ConditionData.Location(2, 3), 4, 5, coverageIdentifier));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 2, new ConditionData.Location(6, 7), 8, 9, coverageIdentifier));

    assertThat(sut.getBranchCoverage(filePath)).isEmpty();
  }

  @Test
  public void branchPointsMerging() {
    final String filePath = "filePath";
    final String coverageIdentifier = "coverageIdentifier";

    Coverage sut = new Coverage();

    // Identical branch points are merged
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 1, new ConditionData.Location(1, 2), 0, 1, coverageIdentifier));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 1, new ConditionData.Location(1, 2), 0, 1, coverageIdentifier));

    // Branch points with different line are not aggregated
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 2, new ConditionData.Location(1, 2), 0, 1, coverageIdentifier));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 3, new ConditionData.Location(1, 2), 0, 1, coverageIdentifier));

    // Branch points with different offset are correctly aggregated
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 4, new ConditionData.Location(1, 2), 0, 1, coverageIdentifier));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 4, new ConditionData.Location(2, 2), 0, 1, coverageIdentifier));

    // Branch points with different offsetEnd are correctly aggregated
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 5, new ConditionData.Location(1, 2), 0, 1, coverageIdentifier));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 5, new ConditionData.Location(1, 3), 0, 1, coverageIdentifier));

    // Branch points with different path are correctly aggregated
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 6, new ConditionData.Location(1, 2), 0, 1, coverageIdentifier));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 6, new ConditionData.Location(1, 2), 1, 1, coverageIdentifier));

    assertThat(sut.getBranchCoverage(filePath))
      .containsExactlyInAnyOrder(
        // For the first 3 lines we will not report branch coverage since they have only one branch point each.
        new BranchCoverage(4, 2, 2),
        new BranchCoverage(5, 2, 2),
        new BranchCoverage(6, 2, 2)
      );
  }

  @Test
  public void givenMultipleConditionDataPerLine_getBranchCoverage_returnsBranchCoverage() {
    final String filePath = "filePath";
    final String coverageIdentifier1 = "coverageIdentifier1";
    final String coverageIdentifier2 = "coverageIdentifier2";

    Coverage sut = new Coverage();
    // Both branch points covered
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 1, new ConditionData.Location(1, 3), 0, 1, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 1, new ConditionData.Location(4, 6), 1, 1, coverageIdentifier1));

    // Only 2 out of 3 branch points covered
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 2, new ConditionData.Location(1, 3), 0, 2, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 2, new ConditionData.Location(4, 6), 1, 0, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 2, new ConditionData.Location(6, 8), 2, 4, coverageIdentifier1));

    // No branch points covered
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 3, new ConditionData.Location(1, 3), 0, 0, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 3, new ConditionData.Location(4, 6), 1, 0, coverageIdentifier1));

    // Same branch points appear multiple times, none covered (when tests are split in multiple test projects)
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 4, new ConditionData.Location(1, 3), 0, 0, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 4, new ConditionData.Location(4, 6), 1, 0, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 4, new ConditionData.Location(1, 3), 0, 0, coverageIdentifier2));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 4, new ConditionData.Location(4, 6), 1, 0, coverageIdentifier2));

    // Same branch points appear multiple times, same coverage (when tests are split in multiple test projects)
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 5, new ConditionData.Location(1, 3), 0, 1, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 5, new ConditionData.Location(4, 6), 1, 0, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 5, new ConditionData.Location(1, 3), 0, 1, coverageIdentifier2));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 5, new ConditionData.Location(4, 6), 1, 0, coverageIdentifier2));

    // Same branch points appear multiple times, different coverage (when tests are split in multiple test projects)
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 6, new ConditionData.Location(1, 3), 0, 1, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 6, new ConditionData.Location(4, 6), 1, 0, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 6, new ConditionData.Location(1, 3), 0, 0, coverageIdentifier2));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 6, new ConditionData.Location(4, 6), 1, 1, coverageIdentifier2));

    assertThat(sut.getBranchCoverage(filePath))
      .containsExactlyInAnyOrder(
        new BranchCoverage(1, 2, 2),
        new BranchCoverage(2, 3, 2),
        new BranchCoverage(3, 2, 0),
        new BranchCoverage(4, 2, 0),
        new BranchCoverage(5, 2, 1),
        new BranchCoverage(6, 2, 2)
      );
  }

  @Test
  public void givenMultipleConditionDataPerLineInDifferentFiles_getBranchCoverage_returnsBranchCoverage() {
    final String firstPath = "firstPath";
    final String secondPath = "secondPath";
    final String coverageIdentifier = "coverageIdentifier";

    Coverage sut = new Coverage();
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,firstPath, 1, new ConditionData.Location(1, 3), 0, 2, coverageIdentifier));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,firstPath, 1, new ConditionData.Location(4, 6), 1, 1, coverageIdentifier));

    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,secondPath, 1, new ConditionData.Location(5, 8), 0, 2, coverageIdentifier));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,secondPath, 1, new ConditionData.Location(10, 12), 1, 0, coverageIdentifier));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,secondPath, 1, new ConditionData.Location(12, 14), 2, 0, coverageIdentifier));

    assertThat(sut.getBranchCoverage(firstPath)).containsExactlyInAnyOrder(new BranchCoverage(1, 2, 2));
    assertThat(sut.getBranchCoverage(secondPath)).containsExactlyInAnyOrder(new BranchCoverage(1, 3, 1));
  }

  @Test
  public void givenConditionDataFromFromDifferentReports_getBranchCoverage_returnsBranchCoverage(){
    final String filePath = "filePath";
    final String coverageIdentifier1 = "coverageIdentifier1";
    final String coverageIdentifier2 = "coverageIdentifier2";

    Coverage sut = new Coverage();

    // ConditionData match exactly, coverage is aggregated correctly
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 1, new ConditionData.Location(0, 5), 0, 1, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 1, new ConditionData.Location(0, 6), 1, 0, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 1, new ConditionData.Location(0, 5), 0, 0, coverageIdentifier2));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 1, new ConditionData.Location(0, 6), 1, 1, coverageIdentifier2));

    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 2, new ConditionData.Location(0, 5), 0, 1, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 2, new ConditionData.Location(0, 6), 1, 0, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 2, new ConditionData.Location(0, 5), 0, 1, coverageIdentifier2));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 2, new ConditionData.Location(0, 6), 1, 0, coverageIdentifier2));

    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 3, new ConditionData.Location(0, 5), 0, 1, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 3, new ConditionData.Location(0, 6), 1, 1, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 3, new ConditionData.Location(0, 5), 0, 1, coverageIdentifier2));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 3, new ConditionData.Location(0, 6), 1, 0, coverageIdentifier2));

    // ConditionData differ, coverage is reported forgivingly
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 4, new ConditionData.Location(0, 5), 0, 1, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 4, new ConditionData.Location(0, 6), 1, 0, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 4, new ConditionData.Location(0, 7), 0, 0, coverageIdentifier2));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 4, new ConditionData.Location(0, 8), 1, 1, coverageIdentifier2));

    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 5, new ConditionData.Location(0, 5), 0, 1, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 5, new ConditionData.Location(0, 6), 1, 0, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 5, new ConditionData.Location(0, 7), 0, 1, coverageIdentifier2));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 5, new ConditionData.Location(0, 8), 1, 0, coverageIdentifier2));

    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 6, new ConditionData.Location(0, 5), 0, 1, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 6, new ConditionData.Location(0, 6), 1, 1, coverageIdentifier1));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 6, new ConditionData.Location(0, 7), 0, 1, coverageIdentifier2));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER,filePath, 6, new ConditionData.Location(0, 8), 1, 0, coverageIdentifier2));

    assertThat(sut.getBranchCoverage(filePath))
      .containsExactlyInAnyOrder(
        new BranchCoverage(1, 2, 2),
        new BranchCoverage(2, 2, 1),
        new BranchCoverage(3, 2, 2),
        new BranchCoverage(4, 2, 2),
        new BranchCoverage(5, 2, 2),
        new BranchCoverage(6, 2, 2)
      );
  }

  @Test
  public void givenTwoCoberturaReports_getBranchCoverage_mergesCorrectly() {
    final String filePath = "filePath";
    final String report1 = "coberturaReport1";
    final String report2 = "coberturaReport2";
    Coverage sut = new Coverage();

    // Report 1: line 1 has 2 branches, first covered
    sut.add(new ConditionData(ConditionData.FORMAT_COBERTURA, filePath, 1, new ConditionData.Location(1, 0), 0, 1, report1));
    sut.add(new ConditionData(ConditionData.FORMAT_COBERTURA, filePath, 1, new ConditionData.Location(1, 0), 1, 0, report1));

    // Report 2: line 1 has 2 branches, second covered
    sut.add(new ConditionData(ConditionData.FORMAT_COBERTURA, filePath, 1, new ConditionData.Location(1, 0), 0, 0, report2));
    sut.add(new ConditionData(ConditionData.FORMAT_COBERTURA, filePath, 1, new ConditionData.Location(1, 0), 1, 1, report2));

    // Same format → merged into one group. 2 unique keys, both covered across reports.
    assertThat(sut.getBranchCoverage(filePath))
      .containsExactlyInAnyOrder(new BranchCoverage(1, 2, 2));
  }

  @Test
  public void givenTwoUnmergeableReports_getBranchCoverage_takesMaxAcrossFormats() {
    final String filePath = "filePath";
    final String report1 = "report1";
    final String report2 = "report2";
    Coverage sut = new Coverage();

    // Report 1 (unmergeable): line 1 has 3 branches, 2 covered
    sut.add(new ConditionData(ConditionData.FORMAT_UNMERGEABLE, filePath, 1, new ConditionData.Location(0, 0), 0, 1, report1));
    sut.add(new ConditionData(ConditionData.FORMAT_UNMERGEABLE, filePath, 1, new ConditionData.Location(1, 0), 1, 1, report1));
    sut.add(new ConditionData(ConditionData.FORMAT_UNMERGEABLE, filePath, 1, new ConditionData.Location(2, 0), 2, 0, report1));

    // Report 2 (unmergeable): line 1 has 3 branches, 1 covered
    sut.add(new ConditionData(ConditionData.FORMAT_UNMERGEABLE, filePath, 1, new ConditionData.Location(0, 0), 0, 0, report2));
    sut.add(new ConditionData(ConditionData.FORMAT_UNMERGEABLE, filePath, 1, new ConditionData.Location(1, 0), 1, 1, report2));
    sut.add(new ConditionData(ConditionData.FORMAT_UNMERGEABLE, filePath, 1, new ConditionData.Location(2, 0), 2, 0, report2));

    // Same format "unmergeable" → single group. Max coverageIdentifier group has 3 branches, 2 covered across reports.
    assertThat(sut.getBranchCoverage(filePath))
      .containsExactlyInAnyOrder(new BranchCoverage(1, 3, 2));
  }

  @Test
  public void givenOpenCoverUnmergeableAndCobertura_getBranchCoverage_takesMaxAcrossFormats() {
    final String filePath = "filePath";
    final String openCoverReport = "openCoverReport";
    final String unmergeableReport = "unmergeableReport";
    final String coberturaReport = "coberturaReport";

    Coverage sut = new Coverage();

    // OpenCover: line 1 has 2 branches, 1 covered
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER, filePath, 1, new ConditionData.Location(1, 2), 0, 1, openCoverReport));
    sut.add(new ConditionData(ConditionData.FORMAT_OPENCOVER, filePath, 1, new ConditionData.Location(4, 6), 1, 0, openCoverReport));

    // Unmergeable: line 1 has 4 branches, 2 covered
    sut.add(new ConditionData(ConditionData.FORMAT_UNMERGEABLE, filePath, 1, new ConditionData.Location(0, 0), 0, 1, unmergeableReport));
    sut.add(new ConditionData(ConditionData.FORMAT_UNMERGEABLE, filePath, 1, new ConditionData.Location(1, 0), 1, 1, unmergeableReport));
    sut.add(new ConditionData(ConditionData.FORMAT_UNMERGEABLE, filePath, 1, new ConditionData.Location(2, 0), 2, 0, unmergeableReport));
    sut.add(new ConditionData(ConditionData.FORMAT_UNMERGEABLE, filePath, 1, new ConditionData.Location(3, 0), 3, 0, unmergeableReport));

    // Cobertura: line 1 has 3 branches, 3 covered
    sut.add(new ConditionData(ConditionData.FORMAT_COBERTURA, filePath, 1, new ConditionData.Location(1, 0), 0, 1, coberturaReport));
    sut.add(new ConditionData(ConditionData.FORMAT_COBERTURA, filePath, 1, new ConditionData.Location(1, 0), 1, 1, coberturaReport));
    sut.add(new ConditionData(ConditionData.FORMAT_COBERTURA, filePath, 1, new ConditionData.Location(1, 0), 2, 1, coberturaReport));

    // Three formats → three groups. OpenCover: (2, 1), Unmergeable: (4, 2), Cobertura: (3, 3). Max: (4, 3).
    assertThat(sut.getBranchCoverage(filePath))
      .containsExactlyInAnyOrder(new BranchCoverage(1, 4, 3));
  }
}
