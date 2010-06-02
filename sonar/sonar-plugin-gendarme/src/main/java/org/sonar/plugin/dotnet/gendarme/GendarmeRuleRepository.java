package org.sonar.plugin.dotnet.gendarme;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.ConfigurationExportable;
import org.sonar.api.rules.Rule;
import org.sonar.plugin.dotnet.core.AbstractDotNetRuleRepository;
import org.sonar.plugin.dotnet.core.CSharpRulesProfile;

public class GendarmeRuleRepository extends AbstractDotNetRuleRepository implements ConfigurationExportable {

	// TODO make IOC works with pico
	
	private final GendarmeRuleParser ruleParser = new GendarmeRuleParserImpl();
	private final GendarmeRuleMarshaller ruleMarshaller = new GendarmeRuleMarshallerImpl();
	/*
	public GendarmeRuleRepository(GendarmeRuleParser ruleParser, GendarmeRuleMarshaller ruleMarshaller) {
	  this.ruleParser = ruleParser;
	  this.ruleMarshaller = ruleMarshaller;
  }*/

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
		List<ActiveRule> activeRules = profile.getActiveRulesByPlugin(GendarmePlugin.KEY);
		return ruleMarshaller.marshall(activeRules);
  }

	@Override
  public List<ActiveRule> importConfiguration(String configuration,
      List<Rule> rules) {
	  List<GendarmeRule> parsedRules = ruleParser.parseRuleConfiguration(configuration);
	  
	  List<ActiveRule> result = new ArrayList<ActiveRule>();
    // First we build a map of configured rules
    Map<String, Rule> rulesMap = new HashMap<String, Rule>();
    for (Rule dbRule : rules)
    {
      String key = dbRule.getConfigKey();
      rulesMap.put(key, dbRule);
    }

    // We try to import all the configured rules
    for (GendarmeRule genRule : parsedRules)
    {
      String ruleId = genRule.getId();
      Rule dbRule = rulesMap.get(ruleId);
      if (dbRule != null)
      {
        ActiveRule activeRule = new ActiveRule(null, dbRule, genRule.getPriority());
        result.add(activeRule);
      }
    }
    return result;
  }
	
}
