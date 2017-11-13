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
package org.sonar.plugins.csharp;

import java.io.InputStreamReader;
import java.nio.charset.StandardCharsets;
import java.util.LinkedHashSet;
import java.util.Set;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.api.server.rule.RulesDefinitionXmlLoader;

import static java.util.Objects.requireNonNull;

@ScannerSide
public class CSharpSonarRulesDefinition implements RulesDefinition {

  static final String REPOSITORY_KEY = "csharpsquid";
  private static final String REPOSITORY_NAME = "SonarAnalyzer";

  private Set<String> allRuleKeys = null;

  @Override
  public void define(Context context) {
    NewRepository repository = context
      .createRepository(REPOSITORY_KEY, CSharpPlugin.LANGUAGE_KEY)
      .setName(REPOSITORY_NAME);

    RulesDefinitionXmlLoader loader = new RulesDefinitionXmlLoader();
    loader.load(repository, new InputStreamReader(getClass().getResourceAsStream("/org/sonar/plugins/csharp/rules.xml"), StandardCharsets.UTF_8));

    allRuleKeys = new LinkedHashSet<>();
    for (NewRule rule : repository.rules()) {
      allRuleKeys.add(rule.key());
    }

    repository.done();
  }

  Set<String> allRuleKeys() {
    requireNonNull(allRuleKeys);
    return allRuleKeys;
  }

}
