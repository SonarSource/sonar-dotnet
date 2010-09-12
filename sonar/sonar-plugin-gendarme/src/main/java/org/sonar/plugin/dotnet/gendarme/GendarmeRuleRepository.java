/*
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

package org.sonar.plugin.dotnet.gendarme;

import java.util.ArrayList;
import java.util.Collection;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.ActiveRuleParam;
import org.sonar.api.rules.ConfigurationExportable;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleParam;
import org.sonar.plugin.dotnet.core.AbstractDotNetRuleRepository;
import org.sonar.plugin.dotnet.core.CSharpRulesProfile;

public class GendarmeRuleRepository extends AbstractDotNetRuleRepository
    implements ConfigurationExportable {

  private final GendarmeRuleParser ruleParser;
  private final GendarmeRuleMarshaller ruleMarshaller;

  public GendarmeRuleRepository(GendarmeRuleParser ruleParser,
      GendarmeRuleMarshaller ruleMarshaller) {
    this.ruleParser = ruleParser;
    this.ruleMarshaller = ruleMarshaller;
  }

  @Override
  public Map<String, String> getBuiltInProfiles() {
    Map<String, String> result = new HashMap<String, String>();
    result.put(CSharpRulesProfile.DEFAULT_WAY, "default.gendarme.config.xml");
    return result;
  }

  @Override
  public String getRepositoryResourcesBase() {
    return "org/sonar/plugin/dotnet/gendarme";
  }

  @Override
  public String exportConfiguration(RulesProfile profile) {
    List<ActiveRule> activeRules = profile
        .getActiveRulesByPlugin(GendarmePlugin.KEY);
    return ruleMarshaller.marshall(activeRules);
  }

  @Override
  public List<ActiveRule> importConfiguration(String configuration,
      List<Rule> rules) {
    Collection<GendarmeRule> parsedRules = ruleParser
        .parseRuleConfiguration(configuration);

    List<ActiveRule> result = new ArrayList<ActiveRule>();
    // First we build a map of configured rules
    Map<String, Rule> rulesMap = new HashMap<String, Rule>();
    for (Rule dbRule : rules) {
      String key = dbRule.getConfigKey();
      rulesMap.put(key, dbRule);
    }

    // We try to import all the configured rules
    for (GendarmeRule genRule : parsedRules) {
      String ruleId = genRule.getId();
      Rule dbRule = rulesMap.get(ruleId);
      if (dbRule != null) {
        ActiveRule activeRule = new ActiveRule(null, dbRule,
            genRule.getPriority());
        result.add(activeRule);
       
        activeRule.setActiveRuleParams(getActiveRuleParams(genRule, dbRule, activeRule));
        
      }
    }
    return result;
  }
  
  // copy pasted from the PmdRulesRepository class
  private List<ActiveRuleParam> getActiveRuleParams(GendarmeRule rule, Rule dbRule, ActiveRule activeRule) {
    List<ActiveRuleParam> activeRuleParams = new ArrayList<ActiveRuleParam>();
    if (rule.getProperties() != null) {
      for (RuleProperty property : rule.getProperties()) {
        if (dbRule.getParams() != null) {
          for (RuleParam ruleParam : dbRule.getParams()) {
            if (ruleParam.getKey().equals(property.getName())) {
              activeRuleParams.add(new ActiveRuleParam(activeRule, ruleParam, property.getValue()));
            }
          }
        }
      }
    }
    return activeRuleParams;
  }

}
