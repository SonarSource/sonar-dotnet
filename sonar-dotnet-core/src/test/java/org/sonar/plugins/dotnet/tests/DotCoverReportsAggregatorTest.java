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
package org.sonar.plugins.dotnet.tests;

import java.io.File;
import java.util.List;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.mockito.Mockito;
import org.sonar.api.testfixtures.log.LogTester;
import org.slf4j.event.Level;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;

public class DotCoverReportsAggregatorTest {

  @Rule
  public LogTester logTester = new LogTester();

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Before
  public void before() {
    logTester.setLevel(Level.DEBUG);
  }

  @Test
  public void no_sources() {
    thrown.expect(IllegalArgumentException.class);
    thrown.expectMessage("The following report dotCover report HTML sources folder cannot be found: ");
    thrown.expectMessage(new File("src/test/resources/dotcover_aggregator/no_sources/src").getAbsolutePath());
    new DotCoverReportsAggregator(mock(DotCoverReportParser.class)).accept(new File("src/test/resources/dotcover_aggregator/no_sources.html"), mock(Coverage.class));
  }

  @Test
  public void not_html() {
    thrown.expect(IllegalArgumentException.class);
    thrown.expectMessage("Only dotCover HTML reports which start with \"<!DOCTYPE html>\" are supported.");
    new DotCoverReportsAggregator(mock(DotCoverReportParser.class)).accept(new File("src/test/resources/dotcover_aggregator/not_html.html"), mock(Coverage.class));
  }

  @Test
  public void no_extension() {
    thrown.expect(IllegalArgumentException.class);
    thrown.expectMessage("The following dotCover report name should have an extension: no_extension");
    new DotCoverReportsAggregator(mock(DotCoverReportParser.class)).accept(new File("src/test/resources/dotcover_aggregator/no_extension"), mock(Coverage.class));
  }

  @Test
  public void empty_folder() {
    thrown.expect(IllegalArgumentException.class);
    thrown.expectMessage("No dotCover report HTML source file found under:");
    new DotCoverReportsAggregator(mock(DotCoverReportParser.class)).accept(new File("src/test/resources/dotcover_aggregator/empty_folder.html"), mock(Coverage.class));
  }

  @Test
  public void valid() {
    DotCoverReportParser parser = mock(DotCoverReportParser.class);

    Coverage coverage = new Coverage();
    new DotCoverReportsAggregator(parser).accept(new File("src/test/resources/dotcover_aggregator/foo.bar.html"), coverage);

    verify(parser).accept(new File("src/test/resources/dotcover_aggregator/foo.bar/src/1.html"), coverage);
    verify(parser).accept(new File("src/test/resources/dotcover_aggregator/foo.bar/src/2.html"), coverage);
    verify(parser, Mockito.never()).accept(new File("src/test/resources/dotcover_aggregator/foo.bar/src/nosource.html"), coverage);

    assertThat(logTester.logs(Level.INFO).get(0)).startsWith("Aggregating the HTML reports from ");
    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs.get(0)).startsWith("The current user dir is '");
    assertThat(debugLogs.get(1)).isEqualTo("dotCover aggregator: collected 3 report files to parse.");
  }

}
