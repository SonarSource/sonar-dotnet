/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonarsource.dotnet.shared.plugins.protobuf;

import com.google.protobuf.Parser;
import java.net.URI;
import java.util.HashSet;
import java.util.Set;
import java.util.function.Function;
import java.util.function.UnaryOperator;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.sensor.SensorContext;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonarsource.dotnet.shared.plugins.SensorContextUtils;

/**
  This class is the base class of all protobuf message importers whose data contain some file path.

  Note that the file path provided by Roslyn might use a different path escaping than the one used by Java, that's why we take a function which transform a path into the
  system real path (this method is OS dependent).
 */
public abstract class ProtobufImporter<T> extends RawProtobufImporter<T> {
  private static final Logger LOG = LoggerFactory.getLogger(ProtobufImporter.class);

  private final Function<T, String> toFilePath;
  private final UnaryOperator<String> toRealPath;
  private final SensorContext context;
  private final Set<URI> filesProcessed = new HashSet<>();

  ProtobufImporter(Parser<T> parser, SensorContext context, Function<T, String> toFilePath, UnaryOperator<String> toRealPath) {
    super(parser);
    this.context = context;
    this.toFilePath = toFilePath;
    this.toRealPath = toRealPath;
  }

  @Override
  final void consume(T message) {
    String filePath = toRealPath.apply(toFilePath.apply(message));
    InputFile inputFile = SensorContextUtils.toInputFile(context.fileSystem(), filePath);

    // file may be null because it's not within the project base dir
    if (inputFile == null) {
      LOG.warn("File '{}' referenced by the protobuf '{}' does not exist in the analysis context", filePath,
        message.getClass().getSimpleName());
      return;
    }

    // process each protobuf file only once but allow overriding
    if (isProcessed(inputFile)) {
      LOG.debug("File '{}' was already processed. Skip it", inputFile);
      return;
    }

    consumeFor(inputFile, message);
  }

  abstract void consumeFor(InputFile inputFile, T message);

  boolean isProcessed(InputFile inputFile) {
    return !filesProcessed.add(inputFile.uri());
  }
}
