/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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

/*
 * Created on May 19, 2009
 */
package org.sonar.plugin.dotnet.stylecop;

import java.io.CharArrayWriter;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;

import org.apache.commons.lang.StringUtils;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.ConfigurationExportable;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RulePriority;
import org.sonar.plugin.dotnet.core.AbstractDotNetRuleRepository;
import org.sonar.plugin.dotnet.core.CSharpRulesProfile;
import org.sonar.plugin.dotnet.core.XmlUtils;
import org.sonar.plugin.dotnet.stylecop.xml.Analyzer;
import org.sonar.plugin.dotnet.stylecop.xml.BooleanProperty;
import org.sonar.plugin.dotnet.stylecop.xml.RuleDef;
import org.sonar.plugin.dotnet.stylecop.xml.StyleCopSettings;

/**
 * Loads and generates style cop configuration files.
 * 
 * @author Jose CHILLAN May 19, 2009
 */
public class StyleCopRuleRepository extends AbstractDotNetRuleRepository
    implements ConfigurationExportable {

  @Override
  public Map<String, String> getBuiltInProfiles() {
    Map<String, String> result = new HashMap<String, String>();
    result.put(CSharpRulesProfile.DEFAULT_WAY, "default-rules.StyleCop");
    result.put(CSharpRulesProfile.DEFAULT_WAY_V2, "default-rules.V2.StyleCop");
    return result;
  }

  @Override
  public String getRepositoryResourcesBase() {
    return "org/sonar/plugin/dotnet/stylecop";
  }

  /**
   * Loads a configuration.
   * 
   * @param configuration
   * @param rules
   * @return
   */
  public List<ActiveRule> importConfiguration(String configuration,
      List<Rule> rules) {
    List<StyleCopRule> styleCopConfig = StyleCopRuleParser.parse(configuration);

    List<ActiveRule> result = new ArrayList<ActiveRule>();
    Map<String, Rule> rulesByName = new HashMap<String, Rule>();
    for (Rule dbRule : rules) {
      String configKey = dbRule.getConfigKey();
      String dbName = StringUtils.substringAfter(configKey, "#");
      rulesByName.put(dbName, dbRule);
    }
    for (StyleCopRule styleCopRule : styleCopConfig) {
      if (styleCopRule.isEnabled()) {
        Rule dbRule = rulesByName.get(styleCopRule.getName());
        String rawPriority = styleCopRule.getPriority();
        RulePriority ruleFailureLevel = RulePriority.MINOR;
        if (StringUtils.isNotEmpty(rawPriority)) {
          ruleFailureLevel = RulePriority.valueOfString(rawPriority);
        }
        ActiveRule activeRule = new ActiveRule(null, dbRule, ruleFailureLevel);
        result.add(activeRule);
      }
    }

    return result;
  }

  /**
   * Export the configuration file corresponding to a profile.
   * 
   * @param activeProfile
   * @return
   */
  @Override
  public String exportConfiguration(RulesProfile activeProfile) {
    List<ActiveRule> activeRules = activeProfile
        .getActiveRulesByPlugin(StyleCopPlugin.KEY);
    List<StyleCopRule> rules = buildRules(activeRules);
    return buildXmlFromRules(rules);
  }

  /**
   * Generates a XML configuration file corresponding to a given set of stylecop
   * rules.
   * 
   * @param rules
   * @return
   */
  private String buildXmlFromRules(List<StyleCopRule> rules) {
    Map<String, List<StyleCopRule>> rulesByAnalyzer = new HashMap<String, List<StyleCopRule>>();
    // We build a map because the style cop settings are grouped by analyzer
    for (StyleCopRule styleCopRule : rules) {
      String analyzerId = styleCopRule.getAnalyzerId();
      List<StyleCopRule> rulesList = rulesByAnalyzer.get(analyzerId);
      if (rulesList == null) {
        rulesList = new ArrayList<StyleCopRule>();
        rulesByAnalyzer.put(analyzerId, rulesList);
      }
      rulesList.add(styleCopRule);
    }

    // The XML Objects are populated
    List<Analyzer> analyzers = new ArrayList<Analyzer>();
    for (Entry<String, List<StyleCopRule>> entry : rulesByAnalyzer.entrySet()) {
      Analyzer analyzer = new Analyzer();
      String id = entry.getKey();
      analyzer.setId(id);
      List<RuleDef> analyzerRules = new ArrayList<RuleDef>();
      List<StyleCopRule> styleRules = entry.getValue();
      analyzer.setRules(analyzerRules);
      // This is not an imbricated loop, but rather another way of
      // browsing the list by grouping the analyzers
      for (StyleCopRule styleCopRule : styleRules) {
        RuleDef ruleDef = new RuleDef();
        String ruleName = styleCopRule.getName();
        String priority = styleCopRule.getPriority();
        ruleDef.setName(ruleName);
        ruleDef.setPriority(priority);
        BooleanProperty property = new BooleanProperty();
        property.setName("Enabled");
        if (styleCopRule.isEnabled()) {
          property.setValue("True");
        } else {
          property.setValue("False");
        }
        ruleDef.setSettings(Collections.singletonList(property));
        analyzerRules.add(ruleDef);
      }
      analyzers.add(analyzer);
    }

    // We populate the result content
    StyleCopSettings settings = new StyleCopSettings();
    settings.setAnalizers(analyzers);

    CharArrayWriter writer = new CharArrayWriter();
    XmlUtils.marshall(settings, writer);
    return writer.toString();
  }

  /**
   * Builds a list of rules.
   * 
   * @param activeRulesByPlugin
   * @return
   */
  private List<StyleCopRule> buildRules(List<ActiveRule> activeRulesByPlugin) {
    List<StyleCopRule> result = new ArrayList<StyleCopRule>();
    Map<String, StyleCopRule> initialRules = getInitialRules();
    for (ActiveRule activeRule : activeRulesByPlugin) {
      // Extracts the rule's date
      Rule rule = activeRule.getRule();
      String configKey = rule.getConfigKey();
      StyleCopRule styleCopRule = initialRules.get(configKey);
      styleCopRule.setEnabled(true);
      RulePriority priority = activeRule.getPriority();
      if (priority != null) {
        styleCopRule.setPriority(priority.name().toLowerCase());
      }
    }

    // We populate the result
    result.addAll(initialRules.values());
    return result;
  }

  /**
   * Generates the stylecop rule for a configuration rule.
   * 
   * @param rule
   *          the configuration rule
   * @return
   */
  private StyleCopRule generateRule(Rule rule) {
    String configKey = rule.getConfigKey();
    String analyzerId = StringUtils.substringBefore(configKey, "#");
    String name = StringUtils.substringAfter(configKey, "#");

    // Creates an populates the rule
    StyleCopRule styleCopRule = new StyleCopRule();
    styleCopRule.setAnalyzerId(analyzerId);
    styleCopRule.setName(name);
    return styleCopRule;
  }

  /**
   * Gets the initial rules for the plugin.
   * 
   * @return
   */
  private Map<String, StyleCopRule> getInitialRules() {
    Map<String, StyleCopRule> result = new LinkedHashMap<String, StyleCopRule>();
    List<Rule> initialReferential = getInitialReferential();
    for (Rule rule : initialReferential) {
      StyleCopRule styleCopRule = generateRule(rule);
      String key = rule.getConfigKey();
      result.put(key, styleCopRule);
    }
    return result;
  }
}
