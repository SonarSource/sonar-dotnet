/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2018 SonarSource SA
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

import com.google.gson.Gson;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.reflect.TypeToken;

import javax.annotation.CheckForNull;
import javax.annotation.Nullable;
import java.io.File;
import java.net.URI;
import java.util.ArrayList;
import java.util.Collection;
import java.util.HashMap;
import java.util.Map;
import java.util.function.Function;
import org.sonar.api.batch.fs.InputModule;

class SarifParser10 implements SarifParser {
  private final InputModule inputModule;
  private final JsonObject root;
  private final Function<String, String> toRealPath;

  SarifParser10(InputModule inputModule, JsonObject root, Function<String, String> toRealPath) {
    this.inputModule = inputModule;
    this.root = root;
    this.toRealPath = toRealPath;
  }

  @Override
  public void accept(SarifParserCallback callback) {
    if (!root.has("runs")) {
      return;
    }

    for (JsonElement runElement : root.get("runs").getAsJsonArray()) {
      JsonObject run = runElement.getAsJsonObject();
      JsonArray results = run.getAsJsonArray("results");
      if (results != null) {
        handleIssues(results, callback);
      }
    }
  }

  private void handleIssues(JsonArray results, SarifParserCallback callback) {
    for (JsonElement resultEl : results) {
      JsonObject resultObj = resultEl.getAsJsonObject();
      handleIssue(resultObj, callback);
    }
  }

  private void handleIssue(JsonObject resultObj, SarifParserCallback callback) {
    if (isSuppressed(resultObj)) {
      return;
    }

    String ruleId = resultObj.get("ruleId").getAsString();
    String message = resultObj.get("message").getAsString();
    if (!handleLocationsElement(resultObj, ruleId, message, callback)) {
      callback.onProjectIssue(ruleId, inputModule, message);
    }
  }

  private boolean handleLocationsElement(JsonObject resultObj, String ruleId, String message, SarifParserCallback callback) {
    if (!resultObj.has("locations")) {
      return false;
    }

    JsonArray locations = resultObj.getAsJsonArray("locations");
    if (locations.size() != 1) {
      return false;
    }

    JsonArray relatedLocations = new JsonArray();
    if (resultObj.has("relatedLocations")) {
      relatedLocations = resultObj.getAsJsonArray("relatedLocations");
    }
    Map<String, String> messageMap = new HashMap<>();
    if (resultObj.has("properties")) {
      JsonObject properties = resultObj.getAsJsonObject("properties");
      if (properties.has("customProperties")) {
        messageMap = new Gson().fromJson(properties.get("customProperties"), new TypeToken<Map<String, String>>() {
        }.getType());
      }
    }

    JsonObject firstIssueLocation = locations.get(0).getAsJsonObject().getAsJsonObject("resultFile");
    return handleResultFileElement(ruleId, message, firstIssueLocation, relatedLocations, messageMap, callback);
  }

  private boolean handleResultFileElement(String ruleId, String message, JsonObject resultFileObj, JsonArray relatedLocations,
    Map<String, String> messageMap, SarifParserCallback callback) {
    if (!resultFileObj.has("uri") || !resultFileObj.has("region")) {
      return false;
    }
    Location primaryLocation = handleLocation(resultFileObj, message);

    if (primaryLocation == null) {
      String uri = resultFileObj.get("uri").getAsString();
      String absolutePath = toRealPath.apply(uriToAbsolutePath(uri));
      callback.onFileIssue(ruleId, absolutePath, message);
    } else {
      Collection<Location> secondaryLocations = new ArrayList<>();
      for (JsonElement relatedLocationEl : relatedLocations) {
        JsonObject relatedLocationObj = relatedLocationEl.getAsJsonObject().getAsJsonObject("physicalLocation");
        if (!relatedLocationObj.has("uri")) {
          return false;
        }
        String secondaryMessage = messageMap.getOrDefault(String.valueOf(secondaryLocations.size()), null);
        Location secondaryLocation = handleLocation(relatedLocationObj, secondaryMessage);
        if (secondaryLocation == null) {
          return false;
        }
        secondaryLocations.add(secondaryLocation);
      }
      callback.onIssue(ruleId, primaryLocation, secondaryLocations);
    }

    return true;
  }

  @CheckForNull
  private Location handleLocation(JsonObject locationObj, @Nullable String message) {
    String uri = locationObj.get("uri").getAsString();
    String absolutePath = toRealPath.apply(uriToAbsolutePath(uri));
    JsonObject region = locationObj.get("region").getAsJsonObject();
    int startLine = region.get("startLine").getAsInt();

    JsonElement startColumnOrNull = region.get("startColumn");
    int startColumn = startColumnOrNull != null ? startColumnOrNull.getAsInt() : 1;
    int startLineOffset = startColumn - 1;

    JsonElement lengthOrNull = region.get("length");
    if (lengthOrNull != null) {
      return new Location(absolutePath, message, startLine, startLineOffset, startLine, startLineOffset + lengthOrNull.getAsInt());
    }

    JsonElement endLineOrNull = region.get("endLine");
    int endLine = endLineOrNull != null ? endLineOrNull.getAsInt() : startLine;

    JsonElement endColumnOrNull = region.get("endColumn");
    int endColumn;
    if (endColumnOrNull != null) {
      endColumn = endColumnOrNull.getAsInt();
    } else if (endLineOrNull != null) {
      endColumn = endLine == startLine ? startColumn : 1;
    } else {
      endColumn = startColumn;
    }

    int endLineOffset = endColumn - 1;

    if (startColumn == endColumn && startLine == endLine) {
      return null;
    }

    return new Location(absolutePath, message, startLine, startLineOffset, endLine, endLineOffset);
  }

  private static boolean isSuppressed(JsonObject resultObj) {
    JsonArray suppressionStates = resultObj.getAsJsonArray("suppressionStates");
    if (suppressionStates != null) {
      for (JsonElement entry : suppressionStates) {
        if ("suppressedInSource".equals(entry.getAsString())) {
          return true;
        }
      }
    }

    return false;
  }

  private static String uriToAbsolutePath(String uri) {
    String uriEscaped = uri.replace("[", "%5B").replace("]", "%5D");
    return new File(URI.create(uriEscaped)).getAbsolutePath();
  }

}
