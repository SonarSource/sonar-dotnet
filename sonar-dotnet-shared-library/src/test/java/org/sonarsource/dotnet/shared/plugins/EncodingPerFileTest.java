/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2017 SonarSource SA
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

import java.io.IOException;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Optional;
import javax.annotation.Nullable;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.fs.InputFile;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.EncodingInfo;
import org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class EncodingPerFileTest {
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  private Path filePath;

  @Before
  public void prepareTestFile() throws IOException {
    filePath = temp.newFile().toPath();
  }

  @Test
  public void should_treat_as_match_when_roslyn_entry_missing_for_file() throws IOException {
    Charset roslynCharset = StandardCharsets.UTF_8;
    Charset sqCharset = StandardCharsets.UTF_16;

    Path protobufFile = createProtobuf(roslynCharset);
    assertEncodingMatch(Optional.of(protobufFile.getParent()), Paths.get("dummy"), sqCharset, true);
  }

  @Test
  public void should_treat_as_mismatch_when_roslyn_encoding_missing() throws IOException {
    Path protobufFile = createProtobuf(null);
    assertEncodingMatch(Optional.of(protobufFile.getParent()), filePath, null, false);
  }

  @Test
  public void should_treat_as_mismatch_when_roslyn_utf8_and_sq_utf16() throws IOException {
    Charset roslynCharset = StandardCharsets.UTF_8;
    Charset sqCharset = StandardCharsets.UTF_16;

    Path protobufFile = createProtobuf(roslynCharset);
    assertEncodingMatch(Optional.of(protobufFile.getParent()), filePath, sqCharset, false);
  }

  @Test
  public void should_treat_as_match_when_roslyn_utf16_and_sq_utf16le() throws IOException {
    Charset roslynCharset = StandardCharsets.UTF_16;
    Charset sqCharset = StandardCharsets.UTF_16LE;

    Path protobufFile = createProtobuf(roslynCharset);
    assertEncodingMatch(Optional.of(protobufFile.getParent()), filePath, sqCharset, true);
  }

  private void assertEncodingMatch(Optional<Path> protobufDir, Path filePath, Charset sqCharset, boolean result) throws IOException {
    EncodingPerFile encodingPerFile = new EncodingPerFile();
    encodingPerFile.init(protobufDir);
    InputFile inputFile = newInputFile(filePath, sqCharset);
    assertThat(encodingPerFile.encodingMatch(inputFile)).isEqualTo(result);
  }

  private Path createProtobuf(@Nullable Charset roslynCharset) throws IOException {
    Path path = temp.newFolder("proto").toPath().resolve(ProtobufImporters.ENCODING_OUTPUT_PROTOBUF_NAME);
    EncodingInfo.Builder infoBuilder = EncodingInfo.newBuilder()
      .setFilePath(filePath.toAbsolutePath().toString());

    if (roslynCharset != null) {
      infoBuilder.setEncoding(roslynCharset.name());
    }
    infoBuilder.build().writeDelimitedTo(Files.newOutputStream(path));
    return path;
  }

  private InputFile newInputFile(Path path, Charset charset) {
    InputFile inputFile = mock(InputFile.class);
    when(inputFile.path()).thenReturn(path);
    when(inputFile.charset()).thenReturn(charset);
    return inputFile;
  }
}
