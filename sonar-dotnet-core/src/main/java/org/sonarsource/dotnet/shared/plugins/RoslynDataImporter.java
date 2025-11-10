/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
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
package org.sonarsource.dotnet.shared.plugins;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.function.UnaryOperator;
import org.sonar.api.batch.rule.ActiveRule;
import org.sonar.api.batch.rule.ActiveRules;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.scanner.ScannerSide;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonarsource.dotnet.shared.StringUtils;
import org.sonarsource.dotnet.shared.sarif.SarifParserCallback;
import org.sonarsource.dotnet.shared.sarif.SarifParserFactory;

import static org.sonarsource.dotnet.shared.CallableUtils.lazy;

@ScannerSide
public class RoslynDataImporter {
  private static final Logger LOG = LoggerFactory.getLogger(RoslynDataImporter.class);
  private static final String ROSLYN_REPOSITORY_PREFIX = "roslyn.";
  private final AbstractLanguageConfiguration config;
  private final PluginMetadata metadata;

  public RoslynDataImporter(PluginMetadata metadata, AbstractLanguageConfiguration config) {
    this.metadata = metadata;
    this.config = config;
  }

  public void importRoslynReports(List<RoslynReport> reports,  final SensorContext context,
    UnaryOperator<String> toRealPath) {
    Map<String, String> repositoryKeyByRoslynRuleKey = repoKeyByRoslynRuleKey(context.activeRules());
    boolean ignoreThirdPartyIssues = config.ignoreThirdPartyIssues();
    SarifParserCallback callback = new SarifParserCallbackImpl(context, repositoryKeyByRoslynRuleKey, ignoreThirdPartyIssues, config.bugCategories(),
      config.codeSmellCategories(), config.vulnerabilityCategories());

    LOG.info("Importing {} Roslyn {}", reports.size(), lazy(() -> StringUtils.pluralize("report", reports.size())));

    for (RoslynReport report : reports) {
      LOG.debug("Processing Roslyn report: {}", report.getReportPath());
      SarifParserFactory.create(report, toRealPath).accept(callback);
    }
  }

  private Map<String, String> repoKeyByRoslynRuleKey(ActiveRules activeRules){
    Map<String, String> repositoryKeyByRoslynRuleKey = new HashMap<>();
    for (ActiveRule activeRule : activeRules.findAll()) {
      if (activeRule.ruleKey().repository().startsWith(ROSLYN_REPOSITORY_PREFIX) || metadata.repositoryKey().equals(activeRule.ruleKey().repository())) {
        String previousRepositoryKey = repositoryKeyByRoslynRuleKey.put(activeRule.ruleKey().rule(), activeRule.ruleKey().repository());
        if (previousRepositoryKey != null) {
          throw new IllegalArgumentException("Rule keys must be unique, but \"" + activeRule.ruleKey().rule() +
            "\" is defined in both the \"" + previousRepositoryKey + "\" and \"" + activeRule.ruleKey().repository() +
            "\" rule repositories.");
        }
      }
    }
    return repositoryKeyByRoslynRuleKey;
  }
}
