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
package org.sonarsource.dotnet.shared.plugins;

import java.io.IOException;
import java.net.URI;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.util.HashMap;
import java.util.Map;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.fs.InputFile;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class EncodingPerFileTest {
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  private URI fileUri;

  @Before
  public void prepareTestFile() throws IOException {
    fileUri = temp.newFile().toURI();
  }

  @Test
  public void should_treat_as_match_when_roslyn_entry_missing_for_file() throws IOException {
    Charset roslynCharset = StandardCharsets.UTF_8;
    Charset sqCharset = StandardCharsets.UTF_16;

    HashMap<URI, Charset> encodingPerUri = new HashMap<>();
    encodingPerUri.put(fileUri, roslynCharset);
    assertEncodingMatch(encodingPerUri, URI.create("dummy"), sqCharset, true);
  }

  @Test
  public void should_treat_as_mismatch_when_roslyn_encoding_missing() throws IOException {
    HashMap<URI, Charset> encodingPerUri = new HashMap<>();
    encodingPerUri.put(fileUri, null);
    assertEncodingMatch(encodingPerUri, fileUri, null, false);
  }

  @Test
  public void should_treat_as_mismatch_when_roslyn_utf8_and_sq_utf16() throws IOException {
    Charset roslynCharset = StandardCharsets.UTF_8;
    Charset sqCharset = StandardCharsets.UTF_16;

    HashMap<URI, Charset> encodingPerUri = new HashMap<>();
    encodingPerUri.put(fileUri, roslynCharset);
    assertEncodingMatch(encodingPerUri, fileUri, sqCharset, false);
  }

  @Test
  public void should_treat_as_match_when_roslyn_utf16_and_sq_utf16le() throws IOException {
    Charset roslynCharset = StandardCharsets.UTF_16;
    Charset sqCharset = StandardCharsets.UTF_16LE;

    HashMap<URI, Charset> encodingPerUri = new HashMap<>();
    encodingPerUri.put(fileUri, roslynCharset);
    assertEncodingMatch(encodingPerUri, fileUri, sqCharset, true);
  }

  private void assertEncodingMatch(Map<URI, Charset> encodingPerUri, URI fileUri, Charset sqCharset, boolean result) throws IOException {
    AbstractGlobalProtobufFileProcessor processor = mock(AbstractGlobalProtobufFileProcessor.class);
    when(processor.getRoslynEncodingPerUri()).thenReturn(encodingPerUri);
    EncodingPerFile encodingPerFile = new EncodingPerFile(processor);
    InputFile inputFile = newInputFile(fileUri, sqCharset);
    assertThat(encodingPerFile.encodingMatch(inputFile)).isEqualTo(result);
  }

  private InputFile newInputFile(URI uri, Charset charset) {
    InputFile inputFile = mock(InputFile.class);
    when(inputFile.uri()).thenReturn(uri);
    when(inputFile.charset()).thenReturn(charset);
    return inputFile;
  }
}
