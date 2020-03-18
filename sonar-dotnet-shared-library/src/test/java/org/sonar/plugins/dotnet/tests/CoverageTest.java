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
  public void testBranchCoverage(){
    Coverage first = new Coverage();

    BranchCoverage fooFirstLine = new BranchCoverage(1, 6, 3);
    BranchCoverage fooThirdLine = new BranchCoverage(3, 2, 1);
    BranchCoverage barFirstLine = new BranchCoverage(1, 5, 1);
    BranchCoverage barSecondLine = new BranchCoverage(2, 2, 2);

    first.addBranchCoverage("foo.txt", fooFirstLine);
    first.addBranchCoverage("foo.txt", fooThirdLine);
    first.addBranchCoverage("bar.txt", barFirstLine);

    assertThat(first.getBranchCoverage("foo.txt")).containsExactly(fooFirstLine, fooThirdLine);
    assertThat(first.getBranchCoverage("bar.txt")).containsExactly(barFirstLine);

    Coverage second = new Coverage();

    second.addBranchCoverage("bar.txt", barSecondLine);

    second.mergeWith(first);

    assertThat(second.getBranchCoverage("foo.txt")).containsExactly(fooFirstLine, fooThirdLine);
    assertThat(second.getBranchCoverage("bar.txt")).containsExactly(barSecondLine, barFirstLine);
  }
}
