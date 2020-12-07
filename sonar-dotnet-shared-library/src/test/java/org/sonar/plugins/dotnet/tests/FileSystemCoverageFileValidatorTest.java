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
import java.util.Collection;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.junit.runners.Parameterized;
import org.sonar.api.batch.fs.FilePredicate;
import org.sonar.api.batch.fs.FilePredicates;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.batch.fs.InputFile;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.anyCollection;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

@RunWith(Parameterized.class)
public class FileSystemCoverageFileValidatorTest {
  private boolean hasAbsolutePath;
  private boolean hasLanguage;
  private boolean hasFiles;
  private boolean expectedResult;

  public FileSystemCoverageFileValidatorTest(boolean hasAbsolutePath, boolean hasLanguage, boolean hasFiles, boolean expectedResult) {
    this.hasAbsolutePath = hasAbsolutePath;
    this.hasLanguage = hasLanguage;
    this.hasFiles = hasFiles;
    this.expectedResult = expectedResult;
  }

  @Parameterized.Parameters
  public static Collection input() {
    return Arrays.asList(new Object[][] {
        // hasAbsolutePath, hasLanguage, hasFiles, expectedResult
        // expectedResult should always be what hasFiles returns
        {false, false, false, false},
        {true, false, false, false},
        {false, true, false, false},
        {true, true, false, false},
        {false, false, true, true},
        {false, true, true, true},
        {true, false, true, true},
        {true, true, true, true},
      }
    );
  }

  @Test
  public void isSupported_returns_hasFiles_result() {
    // arrange
    FilePredicates predicatesFilePredicate = mock(FilePredicates.class);

    FilePredicate hasAbsolutePathFilePredicate = createFilePredicate(hasAbsolutePath);
    when(predicatesFilePredicate.hasAbsolutePath(anyString())).thenReturn(hasAbsolutePathFilePredicate);

    FilePredicate hasLanguageFilePredicate = createFilePredicate(hasLanguage);
    when(predicatesFilePredicate.hasLanguage(anyString())).thenReturn(hasLanguageFilePredicate);

    FilePredicate andFilePredicate = mock(FilePredicate.class);
    when(predicatesFilePredicate.and(anyCollection())).thenReturn(andFilePredicate);

    FileSystem fs = mock(FileSystem.class);
    when(fs.predicates()).thenReturn(predicatesFilePredicate);
    when(fs.hasFiles(any())).thenReturn(hasFiles);

    FileSystemCoverageFileValidator sut = new FileSystemCoverageFileValidator("key", fs);

    // act & assert
    assertThat(sut.isSupported("x")).isEqualTo(expectedResult);
  }

  private FilePredicate createFilePredicate(boolean result) {
    return new FilePredicate() {
      @Override
      public boolean apply(InputFile inputFile) {
        return result;
      }
    };
  }
}