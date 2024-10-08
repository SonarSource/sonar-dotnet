/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
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
package org.sonarsource.dotnet.shared.plugins;

import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.lang.reflect.Type;
import java.nio.charset.StandardCharsets;
import java.util.List;
import java.util.stream.Collectors;
import org.sonar.api.SonarRuntime;
import org.sonar.api.scanner.ScannerSide;
import org.sonar.api.server.rule.RuleParamType;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonarsource.analyzer.commons.RuleMetadataLoader;

@ScannerSide
public abstract class AbstractRulesDefinition implements RulesDefinition {
  private static final String REPOSITORY_NAME = "SonarAnalyzer";
  private static final Gson GSON = new Gson();

  private final String repositoryKey;
  private final String languageKey;
  private final String resourcesDirectory;
  private final SonarRuntime sonarRuntime;

  protected AbstractRulesDefinition(String repositoryKey, String languageKey, String resourcesDirectory, SonarRuntime sonarRuntime) {
    this.repositoryKey = repositoryKey;
    this.languageKey = languageKey;
    this.resourcesDirectory = resourcesDirectory;
    this.sonarRuntime = sonarRuntime;
  }

  @Override
  public void define(Context context) {
    Type ruleListType = new TypeToken<List<Rule>>() {
    }.getType();
    List<Rule> rules = GSON.fromJson(readResource("Rules.json"), ruleListType);

    NewRepository repository = context.createRepository(repositoryKey, languageKey).setName(REPOSITORY_NAME);
    RuleMetadataLoader ruleMetadataLoader = new RuleMetadataLoader(resourcesDirectory, sonarRuntime);
    ruleMetadataLoader.addRulesByRuleKey(repository, rules.stream().map(Rule::getId).toList());

    for (Rule rule : rules) {
      var currentRule = repository.rule(rule.id);
      if (currentRule != null) {
        for (RuleParameter param : rule.parameters) {
          currentRule.createParam(param.key)
            .setType(RuleParamType.parse(param.type))
            .setDescription(param.description)
            .setDefaultValue(param.defaultValue);
        }
      }
    }

    repository.done();
  }

  private String readResource(String name) {
    InputStream stream = getResourceAsStream(resourcesDirectory + "/" + name);
    if (stream == null) {
      throw new IllegalStateException("Resource does not exist: " + name);
    }
    try (BufferedReader reader = new BufferedReader(new InputStreamReader(stream, StandardCharsets.UTF_8))) {
      return reader.lines().collect(Collectors.joining("\n"));
    } catch (IOException e) {
      throw new IllegalStateException("Failed to read: " + name, e);
    }
  }

  // Extracted for testing
  InputStream getResourceAsStream(String name) {
    return getClass().getResourceAsStream(name);
  }

  private static class Rule {
    String id;
    RuleParameter[] parameters;

    public String getId() {
      return id;
    }
  }

  private static class RuleParameter {
    String key;
    String description;
    String type;
    String defaultValue;
  }
}
