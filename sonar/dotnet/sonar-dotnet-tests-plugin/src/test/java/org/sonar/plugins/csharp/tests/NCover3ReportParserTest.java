/*
 * Sonar .NET Plugin :: Tests
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
package org.sonar.plugins.csharp.tests;

import org.fest.assertions.MapAssert;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;

import java.io.File;

import static org.fest.assertions.Assertions.assertThat;

public class NCover3ReportParserTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Test
  public void invalid_root() {
    thrown.expect(RuntimeException.class);
    thrown.expectMessage("<coverage>");
    new NCover3ReportParser(new File("src/test/resources/ncover3/invalid_root.nccov")).coverage();
  }

  @Test
  public void wrong_version() {
    thrown.expect(RuntimeException.class);
    thrown.expectMessage("exportversion");
    new NCover3ReportParser(new File("src/test/resources/ncover3/wrong_version.nccov")).coverage();
  }

  @Test
  public void no_version() {
    thrown.expect(RuntimeException.class);
    thrown.expectMessage("exportversion");
    new NCover3ReportParser(new File("src/test/resources/ncover3/no_version.nccov")).coverage();
  }

  @Test
  public void non_existing_file() {
    thrown.expect(RuntimeException.class);
    thrown.expectMessage("non_existing_file");
    new NCover3ReportParser(new File("src/test/resources/ncover3/non_existing_file.nccov")).coverage();
  }

  @Test
  public void valid() {
    Coverage coverage = new NCover3ReportParser(new File("src/test/resources/ncover3/valid.nccov")).coverage();
    assertThat(coverage.files()).containsOnly(
      "C:\\CSharpPlayground\\MyLibrary\\Adder.cs",
      "C:\\CSharpPlayground\\MyLibraryNUnitTest\\AdderNUnitTest.cs",
      "C:\\CSharpPlayground\\MyLibraryTest\\AdderTest.cs",
      "C:\\CSharpPlayground\\MyLibraryXUnitTest\\AdderXUnitTest.cs");

    assertThat(coverage.hits("C:\\CSharpPlayground\\MyLibrary\\Adder.cs"))
      .hasSize(11)
      .includes(
        MapAssert.entry(12, 2),
        MapAssert.entry(14, 0),
        MapAssert.entry(15, 0),
        MapAssert.entry(18, 2),
        MapAssert.entry(22, 4),
        MapAssert.entry(26, 2),
        MapAssert.entry(27, 2),
        MapAssert.entry(31, 4),
        MapAssert.entry(32, 4),
        MapAssert.entry(36, 2),
        MapAssert.entry(37, 2));
  }

}
