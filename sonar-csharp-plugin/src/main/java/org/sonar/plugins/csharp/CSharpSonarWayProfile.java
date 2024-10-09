/*
 * SonarC#
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
package org.sonar.plugins.csharp;

import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.util.HashSet;
import java.util.Set;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonarsource.dotnet.shared.plugins.AbstractSonarWayProfile;
import org.sonarsource.dotnet.shared.plugins.PluginMetadata;

public class CSharpSonarWayProfile extends AbstractSonarWayProfile {
  private static final Logger LOG = LoggerFactory.getLogger(CSharpSonarWayProfile.class);

  public CSharpSonarWayProfile(PluginMetadata metadata) {
    super(metadata);
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
      Method getRuleKeysMethod = csRulesClass.getMethod("getRepositoryKey");
      return (String) getRuleKeysMethod.invoke(null);
    } catch (ClassNotFoundException | NoSuchMethodException | IllegalAccessException | InvocationTargetException e) {
      LOG.debug("com.sonar.plugins.security.api.CsRules#getRepositoryKey is not found, will use default repository key: {}",
        e.getMessage());
    }
    return metadata.repositoryKey();
  }
}
