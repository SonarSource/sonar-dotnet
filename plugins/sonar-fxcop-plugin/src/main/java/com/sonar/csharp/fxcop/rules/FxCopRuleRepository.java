/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.fxcop.rules;

import java.io.CharArrayWriter;
import java.io.IOException;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;

import org.apache.commons.io.IOUtils;
import org.apache.commons.lang.StringUtils;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Language;
import org.sonar.api.rules.AbstractRulesRepository;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.ConfigurationExportable;
import org.sonar.api.rules.ConfigurationImportable;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RulePriority;

import com.sonar.csharp.fxcop.Constants;
import com.sonar.csharp.fxcop.FxCopPlugin;
import com.sonar.csharp.fxcop.utils.XmlUtils;
import com.sonar.csharp.fxcop.utils.xml.FxCopProject;
import com.sonar.csharp.fxcop.utils.xml.ProjectOptions;
import com.sonar.csharp.fxcop.utils.xml.RuleDef;
import com.sonar.csharp.fxcop.utils.xml.RuleFile;
import com.sonar.csharp.fxcop.utils.xml.RuleSet;

/**
 * Loads and generated the FXCop rules configuration file.
 */
public class FxCopRuleRepository extends AbstractRulesRepository<Language, DefaultRuleMapper> implements ConfigurationImportable,
    ConfigurationExportable {

  public final static String DEFAULT_WAY = "Sonar C# Way";
  public final static String DEFAULT_WAY_V2 = "Sonar C# Way V2";

  /**
   * Constructs a @link{FxCopRuleRepository}.
   */
  public FxCopRuleRepository() {
    // To make it work first
    super(new CSharp(), new DefaultRuleMapper());
  }

  /**
   * Gets all the provided profiles.
   * 
   * @return a list of profiles
   */
  public List<RulesProfile> getProvidedProfiles() {
    List<RulesProfile> profiles = new ArrayList<RulesProfile>();

    Map<String, String> defaultProfiles = new TreeMap<String, String>(getBuiltInProfiles());
    for (Map.Entry<String, String> entry : defaultProfiles.entrySet()) {
      RulesProfile providedProfile = loadProvidedProfile(entry.getKey(), getCheckResourcesBase() + entry.getValue());
      profiles.add(providedProfile);
    }
    return profiles;
  }

  public Map<String, String> getBuiltInProfiles() {
    Map<String, String> result = new HashMap<String, String>();
    result.put(DEFAULT_WAY, "DefaultRules.FxCop");
    result.put(DEFAULT_WAY_V2, "DefaultRules.V2.FxCop");
    return result;
  }

  /**
   * Loads a provided profile.
   * 
   * @param name
   *          the profile name
   * @param filePath
   *          the path of the file containins the profile
   * @return a provided profile.
   */
  public RulesProfile loadProvidedProfile(String name, String filePath) {
    try {
      InputStream profileIn = getClass().getResourceAsStream(filePath);
      if (profileIn == null) {
        throw new IOException("Resource " + profileIn + " not found");
      }
      RulesProfile profile = new RulesProfile(name, "cs");
      List<Rule> initialReferential = getInitialReferential();
      List<ActiveRule> configuration = importConfiguration(IOUtils.toString(profileIn, "UTF-8"), initialReferential);
      profile.setActiveRules(configuration);
      return profile;

    } catch (IOException e) {
      throw new RuntimeException("Configuration file not found for the profile : " + name, e);
    }
  }

  @Override
  public List<Rule> parseReferential(String fileContent) {
    return super.parseReferential(fileContent);
  }

  @Override
  public String getRepositoryResourcesBase() {
    return "com/sonar/csharp/fxcop/rules";
  }

  /**
   * Imports the configuration file with the list of configured rules.
   * 
   * @param configuration
   * @param rules
   * @return the active rules
   */
  public List<ActiveRule> importConfiguration(String configuration, List<Rule> rules) {
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

  public String exportConfiguration(RulesProfile activeProfile) {
    List<ActiveRule> activeRules = activeProfile.getActiveRulesByPlugin(Constants.PLUGIN_KEY);
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