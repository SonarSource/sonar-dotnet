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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import com.google.protobuf.Parser;
import java.nio.file.Path;
import java.util.HashSet;
import java.util.Set;
import java.util.function.Function;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonarsource.dotnet.shared.plugins.SensorContextUtils;

public abstract class ProtobufImporter<T> extends RawProtobufImporter<T> {
  private final Function<T, String> toFilePath;
  private final SensorContext context;
  private final Set<Path> filesProcessed = new HashSet<>();

  ProtobufImporter(Parser<T> parser, SensorContext context, Function<T, String> toFilePath) {
    super(parser);
    this.context = context;
    this.toFilePath = toFilePath;
  }

  @Override
  final void consume(T message) {
    InputFile inputFile = SensorContextUtils.toInputFile(context.fileSystem(), toFilePath.apply(message));

    // file may be null because it's not within the project base dir
    if (inputFile == null) {
      return;
    }

    // process each protobuf file only once but allow overriding
    if (isProcessed(inputFile)) {
      return;
    }
    consumeFor(inputFile, message);
  }

  abstract void consumeFor(InputFile inputFile, T message);

  boolean isProcessed(InputFile inputFile) {
    return !filesProcessed.add(inputFile.path().toAbsolutePath());
  }
}
