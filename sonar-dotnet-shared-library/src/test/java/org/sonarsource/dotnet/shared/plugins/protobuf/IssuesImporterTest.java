/*
 * SonarSource :: .NET :: Shared library
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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import org.junit.Test;
import org.sonar.api.batch.fs.TextRange;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.FileMetadata;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.batch.sensor.issue.Issue;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.util.Collection;
import java.util.stream.Collectors;

import static org.assertj.core.api.Assertions.assertThat;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.ISSUES_OUTPUT_PROTOBUF_NAME;

public class IssuesImporterTest {

  // see src/test/resources/ProtobufImporterTest/README.md for explanation
  private static final File TEST_DATA_DIR = new File("src/test/resources/ProtobufImporterTest");
  private static final String TEST_FILE_PATH = "Program.cs";
  private static final File TEST_FILE = new File(TEST_DATA_DIR, TEST_FILE_PATH);

  @Test
  public void test_issues_get_imported() throws FileNotFoundException {
    SensorContextTester tester = SensorContextTester.create(TEST_DATA_DIR);

    DefaultInputFile inputFile = new TestInputFileBuilder("dummyKey", TEST_FILE_PATH)
      .setMetadata(new FileMetadata().readMetadata(new FileReader(TEST_FILE)))
      .build();
    tester.fileSystem().add(inputFile);

    File protobuf = new File(TEST_DATA_DIR, ISSUES_OUTPUT_PROTOBUF_NAME);
    assertThat(protobuf.isFile()).withFailMessage("no such file: " + protobuf).isTrue();

    new IssuesImporter(tester, "dummy").accept(protobuf.toPath());

    Collection<Issue> issues = tester.allIssues();
    assertThat(issues).hasSize(6);

    assertThat(issues.stream().map(p -> p.ruleKey().rule()).collect(Collectors.toList()))
      .containsOnly("S1186", "S1172", "S1118", "S101", "S101", "S105");

    TextRange exactLocation = issues.stream().filter(i -> i.ruleKey().rule().equals("S1186")).findFirst().get().primaryLocation().textRange();
    assertThat(exactLocation.start().line() != exactLocation.end().line() || exactLocation.start().lineOffset() != exactLocation.end().lineOffset()).isTrue();

    exactLocation = issues.stream().filter(i -> i.ruleKey().rule().equals("S105")).findFirst().get().primaryLocation().textRange();
    assertThat(exactLocation).isNull();
  }

}
