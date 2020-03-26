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

import java.util.Objects;

// This class is responsible to keep SQ/SC metrics related to branch coverage
class BranchCoverage {
  private int line;
  private int conditions;
  private int coveredConditions;

  BranchCoverage(int line, int conditions, int coveredConditions){
    this.line = line;
    this.conditions = conditions;
    this.coveredConditions = coveredConditions;
  }

  public int getLine() {
    return line;
  }

  public int getConditions() {
    return conditions;
  }

  int getCoveredConditions() {
    return coveredConditions;
  }

  public void add(int conditions, int coveredConditions){
    this.conditions += conditions;
    this.coveredConditions += coveredConditions;
  }

  @Override
  public boolean equals(Object o) {
    if (this == o) {
      return true;
    }

    if (o == null || getClass() != o.getClass()) {
      return false;
    }

    BranchCoverage other = (BranchCoverage) o;

    return line == other.line &&
      conditions == other.conditions &&
      coveredConditions == other.coveredConditions;
  }

  @Override
  public int hashCode() {
    return Objects.hash(line, conditions, coveredConditions);
  }

  @Override
  public String toString() {
    return "Branch coverage [line=" + line + ", conditions=" + conditions
      + ", coveredConditions=" + coveredConditions + "]";
  }
}
