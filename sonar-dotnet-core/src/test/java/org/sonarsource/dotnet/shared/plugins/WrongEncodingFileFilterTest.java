/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonarsource.dotnet.shared.plugins;

import org.junit.Before;
import org.junit.Test;
import org.sonar.api.batch.fs.InputFile;
import org.sonarsource.dotnet.shared.plugins.filters.WrongEncodingFileFilter;

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
