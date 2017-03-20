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
import org.assertj.core.api.Assertions;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;

public class NCover3ReportParserTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Test
  public void invalid_root() {
    thrown.expect(RuntimeException.class);
    thrown.expectMessage("<coverage>");
    new NCover3ReportParser().accept(new File("src/test/resources/ncover3/invalid_root.nccov"), mock(Coverage.class));
  }

  @Test
  public void wrong_version() {
    thrown.expect(RuntimeException.class);
    thrown.expectMessage("exportversion");
    new NCover3ReportParser().accept(new File("src/test/resources/ncover3/wrong_version.nccov"), mock(Coverage.class));
  }

  @Test
  public void no_version() {
    thrown.expect(RuntimeException.class);
    thrown.expectMessage("exportversion");
    new NCover3ReportParser().accept(new File("src/test/resources/ncover3/no_version.nccov"), mock(Coverage.class));
  }

  @Test
  public void non_existing_file() {
    thrown.expect(RuntimeException.class);
    thrown.expectMessage("non_existing_file.nccov");
    new NCover3ReportParser().accept(new File("src/test/resources/ncover3/non_existing_file.nccov"), mock(Coverage.class));
  }

  @Test
  public void valid() throws Exception {
    Coverage coverage = new Coverage();
    new NCover3ReportParser().accept(new File("src/test/resources/ncover3/valid.nccov"), coverage);

    assertThat(coverage.files()).containsOnly(
      new File("MyLibrary\\Adder.cs").getCanonicalPath(),
      new File("MyLibraryNUnitTest\\AdderNUnitTest.cs").getCanonicalPath(),
      new File("MyLibraryTest\\AdderTest.cs").getCanonicalPath(),
      new File("MyLibraryXUnitTest\\AdderXUnitTest.cs").getCanonicalPath());

    assertThat(coverage.hits(new File("MyLibrary\\Adder.cs").getCanonicalPath()))
      .hasSize(11)
      .contains(
        Assertions.entry(12, 2),
        Assertions.entry(14, 0),
        Assertions.entry(15, 0),
        Assertions.entry(18, 2),
        Assertions.entry(22, 4),
        Assertions.entry(26, 2),
        Assertions.entry(27, 2),
        Assertions.entry(31, 4),
        Assertions.entry(32, 4),
        Assertions.entry(36, 2),
        Assertions.entry(37, 2));
  }

  @Test
  public void should_not_fail_with_invalid_path() {
    new NCover3ReportParser().accept(new File("src/test/resources/ncover3/invalid_path.nccov"), mock(Coverage.class));
  }

}
