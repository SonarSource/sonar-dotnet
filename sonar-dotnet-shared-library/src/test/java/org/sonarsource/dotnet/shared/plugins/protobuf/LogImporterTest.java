/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2021 SonarSource SA
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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import java.io.File;
import org.junit.Rule;
import org.junit.Test;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import static org.assertj.core.api.Assertions.assertThat;

public class LogImporterTest {
  // see src/test/resources/ProtobufImporterTest/README.md for explanation
  private File regularProtobuf = new File("src/test/resources/ProtobufImporterTest/custom-log.pb");
  private File unknownProfobuf = new File("src/test/resources/ProtobufImporterTest/unknown-log.pb");

  @Rule
  public LogTester logTester = new LogTester();

  @Test
  public void importLogMessages() {
    LogImporter sut = new LogImporter();
    sut.accept(regularProtobuf.toPath());
    sut.save();

    assertThat(logTester.logs(LoggerLevel.DEBUG)).hasSize(2).contains("First debug line", "Second debug line");
    assertThat(logTester.logs(LoggerLevel.INFO)).containsOnly("Single info line");
    assertThat(logTester.logs(LoggerLevel.WARN)).containsOnly("Single warning line");
    assertThat(logTester.logs(LoggerLevel.ERROR)).isEmpty();
  }

  @Test
  public void unknownLogReportedAsInfoWithWarning() {
    LogImporter sut = new LogImporter();
    sut.accept(unknownProfobuf.toPath());
    sut.save();

    assertThat(logTester.logs(LoggerLevel.DEBUG)).isEmpty();
    assertThat(logTester.logs(LoggerLevel.INFO)).containsOnly("Unknown severity for Coverage");
    assertThat(logTester.logs(LoggerLevel.WARN)).containsOnly("Unexpected log message severity: UNKNOWN_SEVERITY");
    assertThat(logTester.logs(LoggerLevel.ERROR)).isEmpty();
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
