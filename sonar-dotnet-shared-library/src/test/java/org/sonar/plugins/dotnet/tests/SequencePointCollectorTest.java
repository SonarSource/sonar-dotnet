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

import org.junit.Test;
import org.mockito.Mockito;

import static org.mockito.Mockito.*;

public class SequencePointCollectorTest {
  @Test
  public void givenEmptyListOfPoints_publishCoverage_doesNotAddBranchCoverage() {
    Coverage coverage = mock(Coverage.class);

    SequencePointCollector sut = new SequencePointCollector();
    sut.publishCoverage(coverage);

    verify(coverage, never()).addBranchCoverage(anyString(), any());
  }

  @Test
  public void givenSinglePoint_publishCoverage_doesNotAddBranchCoverage() {
    Coverage coverage = mock(Coverage.class);

    SequencePointCollector sut = new SequencePointCollector();
    sut.add(new SequencePoint("file\\path", 1 , 1, 2));
    sut.publishCoverage(coverage);

    verify(coverage, never()).addBranchCoverage(anyString(), any());
  }

  @Test
  public void givenSinglePointPerFile_publishCoverage_doesNotAddBranchCoverage() {
    Coverage coverage = mock(Coverage.class);

    SequencePointCollector sut = new SequencePointCollector();
    sut.add(new SequencePoint("first file", 1 , 1, 2));
    sut.add(new SequencePoint("second file", 1 , 1, 2));
    sut.publishCoverage(coverage);

    verify(coverage, never()).addBranchCoverage(anyString(), any());
  }

  @Test
  public void givenSinglePointPerLine_publishCoverage_doesNotAddBranchCoverage() {
    Coverage coverage = mock(Coverage.class);
    String filePath = "file path";

    SequencePointCollector sut = new SequencePointCollector();
    sut.add(new SequencePoint(filePath, 1 , 1, 2));
    sut.add(new SequencePoint(filePath, 2 , 1, 1));
    sut.publishCoverage(coverage);

    verify(coverage, never()).addBranchCoverage(anyString(), any());
  }

  @Test
  public void givenMultiplePointsPerLine_publishCoverage_addsBranchCoverage() {
    Coverage coverage = mock(Coverage.class);
    String filePath = "file path";

    SequencePointCollector sut = new SequencePointCollector();
    sut.add(new SequencePoint(filePath, 1 , 1, 2));
    sut.add(new SequencePoint(filePath, 1 , 1, 1));

    sut.add(new SequencePoint(filePath, 2 , 2, 2));
    sut.add(new SequencePoint(filePath, 2 , 2, 0));
    sut.add(new SequencePoint(filePath, 2 , 2, 4));

    sut.add(new SequencePoint(filePath, 3 , 3, 0));
    sut.add(new SequencePoint(filePath, 3 , 3, 0));
    sut.publishCoverage(coverage);

    verify(coverage, Mockito.times(1))
      .addBranchCoverage(eq(filePath), argThat(branchCoverage -> IsMatch(branchCoverage, 1 , 2, 2)));

    verify(coverage, Mockito.times(1))
      .addBranchCoverage(eq(filePath), argThat(branchCoverage -> IsMatch(branchCoverage, 2 , 3, 2)));

    verify(coverage, Mockito.times(1))
      .addBranchCoverage(eq(filePath), argThat(branchCoverage -> IsMatch(branchCoverage, 3 , 2, 0)));
  }

  @Test
  public void givenMultiplePointsPerLineInDifferentFiles_publishCoverage_addsBranchCoverage() {
    Coverage coverage = mock(Coverage.class);
    String firstPath = "first";
    String secondPath = "second";

    SequencePointCollector sut = new SequencePointCollector();
    sut.add(new SequencePoint(firstPath, 1 , 1, 2));
    sut.add(new SequencePoint(firstPath, 1 , 1, 1));

    sut.add(new SequencePoint(secondPath, 1 , 1, 1));
    sut.add(new SequencePoint(secondPath, 1 , 1, 0));
    sut.publishCoverage(coverage);

    verify(coverage, Mockito.times(1))
      .addBranchCoverage(eq(firstPath), argThat(branchCoverage -> IsMatch(branchCoverage, 1 , 2, 2)));

    verify(coverage, Mockito.times(1))
      .addBranchCoverage(eq(secondPath), argThat(branchCoverage -> IsMatch(branchCoverage, 1 , 2, 1)));
  }

  private Boolean IsMatch(BranchCoverage bc, int line, int conditions, int coveredConditions) {
    return bc.getLine() == line && bc.getConditions() == conditions && bc.getCoveredConditions() == coveredConditions;
  }
}
