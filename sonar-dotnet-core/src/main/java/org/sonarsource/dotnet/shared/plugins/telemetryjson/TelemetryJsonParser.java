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

import com.google.gson.JsonIOException;
import com.google.gson.JsonObject;
import com.google.gson.JsonPrimitive;
import com.google.gson.JsonStreamParser;
import com.google.gson.JsonSyntaxException;
import java.io.Reader;
import java.util.ArrayList;
import java.util.Map;
import java.util.stream.Stream;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

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

  public Stream<Map.Entry<String, String>> parse(Reader jsonReader) {
    JsonStreamParser p = new JsonStreamParser(jsonReader);
    var result = new ArrayList<Map.Entry<String, String>>();

    try {
      collectTelemetry(p, result);
    } catch (JsonSyntaxException exception) {
      LOG.debug("Parsing of telemetry failed.");
    } catch (JsonIOException exception) {
      LOG.debug("Telemetry is empty.");
    }

    return result.stream();
  }

  private static void collectTelemetry(JsonStreamParser parser, ArrayList<Map.Entry<String, String>> result) {
    parser.forEachRemaining(x ->
    {
      if (x instanceof JsonObject object) {   // We expect a stream of something like { key: value }
        for (var entry : object.entrySet()) { // { key1: value1, key2: value2 } is also okay
          if (entry.getValue().isJsonPrimitive()) {
            result.add(Map.entry(entry.getKey(), getString(entry.getValue().getAsJsonPrimitive())));
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
