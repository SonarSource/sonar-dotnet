/*
 * SonarSource :: .NET :: Core
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
package org.sonarsource.dotnet.shared.plugins;

import java.io.File;
import java.io.IOException;
import java.util.Optional;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.TextRange;
import org.sonar.api.batch.fs.internal.DefaultFileSystem;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.fail;
import static org.sonar.api.batch.fs.InputFile.Type;
import static org.sonarsource.dotnet.shared.plugins.SensorContextUtils.hasAnyMainFiles;
import static org.sonarsource.dotnet.shared.plugins.SensorContextUtils.hasFilesOfLanguage;
import static org.sonarsource.dotnet.shared.plugins.SensorContextUtils.hasFilesOfType;
import static org.sonarsource.dotnet.shared.plugins.SensorContextUtils.toInputFile;
import static org.sonarsource.dotnet.shared.plugins.SensorContextUtils.toTextRange;

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
    assertThat(hasAnyMainFiles(fs)).isFalse();
  }

  @Test
  public void hasMainFiles_whenOnlyTestFiles_returnsFalse() {
    addFileToFileSystem("foo", Type.TEST, LANG_ONE);
    assertThat(hasAnyMainFiles(fs)).isFalse();
  }

  @Test
  public void hasMainFiles_whenOnlyMainFiles_returnsTrue() {
    addFileToFileSystem("foo", Type.MAIN, LANG_ONE);
    assertThat(hasAnyMainFiles(fs)).isTrue();
  }

  @Test
  public void hasMainFiles_whenBothTestAndMainFiles_returnsTrue() {
    addFileToFileSystem("foo", Type.MAIN, LANG_ONE);
    addFileToFileSystem("bar", Type.TEST, LANG_TWO);
    assertThat(hasAnyMainFiles(fs)).isTrue();
  }

  @Test
  public void hasFilesOfLanguage_whenOnlyThatLanguageExists_returnsTrue() {
    addFileToFileSystem("foo", Type.MAIN, LANG_ONE);
    addFileToFileSystem("bar", Type.TEST, LANG_ONE);
    assertThat(hasFilesOfLanguage(fs, LANG_ONE)).isTrue();
  }

  @Test
  public void hasFilesOfLanguage_whenMultipleLanguagesExist_returnsTrue() {
    addFileToFileSystem("fooLang1", Type.MAIN, LANG_ONE);
    addFileToFileSystem("barLang1", Type.TEST, LANG_ONE);
    addFileToFileSystem("fooLang2", Type.MAIN, LANG_TWO);
    addFileToFileSystem("barLang2", Type.TEST, LANG_TWO);
    assertThat(hasFilesOfLanguage(fs, LANG_ONE)).isTrue();
  }

  @Test
  public void hasFilesOfLanguage_whenOnlyMainFilesOfThatLanguageExist_returnsTrue() {
    addFileToFileSystem("foo", Type.MAIN, LANG_ONE);
    assertThat(hasFilesOfLanguage(fs, LANG_ONE)).isTrue();
  }

  @Test
  public void hasFilesOfLanguage_whenOnlyTestFilesOfThatLanguageExist_returnsTrue() {
    addFileToFileSystem("bar", Type.TEST, LANG_ONE);
    assertThat(hasFilesOfLanguage(fs, LANG_ONE)).isTrue();
  }

  @Test
  public void hasFilesOfLanguage_whenOnlyOtherLanguageExists_returnsFalse() {
    addFileToFileSystem("foo", Type.MAIN, LANG_TWO);
    addFileToFileSystem("bar", Type.TEST, LANG_TWO);
    assertThat(hasFilesOfLanguage(fs, LANG_ONE)).isFalse();
  }

  @Test
  public void toTextRange_whenMultiLineRangeStartsAtEOL_doesNotFilterOut() {
    var inputFile = new TestInputFileBuilder("mod", "source.cs")
      .setLanguage("cs")
      .setType(Type.MAIN)
      .setContents(
        "Some text \n" +
        "rangeStartingAtEOL1\n" +
        "Some more text rangeStarting\n" +
        "AtEOL2 Some other text")
      .build();
    fs.add(inputFile);
    assertTextRange(toTextRange(inputFile, pbTextRangeOf(1, 10, 2, 18)), 1, 10, 2, 18);
    assertTextRange(toTextRange(inputFile, pbTextRangeOf(2, 19, 4, 5)), 2, 19, 4, 5);
  }

  @Test
  public void toTextRange_whenMultiLineRangeStartsBeyondEOL_trimsStartMovingToNextLine() {
    var inputFile = new TestInputFileBuilder("mod", "source.cs")
      .setLanguage("cs")
      .setType(Type.MAIN)
      .setContents("\tSome text\nrangeStartingAtEOL1\nrangeStarting\nAtEOL2\nAndSpanning\nMultipleLines")
      .build();
    fs.add(inputFile);

    // Possible real scenario: 4 spaces transformed into \t -> new EOL at 10 instead of 13
    assertTextRange(toTextRange(inputFile, pbTextRangeOf(1, 13, 2, 18)), 2, 0, 2, 18);
  }

  @Test
  public void toTextRange_whenMultiLineRangeEndsBeyondEOL_trimsBasedOnEndLineLength() {
    var inputFile = new TestInputFileBuilder("mod", "source.cs")
      .setLanguage("cs")
      .setType(Type.MAIN)
      .setContents("Some text multiline\nRangeWithEndLineOffsetBiggerThanStartLineOffset Some other text")
      .build();
    fs.add(inputFile);
    assertTextRange(toTextRange(inputFile, pbTextRangeOf(1, 10, 2, 64)), 1, 10, 2, 63);
  }

  @Test
  public void toTextRange_whenSingleLineRangeStartsAtEOL_filtersOut() {
    var inputFile = new TestInputFileBuilder("mod", "source.cs")
      .setLanguage("cs")
      .setType(Type.MAIN)
      .setContents("Some text\nSome other text")
      .build();
    fs.add(inputFile);
    assertThat(toTextRange(inputFile, pbTextRangeOf(1, 9, 1, 12))).isEmpty();
    assertThat(toTextRange(inputFile, pbTextRangeOf(1, 9, 1, 9))).isEmpty();
  }

  @Test
  public void toTextRange_whenSingleLineRangeStartsBeyondEOL_filtersOut() {
    var inputFile = new TestInputFileBuilder("mod", "source.cs")
      .setLanguage("cs")
      .setType(Type.MAIN)
      .setContents("Some text\nSome other text")
      .build();
    fs.add(inputFile);
    assertThat(toTextRange(inputFile, pbTextRangeOf(1, 10, 1, 12))).isEmpty();
    assertThat(toTextRange(inputFile, pbTextRangeOf(1, 10, 1, 10))).isEmpty();
  }

  @Test
  public void toTextRange_whenSingleLineRangeEndsBeyondEOL_trimsBasedOnEndLineLength() {
    var inputFile = new TestInputFileBuilder("mod", "source.cs")
      .setLanguage("cs")
      .setType(Type.MAIN)
      .setContents("Some text singleLineRange\n")
      .build();
    fs.add(inputFile);
    assertTextRange(toTextRange(inputFile, pbTextRangeOf(1, 10, 1, 100)), 1, 10, 1, 25);
  }

  private void addFileToFileSystem(String fileName, InputFile.Type fileType, String language) {
    DefaultInputFile inputFile = new TestInputFileBuilder("mod", fileName)
      .setLanguage(language)
      .setType(fileType)
      .build();
    fs.add(inputFile);
  }

  private SonarAnalyzer.TextRange pbTextRangeOf(int startLine, int startLineOffset, int endLine, int endLineOffset) {
    return SonarAnalyzer.TextRange.newBuilder()
      .setStartLine(startLine)
      .setStartOffset(startLineOffset)
      .setEndLine(endLine)
      .setEndOffset(endLineOffset)
      .build();
  }

  private void assertTextRange(Optional<TextRange> textRange, int startLine, int startLineOffset, int endLine, int endLineOffset) {
    textRange.ifPresentOrElse(
      x -> {
        assertThat(x.start().line()).isEqualTo(startLine);
        assertThat(x.start().lineOffset()).isEqualTo(startLineOffset);
        assertThat(x.end().line()).isEqualTo(endLine);
        assertThat(x.end().lineOffset()).isEqualTo(endLineOffset);
      },
      () -> {
        fail("The provided textRange is empty.");
      });
  }
}
