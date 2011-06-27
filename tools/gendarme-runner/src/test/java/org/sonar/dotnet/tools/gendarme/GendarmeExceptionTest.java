/*
 * .NET tools :: Gendarme Runner
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
package org.sonar.dotnet.tools.gendarme;

import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;

import org.junit.Test;

public class GendarmeExceptionTest {

  @Test
  public void testReturnCodes() throws Exception {
    assertThat(new GendarmeException(2).getMessage(),
        is("Gendarme analysis failed: execution was interrupted by I/O errors (e.g. missing files)."));
    assertThat(new GendarmeException(3).getMessage(),
        is("Gendarme analysis failed: errors found in the (default or user supplied) configuration files."));
    assertThat(
        new GendarmeException(4).getMessage(),
        is("Gendarme analysis failed: execution was interrupted by a non-handled exception. This is likely a bug inside Gendarme and should be reported on Novell's bugzilla (http://bugzilla.novell.com) or on the mailing-list."));
  }

  @Test
  public void testOtherConstructors() throws Exception {
    assertThat(new GendarmeException("Foo").getMessage(), is("Foo"));
    assertThat(new GendarmeException("Foo", new Exception()).getMessage(), is("Foo"));
  }

}
