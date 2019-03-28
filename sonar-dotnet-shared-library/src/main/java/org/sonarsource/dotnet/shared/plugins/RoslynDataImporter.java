/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2019 SonarSource SA
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

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.function.UnaryOperator;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.utils.Version;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonarsource.dotnet.shared.sarif.SarifParserCallback;
import org.sonarsource.dotnet.shared.sarif.SarifParserFactory;

@ScannerSide
public class RoslynDataImporter {
  private static final Logger LOG = Loggers.get(RoslynDataImporter.class);
  private final AbstractConfiguration config;

  public RoslynDataImporter(AbstractConfiguration config) {
    this.config = config;
  }

  public void importRoslynReports(List<RoslynReport> reportPaths, final SensorContext context, Map<String, List<RuleKey>> activeRoslynRulesByPartialRepoKey,
    UnaryOperator<String> toRealPath) {
    Map<String, String> repositoryKeyByRoslynRuleKey = getRepoKeyByRoslynRuleKey(activeRoslynRulesByPartialRepoKey);
    boolean ignoreThirdPartyIssues = config.ignoreThirdPartyIssues() || !context.runtime().getApiVersion().isGreaterThanOrEqual(Version.create(7, 4));
    SarifParserCallback callback = new SarifParserCallbackImpl(context, repositoryKeyByRoslynRuleKey, ignoreThirdPartyIssues, config.bugCategories(),
      config.codeSmellCategories(), config.vulnerabilityCategories());

    LOG.info("Importing {} Roslyn {}", reportPaths.size(), pluralize("report", reportPaths.size()));

    for (RoslynReport report : reportPaths) {
      SarifParserFactory.create(report, toRealPath).accept(callback);
    }
  }

  private static Map<String, String> getRepoKeyByRoslynRuleKey(Map<String, List<RuleKey>> activeRoslynRulesByPartialRepoKey) {
    Map<String, String> repositoryKeyByRoslynRuleKey = new HashMap<>();
    for (List<RuleKey> rules : activeRoslynRulesByPartialRepoKey.values()) {
      for (RuleKey activeRoslynRuleKey : rules) {
        String previousRepositoryKey = repositoryKeyByRoslynRuleKey.put(activeRoslynRuleKey.rule(), activeRoslynRuleKey.repository());
        if (previousRepositoryKey != null) {
          throw new IllegalArgumentException("Rule keys must be unique, but \"" + activeRoslynRuleKey.rule() +
            "\" is defined in both the \"" + previousRepositoryKey + "\" and \"" + activeRoslynRuleKey.repository() +
            "\" rule repositories.");
        }
      }
    }
    return repositoryKeyByRoslynRuleKey;
  }

  private static String pluralize(String s, int count) {
    return (count == 1) ? s : (s + "s");
  }
}
