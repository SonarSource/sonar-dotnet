/*
 * SonarSource :: .NET :: Core
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonar.plugins.dotnet.tests.coverage;

import java.io.File;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.slf4j.event.Level;
import org.sonar.api.testfixtures.log.LogTester;
import org.sonar.plugins.dotnet.tests.FileService;

import static org.assertj.core.api.Assertions.assertThat;
import static org.junit.Assert.assertThrows;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class CoberturaReportParserTest {

  @Rule
  public LogTester logTester = new LogTester();

  private FileService alwaysTrue;

  @Before
  public void prepare() {
    logTester.setLevel(Level.TRACE);
    alwaysTrue = mock(FileService.class);
    when(alwaysTrue.isSupportedAbsolute(anyString())).thenReturn(true);
    when(alwaysTrue.getAbsolutePath(anyString())).thenThrow(new UnsupportedOperationException("Should not call this"));
  }

  @Test
  public void invalid_root() {
    var parser = new CoberturaReportParser(alwaysTrue);
    var exception = assertThrows(RuntimeException.class,
      () -> parser.accept(new File("src/test/resources/cobertura/invalid_root.xml"), mock(Coverage.class)));
    assertThat(exception).hasMessageContaining("<coverage>");
  }

  @Test
  public void non_existing_file() {
    var parser = new CoberturaReportParser(alwaysTrue);
    var exception = assertThrows(RuntimeException.class,
      () -> parser.accept(new File("src/test/resources/cobertura/non_existing_file.xml"), mock(Coverage.class)));
    assertThat(exception).hasMessageContaining("non_existing_file.xml");
  }

  @Test
  public void valid_empty() {
    Coverage coverage = new Coverage();
    new CoberturaReportParser(alwaysTrue).accept(new File("src/test/resources/cobertura/valid_empty.xml"), coverage);
    assertThat(coverage.files()).isEmpty();
  }
}
