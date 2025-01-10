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

import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.util.HashSet;
import java.util.Set;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonarsource.dotnet.shared.plugins.AbstractSonarWayProfile;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;
import org.sonarsource.dotnet.shared.plugins.RoslynRules;

public class CSharpSonarWayProfile extends AbstractSonarWayProfile {
  private static final Logger LOG = LoggerFactory.getLogger(CSharpSonarWayProfile.class);

  public CSharpSonarWayProfile(PluginMetadata metadata, RoslynRules roslynRules) {
    super(metadata, roslynRules);
  }

  @Override
  protected void activateSecurityRules(NewBuiltInQualityProfile sonarWay) {
    final String securityRepositoryKey = getSecurityRepositoryKey();
    try {
      getSecurityRuleKeys().forEach(key -> sonarWay.activateRule(securityRepositoryKey, key));
    } catch (IllegalArgumentException | IllegalStateException e) {
      LOG.warn("Could not activate C# security rules", e);
    }
  }

  private static Set<String> getSecurityRuleKeys() {
    try {
      Class<?> csRulesClass = Class.forName("com.sonar.plugins.security.api.CsRules");
      Method getRuleKeysMethod = csRulesClass.getMethod("getRuleKeys");
      return (Set<String>) getRuleKeysMethod.invoke(null);
    } catch (ClassNotFoundException | NoSuchMethodException e) {
      LOG.debug("com.sonar.plugins.security.api.CsRules#getRuleKeys is not found, no security rules added to Sonar way cs profile: {}",
        e.getMessage());
    } catch (IllegalAccessException | InvocationTargetException e) {
      LOG.debug("[{}] No security rules added to Sonar way cs profile: {}", e.getClass().getName(), e.getMessage());
    }

    return new HashSet<>();
  }

  private String getSecurityRepositoryKey() {
    try {
      Class<?> csRulesClass = Class.forName("com.sonar.plugins.security.api.CsRules");
      Method getRepositoryKeyMethod = csRulesClass.getMethod("getRepositoryKey");
      return (String) getRepositoryKeyMethod.invoke(null);
    } catch (ClassNotFoundException | NoSuchMethodException | IllegalAccessException | InvocationTargetException e) {
      LOG.debug("com.sonar.plugins.security.api.CsRules#getRepositoryKey is not found, will use default repository key: {}",
        e.getMessage());
    }
    return metadata.repositoryKey();
  }
}
