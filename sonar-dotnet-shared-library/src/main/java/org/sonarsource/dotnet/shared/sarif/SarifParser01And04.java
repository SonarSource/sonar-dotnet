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

import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import java.util.ArrayList;
import java.util.Collection;
import java.util.function.Function;
import java.util.function.UnaryOperator;
import javax.annotation.CheckForNull;
import javax.annotation.Nullable;
import org.sonar.api.batch.fs.InputModule;

class SarifParser01And04 implements SarifParser {
  private static final String FILE_PROTOCOL = "file:///";
  private final InputModule inputModule;
  private final JsonObject root;
  private final UnaryOperator<String> toRealPath;

  SarifParser01And04(InputModule inputModule, JsonObject root, UnaryOperator<String> toRealPath) {
    this.inputModule = inputModule;
    this.root = root;
    this.toRealPath = toRealPath;
  }

  @Override
  public void accept(SarifParserCallback callback) {
    if (root.has("runLogs")) {
      JsonElement runLogs = root.get("runLogs");
      for (JsonElement runLogElement : runLogs.getAsJsonArray()) {
        JsonObject runLog = runLogElement.getAsJsonObject();
        JsonArray results = runLog.getAsJsonArray("results");
        if (results != null) {
          handleIssues(results, false, callback);
        }
      }
    } else if (root.has("issues")) {
      JsonElement issues = root.get("issues");
      handleIssues(issues.getAsJsonArray(), true, callback);
    }
  }

  private void handleIssues(JsonArray issues, boolean offsetStartAtZero, SarifParserCallback callback) {
    for (JsonElement issueElement : issues) {
      JsonObject issue = issueElement.getAsJsonObject();
      handleIssue(issue, offsetStartAtZero, callback);
    }
  }

  private void handleIssue(JsonObject issue, boolean offsetStartAtZero, SarifParserCallback callback) {
    if (isSuppressed(issue)) {
      return;
    }

    String ruleId = issue.get("ruleId").getAsString();
    String message = issue.get(issue.has("shortMessage") ? "shortMessage" : "fullMessage").getAsString();

    JsonArray locationsArray = issue.getAsJsonArray("locations");
    if (locationsArray.size() == 0) {
      callback.onProjectIssue(ruleId, null, inputModule, message);
      return;
    }

    JsonObject primaryLocationObject = getAnalysisTargetAt(locationsArray, 0);
    if (primaryLocationObject == null) {
      callback.onProjectIssue(ruleId, null, inputModule, message);
      return;
    }

    String primaryLocationPath = toRealPath.apply(uriToAbsolutePath(primaryLocationObject.get("uri").getAsString()));
    Location primaryLocation = getLocation(offsetStartAtZero, primaryLocationObject, primaryLocationPath, message);
    if (primaryLocation == null) {
      callback.onFileIssue(ruleId, null, primaryLocationPath, message);
      return;
    }

    Collection<Location> secondaryLocations = new ArrayList<>();
    for (int i = 1; i < locationsArray.size(); i++) {
      JsonObject secondaryLocationObject = getAnalysisTargetAt(locationsArray, i);
      if (secondaryLocationObject != null) {
        String secondaryLocationPath = toRealPath.apply(uriToAbsolutePath(secondaryLocationObject.get("uri").getAsString()));
        Location secondaryLocation = getLocation(offsetStartAtZero, secondaryLocationObject, secondaryLocationPath, getSecondaryMessage(issue, i - 1));
        if (secondaryLocation != null) {
          secondaryLocations.add(secondaryLocation);
        }
      }
    }

    callback.onIssue(ruleId, null, primaryLocation, secondaryLocations);
  }

  @CheckForNull
  private static String getSecondaryMessage(JsonObject issue, int index) {
    JsonObject properties = issue.getAsJsonObject("properties");
    if (properties == null) {
      return null;
    }

    JsonElement messageElement = properties.get("customProperties." + index);
    if (messageElement == null) {
      return null;
    }

    return messageElement.getAsString();
  }

  @CheckForNull
  private static JsonObject getAnalysisTargetAt(JsonArray locationsArray, int index) {
    JsonObject analysisTargetWrapper = locationsArray.get(index).getAsJsonObject();

    JsonArray analysisTargetArray = analysisTargetWrapper.getAsJsonArray("analysisTarget");
    if (analysisTargetArray == null || analysisTargetArray.size() == 0) {
      return null;
    }

    return analysisTargetArray.get(0).getAsJsonObject();
  }

  private static Location getLocation(boolean offsetStartAtZero, JsonObject analysisTarget, String absolutePath, @Nullable String message) {
    JsonObject region = analysisTarget.getAsJsonObject("region");
    int startLine = region.get("startLine").getAsInt();
    int startLineFixed = offsetStartAtZero ? (startLine + 1) : startLine;

    JsonElement startColumnOrNull = region.get("startColumn");
    int startColumn = startColumnOrNull != null ? startColumnOrNull.getAsInt() : 1;
    int startLineOffset = offsetStartAtZero ? startColumn : (startColumn - 1);

    JsonElement lengthOrNull = region.get("length");
    if (lengthOrNull != null) {
      return new Location(absolutePath, message, startLineFixed, startLineOffset, startLineFixed, startLineOffset + lengthOrNull.getAsInt());
    }

    JsonElement endLineOrNull = region.get("endLine");
    int endLine = endLineOrNull != null ? endLineOrNull.getAsInt() : startLine;
    int endLineFixed = offsetStartAtZero ? (endLine + 1) : endLine;

    JsonElement endColumnOrNull = region.get("endColumn");
    int endColumn;
    if (endColumnOrNull != null) {
      endColumn = endColumnOrNull.getAsInt();
    } else if (endLineOrNull != null) {
      endColumn = endLine == startLine ? startColumn : 1;
    } else {
      endColumn = startColumn;
    }

    int endLineOffset = offsetStartAtZero ? endColumn : (endColumn - 1);

    if (startColumn == endColumn && startLineFixed == endLineFixed) {
      return null;
    }

    return new Location(absolutePath, message, startLineFixed, startLineOffset, endLineFixed, endLineOffset);
  }

  private static boolean isSuppressed(JsonObject issue) {
    JsonElement isSuppressedInSource = issue.get("isSuppressedInSource");
    if (isSuppressedInSource != null) {
      return isSuppressedInSource.getAsBoolean();
    }

    JsonElement properties = issue.get("properties");
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

    return uri.substring(FILE_PROTOCOL.length()).replace('/', '\\');
  }
}
