/*
 * SonarQube C# Plugin
 * Copyright (C) 2014-2016 SonarSource SA
 * mailto:contact AT sonarsource DOT com
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
package org.sonar.plugins.csharp.sarif;

import static org.fest.assertions.Assertions.assertThat;

import java.io.File;
import java.io.IOException;

import org.apache.commons.io.FileUtils;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.junit.rules.TemporaryFolder;

public class SarifParserFactoryTest {
  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Test
  public void testNonExisting() {
    thrown.expectMessage("Unable to read the Roslyn SARIF report file: ");
    thrown.expectMessage("non_existing.json");

    SarifParserFactory.create(new File("non_existing.json"));
  }

  @Test
  public void testUnknownVersion() throws IOException {
    File f = temp.newFile("unknown.json");
    FileUtils.write(f, "{\"$schema\": \"http://json.schemastore.org/sarif-1.1.0\",\"version\": \"1.1.0\"}");
    SarifParser parser = SarifParserFactory.create(f);
    assertThat(parser).isInstanceOf(SarifParser10.class);
  }

  @Test
  public void testAllJsonFiles() {
    File folder = new File("src/test/resources/SarifParserTest");
    for (File f : folder.listFiles()) {
      assertThat(SarifParserFactory.create(f)).isNotNull();
    }
  }

  @Test
  public void testInvalidFormat() throws IOException {
    thrown.expectMessage("Not a JSON Object");

    File f = temp.newFile("invalid.json");
    FileUtils.write(f, "trash");
    SarifParserFactory.create(f);
  }

  @Test
  public void testInvalidJson() throws IOException {
    thrown.expectMessage("Unable to parse the Roslyn SARIF report file:");
    thrown.expectMessage("invalid.json");
    thrown.expectMessage("Unrecognized format");

    File f = temp.newFile("invalid.json");
    FileUtils.write(f, "{}");
    SarifParserFactory.create(f);
  }

  @Test
  public void testCreate_v10() {
    SarifParser parser = SarifParserFactory.create(new File("src/test/resources/SarifParserTest/v1_0.json"));
    assertThat(parser).isInstanceOf(SarifParser10.class);
  }

  @Test
  public void testCreate_v04() {
    SarifParser parser = SarifParserFactory.create(new File("src/test/resources/SarifParserTest/v0_4.json"));
    assertThat(parser).isInstanceOf(SarifParser01And04.class);

  }

  @Test
  public void testCreate_v01() {
    SarifParser parser = SarifParserFactory.create(new File("src/test/resources/SarifParserTest/v0_1.json"));
    assertThat(parser).isInstanceOf(SarifParser01And04.class);
  }
}
