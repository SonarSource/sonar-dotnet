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
package org.sonarsource.dotnet.shared.sarif;

import java.io.IOException;
import java.io.InputStreamReader;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.util.function.Function;

import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import java.util.function.UnaryOperator;
import org.sonarsource.dotnet.shared.plugins.RoslynReport;

public class SarifParserFactory {
  private SarifParserFactory() {
    // private
  }

  public static SarifParser create(RoslynReport report, UnaryOperator<String> toRealPath) {
    try (InputStreamReader reader = new InputStreamReader(Files.newInputStream(report.getReportPath()), StandardCharsets.UTF_8)) {

      JsonParser parser = new JsonParser();
      JsonObject root = parser.parse(reader).getAsJsonObject();
      if (root.has("version")) {
        String version = root.get("version").getAsString();

        switch (version) {
          case "0.4":
          case "0.1":
            return new SarifParser01And04(report.getModule(), root, toRealPath);
          case "1.0":
          default:
            return new SarifParser10(report.getModule(),root, toRealPath);
        }
      }
    } catch (IOException e) {
      throw new IllegalStateException("Unable to read the Roslyn SARIF report file: " + report.getReportPath().toAbsolutePath(), e);
    }

    throw new IllegalStateException(String.format("Unable to parse the Roslyn SARIF report file: %s. Unrecognized format", report.getReportPath().toAbsolutePath()));
  }
}
