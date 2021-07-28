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
import java.util.List;
import org.junit.Before;
import org.junit.Test;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;

import static org.assertj.core.api.Assertions.assertThat;

public class LogImporterTest {
  // see src/test/resources/ProtobufImporterTest/README.md for explanation
  private File protobuf = new File("src/test/resources/ProtobufImporterTest/custom-log.pb");
  LogImporter sut = new LogImporter();

  @Before
  public void before() {
    assertThat(protobuf).withFailMessage("no such file: " + protobuf).isFile();
  }

  @Test
  public void importLogMessages() {
    sut.accept(protobuf.toPath());
    List<SonarAnalyzer.LogInfo> ret = sut.messages();

    assertThat(ret).hasSize(4);
    assertThat(ret.get(0).getSeverity()).isEqualTo(SonarAnalyzer.LogSeverity.DEBUG);
    assertThat(ret.get(0).getText()).isEqualTo("First debug line");
    assertThat(ret.get(1).getSeverity()).isEqualTo(SonarAnalyzer.LogSeverity.DEBUG);
    assertThat(ret.get(1).getText()).isEqualTo("Second debug line");
    assertThat(ret.get(2).getSeverity()).isEqualTo(SonarAnalyzer.LogSeverity.INFO);
    assertThat(ret.get(2).getText()).isEqualTo("Single info line");
    assertThat(ret.get(3).getSeverity()).isEqualTo(SonarAnalyzer.LogSeverity.WARNING);
    assertThat(ret.get(3).getText()).isEqualTo("Single warning line");
  }
}
