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
/*
   * SonarC#
   * Copyright (C) 2014-2017 SonarSource SA
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

import java.io.InputStreamReader;
import java.nio.charset.StandardCharsets;
import java.util.Collections;
import java.util.LinkedHashSet;
import java.util.Set;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.api.server.rule.RulesDefinitionXmlLoader;

import static java.util.Objects.requireNonNull;

@ScannerSide
public abstract class AbstractRulesDefinition implements RulesDefinition {
  private Set<String> allRuleKeys = null;

  private final String repositoryKey;
  private final String repositoryName;
  private final String languageKey;
  private final String rulesXmlFilePath;

  protected AbstractRulesDefinition(String repositoryKey, String repositoryName, String languageKey, String rulesXmlFilePath) {
    this.repositoryKey = repositoryKey;
    this.repositoryName = repositoryName;
    this.languageKey = languageKey;
    this.rulesXmlFilePath = rulesXmlFilePath;

  }

  @Override
  public void define(Context context) {
    NewRepository repository = context
      .createRepository(repositoryKey, languageKey)
      .setName(repositoryName);

    RulesDefinitionXmlLoader loader = new RulesDefinitionXmlLoader();
    loader.load(repository, new InputStreamReader(getClass().getResourceAsStream(rulesXmlFilePath), StandardCharsets.UTF_8));

    allRuleKeys = new LinkedHashSet<>();
    for (NewRule rule : repository.rules()) {
      allRuleKeys.add(rule.key());
    }

    allRuleKeys = Collections.unmodifiableSet(allRuleKeys);
    repository.done();
  }

  public Set<String> allRuleKeys() {
    requireNonNull(allRuleKeys);
    return allRuleKeys;
  }
}
