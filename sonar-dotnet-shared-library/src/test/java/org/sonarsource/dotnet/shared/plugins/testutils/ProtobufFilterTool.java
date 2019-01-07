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
package org.sonarsource.dotnet.shared.plugins.testutils;

import com.google.protobuf.MessageLite;
import com.google.protobuf.Parser;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Optional;
import java.util.function.Function;
import java.util.function.Predicate;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;

import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.CPDTOKENS_OUTPUT_PROTOBUF_NAME;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.HIGHLIGHT_OUTPUT_PROTOBUF_NAME;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.METRICS_OUTPUT_PROTOBUF_NAME;
import static org.sonarsource.dotnet.shared.plugins.protobuf.ProtobufImporters.SYMBOLREFS_OUTPUT_PROTOBUF_NAME;

/**
 * Utility class to filter protobuf binary files to contain a single input-file (Program.cs)
 *
 * See src/test/resources/ProtobufImporterTest/README.md for explanation.
 */
public class ProtobufFilterTool {

  private static final File TEST_DATA_DIR = new File("src/test/resources/ProtobufImporterTest");
  private static final String TEST_FILENAME = "Program.cs";

  public static void main(String[] args) throws IOException {
    String pathSuffix = "\\" + TEST_FILENAME;

    rewrite(SYMBOLREFS_OUTPUT_PROTOBUF_NAME, SonarAnalyzer.SymbolReferenceInfo.parser(),
      m -> m.getFilePath().endsWith(pathSuffix), m -> m.toBuilder().setFilePath(TEST_FILENAME).build());

    rewrite(HIGHLIGHT_OUTPUT_PROTOBUF_NAME, SonarAnalyzer.TokenTypeInfo.parser(),
      m -> m.getFilePath().endsWith(pathSuffix), m -> m.toBuilder().setFilePath(TEST_FILENAME).build());

    rewrite(CPDTOKENS_OUTPUT_PROTOBUF_NAME, SonarAnalyzer.CopyPasteTokenInfo.parser(),
      m -> m.getFilePath().endsWith(pathSuffix), m -> m.toBuilder().setFilePath(TEST_FILENAME).build());

    rewrite(METRICS_OUTPUT_PROTOBUF_NAME, SonarAnalyzer.MetricsInfo.parser(),
      m -> m.getFilePath().endsWith(pathSuffix), m -> m.toBuilder().setFilePath(TEST_FILENAME).build());

  }

  private static <T extends MessageLite> void rewrite(String filename, Parser<T> parser, Predicate<T> predicate, Function<T, T> rewriter) throws IOException {
    Path path = new File(TEST_DATA_DIR, filename).toPath();
    readFirstMatching(path, parser, predicate).ifPresent(m -> save(path, rewriter.apply(m)));
  }

  private static <T> Optional<T> readFirstMatching(Path path, Parser<T> parser, Predicate<T> predicate) {
    try (InputStream inputStream = Files.newInputStream(path)) {
      while (true) {
        T message = parser.parseDelimitedFrom(inputStream);
        if (message == null) {
          break;
        }
        if (predicate.test(message)) {
          return Optional.of(message);
        }
      }
    } catch (IOException e) {
      throw new IllegalStateException("unexpected error while parsing protobuf file: " + path, e);
    }
    return Optional.empty();
  }

  private static <T extends MessageLite> void save(Path path, T message) {
    try (OutputStream output = Files.newOutputStream(path)) {
      message.writeDelimitedTo(output);
    } catch (IOException e) {
      throw new IllegalStateException("could not save message to file: " + path + " " + message, e);
    }
  }
}
