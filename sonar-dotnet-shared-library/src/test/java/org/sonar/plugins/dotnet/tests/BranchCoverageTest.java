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

import java.util.Objects;

import static org.assertj.core.api.Assertions.assertThat;

public class BranchCoverageTest {
  @Test
  public void givenSameObjectEqualsReturnsTrue(){
    BranchCoverage coverage = new BranchCoverage(3, 2, 1);
    assertThat(coverage.equals(coverage)).isTrue();
  }

  @Test
  public void givenNullEqualsReturnsFalse(){
    assertThat(new BranchCoverage(3, 2, 1).equals(null)).isFalse();
  }

  @Test
  public void givenDifferentClassEqualsReturnsFalse(){
    assertThat(new BranchCoverage(3, 2, 1).equals("1")).isFalse();
  }

  @Test
  public void givenDifferentLineEqualsReturnsFalse(){
    assertThat(new BranchCoverage(3, 2, 1).equals(new BranchCoverage(2, 2, 1))).isFalse();
  }

  @Test
  public void givenDifferentConditionsEqualsReturnsFalse(){
    assertThat(new BranchCoverage(3, 2, 1).equals(new BranchCoverage(3, 4, 1))).isFalse();
  }

  @Test
  public void givenDifferentCoveredConditionsEqualsReturnsFalse(){
    assertThat(new BranchCoverage(3, 2, 1).equals(new BranchCoverage(3, 2, 0))).isFalse();
  }

  @Test
  public void givenEqualBranchCoverageEqualsReturnsTrue(){
    assertThat(new BranchCoverage(3, 2, 1).equals(new BranchCoverage(3, 2, 1))).isTrue();
  }

  @Test
  public void givenLineConditionsAndCoveredConditionsHashCodeConsidersAll(){
    assertThat(new BranchCoverage(3, 2, 1).hashCode()).isEqualTo(Objects.hash(3, 2, 1));
  }

  @Test
  public void toStringTest() {
    assertThat(new BranchCoverage(3, 2, 1).toString())
      .isEqualTo("Branch coverage [line=3, conditions=2, coveredConditions=1]");
  }
}
