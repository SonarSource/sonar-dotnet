/*
 * SonarQube .NET Tests Library
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
package org.sonar.plugins.dotnet.tests;

import java.io.File;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.mockito.Mockito;

import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;

public class DotCoverReportsAggregatorTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

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
  }

}
