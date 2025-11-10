/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
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
package org.sonarsource.dotnet.shared.plugins;

import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;
import java.io.IOException;
import java.io.InputStream;
import java.lang.reflect.Type;
import java.nio.charset.StandardCharsets;
import java.util.List;
import org.sonar.api.server.ServerSide;
import org.sonarsource.api.sonarlint.SonarLintSide;

@ServerSide
@SonarLintSide
public class RoslynRules {
  private static final Gson GSON = new Gson();

  private final PluginMetadata metadata;
  private List<Rule> rules;

  public RoslynRules(PluginMetadata metadata) {
    this.metadata = metadata;
  }

  public List<Rule> rules() {
    if (rules == null) {
      Type ruleListType = new TypeToken<List<Rule>>() {
      }.getType();
      rules = GSON.fromJson(readResource("Rules.json"), ruleListType);
    }
    return rules;
  }

  private String readResource(String name) {
    InputStream stream = getResourceAsStream(metadata.resourcesDirectory() + "/" + name);
    if (stream == null) {
      throw new IllegalStateException("Resource does not exist: " + name);
    }
    try {
      return new String(stream.readAllBytes(), StandardCharsets.UTF_8);
    } catch (IOException e) {
      throw new IllegalStateException("Failed to read: " + name, e);
    }
  }

  // Extracted for testing
  InputStream getResourceAsStream(String name) {
    return getClass().getResourceAsStream(name);
  }

  public static final class Rule {
    String id;
    RuleParameter[] parameters;

    public String getId() {
      return id;
    }
  }

  public static final class RuleParameter {
    String key;
    String description;
    String type;
    String defaultValue;
  }
}
