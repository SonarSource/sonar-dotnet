/*
 * SonarQube C# Plugin
 * Copyright (C) 2014-2016 SonarSource SA
 * mailto:contact AT sonarsource DOT com
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
package org.sonar.plugins.csharp.sarif;

import java.io.File;
import java.io.IOException;

import com.google.common.base.Charsets;
import com.google.common.io.Files;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

public class SarifParserFactory {
  private SarifParserFactory() {
    // private
  }

  public static SarifParser create(File file) {
    String contents;
    try {
      contents = Files.toString(file, Charsets.UTF_8);
    } catch (IOException e) {
      throw new IllegalStateException("Unable to read the Roslyn SARIF report file: " + file.getAbsolutePath(), e);
    }

    JsonParser parser = new JsonParser();
    JsonObject root = parser.parse(contents).getAsJsonObject();
    if (root.has("version")) {
      String version = root.get("version").getAsString();

      switch (version) {
        case "0.4":
        case "0.1":
          return new SarifParser01And04(contents);
        case "1.0":
        default:
          return new SarifParser10(contents);
      }
    }

    throw new IllegalStateException(String.format("Unable to parse the Roslyn SARIF report file: %s. Unrecognized format", file.getAbsolutePath()));
  }
}
