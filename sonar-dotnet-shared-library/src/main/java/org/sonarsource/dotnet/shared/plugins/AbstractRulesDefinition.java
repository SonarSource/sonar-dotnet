/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2020 SonarSource SA
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
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.nio.charset.StandardCharsets;
import java.util.Collection;
import java.util.Map;
import java.util.Set;
import java.util.stream.Collectors;
import org.sonar.api.rules.RuleType;
import org.sonar.api.scanner.ScannerSide;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.api.server.rule.RulesDefinitionXmlLoader;

@ScannerSide
public abstract class AbstractRulesDefinition implements RulesDefinition {
  private static final Gson GSON = new Gson();

  private final String repositoryKey;
  private final String repositoryName;
  private final String languageKey;
  private final String rulesXmlFilePath;

  protected AbstractRulesDefinition(String repositoryKey, String repositoryName, String languageKey, String rulesXmlFilePath) {
    this.repositoryKey = repositoryKey;
    this.repositoryName = repositoryName;
    this.languageKey = languageKey;
    this.rulesXmlFilePath = rulesXmlFilePath;
  }

  @Override
  public void define(Context context) {
    NewRepository repository = context
      .createRepository(repositoryKey, languageKey)
      .setName(repositoryName);

    RulesDefinitionXmlLoader loader = new RulesDefinitionXmlLoader();
    loader.load(repository, new InputStreamReader(getClass().getResourceAsStream(rulesXmlFilePath), StandardCharsets.UTF_8));

    setupHotspotRules(repository.rules());

    repository.done();
  }

  private void setupHotspotRules(Collection<NewRule> rules) {
    Map<NewRule, RuleMetadata> allRuleMetadata = rules.stream()
      .collect(Collectors.toMap(rule -> rule, rule -> readRuleMetadata(rule.key())));

    Set<NewRule> hotspotRules = getHotspotRules(allRuleMetadata);

    allRuleMetadata.forEach(AbstractRulesDefinition::updateSecurityStandards);
    hotspotRules.forEach(rule -> rule.setType(RuleType.SECURITY_HOTSPOT));
  }

  private static Set<NewRule> getHotspotRules(Map<NewRule, RuleMetadata> allRuleMetadata) {
    return allRuleMetadata.entrySet()
      .stream()
      .filter(entry -> entry.getValue().isSecurityHotspot())
      .map(Map.Entry::getKey)
      .collect(Collectors.toSet());
  }

  private static void updateSecurityStandards(NewRule rule, RuleMetadata ruleMetadata) {
    for (String s : ruleMetadata.securityStandards.OWASP) {
      rule.addOwaspTop10(RulesDefinition.OwaspTop10.valueOf(s));
    }
    rule.addCwe(ruleMetadata.securityStandards.CWE);
  }

  private RuleMetadata readRuleMetadata(String ruleKey) {
    String resourcePath = getRuleJson(ruleKey);
    try (InputStream stream = AbstractRulesDefinition.class.getResourceAsStream(resourcePath)) {
      return  stream != null
        ? GSON.fromJson(new InputStreamReader(stream, StandardCharsets.UTF_8), RuleMetadata.class)
        : new RuleMetadata();
    } catch (IOException e) {
      throw new IllegalStateException("Failed to read: " + resourcePath, e);
    }
  }

  protected abstract String getRuleJson(String ruleKey);

  private static class RuleMetadata {
    private static final String SECURITY_HOTSPOT = "SECURITY_HOTSPOT";

    String sqKey;
    String type;
    SecurityStandards securityStandards = new SecurityStandards();

    String getKey() {
      return sqKey;
    }
    boolean isSecurityHotspot() {
      return SECURITY_HOTSPOT.equals(type);
    }
  }

  // for deserialization purposes
  @SuppressWarnings("squid:S00116")
  private static class SecurityStandards {
    int[] CWE = {};
    String[] OWASP = {};
  }
}
