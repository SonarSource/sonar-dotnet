package org.sonar.plugin.dotnet.gendarme;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.apache.commons.lang.StringUtils;
import org.sonar.api.rules.ActiveRule;

public class GendarmeRuleMarshallerImpl implements GendarmeRuleMarshaller {

	@Override
	public String marshall(List<ActiveRule> rules) {
		StringBuilder builder = new StringBuilder();
		builder.append("<gendarme><ruleset name=\"default\">");
		
		Map<String, List<String>> assemblyRulesMap = new HashMap<String, List<String>>();
		for (ActiveRule activeRule : rules) {
			String key = activeRule.getConfigKey();
			String assembly = StringUtils.substringBefore(key, "$");
			String ruleName = StringUtils.substringAfter(key, "$");
			List<String> assemblyRules = assemblyRulesMap.get(assembly);
			if (assemblyRules==null) {
				assemblyRules = new ArrayList<String>();
				assemblyRulesMap.put(assembly, assemblyRules);
			}
			assemblyRules.add(ruleName);
    }
		
		for (String assembly : assemblyRulesMap.keySet()) {
	    builder.append("\n<rules include=\"");
	    builder.append(StringUtils.join(assemblyRulesMap.get(assembly), " | "));
	    builder.append("\" from=\"");
	    builder.append(assembly);
	    builder.append("\"/>");
    }
		
		builder.append("</ruleset></gendarme>");
		return builder.toString();
	}

}
