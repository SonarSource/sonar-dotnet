/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonarsource.dotnet.shared.plugins.telemetryjson;

import com.google.gson.JsonObject;
import com.google.gson.JsonPrimitive;
import com.google.gson.JsonStreamParser;
import com.google.gson.JsonSyntaxException;
import java.io.IOException;
import java.io.Reader;
import java.util.ArrayList;
import java.util.stream.Stream;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.internal.apachecommons.io.IOUtils;
import org.sonar.api.internal.apachecommons.lang3.tuple.ImmutablePair;

/**
 * Takes a streaming json like
 * <pre>
 *   { key1: 42 }
 *   { key2: "Hello" }
 * </pre>
 * and parses it into a list of pairs. Note that the property names might NOT be unique.
 */
public class TelemetryJsonParser {

  private static final Logger LOG = LoggerFactory.getLogger(TelemetryJsonParser.class);

  public Stream<ImmutablePair<String, String>> parse(Reader jsonReader) {
    JsonStreamParser p = new JsonStreamParser(jsonReader);
    var result = new ArrayList<ImmutablePair<String, String>>();

    try {
      collectTelemetry(p, result);
    } catch (JsonSyntaxException exception) {
      String json = "Reader reset failed";
      try {
        jsonReader.reset();
        json = IOUtils.toString(jsonReader);
      } catch (IOException ignored) {
        // The debug message will be empty. Not the end of the world.
      }
      LOG.debug("Parsing of telemetry failed. JSON: {}", json);
    }

    return result.stream();
  }

  private static void collectTelemetry(JsonStreamParser parser, ArrayList<ImmutablePair<String, String>> result) {
    parser.forEachRemaining(x ->
    {
      if (x instanceof JsonObject object) {   // We expect a stream of something like { key: value }
        for (var entry : object.entrySet()) { // { key1: value1, key2: value2 } is also okay
          if (entry.getValue().isJsonPrimitive()) {
            result.add(ImmutablePair.of(entry.getKey(), getString(entry.getValue().getAsJsonPrimitive())));
          } else {
            LOG.debug("Could not parse telemetry property {}", x);
          }
        }
      } else {
        LOG.debug("Could not parse telemetry entry {}", x);
      }
    });
  }

  private static String getString(JsonPrimitive entry) {
    return entry.isString()
      ? entry.getAsString()
      : entry.toString();
  }
}
