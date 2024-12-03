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
package org.sonar.plugins.dotnet.tests.coverage;

import org.junit.Test;
import org.mockito.Mockito;

import java.io.File;

import static org.mockito.Mockito.*;

public class CoverageCacheTest {

  @Test
  public void test() {
    CoverageCache cache = new CoverageCache();
    CoverageParser parser = mock(CoverageParser.class);
    File reportFile = mock(File.class);
    when(reportFile.getAbsolutePath()).thenReturn("foo.txt");

    Coverage coverage = cache.readCoverageFromCacheOrParse(parser, reportFile);
    verify(parser, Mockito.times(1)).accept(reportFile, coverage);

    cache.readCoverageFromCacheOrParse(parser, reportFile);
    verify(parser, Mockito.times(1)).accept(Mockito.eq(reportFile), Mockito.any(Coverage.class));
  }

}
