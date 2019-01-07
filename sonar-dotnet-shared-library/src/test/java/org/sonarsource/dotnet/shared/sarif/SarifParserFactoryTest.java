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
package org.sonarsource.dotnet.shared.sarif;

import static org.assertj.core.api.Assertions.assertThat;

import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardOpenOption;

import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.junit.rules.TemporaryFolder;
import org.sonarsource.dotnet.shared.plugins.RoslynReport;

public class SarifParserFactoryTest {
  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Test
  public void testNonExisting() {
    thrown.expectMessage("Unable to read the Roslyn SARIF report file: ");
    thrown.expectMessage("non_existing.json");

    SarifParserFactory.create(new RoslynReport(null, Paths.get("non_existing.json")), String::toString);
  }

  @Test
  public void testUnknownVersion() throws IOException {
    Path f = temp.newFile("unknown.json").toPath();
    Files.write(f, "{\"$schema\": \"http://json.schemastore.org/sarif-1.1.0\",\"version\": \"1.1.0\"}".getBytes(StandardCharsets.UTF_8), StandardOpenOption.CREATE);
    SarifParser parser = SarifParserFactory.create(new RoslynReport(null, f), String::toString);
    assertThat(parser).isInstanceOf(SarifParser10.class);
  }

  @Test
  public void testAllJsonFiles() throws IOException {
    Path folder = Paths.get("src/test/resources/SarifParserTest");
    Files.list(folder).forEach(f -> assertThat(SarifParserFactory.create(new RoslynReport(null, f), String::toString)).isNotNull());
  }

  @Test
  public void testInvalidFormat() throws IOException {
    thrown.expectMessage("Not a JSON Object");

    Path f = temp.newFile("invalid.json").toPath();
    Files.write(f, "trash".getBytes(StandardCharsets.UTF_8), StandardOpenOption.CREATE);
    SarifParserFactory.create(new RoslynReport(null, f), String::toString);
  }

  @Test
  public void testInvalidJson() throws IOException {
    thrown.expectMessage("Unable to parse the Roslyn SARIF report file:");
    thrown.expectMessage("invalid.json");
    thrown.expectMessage("Unrecognized format");

    Path f = temp.newFile("invalid.json").toPath();
    Files.write(f, "{}".getBytes(StandardCharsets.UTF_8), StandardOpenOption.CREATE);
    SarifParserFactory.create(new RoslynReport(null, f), String::toString);
  }

  @Test
  public void testCreate_v10() {
    SarifParser parser = SarifParserFactory.create(new RoslynReport(null, Paths.get("src/test/resources/SarifParserTest/v1_0.json")), String::toString);
    assertThat(parser).isInstanceOf(SarifParser10.class);
  }

  @Test
  public void testCreate_v04() {
    SarifParser parser = SarifParserFactory.create(new RoslynReport(null, Paths.get("src/test/resources/SarifParserTest/v0_4.json")), String::toString);
    assertThat(parser).isInstanceOf(SarifParser01And04.class);

  }

  @Test
  public void testCreate_v01() {
    SarifParser parser = SarifParserFactory.create(new RoslynReport(null, Paths.get("src/test/resources/SarifParserTest/v0_1.json")), String::toString);
    assertThat(parser).isInstanceOf(SarifParser01And04.class);
  }
}
