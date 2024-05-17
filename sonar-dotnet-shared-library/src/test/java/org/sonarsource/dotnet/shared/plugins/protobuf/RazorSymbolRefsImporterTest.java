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
  private final File protobuf = new File(TEST_DATA_DIR, SYMBOLREFS_FILENAME);

  @Override
  @Before
  public void setUp() throws FileNotFoundException {
    super.setUp();
    assertThat(protobuf).withFailMessage("no such file: " + protobuf).isFile();
  }

  @Test
  public void test_symbol_refs_get_imported_cases() {

    var inputFile = CasesInputFile;
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

  @Test
  public void test_symbol_refs_get_imported_overlapSymbolReferences() {
    var inputFile = OverlapSymbolReferencesInputFile;
    var sut = new SymbolRefsImporter(sensorContext, s -> Paths.get(s).getFileName().toString());
    sut.accept(protobuf.toPath());
    sut.save();

    // the issue with overlapping symbols has been fixed in dotnet 8.0.5
    assertThat(sensorContext.referencesForSymbolAt(inputFile.key(), 1, 11)).hasSize(1);
    assertThat(logTester.logs(Level.DEBUG)).isEmpty();
  }
}
