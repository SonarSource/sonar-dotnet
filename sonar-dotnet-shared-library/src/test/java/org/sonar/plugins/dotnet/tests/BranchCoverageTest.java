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

import java.util.Objects;
import org.junit.Test;

import static org.assertj.core.api.Assertions.assertThat;

public class BranchCoverageTest {
  @Test
  public void givenSameObjectEqualsReturnsTrue() {
    BranchCoverage coverage = new BranchCoverage(3, 2, 1);
    assertThat(coverage).isEqualTo(coverage);
  }

  @Test
  public void givenNullEqualsReturnsFalse() {
    assertThat(new BranchCoverage(3, 2, 1)).isNotEqualTo(null);
  }

  @Test
  public void givenDifferentClassEqualsReturnsFalse() {
    assertThat(new BranchCoverage(3, 2, 1)).isNotEqualTo("1");
  }

  @Test
  public void givenDifferentLineEqualsReturnsFalse() {
    assertThat(new BranchCoverage(3, 2, 1)).isNotEqualTo(new BranchCoverage(2, 2, 1));
  }

  @Test
  public void givenDifferentConditionsEqualsReturnsFalse() {
    assertThat(new BranchCoverage(3, 2, 1)).isNotEqualTo(new BranchCoverage(3, 4, 1));
  }

  @Test
  public void givenDifferentCoveredConditionsEqualsReturnsFalse() {
    assertThat(new BranchCoverage(3, 2, 1)).isNotEqualTo(new BranchCoverage(3, 2, 0));
  }

  @Test
  public void givenEqualBranchCoverageEqualsReturnsTrue() {
    assertThat(new BranchCoverage(3, 2, 1)).isEqualTo(new BranchCoverage(3, 2, 1));
  }

  @Test
  public void givenLineConditionsAndCoveredConditionsHashCodeConsidersAll() {
    assertThat(new BranchCoverage(3, 2, 1).hashCode()).isEqualTo(Objects.hash(3, 2, 1));
  }

  @Test
  public void toStringTest() {
    assertThat(new BranchCoverage(3, 2, 1)).hasToString("Branch coverage [line=3, conditions=2, coveredConditions=1]");
  }
}
