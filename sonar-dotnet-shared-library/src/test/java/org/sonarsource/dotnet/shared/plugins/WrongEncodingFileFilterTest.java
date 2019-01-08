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
package org.sonarsource.dotnet.shared.plugins;

import org.junit.Before;
import org.junit.Test;
import org.sonar.api.batch.fs.InputFile;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class WrongEncodingFileFilterTest {
  private WrongEncodingFileFilter filter;
  private EncodingPerFile encodingPerFile;

  @Before
  public void setUp() {
    encodingPerFile = mock(EncodingPerFile.class);
    filter = new WrongEncodingFileFilter(encodingPerFile);
  }

  @Test
  public void should_exclude_files_with_mismatching_encoding() {
    InputFile file = mock(InputFile.class);
    when(encodingPerFile.encodingMatch(file)).thenReturn(true);
    assertThat(filter.accept(file)).isTrue();
  }

  @Test
  public void should_accept_files_with_matching_encoding() {
    InputFile file = mock(InputFile.class);
    when(encodingPerFile.encodingMatch(file)).thenReturn(false);
    assertThat(filter.accept(file)).isFalse();
  }

}
