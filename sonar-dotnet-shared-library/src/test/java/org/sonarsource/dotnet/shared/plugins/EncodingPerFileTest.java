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

import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Collections;
import java.util.Map;
import org.junit.Test;
import org.sonar.api.SonarQubeVersion;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.utils.Version;
import org.sonarsource.dotnet.shared.plugins.protobuf.EncodingImporter;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class EncodingPerFileTest {
  @Test
  public void should_treat_as_match_when_roslyn_entry_missing_for_file() {
    Path path = Paths.get("dummy");

    EncodingPerFile encodingPerFile = new EncodingPerFile(null, null);
    encodingPerFile.init(path);

    InputFile inputFile = newInputFile(path);
    assertThat(encodingPerFile.encodingMatch(inputFile)).isTrue();
  }

  @Test
  public void should_treat_as_mismatch_when_roslyn_encoding_missing() {
    Path path = Paths.get("dummy").toAbsolutePath();
    Map<Path, Charset> charsetMap = Collections.singletonMap(path, null);
    EncodingPerFile encodingPerFile = new EncodingPerFile(null, null) {
      @Override
      EncodingImporter getEncodingImporter() {
        EncodingImporter encodingImporter = mock(EncodingImporter.class);
        when(encodingImporter.getEncodingPerPath()).thenReturn(charsetMap);
        return encodingImporter;
      }
    };
    encodingPerFile.init(path);

    InputFile inputFile = newInputFile(path);
    assertThat(encodingPerFile.encodingMatch(inputFile)).isFalse();
  }

  @Test
  public void should_treat_as_mismatch_when_roslyn_utf8_and_sq_utf16() {
    Path path = Paths.get("dummy").toAbsolutePath();

    Charset roslynCharset = StandardCharsets.UTF_8;
    Charset sqCharset = StandardCharsets.UTF_16;

    Map<Path, Charset> charsetMap = Collections.singletonMap(path, roslynCharset);

    SonarQubeVersion sqVersion = new SonarQubeVersion(Version.create(6, 1));
    EncodingPerFile encodingPerFile = new EncodingPerFile(null, sqVersion) {
      @Override
      EncodingImporter getEncodingImporter() {
        EncodingImporter encodingImporter = mock(EncodingImporter.class);
        when(encodingImporter.getEncodingPerPath()).thenReturn(charsetMap);
        return encodingImporter;
      }
    };
    encodingPerFile.init(path);

    InputFile inputFile = newInputFile(path, sqCharset);
    assertThat(encodingPerFile.encodingMatch(inputFile)).isFalse();
  }

  @Test
  public void should_treat_as_match_when_roslyn_utf16_and_sq_utf16le() {
    Path path = Paths.get("dummy").toAbsolutePath();

    Charset roslynCharset = StandardCharsets.UTF_16;
    Charset sqCharset = StandardCharsets.UTF_16LE;

    Map<Path, Charset> charsetMap = Collections.singletonMap(path, roslynCharset);

    SonarQubeVersion sqVersion = new SonarQubeVersion(Version.create(6, 1));
    EncodingPerFile encodingPerFile = new EncodingPerFile(null, sqVersion) {
      @Override
      EncodingImporter getEncodingImporter() {
        EncodingImporter encodingImporter = mock(EncodingImporter.class);
        when(encodingImporter.getEncodingPerPath()).thenReturn(charsetMap);
        return encodingImporter;
      }
    };
    encodingPerFile.init(path);

    InputFile inputFile = newInputFile(path, sqCharset);
    assertThat(encodingPerFile.encodingMatch(inputFile)).isTrue();
  }

  private InputFile newInputFile(Path path) {
    return newInputFile(path, StandardCharsets.UTF_8);
  }

  private InputFile newInputFile(Path path, Charset charset) {
    InputFile inputFile = mock(InputFile.class);
    when(inputFile.path()).thenReturn(path);
    when(inputFile.charset()).thenReturn(charset);
    return inputFile;
  }
}
