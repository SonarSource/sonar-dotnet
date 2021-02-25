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
package org.sonarsource.dotnet.shared.plugins;

import java.io.File;
import java.io.IOException;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.internal.DefaultFileSystem;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;

import static org.assertj.core.api.Assertions.assertThat;
import static org.sonar.api.batch.fs.InputFile.Type;
import static org.sonarsource.dotnet.shared.plugins.SensorContextUtils.hasFilesOfType;
import static org.sonarsource.dotnet.shared.plugins.SensorContextUtils.hasMainFiles;
import static org.sonarsource.dotnet.shared.plugins.SensorContextUtils.toInputFile;

public class SensorContextUtilsTest {
  private static final String LANG_ONE = "LANG_ONE";
  private static final String LANG_TWO = "LANG_TWO";

  @Rule
  public TemporaryFolder temporaryFolder = new TemporaryFolder();

  private DefaultFileSystem fs;

  @Before
  public void setUp() {
    fs = new DefaultFileSystem(temporaryFolder.getRoot());
  }

  @Test
  public void toInputFile_should_return_file_if_exists() throws IOException {
    // note: the .getCanonicalFile() is a precaution for filename-shortening on windows
    File file = temporaryFolder.newFile().getCanonicalFile();
    fs.add(new TestInputFileBuilder("dummy", file.getName())
      .setModuleBaseDir(file.getParentFile().toPath())
      .build());
    assertThat(toInputFile(fs, file.getName()).uri()).isEqualTo(file.toURI());
  }

  @Test
  public void toInputFile_should_return_null_if_file_nonexistent() {
    assertThat(toInputFile(fs, "nonexistent")).isNull();
  }

  @Test
  public void toInputFile_should_return_null_if_file_is_a_dir() throws IOException {
    File folder = temporaryFolder.newFolder();
    fs.add(new TestInputFileBuilder("dummy", folder.getName()).build());
    assertThat(toInputFile(fs, "nonexistent")).isNull();
  }

  @Test
  public void hasFilesOfType_whenNoFiles_returnsFalse() {
    assertThat(hasFilesOfType(fs, Type.MAIN, LANG_ONE)).isFalse();
    assertThat(hasFilesOfType(fs, Type.TEST, LANG_ONE)).isFalse();
  }

  @Test
  public void hasFilesOfType_whenTypeIsCorrect_andLanguageIsDifferent_returnFalse() {
    addFileToFileSystem("foo", Type.MAIN, LANG_ONE);
    assertThat(hasFilesOfType(fs, Type.MAIN, LANG_TWO)).isFalse();
  }

  @Test
  public void hasFilesOfType_whenLanguageIsCorrect_andTypeIsDifferent_returnsFalse() {
    addFileToFileSystem("foo", Type.MAIN, LANG_ONE);
    assertThat(hasFilesOfType(fs, Type.TEST, LANG_ONE)).isFalse();
  }

  @Test
  public void hasFilesOfType_whenLanguageAndTypeAreCorrect_returnsTrue() {
    addFileToFileSystem("foo", Type.MAIN, LANG_ONE);
    assertThat(hasFilesOfType(fs, Type.MAIN, LANG_ONE)).isTrue();
  }

  @Test
  public void hasMainFiles_whenNoFiles_returnsFalse() {
    assertThat(hasMainFiles(fs)).isFalse();
  }

  @Test
  public void hasMainFiles_whenOnlyTestFiles_returnsFalse() {
    addFileToFileSystem("foo", Type.TEST, LANG_ONE);
    assertThat(hasMainFiles(fs)).isFalse();
  }

  @Test
  public void hasMainFiles_whenOnlyMainFiles_returnsTrue() {
    addFileToFileSystem("foo", Type.MAIN, LANG_ONE);
    assertThat(hasMainFiles(fs)).isTrue();
  }

  @Test
  public void hasMainFiles_whenBothTestAndMainFiles_returnsTrue() {
    addFileToFileSystem("foo", Type.MAIN, LANG_ONE);
    addFileToFileSystem("bar", Type.TEST, LANG_TWO);
    assertThat(hasMainFiles(fs)).isTrue();
  }

  private void addFileToFileSystem(String fileName, InputFile.Type fileType, String language) {
    DefaultInputFile inputFile = new TestInputFileBuilder("mod", fileName)
      .setLanguage(language)
      .setType(fileType)
      .build();
    fs.add(inputFile);
  }
}
