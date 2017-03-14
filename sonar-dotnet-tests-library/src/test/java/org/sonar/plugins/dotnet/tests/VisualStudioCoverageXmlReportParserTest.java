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

public class VisualStudioCoverageXmlReportParserTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Test
  public void invalid_root() {
    thrown.expect(RuntimeException.class);
    thrown.expectMessage("<results>");
    new VisualStudioCoverageXmlReportParser().accept(new File("src/test/resources/visualstudio_coverage_xml/invalid_root.coveragexml"), mock(Coverage.class));
  }

  @Test
  public void non_existing_file() {
    thrown.expect(RuntimeException.class);
    thrown.expectMessage("non_existing_file.coveragexml");
    new VisualStudioCoverageXmlReportParser().accept(new File("src/test/resources/visualstudio_coverage_xml/non_existing_file.coveragexml"), mock(Coverage.class));
  }

  @Test
  public void wrong_covered() {
    thrown.expect(RuntimeException.class);
    thrown.expectMessage("Unsupported \"covered\" value \"foo\", expected one of \"yes\", \"partial\" or \"no\"");
    thrown.expectMessage("wrong_covered.coveragexml");
    thrown.expectMessage("line 40");
    new VisualStudioCoverageXmlReportParser().accept(new File("src/test/resources/visualstudio_coverage_xml/wrong_covered.coveragexml"), mock(Coverage.class));
  }

  @Test
  public void valid() throws Exception {
    Coverage coverage = new Coverage();
    new VisualStudioCoverageXmlReportParser().accept(new File("src/test/resources/visualstudio_coverage_xml/valid.coveragexml"), coverage);

    assertThat(coverage.files()).containsOnly(
      new File("CalcMultiplyTest\\MultiplyTest.cs").getCanonicalPath(),
      new File("MyLibrary\\Calc.cs").getCanonicalPath());

    assertThat(coverage.hits(new File("MyLibrary\\Calc.cs").getCanonicalPath()))
      .hasSize(16)
      .contains(
        Assertions.entry(12, 0),
        Assertions.entry(13, 0),
        Assertions.entry(14, 0),
        Assertions.entry(17, 1),
        Assertions.entry(18, 1),
        Assertions.entry(19, 1),
        Assertions.entry(22, 0),
        Assertions.entry(23, 0),
        Assertions.entry(24, 0),
        Assertions.entry(25, 0),
        Assertions.entry(26, 0),
        Assertions.entry(28, 0),
        Assertions.entry(29, 0),
        Assertions.entry(32, 0),
        Assertions.entry(33, 0),
        Assertions.entry(34, 0));
  }

  @Test
  public void should_not_fail_with_invalid_path() {
    new VisualStudioCoverageXmlReportParser().accept(new File("src/test/resources/visualstudio_coverage_xml/invalid_path.coveragexml"), mock(Coverage.class));
  }

}
