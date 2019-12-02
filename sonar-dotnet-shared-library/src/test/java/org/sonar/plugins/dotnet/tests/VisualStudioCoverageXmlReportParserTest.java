/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2019 SonarSource SA
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

import java.util.function.Predicate;
import org.assertj.core.api.Assertions;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;

import java.io.File;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;

public class VisualStudioCoverageXmlReportParserTest {

  @Rule
  public LogTester logTester = new LogTester();

  @Rule
  public ExpectedException thrown = ExpectedException.none();
  private Predicate<String> alwaysTrue = s -> true;

  @Test
  public void invalid_root() {
    thrown.expect(RuntimeException.class);
    thrown.expectMessage("<results>");
    new VisualStudioCoverageXmlReportParser(alwaysTrue).accept(new File("src/test/resources/visualstudio_coverage_xml/invalid_root.coveragexml"), mock(Coverage.class));
  }

  @Test
  public void non_existing_file() {
    thrown.expect(RuntimeException.class);
    thrown.expectMessage("non_existing_file.coveragexml");
    new VisualStudioCoverageXmlReportParser(alwaysTrue).accept(new File("src/test/resources/visualstudio_coverage_xml/non_existing_file.coveragexml"), mock(Coverage.class));
  }

  @Test
  public void wrong_covered() {
    thrown.expect(RuntimeException.class);
    thrown.expectMessage("Unsupported \"covered\" value \"foo\", expected one of \"yes\", \"partial\" or \"no\"");
    thrown.expectMessage("wrong_covered.coveragexml");
    thrown.expectMessage("line 40");
    new VisualStudioCoverageXmlReportParser(alwaysTrue).accept(new File("src/test/resources/visualstudio_coverage_xml/wrong_covered.coveragexml"), mock(Coverage.class));
  }

  @Test
  public void valid_with_correct_file_language() throws Exception {
    Coverage coverage = new Coverage();
    new VisualStudioCoverageXmlReportParser(alwaysTrue).accept(new File("src/test/resources/visualstudio_coverage_xml/valid.coveragexml"), coverage);

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

    assertThat(logTester.logs(LoggerLevel.INFO).get(0)).startsWith("Parsing the Visual Studio coverage XML report ");
    assertThat(logTester.logs(LoggerLevel.DEBUG).get(0)).startsWith("The current user dir is ");
    assertThat(logTester.logs(LoggerLevel.TRACE)).hasSize(3);
    assertThat(logTester.logs(LoggerLevel.TRACE).get(1))
      .startsWith("Found covered lines for id '0' for path ")
      .endsWith("\\MyLibrary\\Calc.cs'");
  }

  @Test
  public void valid_with_wrong_file_language() throws Exception {
    Coverage coverage = new Coverage();
    new VisualStudioCoverageXmlReportParser(s -> false).accept(new File("src/test/resources/visualstudio_coverage_xml/valid.coveragexml"), coverage);

    assertThat(coverage.files()).isEmpty();

    assertThat(coverage.hits(new File("MyLibrary\\Calc.cs").getCanonicalPath()))
      .hasSize(0);

    assertThat(logTester.logs(LoggerLevel.INFO).get(0)).startsWith("Parsing the Visual Studio coverage XML report ");
    assertThat(logTester.logs(LoggerLevel.DEBUG).get(0)).startsWith("The current user dir is ");
    assertThat(logTester.logs(LoggerLevel.DEBUG).get(1))
      .startsWith("Skipping file with path '")
      .endsWith("\\CalcMultiplyTest\\MultiplyTest.cs' because it is not indexed or does not have the supported language.");
  }

  @Test
  public void should_not_fail_with_invalid_path() {
    new VisualStudioCoverageXmlReportParser(alwaysTrue).accept(new File("src/test/resources/visualstudio_coverage_xml/invalid_path.coveragexml"), mock(Coverage.class));
    assertThat(logTester.logs(LoggerLevel.INFO).get(0)).startsWith("Parsing the Visual Studio coverage XML report ");
    assertThat(logTester.logs(LoggerLevel.DEBUG).get(0)).startsWith("The current user dir is ");
    assertThat(logTester.logs(LoggerLevel.WARN).get(0))
      .isEqualTo("Skipping the import of Visual Studio XML code coverage for the invalid file path: z:\\*\"?.cs at line 55");
  }

}
