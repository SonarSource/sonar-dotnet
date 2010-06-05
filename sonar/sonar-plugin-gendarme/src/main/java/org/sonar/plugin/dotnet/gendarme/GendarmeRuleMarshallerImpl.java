package org.sonar.plugin.dotnet.gendarme;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.apache.commons.lang.StringUtils;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.RulePriority;

public class GendarmeRuleMarshallerImpl implements GendarmeRuleMarshaller {

	@Override
	public String marshall(List<ActiveRule> rules) {
		StringBuilder builder = new StringBuilder();
		builder.append("<gendarme>");

		builder.append(marshall(rules, null));

		RulePriority[] priorities = RulePriority.values();
		for (RulePriority rulePriority : priorities) {
			builder.append(marshall(rules, rulePriority));
		}

		builder.append("</gendarme>");
		return builder.toString();
	}

	private String marshall(List<ActiveRule> rules, RulePriority priority) {
		StringBuilder builder = new StringBuilder();

		Map<String, List<String>> assemblyRulesMap = new HashMap<String, List<String>>();
		boolean assemblyRulesMapEmpty = true;
		for (ActiveRule activeRule : rules) {

			if (priority != null && priority != activeRule.getPriority()) {
				continue;
			}

			String key = activeRule.getConfigKey();
			String assembly = StringUtils.substringAfter(key, "@");
			String ruleName = StringUtils.substringBefore(key, "@");
			List<String> assemblyRules = assemblyRulesMap.get(assembly);
			if (assemblyRules == null) {
				assemblyRules = new ArrayList<String>();
				assemblyRulesMap.put(assembly, assemblyRules);
				assemblyRulesMapEmpty = false;
			}
			assemblyRules.add(ruleName);
		}

		if (!assemblyRulesMapEmpty) {
			if (priority == null) {
				builder.append("<ruleset name=\"default\">");
			} else {
				builder.append("<ruleset name=\"" + priority.name().toLowerCase()
				    + "\">");
			}

			for (String assembly : assemblyRulesMap.keySet()) {
				builder.append("\n<rules include=\"");
				builder.append(StringUtils.join(assemblyRulesMap.get(assembly), " | "));
				builder.append("\" from=\"");
				builder.append(assembly);
				builder.append("\"/>");
			}
			builder.append("</ruleset>");
		}
		

		return builder.toString();
	}

}
