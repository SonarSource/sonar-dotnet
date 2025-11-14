/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonarsource.dotnet.shared.plugins.protobuf;

import java.io.File;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.sonar.api.testfixtures.log.LogTester;
import org.slf4j.event.Level;

import static org.assertj.core.api.Assertions.assertThat;

public class LogImporterTest {
  // see src/test/resources/ProtobufImporterTest/README.md for explanation
  private File regularProtobuf = new File("src/test/resources/ProtobufImporterTest/custom-log.pb");
  private File unknownProfobuf = new File("src/test/resources/ProtobufImporterTest/unknown-log.pb");

  @Rule
  public LogTester logTester = new LogTester();

  @Before
  public void before() {
    logTester.setLevel(Level.DEBUG);
  }

  @Test
  public void importLogMessages() {
    LogImporter sut = new LogImporter();
    sut.accept(regularProtobuf.toPath());
    sut.save();

    assertThat(logTester.logs(Level.DEBUG)).hasSize(2).contains("First debug line", "Second debug line");
    assertThat(logTester.logs(Level.INFO)).containsOnly("Single info line");
    assertThat(logTester.logs(Level.WARN)).containsOnly("Single warning line");
    assertThat(logTester.logs(Level.ERROR)).isEmpty();
  }

  @Test
  public void unknownLogReportedAsInfoWithWarning() {
    LogImporter sut = new LogImporter();
    sut.accept(unknownProfobuf.toPath());
    sut.save();

    assertThat(logTester.logs(Level.DEBUG)).isEmpty();
    assertThat(logTester.logs(Level.INFO)).containsOnly("Unknown severity for Coverage");
    assertThat(logTester.logs(Level.WARN)).containsOnly("Unexpected log message severity: UNKNOWN_SEVERITY");
    assertThat(logTester.logs(Level.ERROR)).isEmpty();
  }

  @Test
  public void clearsInternalStateOnSave() {
    LogImporter sut = new LogImporter();
    sut.accept(regularProtobuf.toPath());
    sut.save();
    assertThat(logTester.logs()).isNotEmpty();
    logTester.clear();

    sut.save(); // Previous save cleared internal state so it should be empty this time
    assertThat(logTester.logs()).isEmpty();
  }
}
