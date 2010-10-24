/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
 *
 * Sonar is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * Sonar is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Sonar; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

/*
 * Created on May 7, 2009
 */
package org.sonar.plugin.dotnet.fxcop;

import java.io.CharArrayWriter;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.apache.commons.lang.StringUtils;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.ConfigurationExportable;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RulePriority;
import org.sonar.plugin.dotnet.core.AbstractDotNetRuleRepository;
import org.sonar.plugin.dotnet.core.CSharpRulesProfile;
import org.sonar.plugin.dotnet.core.XmlUtils;
import org.sonar.plugin.dotnet.fxcop.xml.FxCopProject;
import org.sonar.plugin.dotnet.fxcop.xml.ProjectOptions;
import org.sonar.plugin.dotnet.fxcop.xml.RuleDef;
import org.sonar.plugin.dotnet.fxcop.xml.RuleFile;
import org.sonar.plugin.dotnet.fxcop.xml.RuleSet;

/**
 * Loads and generated the FXCop rules configuration file.
 * 
 * @author Jose CHILLAN May 7, 2009
 */
public class FxCopRuleRepository extends AbstractDotNetRuleRepository implements
    ConfigurationExportable {

  /**
   * Constructs a @link{FxCopRuleRepository}.
   */
  public FxCopRuleRepository() {
  }

  @Override
  public Map<String, String> getBuiltInProfiles() {
    Map<String, String> result = new HashMap<String, String>();
    result.put(CSharpRulesProfile.DEFAULT_WAY, "DefaultRules.FxCop");
    result.put(CSharpRulesProfile.DEFAULT_WAY_V2, "DefaultRules.V2.FxCop");
    return result;
  }

  @Override
  public List<Rule> parseReferential(String fileContent) {
    return super.parseReferential(fileContent);
  }

  @Override
  public String getRepositoryResourcesBase() {
    return "org/sonar/plugin/dotnet/fxcop";
  }

  /**
   * Imports the configuration file with the list of configured rules.
   * 
   * @param configuration
   * @param rules
   * @return the active rules
   */
  public List<ActiveRule> importConfiguration(String configuration,
      List<Rule> rules) {
    List<FxCopRule> fxcopConfig = FxCopRuleParser.parse(configuration);

    List<ActiveRule> result = new ArrayList<ActiveRule>();
    // First we build a map of configured rules
    Map<String, Rule> rulesMap = new HashMap<String, Rule>();
    for (Rule dbRule : rules) {
      String key = dbRule.getKey();
      rulesMap.put(key, dbRule);
    }

    // We try to import all the configured rules
    for (FxCopRule fxCopRule : fxcopConfig) {
      String ruleName = fxCopRule.getName();
      Rule dbRule = rulesMap.get(ruleName);
      if (dbRule != null) {
        String rawPriority = fxCopRule.getPriority();
        RulePriority rulePriority = RulePriority.MAJOR;
        if (StringUtils.isNotEmpty(rawPriority)) {
          rulePriority = RulePriority.valueOfString(rawPriority);
        }
        ActiveRule activeRule = new ActiveRule(null, dbRule, rulePriority);
        result.add(activeRule);
      }
    }
    return result;
  }

  @Override
  public String exportConfiguration(RulesProfile activeProfile) {
    List<ActiveRule> activeRules = activeProfile
        .getActiveRulesByPlugin(FxCopPlugin.KEY);
    List<FxCopRule> rules = buildRules(activeRules);
    String xmlModules = buildXmlFromRules(rules);
    return xmlModules;
  }

  /**
   * Builds a FxCop rule file from the configured rules.
   * 
   * @param allRules
   * @return
   */
  private String buildXmlFromRules(List<FxCopRule> allRules) {
    FxCopProject report = new FxCopProject();
    Map<String, List<FxCopRule>> rulesByFile = new HashMap<String, List<FxCopRule>>();
    // We group the rules by filename
    for (FxCopRule fxCopRule : allRules) {
      String fileName = fxCopRule.getFileName();
      List<FxCopRule> rulesList = rulesByFile.get(fileName);
      if (rulesList == null) {
        rulesList = new ArrayList<FxCopRule>();
        rulesByFile.put(fileName, rulesList);
      }
      rulesList.add(fxCopRule);
    }

    // This is the main list
    List<RuleFile> ruleFiles = new ArrayList<RuleFile>();
    for (Map.Entry<String, List<FxCopRule>> fileEntry : rulesByFile.entrySet()) {
      RuleFile ruleFile = new RuleFile();
      ruleFile.setEnabled("True");

      // We copy all the rules informations
      ruleFile.setName(fileEntry.getKey());
      List<RuleDef> ruleDefinitions = new ArrayList<RuleDef>();
      List<FxCopRule> rules = fileEntry.getValue();
      for (FxCopRule fxCopRule : rules) {
        RuleDef currentRule = new RuleDef();
        currentRule.setName(fxCopRule.getName());
        currentRule.setPriority(fxCopRule.getPriority());
        ruleDefinitions.add(currentRule);
      }
      ruleFile.setRules(ruleDefinitions);
      ruleFiles.add(ruleFile);
    }

    RuleSet ruleSet = new RuleSet();
    ruleSet.setRules(ruleFiles);
    report.setProjectOptions(new ProjectOptions());
    report.setRules(ruleSet);

    CharArrayWriter writer = new CharArrayWriter();
    XmlUtils.marshall(report, writer);
    String config = writer.toString();
    return config;
  }

  /**
   * Builds all the FxCop rules from the active rules
   * 
   * @param activeRulesByPlugin
   * @return
   */
  private List<FxCopRule> buildRules(List<ActiveRule> activeRulesByPlugin) {
    List<FxCopRule> result = new ArrayList<FxCopRule>();

    for (ActiveRule activeRule : activeRulesByPlugin) {
      // Extracts the rule's date
      Rule rule = activeRule.getRule();
      String configKey = rule.getConfigKey();
      String fileName = StringUtils.substringAfter(configKey, "@");
      String name = StringUtils.substringBefore(configKey, "@");

      // Creates an populates the rule
      FxCopRule fxCopRule = new FxCopRule();
      fxCopRule.setCategory(rule.getRulesCategory().getName());
      fxCopRule.setEnabled(true);
      fxCopRule.setFileName(fileName);
      fxCopRule.setName(name);
      
      RulePriority priority = activeRule.getPriority();
      if (priority != null) {
        fxCopRule.setPriority(priority.name().toLowerCase());
      }
      
      result.add(fxCopRule);
    }
    return result;
  }
}