/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2022 SonarSource SA
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

import com.google.gson.Gson;
import com.google.gson.annotations.SerializedName;
import com.google.gson.reflect.TypeToken;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.lang.reflect.Type;
import java.nio.charset.StandardCharsets;
import java.util.List;
import java.util.Locale;
import java.util.stream.Collectors;
import org.sonar.api.SonarRuntime;
import org.sonar.api.rule.RuleStatus;
import org.sonar.api.rules.RuleType;
import org.sonar.api.scanner.ScannerSide;
import org.sonar.api.server.debt.DebtRemediationFunction;
import org.sonar.api.server.rule.RuleParamType;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.api.utils.Version;

@ScannerSide
public abstract class AbstractRulesDefinition implements RulesDefinition {
  private static final String REPOSITORY_NAME = "SonarAnalyzer";
  private static final Gson GSON = new Gson();

  private final String repositoryKey;
  private final String languageKey;
  private final String resourcesDirectory;
  private final String metadataSuffix;
  private final boolean isOwaspByVersionSupported;
  private final boolean isAddPciDssSupported;
  private final boolean isASVSSupported;

  protected AbstractRulesDefinition(String repositoryKey, String languageKey, SonarRuntime sonarRuntime, String resourcesDirectory, String metadataSuffix) {
    this.repositoryKey = repositoryKey;
    this.languageKey = languageKey;
    this.resourcesDirectory = resourcesDirectory;
    this.metadataSuffix = metadataSuffix;
    this.isOwaspByVersionSupported = sonarRuntime.getApiVersion().isGreaterThanOrEqual(Version.create(9, 3));
    this.isAddPciDssSupported = sonarRuntime.getApiVersion().isGreaterThanOrEqual(Version.create(9, 5));
    this.isASVSSupported = sonarRuntime.getApiVersion().isGreaterThanOrEqual(Version.create(9, 9));
  }

  @Override
  public void define(Context context) {
    NewRepository repository = context
      .createRepository(repositoryKey, languageKey)
      .setName(REPOSITORY_NAME);
    Type ruleListType = new TypeToken<List<Rule>>() {
    }.getType();
    List<Rule> rules = GSON.fromJson(readResource("Rules.json"), ruleListType);
    for (Rule rule : rules) {
      NewRule newRule = repository.createRule(rule.id);
      configureRule(newRule, loadMetadata(rule.id), rule.parameters);
      newRule.setHtmlDescription(readResource(rule.id + metadataSuffix + ".html"));
    }
    repository.done();
  }

  private void configureRule(NewRule rule, RuleMetadata metadata, RuleParameter[] parameters) {
    rule
      .setName(metadata.title)
      .setType(RuleType.valueOf(metadata.type))
      .setStatus(RuleStatus.valueOf(metadata.status.toUpperCase(Locale.ROOT)))
      .setSeverity(metadata.defaultSeverity.toUpperCase(Locale.ROOT))
      .setTags(metadata.tags);
    if (metadata.remediation != null) { // Hotspots do not have remediation
      rule.setDebtRemediationFunction(metadata.remediation.remediationFunction(rule));
      rule.setGapDescription(metadata.remediation.linearDesc);
    }

    for (RuleParameter param : parameters) {
      rule.createParam(param.key)
        .setType(RuleParamType.parse(param.type))
        .setDescription(param.description)
        .setDefaultValue(param.defaultValue);
    }

    addSecurityStandards(rule, metadata.securityStandards);
  }

  private void addSecurityStandards(NewRule rule, SecurityStandards securityStandards) {
    for (String s : securityStandards.owasp2017) {
      rule.addOwaspTop10(RulesDefinition.OwaspTop10.valueOf(s));
    }
    if (isOwaspByVersionSupported) {
      for (String s : securityStandards.owasp2021) {
        rule.addOwaspTop10(RulesDefinition.OwaspTop10Version.Y2021, RulesDefinition.OwaspTop10.valueOf(s));
      }
    }

    addPciDss(rule, securityStandards);
    addASVS(rule, securityStandards);

    rule.addCwe(securityStandards.cwe);
  }

  private void addPciDss(NewRule rule, SecurityStandards securityStandards) {
    if (!isAddPciDssSupported) {
      return;
    }

    if (securityStandards.pciDss3_2.length > 0){
      rule.addPciDss(PciDssVersion.V3_2, securityStandards.pciDss3_2);
    }

    if (securityStandards.pciDss4_0.length > 0){
      rule.addPciDss(PciDssVersion.V4_0, securityStandards.pciDss4_0);
    }
  }

  private void addASVS(NewRule rule, SecurityStandards securityStandards){
    if (!isASVSSupported) {
      return;
    }

    if (securityStandards.asvs4_0.length > 0){
      rule.addOwaspAsvs(OwaspAsvsVersion.V4_0, securityStandards.asvs4_0);
    }
  }

  private RuleMetadata loadMetadata(String id) {
    return GSON.fromJson(readResource(id + metadataSuffix + ".json"), RuleMetadata.class);
  }

  private String readResource(String name) {
    InputStream stream = getResourceAsStream(resourcesDirectory + name);
    if (stream == null) {
      throw new IllegalStateException("Resource does not exist: " + name);
    }
    try (BufferedReader reader = new BufferedReader(new InputStreamReader(stream, StandardCharsets.UTF_8))) {
      return reader.lines().collect(Collectors.joining("\n"));
    } catch (IOException e) {
      throw new IllegalStateException("Failed to read: " + name, e);
    }
  }

  // Extracted for testing
  InputStream getResourceAsStream(String name) {
    return getClass().getResourceAsStream(name);
  }

  private static class Rule {
    String id;
    RuleParameter[] parameters;
  }

  private static class RuleParameter {
    String key;
    String description;
    String type;
    String defaultValue;
  }

  static class RuleMetadata {
    String title;
    String status;
    String type;
    String[] tags;
    String defaultSeverity;
    Remediation remediation;
    SecurityStandards securityStandards = new SecurityStandards();
  }

  private static class Remediation {
    String func;
    String constantCost;
    String linearDesc;
    String linearOffset;
    String linearFactor;

    public DebtRemediationFunction remediationFunction(NewRule rule) {
      if (func.startsWith("Constant")) {
        return rule.debtRemediationFunctions().constantPerIssue(constantCost);
      } else if ("Linear".equals(func)) {
        return rule.debtRemediationFunctions().linear(linearFactor);
      } else {
        return rule.debtRemediationFunctions().linearWithOffset(linearFactor, linearOffset);
      }
    }
  }

  private static class SecurityStandards {
    @SerializedName("CWE")
    int[] cwe = {};

    @SerializedName("OWASP Top 10 2021")
    String[] owasp2021 = {};

    @SerializedName("OWASP")
    String[] owasp2017 = {};

    @SerializedName("PCI DSS 3.2")
    String[] pciDss3_2 = {};

    @SerializedName("PCI DSS 4.0")
    String[] pciDss4_0 = {};

    @SerializedName("ASVS 4.0")
    String[] asvs4_0 = {};
  }
}
