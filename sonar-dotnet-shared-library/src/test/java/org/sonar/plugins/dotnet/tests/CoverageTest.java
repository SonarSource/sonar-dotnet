/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2019 SonarSource SA
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

}
