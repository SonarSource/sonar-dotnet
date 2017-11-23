/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2017 SonarSource SA
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

import java.nio.file.Paths;
import java.util.Optional;
import org.junit.Before;
import org.junit.Test;
import org.sonar.api.batch.fs.InputFile;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class WrongEncodingFileFilterTest {
  private WrongEncodingFileFilter filter;
  private EncodingPerFile encodingPerFile;
  private AbstractConfiguration config;

  @Before
  public void setUp() {
    encodingPerFile = mock(EncodingPerFile.class);
    config = mock(AbstractConfiguration.class);
    filter = new WrongEncodingFileFilter(encodingPerFile, config);
  }

  @Test
  public void should_exclude_files_with_mismatching_encoding() {
    when(config.protobufReportPathSilent()).thenReturn(Optional.of(Paths.get("report")));
    InputFile file = mock(InputFile.class);
    when(encodingPerFile.encodingMatch(file)).thenReturn(true);
    assertThat(filter.accept(file)).isTrue();
  }

  @Test
  public void should_accept_files_with_matching_encoding() {
    when(config.protobufReportPathSilent()).thenReturn(Optional.of(Paths.get("report")));
    InputFile file = mock(InputFile.class);
    when(encodingPerFile.encodingMatch(file)).thenReturn(false);
    assertThat(filter.accept(file)).isFalse();
  }

  @Test
  public void should_be_skipped_if_no_proto_file_found() {
    when(config.protobufReportPathSilent()).thenReturn(Optional.empty());
    assertThat(filter.accept(mock(InputFile.class))).isTrue();
  }

}
