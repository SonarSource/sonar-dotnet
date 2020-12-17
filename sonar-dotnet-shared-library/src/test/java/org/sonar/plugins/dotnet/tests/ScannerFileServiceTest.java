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

import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import java.util.Optional;
import java.util.stream.Collectors;
import org.junit.Rule;
import org.junit.Test;
import org.mockito.ArgumentCaptor;
import org.sonar.api.batch.fs.FilePredicate;
import org.sonar.api.batch.fs.FilePredicates;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

public class ScannerFileServiceTest {
  @Rule
  public LogTester logTester = new LogTester();

  @Test
  public void isSupportedAbsolute_passes_correct_argument() {
    // arrange
    FileSystem fs = mock(FileSystem.class);
    FilePredicates filePredicates = mock(FilePredicates.class);

    ArgumentCaptor<String> argumentCaptor = ArgumentCaptor.forClass(String.class);
    when(filePredicates.hasAbsolutePath(argumentCaptor.capture())).thenReturn(mock(FilePredicate.class));
    when(fs.predicates()).thenReturn(filePredicates);
    when(fs.hasFiles(any())).thenReturn(true);

    // act
    ScannerFileService sut = new ScannerFileService("key", fs);
    sut.isSupportedAbsolute("/_/some/path/file.cs");

    // assert
    assertThat(argumentCaptor.getValue()).isEqualTo("/_/some/path/file.cs");
    assertThat(logTester.logs()).isEmpty();
  }

  @Test
  public void isSupportedAbsolute_returns_fileSystem_result_when_true() {
    FileSystem fs = createFileSystemForHasFiles(true);

    ScannerFileService sut = new ScannerFileService("key", fs);
    boolean result = sut.isSupportedAbsolute("/_/some/path/file.cs");

    assertThat(result).isTrue();
    assertThat(logTester.logs()).isEmpty();
  }

  @Test
  public void isSupportedAbsolute_returns_fileSystem_result_when_false() {
    FileSystem fs = createFileSystemForHasFiles(false);

    ScannerFileService sut = new ScannerFileService("key", fs);
    boolean result = sut.isSupportedAbsolute("/_/some/path/file.cs");

    assertThat(result).isFalse();
    assertThat(logTester.logs()).isEmpty();
  }

  @Test
  public void getAbsolutePath_passes_correct_arguments_to_filesystem_api() {
    // arrange
    FileSystem fs = mock(FileSystem.class);
    FilePredicates filePredicates = mock(FilePredicates.class);
    FilePredicate languageKeyMock = mock(FilePredicate.class);

    when(filePredicates.hasLanguage("key")).thenReturn(languageKeyMock);
    when(fs.predicates()).thenReturn(filePredicates);

    ArgumentCaptor<FilePredicate> andArg1 = ArgumentCaptor.forClass(FilePredicate.class);
    ArgumentCaptor<FilePredicate> andArg2 = ArgumentCaptor.forClass(FilePredicate.class);

    // act
    ScannerFileService sut = new ScannerFileService("key", fs);
    sut.getAbsolutePath("/_/foo");

    // assert
    verify(filePredicates).and(andArg1.capture(), andArg2.capture());
    assertThat(andArg1.getValue()).isEqualTo(languageKeyMock);
    Object argument = andArg2.getValue();
    assertThat(andArg2.getValue()).isExactlyInstanceOf(PathSuffixPredicate.class);
    PathSuffixPredicate pathPredicate = (PathSuffixPredicate)argument;
    assertThat(pathPredicate.getPathSuffix()).isEqualTo("foo");
    verify(fs).inputFiles(any());
  }

  @Test
  public void getAbsolutePath_regex_test() {
    // arrange
    FileSystem fs = mock(FileSystem.class);
    FilePredicates filePredicates = mock(FilePredicates.class);
    when(fs.predicates()).thenReturn(filePredicates);
    ArgumentCaptor<FilePredicate> captor = ArgumentCaptor.forClass(FilePredicate.class);
    List<String> testInput = Arrays.asList(
      "/_/some/path/file.cs",
      "/_1/some/path/file.cs",
      "/_10/some/path/file.cs",
      "\\_2\\some\\path\\file.cs",
      "\\_1234\\some\\path\\file.cs",
      "\\_9999\\some/path/file.cs");

    // act
    ScannerFileService sut = new ScannerFileService("key", fs);
    testInput.forEach(sut::getAbsolutePath);

    // assert
    verify(filePredicates, times(testInput.size())).and(any(), captor.capture());
    List<PathSuffixPredicate> predicates = captor.getAllValues().stream().map(obj -> (PathSuffixPredicate) obj).collect(Collectors.toList());
    for (PathSuffixPredicate predicate : predicates) {
      assertThat(predicate.getPathSuffix()).isEqualTo("some/path/file.cs");
    }
  }

  @Test
  public void getAbsolutePath_when_filesystem_returns_empty_returns_empty() {
    FileSystem fs = createFileSystemReturningAllFiles(Collections.emptyList());

    // act
    ScannerFileService sut = new ScannerFileService("key", fs);
    Optional<String> result = sut.getAbsolutePath("/_/some/path/file.cs");

    // assert
    assertThat(result).isEmpty();
    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsExactly("Found 0 indexed files for '/_/some/path/file.cs' (normalized to 'some/path/file.cs'). Will skip this coverage entry. Verify sonar.sources in .sonarqube\\out\\sonar-project.properties.");
  }

  @Test
  public void getAbsolutePath_when_multiple_indexed_files_match_returns_empty_and_logs() {
    FileSystem fs = createFileSystemReturningAllFiles(Arrays.asList(mockInput("foo"), mockInput("bar")));

    ScannerFileService sut = new ScannerFileService("key", fs);
    Optional<String> result = sut.getAbsolutePath("/_/some/path/file.cs");

    assertThat(result).isEmpty();
    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsExactly("Found 2 indexed files for '/_/some/path/file.cs' (normalized to 'some/path/file.cs'). Will skip this coverage entry. Verify sonar.sources in .sonarqube\\out\\sonar-project.properties.");
  }

  @Test
  public void getAbsolutePath_when_single_indexed_files_match_returns_file_logs_trace() {
    InputFile expectedResult = mockInput("root/some/path/file.cs");
    FileSystem fs = createFileSystemReturningAllFiles(Collections.singleton(expectedResult));

    ScannerFileService sut = new ScannerFileService("key", fs);
    Optional<String> result = sut.getAbsolutePath("/_/path/file.cs");

    assertThat(result).hasValue(expectedResult.uri().getPath());
    assertThat(logTester.logs(LoggerLevel.TRACE)).hasSize(1);
    assertThat(logTester.logs(LoggerLevel.TRACE).get(0))
      .startsWith("Found indexed file ")
      .endsWith("/sonar-dotnet-shared-library/mod/root/some/path/file.cs' for '/_/path/file.cs' (normalized to 'path/file.cs').");
  }

  @Test
  public void getAbsolutePath_with_no_deterministic_path_in_windows_path_returns_empty() {
    FileSystem fs = mock(FileSystem.class);

    ScannerFileService sut = new ScannerFileService("key", fs);
    Optional<String> result = sut.getAbsolutePath("C:\\_\\some\\path\\file.cs");

    assertThat(result).isEmpty();
    verify(fs, never()).predicates();
    assertThat(logTester.logs(LoggerLevel.TRACE)).isEmpty();
    assertThat(logTester.logs(LoggerLevel.DEBUG)).hasSize(1);
    assertThat(logTester.logs(LoggerLevel.DEBUG).get(0)).isEqualTo("Did not find deterministic source path in 'C:\\_\\some\\path\\file.cs'." +
      " Will skip this coverage entry. Verify sonar.sources in .sonarqube\\out\\sonar-project.properties.");
  }

  @Test
  public void getAbsolutePath_with_no_deterministic_source_path_in_unix_path_returns_empty() {
    FileSystem fs = mock(FileSystem.class);

    ScannerFileService sut = new ScannerFileService("key", fs);
    Optional<String> result = sut.getAbsolutePath("some/path/file.cs");

    assertThat(result).isEmpty();
    verify(fs, never()).predicates();
    assertThat(logTester.logs(LoggerLevel.TRACE)).isEmpty();
  }

  private FileSystem createFileSystemReturningAllFiles(Iterable<InputFile> inputFilesResult) {
    FileSystem fs = mock(FileSystem.class);
    when(fs.predicates()).thenReturn(mock(FilePredicates.class));
    when(fs.inputFiles(any())).thenReturn(inputFilesResult);
    return fs;
  }

  private FileSystem createFileSystemForHasFiles(boolean result) {
    FileSystem fs = mock(FileSystem.class);
    when(fs.hasFiles(any())).thenReturn(result);
    when(fs.predicates()).thenReturn(mock(FilePredicates.class));
    return fs;
  }

  private InputFile mockInput(String path) {
    return new TestInputFileBuilder("mod", path).setLanguage("cs").build();
  }

}
