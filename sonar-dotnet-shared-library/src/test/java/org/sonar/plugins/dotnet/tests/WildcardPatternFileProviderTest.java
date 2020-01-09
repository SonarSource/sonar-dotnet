/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2020 SonarSource SA
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
package org.sonar.plugins.dotnet.tests;

import com.google.common.base.Joiner;
import java.util.List;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.junit.rules.TemporaryFolder;

import java.io.File;
import java.util.Set;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import static org.assertj.core.api.Assertions.assertThat;

public class WildcardPatternFileProviderTest {

  @Rule
  public TemporaryFolder tmp = new TemporaryFolder();

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Rule
  public LogTester logTester = new LogTester();

  @Before
  public void init() throws Exception {
    tmp.newFile("foo.txt");
    tmp.newFile("bar.txt");
    tmp.newFolder("a");
    tmp.newFile(path("a", "foo.txt"));
    tmp.newFolder("a", "a21");
    tmp.newFolder("b");

    tmp.newFolder("c");
    tmp.newFolder("c", "c21");
    tmp.newFile(path("c", "c21", "foo.txt"));
    tmp.newFolder("c", "c22");
    tmp.newFolder("c", "c22", "c31");
    tmp.newFile(path("c", "c22", "c31", "foo.txt"));
    tmp.newFile(path("c", "c22", "c31", "bar.txt"));
  }

  @Test
  public void logging_added_and_skipped_files() {
    File tmpRoot = tmp.getRoot();
    String givenPattern = new File(tmpRoot, path("**\\c\\c21", "foo.txt")).getAbsolutePath();
    listFiles(givenPattern);

    List<String> debugLogs = logTester.logs((LoggerLevel.DEBUG));
    assertThat(debugLogs).hasSize(3);
    assertThat(debugLogs.get(0)).isEqualTo("Pattern matcher extracted prefix/absolute path '" + tmpRoot + "' from the given pattern '" + givenPattern + "'.");
    assertThat(debugLogs.get(1)).isEqualTo("Gathering files for wildcardPattern '**\\c\\c21\\foo.txt'.");
    assertThat(debugLogs.get(2)).startsWith("Pattern matcher returns '1' files.");

    List<String> traceLogs = logTester.logs((LoggerLevel.TRACE));
    assertThat(traceLogs).hasSize(13);
    // we don't know the order of logging, so we're just doing a sanity check
    assertThat(traceLogs).contains("Skipping file '" +
      tmpRoot +
      "\\c\\c22\\c31\\foo.txt'" +
      " because it does not match pattern '**\\c\\c21\\foo.txt'.");
    assertThat(traceLogs).contains("Adding file '" + tmpRoot + "\\c\\c21\\foo.txt" + "' to result list.");
  }

  @Test
  public void logging_early_return_absolute_path() {
    File tmpRoot = tmp.getRoot();
    String absoluteFilePath = new File(tmpRoot,  "foo.txt").getAbsolutePath();
    listFiles(absoluteFilePath);

    List<String> logs = logTester.logs((LoggerLevel.DEBUG));
    assertThat(logs).hasSize(2);
    assertThat(logs.get(0)).isEqualTo("Pattern matcher extracted prefix/absolute path '" + absoluteFilePath + "' from the given pattern '" + absoluteFilePath + "'.");
    assertThat(logs.get(1)).isEqualTo("Pattern matcher returns a single file: '" + absoluteFilePath + "'.");
  }

  @Test
  public void logging_early_return_no_file_found() {
    File tmpRoot = tmp.getRoot();
    String nonExistingFilePath = new File(tmpRoot,  "not-existing-file").getAbsolutePath();
    listFiles(nonExistingFilePath);

    List<String> logs = logTester.logs((LoggerLevel.DEBUG));
    assertThat(logs).hasSize(2);
    assertThat(logs.get(0)).isEqualTo("Pattern matcher extracted prefix/absolute path '" + nonExistingFilePath + "' from the given pattern '" + nonExistingFilePath + "'.");
    assertThat(logs.get(1)).isEqualTo("Pattern matcher did not find any files matching the pattern '" + nonExistingFilePath + "'.");
  }

  @Test
  public void absolute_paths() {
    assertThat(listFiles(new File(tmp.getRoot(), "foo.txt").getAbsolutePath()))
      .containsOnly(new File(tmp.getRoot(), "foo.txt"));

    assertThat(listFiles(new File(tmp.getRoot(), path("c", "c22", "c31", "foo.txt")).getAbsolutePath()))
      .containsOnly(new File(tmp.getRoot(), path("c", "c22", "c31", "foo.txt")));

    assertThat(listFiles(new File(tmp.getRoot(), "nonexisting.txt").getAbsolutePath())).isEmpty();
  }

  @Test
  public void absolute_paths_with_current_and_parent_folder_access() {
    assertThat(listFiles(new File(tmp.getRoot(), path("a", "..", ".", "foo.txt")).getAbsolutePath()))
      .containsOnly(new File(tmp.getRoot(), path("a", "..", ".", "foo.txt")));

    assertThat(listFiles(new File(tmp.getRoot(), path("a", "..", ".", "foo.txt")).getAbsolutePath()))
      .containsOnly(new File(tmp.getRoot(), path("a", "..", ".", "foo.txt")));

    assertThat(listFiles(new File(tmp.getRoot(), path("a", "..", ".", "foo.txt")).getAbsolutePath()))
      .containsOnly(new File(tmp.getRoot(), path("a", "..", ".", "foo.txt")));
  }

  @Test
  public void absolute_paths_with_wildcards() {
    assertThat(listFiles(new File(tmp.getRoot(), "*.txt").getAbsolutePath()))
      .containsOnly(new File(tmp.getRoot(), "foo.txt"), new File(tmp.getRoot(), "bar.txt"));

    assertThat(listFiles(new File(tmp.getRoot(), "f*").getAbsolutePath()))
      .containsOnly(new File(tmp.getRoot(), "foo.txt"));

    assertThat(listFiles(new File(tmp.getRoot(), "fo?.txt").getAbsolutePath()))
      .containsOnly(new File(tmp.getRoot(), "foo.txt"));

    assertThat(listFiles(new File(tmp.getRoot(), "foo?.txt").getAbsolutePath())).isEmpty();

    assertThat(listFiles(new File(tmp.getRoot(), path("**", "foo.txt")).getAbsolutePath()))
      .containsOnly(
        new File(tmp.getRoot(), "foo.txt"),
        new File(tmp.getRoot(), path("a", "foo.txt")),
        new File(tmp.getRoot(), path("c", "c21", "foo.txt")),
        new File(tmp.getRoot(), path("c", "c22", "c31", "foo.txt")));

    assertThat(listFiles(new File(tmp.getRoot(), path("**", "c31", "foo.txt")).getAbsolutePath()))
      .containsOnly(new File(tmp.getRoot(), path("c", "c22", "c31", "foo.txt")));

    assertThat(listFiles(new File(tmp.getRoot(), path("**", "c?1", "foo.txt")).getAbsolutePath()))
      .containsOnly(new File(tmp.getRoot(), path("c", "c21", "foo.txt")), new File(tmp.getRoot(), path("c", "c22", "c31", "foo.txt")));

    assertThat(listFiles(new File(tmp.getRoot(), path("?", "**", "foo.txt")).getAbsolutePath()))
      .containsOnly(
        new File(tmp.getRoot(), path("a", "foo.txt")),
        new File(tmp.getRoot(), path("c", "c21", "foo.txt")),
        new File(tmp.getRoot(), path("c", "c22", "c31", "foo.txt")));
  }

  @Test
  public void relative_paths() {
    assertThat(listFiles("foo.txt", tmp.getRoot()))
      .containsOnly(new File(tmp.getRoot(), "foo.txt"));

    assertThat(listFiles(path("c", "c22", "c31", "foo.txt"), tmp.getRoot()))
      .containsOnly(new File(tmp.getRoot(), path("c", "c22", "c31", "foo.txt")));

    assertThat(listFiles("nonexisting.txt", tmp.getRoot())).isEmpty();
  }

  @Test
  public void relative_paths_with_current_and_parent_folder_access() {
    assertThat(listFiles(path("..", "foo.txt"), new File(tmp.getRoot(), "a")))
      .containsOnly(new File(new File(tmp.getRoot(), "a"), path("..", "foo.txt")));

    assertThat(listFiles(path(".", "foo.txt"), tmp.getRoot()))
      .containsOnly(new File(tmp.getRoot(), path(".", "foo.txt")));

    assertThat(listFiles(path("a", "..", "foo.txt"), tmp.getRoot()))
      .containsOnly(new File(tmp.getRoot(), path("a", "..", "foo.txt")));
  }

  @Test
  public void relative_paths_with_wildcards() {
    assertThat(listFiles("*.txt", tmp.getRoot()))
      .containsOnly(new File(tmp.getRoot(), "foo.txt"), new File(tmp.getRoot(), "bar.txt"));

    assertThat(listFiles("f*", tmp.getRoot()))
      .containsOnly(new File(tmp.getRoot(), "foo.txt"));

    assertThat(listFiles("fo?.txt", tmp.getRoot()))
      .containsOnly(new File(tmp.getRoot(), "foo.txt"));

    assertThat(listFiles("foo?.txt", tmp.getRoot())).isEmpty();

    assertThat(listFiles(path("**", "foo.txt"), tmp.getRoot()))
      .containsOnly(
        new File(tmp.getRoot(), "foo.txt"),
        new File(tmp.getRoot(), path("a", "foo.txt")),
        new File(tmp.getRoot(), path("c", "c21", "foo.txt")),
        new File(tmp.getRoot(), path("c", "c22", "c31", "foo.txt")));

    assertThat(listFiles(path("**", "c31", "foo.txt"), tmp.getRoot()))
      .containsOnly(new File(tmp.getRoot(), path("c", "c22", "c31", "foo.txt")));

    assertThat(listFiles(path("**", "c?1", "foo.txt"), tmp.getRoot()))
      .containsOnly(new File(tmp.getRoot(), path("c", "c21", "foo.txt")), new File(tmp.getRoot(), path("c", "c22", "c31", "foo.txt")));

    assertThat(listFiles(path("?", "**", "foo.txt"), tmp.getRoot()))
      .containsOnly(
        new File(tmp.getRoot(), path("a", "foo.txt")),
        new File(tmp.getRoot(), path("c", "c21", "foo.txt")),
        new File(tmp.getRoot(), path("c", "c22", "c31", "foo.txt")));
  }

  @Test
  public void should_fail_with_current_folder_access_after_wildcard() {
    thrown.expect(IllegalArgumentException.class);
    thrown.expectMessage("Cannot contain '.' or '..' after the first wildcard.");

    listFiles(new File(tmp.getRoot(), path("?", ".", "foo.txt")).getAbsolutePath());
  }

  @Test
  public void should_fail_with_parent_folder_access_after_wildcard() {
    thrown.expect(IllegalArgumentException.class);
    thrown.expectMessage("Cannot contain '.' or '..' after the first wildcard.");

    listFiles(path("*", "..", "foo.txt"), tmp.getRoot());
  }

  private static String path(String... elements) {
    return Joiner.on(File.separator).join(elements);
  }

  private static Set<File> listFiles(String pattern) {
    return listFiles(pattern, null);
  }

  private static Set<File> listFiles(String pattern, File baseDir) {
    return new WildcardPatternFileProvider(baseDir, File.separator).listFiles(pattern);
  }

}
