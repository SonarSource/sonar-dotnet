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
import static org.junit.Assert.assertThat;

import org.junit.Test;

public class GallioExceptionTest {

  @Test
  public void testReturnCodes() throws Exception {
    assertThat(new GallioException(1).getMessage(), is("Gallio analysis failed: some tests failed."));
    assertThat(new GallioException(2).getMessage(), is("Gallio analysis failed: the tests were canceled."));
    assertThat(new GallioException(3).getMessage(),
        is("Gallio analysis failed: a fatal runtime exception occurred. Check the result file to potentially know more about it."));
    assertThat(
        new GallioException(10).getMessage(),
        is("Gallio analysis failed: invalid arguments were supplied on the command line. Please contact the support on 'user@sonar.codehaus.org'."));
  }

  @Test
  public void testOtherConstructors() throws Exception {
    assertThat(new GallioException("Foo").getMessage(), is("Foo"));
    assertThat(new GallioException("Foo", new Exception()).getMessage(), is("Foo"));
  }

}
