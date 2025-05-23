/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2024 SonarSource SA
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
  public void givenEmptyListOfBranchPoints_getBranchCoverage_returnsEmpty() {
    Coverage sut = new Coverage();

    assertThat(sut.getBranchCoverage("fileName")).isEmpty();
  }

  @Test
  public void givenSingleBranchPoint_getBranchCoverage_returnsEmpty() {
    final String filePath = "filePath";

    Coverage sut = new Coverage();
    sut.add(new BranchPoint(filePath, 1, 2, 3, 4, 5, "coverageIdentifier"));

    // Normally this case should not happen but if we have only one branch point
    // we should not report coverage as line coverage is already covering that.
    assertThat(sut.getBranchCoverage(filePath)).isEmpty();
  }

  @Test
  public void givenSingleBranchPointPerFile_getBranchCoverage_returnsEmpty() {
    final String firstPath = "firstPath";
    final String secondPath = "secondPath";
    final String coverageIdentifier = "coverageIdentifier";

    Coverage sut = new Coverage();
    sut.add(new BranchPoint(firstPath, 1, 2, 3, 4, 5, coverageIdentifier));
    sut.add(new BranchPoint(secondPath, 1, 6, 7, 8, 9, coverageIdentifier));

    // Normally this case should not happen but if we have only one branch point
    // we should not report coverage as line coverage is already covering that.
    assertThat(sut.getBranchCoverage(firstPath)).isEmpty();
    assertThat(sut.getBranchCoverage(secondPath)).isEmpty();
  }

  @Test
  public void givenSingleBranchPointPerLine_getBranchCoverage_returnsEmpty() {
    final String filePath = "filePath";
    final String coverageIdentifier = "coverageIdentifier";

    Coverage sut = new Coverage();
    sut.add(new BranchPoint(filePath, 1, 2, 3, 4, 5, coverageIdentifier));
    sut.add(new BranchPoint(filePath, 2, 6, 7, 8, 9, coverageIdentifier));

    assertThat(sut.getBranchCoverage(filePath)).isEmpty();
  }

  @Test
  public void branchPointsMerging() {
    final String filePath = "filePath";
    final String coverageIdentifier = "coverageIdentifier";

    Coverage sut = new Coverage();

    // Identical branch points are merged
    sut.add(new BranchPoint(filePath, 1, 1, 2, 0, 1, coverageIdentifier));
    sut.add(new BranchPoint(filePath, 1, 1, 2, 0, 1, coverageIdentifier));

    // Branch points with different line are not aggregated
    sut.add(new BranchPoint(filePath, 2, 1, 2, 0, 1, coverageIdentifier));
    sut.add(new BranchPoint(filePath, 3, 1, 2, 0, 1, coverageIdentifier));

    // Branch points with different offset are correctly aggregated
    sut.add(new BranchPoint(filePath, 4, 1, 2, 0, 1, coverageIdentifier));
    sut.add(new BranchPoint(filePath, 4, 2, 2, 0, 1, coverageIdentifier));

    // Branch points with different offsetEnd are correctly aggregated
    sut.add(new BranchPoint(filePath, 5, 1, 2, 0, 1, coverageIdentifier));
    sut.add(new BranchPoint(filePath, 5, 1, 3, 0, 1, coverageIdentifier));

    // Branch points with different path are correctly aggregated
    sut.add(new BranchPoint(filePath, 6, 1, 2, 0, 1, coverageIdentifier));
    sut.add(new BranchPoint(filePath, 6, 1, 2, 1, 1, coverageIdentifier));

    assertThat(sut.getBranchCoverage(filePath))
      .containsExactlyInAnyOrder(
        // For the first 3 lines we will not report branch coverage since they have only one branch point each.
        new BranchCoverage(4, 2, 2),
        new BranchCoverage(5, 2, 2),
        new BranchCoverage(6, 2, 2)
      );
  }

  @Test
  public void givenMultipleBranchPointsPerLine_getBranchCoverage_returnsBranchCoverage() {
    final String filePath = "filePath";
    final String coverageIdentifier1 = "coverageIdentifier1";
    final String coverageIdentifier2 = "coverageIdentifier2";

    Coverage sut = new Coverage();
    // Both branch points covered
    sut.add(new BranchPoint(filePath, 1, 1, 3, 0, 1, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 1, 4, 6, 1, 1, coverageIdentifier1));

    // Only 2 out of 3 branch points covered
    sut.add(new BranchPoint(filePath, 2, 1, 3, 0, 2, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 2, 4, 6, 1, 0, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 2, 6, 8, 2, 4, coverageIdentifier1));

    // No branch points covered
    sut.add(new BranchPoint(filePath, 3, 1, 3, 0, 0, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 3, 4, 6, 1, 0, coverageIdentifier1));

    // Same branch points appear multiple times, none covered (when tests are split in multiple test projects)
    sut.add(new BranchPoint(filePath, 4, 1, 3, 0, 0, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 4, 4, 6, 1, 0, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 4, 1, 3, 0, 0, coverageIdentifier2));
    sut.add(new BranchPoint(filePath, 4, 4, 6, 1, 0, coverageIdentifier2));

    // Same branch points appear multiple times, same coverage (when tests are split in multiple test projects)
    sut.add(new BranchPoint(filePath, 5, 1, 3, 0, 1, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 5, 4, 6, 1, 0, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 5, 1, 3, 0, 1, coverageIdentifier2));
    sut.add(new BranchPoint(filePath, 5, 4, 6, 1, 0, coverageIdentifier2));

    // Same branch points appear multiple times, different coverage (when tests are split in multiple test projects)
    sut.add(new BranchPoint(filePath, 6, 1, 3, 0, 1, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 6, 4, 6, 1, 0, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 6, 1, 3, 0, 0, coverageIdentifier2));
    sut.add(new BranchPoint(filePath, 6, 4, 6, 1, 1, coverageIdentifier2));

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
  public void givenMultipleBranchPointsPerLineInDifferentFiles_getBranchCoverage_returnsBranchCoverage() {
    final String firstPath = "firstPath";
    final String secondPath = "secondPath";
    final String coverageIdentifier = "coverageIdentifier";

    Coverage sut = new Coverage();
    sut.add(new BranchPoint(firstPath, 1, 1, 3, 0, 2, coverageIdentifier));
    sut.add(new BranchPoint(firstPath, 1, 4, 6, 1, 1, coverageIdentifier));

    sut.add(new BranchPoint(secondPath, 1, 5, 8, 0, 2, coverageIdentifier));
    sut.add(new BranchPoint(secondPath, 1, 10, 12, 1, 0, coverageIdentifier));
    sut.add(new BranchPoint(secondPath, 1, 12, 14, 2, 0, coverageIdentifier));

    assertThat(sut.getBranchCoverage(firstPath)).containsExactlyInAnyOrder(new BranchCoverage(1, 2, 2));
    assertThat(sut.getBranchCoverage(secondPath)).containsExactlyInAnyOrder(new BranchCoverage(1, 3, 1));
  }

  @Test
  public void givenBranchPointsFromFromDifferentReports_getBranchCoverage_returnsBranchCoverage(){
    final String filePath = "filePath";
    final String coverageIdentifier1 = "coverageIdentifier1";
    final String coverageIdentifier2 = "coverageIdentifier2";

    Coverage sut = new Coverage();

    // BranchPoints match exactly, coverage is aggregated correctly
    sut.add(new BranchPoint(filePath, 1, 0, 5, 0, 1, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 1, 0, 6, 1, 0, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 1, 0, 5, 0, 0, coverageIdentifier2));
    sut.add(new BranchPoint(filePath, 1, 0, 6, 1, 1, coverageIdentifier2));

    sut.add(new BranchPoint(filePath, 2, 0, 5, 0, 1, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 2, 0, 6, 1, 0, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 2, 0, 5, 0, 1, coverageIdentifier2));
    sut.add(new BranchPoint(filePath, 2, 0, 6, 1, 0, coverageIdentifier2));

    sut.add(new BranchPoint(filePath, 3, 0, 5, 0, 1, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 3, 0, 6, 1, 1, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 3, 0, 5, 0, 1, coverageIdentifier2));
    sut.add(new BranchPoint(filePath, 3, 0, 6, 1, 0, coverageIdentifier2));

    // BranchPoints differ, coverage is reported forgivingly
    sut.add(new BranchPoint(filePath, 4, 0, 5, 0, 1, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 4, 0, 6, 1, 0, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 4, 0, 7, 0, 0, coverageIdentifier2));
    sut.add(new BranchPoint(filePath, 4, 0, 8, 1, 1, coverageIdentifier2));

    sut.add(new BranchPoint(filePath, 5, 0, 5, 0, 1, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 5, 0, 6, 1, 0, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 5, 0, 7, 0, 1, coverageIdentifier2));
    sut.add(new BranchPoint(filePath, 5, 0, 8, 1, 0, coverageIdentifier2));

    sut.add(new BranchPoint(filePath, 6, 0, 5, 0, 1, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 6, 0, 6, 1, 1, coverageIdentifier1));
    sut.add(new BranchPoint(filePath, 6, 0, 7, 0, 1, coverageIdentifier2));
    sut.add(new BranchPoint(filePath, 6, 0, 8, 1, 0, coverageIdentifier2));

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
}
