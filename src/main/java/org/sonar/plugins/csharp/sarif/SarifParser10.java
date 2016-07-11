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
import java.net.URI;

import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

/**
 * Parser for SARIF report v1.0 (introduced in VS 2015 update 3)
 */
public class SarifParser10 implements SarifParser {
  private static final String FILE_PROTOCOL = "file:///";
  private String contents;

  SarifParser10(String contents) {
    this.contents = contents;
  }

  @Override
  public void parse(SarifParserCallback callback) {
    JsonParser parser = new JsonParser();
    JsonObject root = parser.parse(contents).getAsJsonObject();
    JsonElement runLogs = root.get("runs");

    if (runLogs == null) {
      return;
    }
    for (JsonElement runElement : runLogs.getAsJsonArray()) {
      JsonObject run = runElement.getAsJsonObject();
      JsonArray results = run.getAsJsonArray("results");
      if (results != null) {
        handleIssues(results, callback);
      }
    }
  }

  private static void handleIssues(JsonArray results, SarifParserCallback callback) {
    for (JsonElement resultEl : results) {
      JsonObject resultObj = resultEl.getAsJsonObject();
      handleIssue(resultObj, callback);
    }
  }

  private static void handleIssue(JsonObject resultObj, SarifParserCallback callback) {
    if (isSuppressed(resultObj)) {
      return;
    }

    String ruleId = resultObj.get("ruleId").getAsString();
    String message = resultObj.get("message").getAsString();

    boolean hasLocation = false;
    if (resultObj.has("locations")) {
      for (JsonElement locationEl : resultObj.get("locations").getAsJsonArray()) {
        JsonObject locationObj = locationEl.getAsJsonObject();
        if (locationObj.has("resultFile")) {
          JsonObject resultFileObj = locationObj.get("resultFile").getAsJsonObject();

          if (!resultFileObj.has("uri") || !resultFileObj.has("region")) {
            continue;
          }

          String uri = resultFileObj.get("uri").getAsString();
          String absolutePath = uriToAbsolutePath(uri);
          JsonObject region = resultFileObj.get("region").getAsJsonObject();
          int startLine = region.get("startLine").getAsInt();

          callback.onIssue(ruleId, absolutePath, message, startLine);
          hasLocation = true;
        }
      }
    }

    if (!hasLocation) {
      callback.onProjectIssue(ruleId, message);
    }
  }

  private static boolean isSuppressed(JsonObject resultObj) {
    JsonElement isSuppressedInSource = resultObj.get("isSuppressedInSource");
    if (isSuppressedInSource != null) {
      return isSuppressedInSource.getAsBoolean();
    }

    JsonElement properties = resultObj.get("properties");
    if (properties != null && properties.isJsonObject()) {
      isSuppressedInSource = properties.getAsJsonObject().get("isSuppressedInSource");
      if (isSuppressedInSource != null) {
        return isSuppressedInSource.getAsBoolean();
      }
    }

    return false;
  }

  private static String uriToAbsolutePath(String uri) {
    if (!uri.startsWith(FILE_PROTOCOL)) {
      return uri;
    }

    return new File(URI.create(uri)).getAbsolutePath();
  }

}
