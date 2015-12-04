/*
 * SonarQube C# Plugin
 * Copyright (C) 2014 SonarSource
 * sonarqube@googlegroups.com
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
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package org.sonar.plugins.csharp;

import com.google.common.base.Charsets;
import com.google.common.base.Preconditions;
import com.google.common.collect.ImmutableSet;
import org.sonar.api.BatchExtension;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.api.server.rule.RulesDefinitionXmlLoader;
import org.sonar.squidbridge.rules.SqaleXmlLoader;

import java.io.InputStreamReader;
import java.util.Set;

public class CSharpSonarRulesDefinition implements RulesDefinition, BatchExtension {

  private Set<String> allRuleKeys = null;

  @Override
  public void define(Context context) {
    NewRepository repository = context
      .createRepository(CSharpPlugin.REPOSITORY_KEY, CSharpPlugin.LANGUAGE_KEY)
      .setName(CSharpPlugin.REPOSITORY_NAME);

    RulesDefinitionXmlLoader loader = new RulesDefinitionXmlLoader();
    loader.load(repository, new InputStreamReader(getClass().getResourceAsStream("/org/sonar/plugins/csharp/rules.xml"), Charsets.UTF_8));
    SqaleXmlLoader.load(repository, "/org/sonar/plugins/csharp/sqale.xml");

    ImmutableSet.Builder<String> builder = ImmutableSet.builder();
    for (NewRule rule : repository.rules()) {
      builder.add(rule.key());
    }
    allRuleKeys = builder.build();

    repository.done();
  }

  public Set<String> allRuleKeys() {
    Preconditions.checkNotNull(allRuleKeys);
    return allRuleKeys;
  }

}
