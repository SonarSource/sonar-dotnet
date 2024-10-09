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
    RuleMetadataLoader ruleMetadataLoader = new RuleMetadataLoader(metadata.resourcesDirectory(), sonarRuntime);
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
