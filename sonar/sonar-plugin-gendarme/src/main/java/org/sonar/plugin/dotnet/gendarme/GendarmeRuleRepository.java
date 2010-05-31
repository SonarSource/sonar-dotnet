package org.sonar.plugin.dotnet.gendarme;

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
	  // TODO Auto-generated method stub
	  return null;
  }

	@Override
  public List<ActiveRule> importConfiguration(String configuration,
      List<Rule> rules) {
	  // TODO Auto-generated method stub
	  return null;
  }
	
}
