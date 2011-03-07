/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.fxcop.profiles;

import java.io.CharArrayWriter;
import java.io.IOException;
import java.io.Writer;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.apache.commons.lang.StringUtils;
import org.sonar.api.profiles.ProfileExporter;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RulePriority;
import org.sonar.api.utils.SonarException;

import com.sonar.csharp.fxcop.Constants;
import com.sonar.csharp.fxcop.profiles.utils.FxCopRule;
import com.sonar.csharp.fxcop.profiles.utils.XmlUtils;
import com.sonar.csharp.fxcop.profiles.xml.FxCopProject;
import com.sonar.csharp.fxcop.profiles.xml.ProjectOptions;
import com.sonar.csharp.fxcop.profiles.xml.RuleDef;
import com.sonar.csharp.fxcop.profiles.xml.RuleFile;
import com.sonar.csharp.fxcop.profiles.xml.RuleSet;

public class FxCopProfileExporter extends ProfileExporter {

  public FxCopProfileExporter() {
    super(Constants.REPOSITORY_KEY, Constants.PLUGIN_NAME);
    setSupportedLanguages(Constants.LANGUAGE_KEY);
    setMimeType("application/xml");
  }

  public void exportProfile(RulesProfile profile, Writer writer) {
    List<ActiveRule> activeRules = profile.getActiveRulesByRepository(Constants.REPOSITORY_KEY);
    List<FxCopRule> rules = buildRules(activeRules);
    String xmlModules = buildXmlFromRules(rules);
    try {
      writer.write(xmlModules);
    } catch (IOException e) {
      throw new SonarException("Fail to export the profile " + profile, e);
    }
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

      RulePriority priority = activeRule.getSeverity();
      if (priority != null) {
        fxCopRule.setPriority(priority.name().toLowerCase());
      }

      result.add(fxCopRule);
    }
    return result;
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

}
