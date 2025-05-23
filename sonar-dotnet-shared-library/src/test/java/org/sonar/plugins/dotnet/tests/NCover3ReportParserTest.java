/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2024 SonarSource SA
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
import java.util.List;
import org.assertj.core.api.Assertions;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.sonar.api.notifications.AnalysisWarnings;
import org.sonar.api.testfixtures.log.LogTester;
import org.slf4j.event.Level;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

public class NCover3ReportParserTest {

  @Rule
  public LogTester logTester = new LogTester();

  @Rule
  public ExpectedException thrown = ExpectedException.none();
  private FileService alwaysTrue;
  private FileService alwaysFalse;

  @Before
  public void prepare() {
    logTester.setLevel(Level.TRACE);
    alwaysTrue = mock(FileService.class);
    when(alwaysTrue.isSupportedAbsolute(anyString())).thenReturn(true);
    when(alwaysTrue.getAbsolutePath(anyString())).thenThrow(new UnsupportedOperationException("Should not call this"));
    alwaysFalse = mock(FileService.class);
    when(alwaysFalse.isSupportedAbsolute(anyString())).thenReturn(false);
    when(alwaysFalse.getAbsolutePath(anyString())).thenThrow(new UnsupportedOperationException("Should not call this"));
  }

  private String deprecationMessage = "NCover3 coverage import is deprecated since version 8.6 of the plugin. " +
    "Consider using a different code coverage tool instead.";

  @Test
  public void invalid_root() {
    thrown.expect(RuntimeException.class);
    thrown.expectMessage("<coverage>");
    new NCover3ReportParser(alwaysTrue, mock(AnalysisWarnings.class)).accept(new File("src/test/resources/ncover3/invalid_root.nccov"), mock(Coverage.class));
  }

  @Test
  public void wrong_version() {
    thrown.expect(RuntimeException.class);
    thrown.expectMessage("exportversion");
    new NCover3ReportParser(alwaysTrue, mock(AnalysisWarnings.class)).accept(new File("src/test/resources/ncover3/wrong_version.nccov"), mock(Coverage.class));
  }

  @Test
  public void no_version() {
    thrown.expect(RuntimeException.class);
    thrown.expectMessage("exportversion");
    new NCover3ReportParser(alwaysTrue, mock(AnalysisWarnings.class)).accept(new File("src/test/resources/ncover3/no_version.nccov"), mock(Coverage.class));
  }

  @Test
  public void non_existing_file() {
    thrown.expect(RuntimeException.class);
    thrown.expectMessage("non_existing_file.nccov");
    new NCover3ReportParser(alwaysTrue, mock(AnalysisWarnings.class)).accept(new File("src/test/resources/ncover3/non_existing_file.nccov"), mock(Coverage.class));
  }

  @Test
  public void valid() throws Exception {
    AnalysisWarnings analysisWarnings = mock(AnalysisWarnings.class);
    Coverage coverage = new Coverage();
    new NCover3ReportParser(alwaysTrue, analysisWarnings).accept(new File("src/test/resources/ncover3/valid.nccov"), coverage);

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

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs.get(0)).startsWith("The current user dir is '");
    List<String> traceLogs = logTester.logs(Level.TRACE);
    assertThat(traceLogs.get(0)).isEqualTo("Analyzing the doc tag with NCover3 ID '1' and url 'MyLibrary\\Adder.cs'.");
    assertThat(traceLogs.get(1))
      .startsWith("NCover3 ID '1' with url 'MyLibrary\\Adder.cs' is resolved as '")
      .endsWith("MyLibrary\\Adder.cs'.");

    assertThat(logTester.logs(Level.WARN)).containsExactly(deprecationMessage);
    verify(analysisWarnings).addUnique(deprecationMessage);
  }

  @Test
  public void log_unsupported_file_extension() throws Exception {
    AnalysisWarnings analysisWarnings = mock(AnalysisWarnings.class);
    Coverage coverage = new Coverage();
    // use "one_file.nccov" to easily check the logs (it has only one coverage entry)
    new NCover3ReportParser(alwaysFalse, analysisWarnings).accept(new File("src/test/resources/ncover3/one_file.nccov"), coverage);

    assertThat(coverage.files()).isEmpty();

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs.get(0)).startsWith("The current user dir is '");
    assertThat(debugLogs.get(1))
      .startsWith("NCover3 doc '1', line '31', vc '4' will be skipped because it has a path '")
      .endsWith("\\MyLibrary\\Adder.cs' which is not indexed or does not have the supported language. "
        + "Verify sonar.sources in .sonarqube\\out\\sonar-project.properties.");
    assertThat(debugLogs.get(2))
      .startsWith("NCover3 doc '1', line '32', vc '4' will be skipped because it has a path '")
      .endsWith("\\MyLibrary\\Adder.cs' which is not indexed or does not have the supported language. "
        + "Verify sonar.sources in .sonarqube\\out\\sonar-project.properties.");
    List<String> traceLogs = logTester.logs(Level.TRACE);
    assertThat(traceLogs.get(0)).isEqualTo("Analyzing the doc tag with NCover3 ID '1' and url 'MyLibrary\\Adder.cs'.");
    assertThat(traceLogs.get(1))
      .startsWith("NCover3 ID '1' with url 'MyLibrary\\Adder.cs' is resolved as '")
      .endsWith("MyLibrary\\Adder.cs'.");

    assertThat(logTester.logs(Level.WARN)).containsExactly(deprecationMessage);
    verify(analysisWarnings).addUnique(deprecationMessage);
  }

  @Test
  public void should_not_fail_with_invalid_path() {
    AnalysisWarnings analysisWarnings = mock(AnalysisWarnings.class);
    new NCover3ReportParser(alwaysTrue, analysisWarnings).accept(new File("src/test/resources/ncover3/invalid_path.nccov"), mock(Coverage.class));
    assertThat(logTester.logs(Level.DEBUG).get(0)).startsWith("The current user dir is '");
    assertThat(logTester.logs(Level.DEBUG)).contains(
      "Skipping the import of NCover3 code coverage for the invalid file path: z:\\*\"?.cs at line 7");
    List<String> traceLogs = logTester.logs(Level.TRACE);
    assertThat(traceLogs.get(0)).isEqualTo("Analyzing the doc tag with NCover3 ID '1' and url 'z:\\*\"?.cs'.");

    assertThat(logTester.logs(Level.WARN)).containsExactly(deprecationMessage);
    verify(analysisWarnings).addUnique(deprecationMessage);
  }

}
