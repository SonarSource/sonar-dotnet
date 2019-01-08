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
package org.sonarsource.dotnet.shared.plugins;

import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.fs.internal.DefaultFileSystem;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;

import java.io.File;
import java.io.IOException;

import static org.assertj.core.api.Assertions.assertThat;
import static org.sonarsource.dotnet.shared.plugins.SensorContextUtils.toInputFile;

public class SensorContextUtilsTest {

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
    assertThat(toInputFile(fs, file.getName()).file()).isEqualTo(file);
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

}
