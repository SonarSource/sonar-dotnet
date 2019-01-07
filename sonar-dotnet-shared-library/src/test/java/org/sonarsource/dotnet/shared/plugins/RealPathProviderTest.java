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

import java.io.File;
import java.io.IOException;
import org.junit.Assume;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import static org.assertj.core.api.Assertions.assertThat;

public class RealPathProviderTest {
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Rule
  public LogTester logger = new LogTester();

  @Test
  public void when_relative_path_and_file_does_not_exist_returns_same_path() {
    assertThat(new RealPathProvider().getRealPath("File.cs")).isEqualTo("File.cs");
    assertThat(logger.logs(LoggerLevel.DEBUG)).containsOnly("Failed to retrieve the real full path for 'File.cs'");
  }

  @Test
  public void when_relative_path_with_back_apostrophe_and_file_does_not_exist_returns_same_path() {
    assertThat(new RealPathProvider().getRealPath("File`1.cs")).isEqualTo("File`1.cs");
    assertThat(logger.logs(LoggerLevel.DEBUG)).containsOnly("Failed to retrieve the real full path for 'File`1.cs'");
  }

  @Test
  public void when_relative_path_with_special_characters_and_file_does_not_exist_returns_same_path() {
    assertThat(new RealPathProvider().getRealPath("P@!$%23&+-=r%5E%7B%7Dog_r()a%20m[1].cs")).isEqualTo("P@!$%23&+-=r%5E%7B%7Dog_r()a%20m[1].cs");
    assertThat(logger.logs(LoggerLevel.DEBUG)).containsOnly("Failed to retrieve the real full path for 'P@!$%23&+-=r%5E%7B%7Dog_r()a%20m[1].cs'");
  }

  @Test
  public void when_file_exists_fix_case() throws IOException {
    Assume.assumeTrue(System.getProperty("os.name").toLowerCase().startsWith("win"));
    File expectedFile = temp.newFile("FILE.CS");
    expectedFile.createNewFile();
    assertThat(new RealPathProvider().getRealPath(new File(temp.getRoot(),"file.cs").getPath())).isEqualTo(expectedFile.getCanonicalPath());
    assertThat(logger.logs(LoggerLevel.DEBUG)).isEmpty();
  }

  @Test
  public void cache_process_value_only_once() {
    RealPathProvider testSubject = new RealPathProvider();
    assertThat(testSubject.apply("File.cs")).isEqualTo("File.cs");
    assertThat(testSubject.apply("File.cs")).isEqualTo("File.cs");
    assertThat(testSubject.apply("File.cs")).isEqualTo("File.cs");
    assertThat(logger.logs(LoggerLevel.DEBUG)).containsOnlyOnce("Failed to retrieve the real full path for 'File.cs'");
  }
}
