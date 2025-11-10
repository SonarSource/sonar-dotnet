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

import org.sonar.api.SonarRuntime;
import org.sonar.api.server.rule.RuleParamType;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonarsource.analyzer.commons.RuleMetadataLoader;

public class DotNetRulesDefinition implements RulesDefinition {
  private static final String REPOSITORY_NAME = "SonarAnalyzer";

  private final PluginMetadata metadata;
  private final SonarRuntime sonarRuntime;
  private final RoslynRules roslynRules;

  public DotNetRulesDefinition(PluginMetadata metadata, SonarRuntime sonarRuntime, RoslynRules roslynRules) {
    this.metadata = metadata;
    this.sonarRuntime = sonarRuntime;
    this.roslynRules = roslynRules;
  }

  @Override
  public void define(Context context) {
    NewRepository repository = context.createRepository(metadata.repositoryKey(), metadata.languageKey()).setName(REPOSITORY_NAME);
    // Path to SonarWay JSON sets the rule.setActivatedByDefault(true) that is needed by SonarLint in standalone mode
    RuleMetadataLoader ruleMetadataLoader = new RuleMetadataLoader(metadata.resourcesDirectory(), metadata.resourcesDirectory() + "/Sonar_way_profile.json", sonarRuntime);
    ruleMetadataLoader.addRulesByRuleKey(repository, roslynRules.rules().stream().map(RoslynRules.Rule::getId).toList());

    for (RoslynRules.Rule rule : roslynRules.rules()) {
      var currentRule = repository.rule(rule.id);
      if (currentRule != null) {
        for (RoslynRules.RuleParameter param : rule.parameters) {
          currentRule.createParam(param.key)
            .setType(RuleParamType.parse(param.type))
            .setDescription(param.description)
            .setDefaultValue(param.defaultValue);
        }
      }
    }

    repository.done();
  }
}
