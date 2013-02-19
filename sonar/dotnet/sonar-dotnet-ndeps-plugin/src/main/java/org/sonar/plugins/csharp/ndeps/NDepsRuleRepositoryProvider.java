/*
 * Sonar .NET Plugin :: NDeps
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
package org.sonar.plugins.csharp.ndeps;

import org.sonar.api.rules.RuleRepository;

import com.google.common.collect.Lists;

import org.sonar.api.ExtensionProvider;
import org.sonar.api.ServerExtension;
import java.util.List;

public class NDepsRuleRepositoryProvider  extends ExtensionProvider implements ServerExtension {

  @Override
  public Object provide() {
    List<RuleRepository> extensions = Lists.newArrayList();
    for (String languageKey : NDepsConstants.SUPPORTED_LANGUAGES) {
      String repoKey = NDepsConstants.REPOSITORY_KEY + "-" + languageKey;
      extensions.add(new NDepsRuleRepository(repoKey, languageKey).setName("NDeps"));
    }
    return extensions;
  }
}
