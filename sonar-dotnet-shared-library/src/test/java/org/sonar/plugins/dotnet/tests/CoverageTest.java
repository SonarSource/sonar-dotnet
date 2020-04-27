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

import com.google.common.collect.ImmutableMap;
import org.junit.Test;

import static org.assertj.core.api.Assertions.assertThat;

public class CoverageTest {

  @Test
  public void test() {
    Coverage coverage = new Coverage();
    assertThat(coverage.files()).isEmpty();
    assertThat(coverage.hits("foo.txt")).isEmpty();

    coverage.addHits("foo.txt", 42, 1);
    assertThat(coverage.files()).containsOnly("foo.txt");
    assertThat(coverage.hits("foo.txt")).isEqualTo(ImmutableMap.of(42, 1));

    coverage.addHits("foo.txt", 42, 3);
    assertThat(coverage.files()).containsOnly("foo.txt");
    assertThat(coverage.hits("foo.txt")).isEqualTo(ImmutableMap.of(42, 4));

    coverage.addHits("foo.txt", 1234, 11);
    assertThat(coverage.files()).containsOnly("foo.txt");
    assertThat(coverage.hits("foo.txt")).isEqualTo(ImmutableMap.of(42, 4, 1234, 11));

    coverage.addHits("bar.txt", 1, 2);
    assertThat(coverage.files()).containsOnly("foo.txt", "bar.txt");
    assertThat(coverage.hits("foo.txt")).isEqualTo(ImmutableMap.of(42, 4, 1234, 11));
    assertThat(coverage.hits("bar.txt")).isEqualTo(ImmutableMap.of(1, 2));

    Coverage other = new Coverage();

    coverage.mergeWith(other);
    assertThat(coverage.files()).containsOnly("foo.txt", "bar.txt");
    assertThat(coverage.hits("foo.txt")).isEqualTo(ImmutableMap.of(42, 4, 1234, 11));
    assertThat(coverage.hits("bar.txt")).isEqualTo(ImmutableMap.of(1, 2));

    other.addHits("baz.txt", 2, 7);
    assertThat(other.files()).containsOnly("baz.txt");
    assertThat(other.hits("baz.txt")).isEqualTo(ImmutableMap.of(2, 7));

    coverage.mergeWith(other);
    assertThat(other.files()).containsOnly("baz.txt");
    assertThat(other.hits("baz.txt")).isEqualTo(ImmutableMap.of(2, 7));
    assertThat(coverage.files()).containsOnly("foo.txt", "bar.txt", "baz.txt");
    assertThat(coverage.hits("foo.txt")).isEqualTo(ImmutableMap.of(42, 4, 1234, 11));
    assertThat(coverage.hits("bar.txt")).isEqualTo(ImmutableMap.of(1, 2));
    assertThat(coverage.hits("baz.txt")).isEqualTo(ImmutableMap.of(2, 7));
  }

  @Test
  public void testBranchCoverageBySequencePoint(){
    Coverage first = new Coverage();

    BranchCoverage fooFirstLine = new BranchCoverage(1, 6, 3);
    BranchCoverage fooThirdLine = new BranchCoverage(3, 2, 1);
    BranchCoverage barFirstLine = new BranchCoverage(1, 5, 1);
    BranchCoverage barSecondLine = new BranchCoverage(2, 2, 2);
    BranchCoverage barSecondLineOther = new BranchCoverage(2, 2, 1);

    first.addBranchCoverage("foo.txt", fooFirstLine);
    first.addBranchCoverage("foo.txt", fooThirdLine);
    first.addBranchCoverage("bar.txt", barFirstLine);

    assertThat(first.getBranchCoverageBySequencePoints("foo.txt")).containsExactly(fooFirstLine, fooThirdLine);
    assertThat(first.getBranchCoverageBySequencePoints("bar.txt")).containsExactly(barFirstLine);

    Coverage second = new Coverage();

    second.addBranchCoverage("bar.txt", barSecondLine);
    second.addBranchCoverage("bar.txt", barSecondLineOther);

    second.mergeWith(first);

    assertThat(second.getBranchCoverageBySequencePoints("foo.txt")).containsExactly(fooFirstLine, fooThirdLine);
    assertThat(second.getBranchCoverageBySequencePoints("bar.txt")).containsExactly(new BranchCoverage(2, 4, 3), barFirstLine);
  }

  @Test
  public void testBranchCoverageWithMultipleEntriesOnTheSameLine(){
    final String fileName = "fileName";
    Coverage coverage = new Coverage();

    coverage.addBranchCoverage(fileName, new BranchCoverage(1, 2, 1));
    assertThat(coverage.getBranchCoverageBySequencePoints(fileName)).containsExactly(new BranchCoverage(1, 2, 1));

    coverage.addBranchCoverage(fileName, new BranchCoverage(3, 2, 1));
    assertThat(coverage.getBranchCoverageBySequencePoints(fileName)).containsExactly(
      new BranchCoverage(1, 2, 1),
      new BranchCoverage(3, 2, 1));

    coverage.addBranchCoverage(fileName, new BranchCoverage(3, 3, 2));
    assertThat(coverage.getBranchCoverageBySequencePoints(fileName)).containsExactly(
      new BranchCoverage(1, 2, 1),
      new BranchCoverage(3, 5, 3));

    coverage.addBranchCoverage(fileName, new BranchCoverage(3, 4, 4));
    assertThat(coverage.getBranchCoverageBySequencePoints(fileName)).containsExactly(
      new BranchCoverage(1, 2, 1),
      new BranchCoverage(3, 9, 7));
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
    sut.add(new BranchPoint(filePath, 1, 2, 3, 4));

    // Normally this case should not happen but if we have only one branch point
    // we should not report coverage as line coverage is already covering that.
    assertThat(sut.getBranchCoverage(filePath)).isEmpty();
  }

  @Test
  public void givenSingleBranchPointPerFile_getBranchCoverage_returnsEmpty() {
    final String firstPath = "firstPath";
    final String secondPath = "secondPath";

    Coverage sut = new Coverage();
    sut.add(new BranchPoint(firstPath, 1, 2, 3, 4));
    sut.add(new BranchPoint(secondPath, 1, 5, 6, 7));

    // Normally this case should not happen but if we have only one branch point
    // we should not report coverage as line coverage is already covering that.
    assertThat(sut.getBranchCoverage(firstPath)).isEmpty();
    assertThat(sut.getBranchCoverage(secondPath)).isEmpty();
  }

  @Test
  public void givenSingleBranchPointPerLine_getBranchCoverage_returnsEmpty() {
    final String filePath = "filePath";

    Coverage sut = new Coverage();
    sut.add(new BranchPoint(filePath, 1, 2, 3, 4));
    sut.add(new BranchPoint(filePath, 2, 5, 6, 7));

    assertThat(sut.getBranchCoverage(filePath)).isEmpty();
  }

  @Test
  public void givenMultipleBranchPointsPerLine_getBranchCoverage_returnsBranchCoverage() {
    final String filePath = "filePath";

    Coverage sut = new Coverage();
    // Both branch points covered
    sut.add(new BranchPoint(filePath, 1, 1, 3, 2));
    sut.add(new BranchPoint(filePath, 1, 4, 6, 1));

    // Only 2 out of 3 branch points covered
    sut.add(new BranchPoint(filePath, 2, 1, 3, 2));
    sut.add(new BranchPoint(filePath, 2, 4, 6, 0));
    sut.add(new BranchPoint(filePath, 2, 6, 8, 4));

    // No branch points covered
    sut.add(new BranchPoint(filePath, 3, 1, 3, 0));
    sut.add(new BranchPoint(filePath, 3, 4, 6, 0));

    // Same branch points appear multiple times, none covered (when tests are split in multiple test projects)
    sut.add(new BranchPoint(filePath, 4, 1, 3, 0));
    sut.add(new BranchPoint(filePath, 4, 4, 6, 0));
    sut.add(new BranchPoint(filePath, 4, 1, 3, 0));
    sut.add(new BranchPoint(filePath, 4, 4, 6, 0));

    // Same branch points appear multiple times, same coverage (when tests are split in multiple test projects)
    sut.add(new BranchPoint(filePath, 5, 1, 3, 1));
    sut.add(new BranchPoint(filePath, 5, 4, 6, 0));
    sut.add(new BranchPoint(filePath, 5, 1, 3, 1));
    sut.add(new BranchPoint(filePath, 5, 4, 6, 0));

    // Same branch points appear multiple times, different coverage (when tests are split in multiple test projects)
    sut.add(new BranchPoint(filePath, 6, 1, 3, 1));
    sut.add(new BranchPoint(filePath, 6, 4, 6, 0));
    sut.add(new BranchPoint(filePath, 6, 1, 3, 0));
    sut.add(new BranchPoint(filePath, 6, 4, 6, 1));

    assertThat(sut.getBranchCoverage(filePath))
      .hasSize(6)
      .containsOnly(
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

    Coverage sut = new Coverage();
    sut.add(new BranchPoint(firstPath, 1, 1, 3, 2));
    sut.add(new BranchPoint(firstPath, 1, 4, 6, 1));

    sut.add(new BranchPoint(secondPath, 1, 5, 8, 2));
    sut.add(new BranchPoint(secondPath, 1, 10, 12, 0));
    sut.add(new BranchPoint(secondPath, 1, 12, 14, 0));

    assertThat(sut.getBranchCoverage(firstPath)).containsOnly(new BranchCoverage(1, 2, 2));
    assertThat(sut.getBranchCoverage(secondPath)).containsOnly(new BranchCoverage(1, 3, 1));
  }
}
