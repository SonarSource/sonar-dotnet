/*
 * SonarSource :: VB.NET :: Core
 * Copyright (C) 2012-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
// This class needs to be in this specific package in order to be accessed by other plugins (e.g. sonar-security-vbnet-frontend-plugin)
// See https://docs.sonarsource.com/sonarqube-server/latest/extension-guide/developing-a-plugin/plugin-basics/#exposing-apis-to-other-plugins
package org.sonar.plugins.vbnetenterprise.api;

import org.sonar.api.rule.RuleKey;
import org.sonar.api.server.ServerSide;
import org.sonarsource.api.sonarlint.SonarLintSide;

import java.util.Collection;

/**
 * This interface can be used to provide additional rule keys in the builtin default quality profile.
 *
 * <pre>
 *   {@code
 *     public void register(RegistrarContext registrarContext) {
 *       registrarContext.registerDefaultQualityProfileRules(ruleKeys);
 *     }
 *   }
 * </pre>
 */
@SonarLintSide
@ServerSide
public interface ProfileRegistrar {

  /**
   * This method is called on server side and during an analysis to modify the builtin default quality profile for vbnet.
   */
  void register(RegistrarContext registrarContext);

  interface RegistrarContext {

    /**
     * Registers additional rules into the "Sonar Way" default quality profile for the language "vbnet".
     */
    void registerDefaultQualityProfileRules(Collection<RuleKey> ruleKeys);
  }
}
