/*
 * .NET tools :: Gallio Runner
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package org.sonar.dotnet.tools.gallio;

import static org.hamcrest.Matchers.is;
import static org.hamcrest.Matchers.nullValue;
import static org.junit.Assert.assertThat;

import org.junit.Test;

public class CoverageToolTest {

  @Test
  public void testFindFromName() throws Exception {
    assertThat(CoverageTool.findFromName("PartCover"), is(CoverageTool.PARTCOVER));
  }

  @Test
  public void testFindFromWrongName() throws Exception {
    assertThat(CoverageTool.findFromName("UnexistingTool"), nullValue());
  }

  @Test
  public void testGetRunner() throws Exception {
    assertThat(CoverageTool.NCOVER.getGallioRunner(), is("NCover3"));
    assertThat(CoverageTool.PARTCOVER.getGallioRunner(), is("IsolatedAppDomain"));
  }

}
