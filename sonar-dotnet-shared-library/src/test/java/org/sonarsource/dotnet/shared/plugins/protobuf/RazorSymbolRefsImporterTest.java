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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import java.io.File;
import java.io.FileNotFoundException;
import java.nio.file.Paths;
import org.junit.Before;
import org.junit.Test;
import org.slf4j.event.Level;

import static org.assertj.core.api.Assertions.assertThat;
import static org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter.SYMBOLREFS_FILENAME;

public class RazorSymbolRefsImporterTest extends RazorImporterTestBase {
  private static final File PROTOBUF_4_9_FILE = new File(ROSLYN_4_9_DIR, SYMBOLREFS_FILENAME);
  private static final File PROTOBUF_4_10_FILE = new File(ROSLYN_4_10_DIR, SYMBOLREFS_FILENAME);

  @Override
  @Before
  public void setUp() {
    super.setUp();
    assertThat(PROTOBUF_4_9_FILE).withFailMessage("no such file: " + PROTOBUF_4_9_FILE).isFile();
  }

  @Test
  public void test_symbol_refs_get_imported_cases_before_4_10() throws FileNotFoundException {

    verifySymbolRef(PROTOBUF_4_9_FILE);
  }

  @Test
  public void test_symbol_refs_get_imported_cases_after_4_10() throws FileNotFoundException {
    verifySymbolRef(PROTOBUF_4_10_FILE);
  }

  @Test
  public void test_symbol_refs_get_imported_overlapSymbolReferences_before_4_10() throws FileNotFoundException {
    var inputFile = addTestFileToContext("OverlapSymbolReferences.razor");
    var sut = new SymbolRefsImporter(sensorContext, s -> Paths.get(s).getFileName().toString());
    sut.accept(PROTOBUF_4_9_FILE.toPath());
    sut.save();

    var references = sensorContext.referencesForSymbolAt(inputFile.key(), 1, 1);
    assertThat(references)
      .isNotNull() // The symbol declaration can be found,
      .isEmpty();  // but there are no references, due to the overlap.

    assertThat(logTester.logs(Level.DEBUG)).containsExactly(
      "The declaration token at Range[from [line=1, lineOffset=0] to [line=1, lineOffset=17]] overlaps with the referencing token Range[from [line=1, lineOffset=6] to [line=1, lineOffset=23]] in file OverlapSymbolReferences.razor");
  }

  @Test
  public void test_symbol_refs_get_imported_overlapSymbolReferences_after_4_10() throws FileNotFoundException {
    var inputFile = addTestFileToContext("OverlapSymbolReferences.razor");
    var sut = new SymbolRefsImporter(sensorContext, s -> Paths.get(s).getFileName().toString());
    sut.accept(PROTOBUF_4_10_FILE.toPath());
    sut.save();

    // the issue with overlapping symbols has been fixed in dotnet 8.0.5
    assertThat(sensorContext.referencesForSymbolAt(inputFile.key(), 1, 11)).hasSize(1);
    assertThat(logTester.logs(Level.DEBUG)).isEmpty();
  }

  private void verifySymbolRef(File protobuf) throws FileNotFoundException {
    var inputFile = addTestFileToContext("Cases.razor");
    var sut = new SymbolRefsImporter(sensorContext, RazorImporterTestBase::fileName);
    sut.accept(protobuf.toPath());
    sut.save();

    // a symbol is defined at this location, and referenced at 3 other locations
    assertThat(sensorContext.referencesForSymbolAt(inputFile.key(), 8, 15)).hasSize(2);

    // ... other similar examples ...
    assertThat(sensorContext.referencesForSymbolAt(inputFile.key(), 16, 16)).hasSize(4);
    assertThat(sensorContext.referencesForSymbolAt(inputFile.key(), 19, 15)).hasSize(3);
    assertThat(sensorContext.referencesForSymbolAt(inputFile.key(), 21, 17)).isEmpty();

    assertThat(logTester.logs(Level.DEBUG)).isEmpty();
  }
}
