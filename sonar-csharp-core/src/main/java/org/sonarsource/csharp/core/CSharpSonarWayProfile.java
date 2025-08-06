/*
 * SonarSource :: C# :: Core
 * Copyright (C) 2014-2025 SonarSource SA
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
package org.sonarsource.csharp.core;

import org.sonar.api.rule.RuleKey;
import org.sonar.plugins.csharpenterprise.api.ProfileRegistrar;
import org.sonarsource.dotnet.shared.plugins.AbstractSonarWayProfile;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.RoslynRules;

public class CSharpSonarWayProfile extends AbstractSonarWayProfile {
  private final ProfileRegistrar[] profileRegistrars;

  // The constructors cannot be merged because SonarQube Cloud does not support dependency injection for @Nullable arguments.
  public CSharpSonarWayProfile(PluginMetadata metadata, RoslynRules roslynRules) {
    super(metadata, roslynRules);
    this.profileRegistrars = null;
  }

  public CSharpSonarWayProfile(PluginMetadata metadata, RoslynRules roslynRules, ProfileRegistrar[] profileRegistrars) {
    super(metadata, roslynRules);
    this.profileRegistrars = profileRegistrars;
  }

  @Override
  protected void registerRulesFromRegistrars(NewBuiltInQualityProfile profile) {
    if (profileRegistrars != null) {
      for (var profileRegistrar : profileRegistrars) {
        profileRegistrar.register(rules -> {
          for (RuleKey ruleKey : rules) {
            profile.activateRule(ruleKey.repository(), ruleKey.rule());
          }
        });
      }
    }
  }
}
