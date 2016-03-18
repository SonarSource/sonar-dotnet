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
package org.sonar.plugins.csharp;

import com.google.common.base.Charsets;
import com.google.common.io.Files;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import java.io.File;
import java.io.IOException;

public class SarifParser {

  private SarifParserCallback callback;

  public SarifParser(SarifParserCallback callback) {
    this.callback = callback;
  }

  public void parse(File file) {
    String contents;
    try {
      contents = Files.toString(file, Charsets.UTF_8);
    } catch (IOException e) {
      throw new IllegalStateException("Unable to read the Roslyn SARIF report file: " + file.getAbsolutePath(), e);
    }

    JsonParser parser = new JsonParser();
    JsonElement issues = parser.parse(contents).getAsJsonObject().get("issues");
    if (issues != null) {
      for (JsonElement issueElement : issues.getAsJsonArray()) {
        JsonObject issue = issueElement.getAsJsonObject();

        String ruleId = issue.get("ruleId").getAsString();

        String message = issue.get(issue.has("shortMessage") ? "shortMessage" : "fullMessage").getAsString();
        boolean hasLocation = false;
        for (JsonElement locationElement : issue.get("locations").getAsJsonArray()) {
          JsonObject location = locationElement.getAsJsonObject();
          if (location.has("analysisTarget")) {
            for (JsonElement analysisTargetElement : location.get("analysisTarget").getAsJsonArray()) {
              hasLocation = true;
              JsonObject analysisTarget = analysisTargetElement.getAsJsonObject();
              String uri = analysisTarget.get("uri").getAsString();
              JsonObject region = analysisTarget.get("region").getAsJsonObject();
              int startLine = region.get("startLine").getAsInt();

              callback.onIssue(ruleId, uri, message, startLine);
            }
          }
        }

        if (!hasLocation) {
          callback.onProjectIssue(ruleId, message);
        }
      }
    }
  }

}
