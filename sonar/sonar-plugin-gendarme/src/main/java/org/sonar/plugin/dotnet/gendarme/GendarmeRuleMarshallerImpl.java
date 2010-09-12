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
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.apache.commons.lang.StringUtils;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.ActiveRuleParam;
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

		Map<String, List<ActiveRule>> assemblyRulesMap = new HashMap<String, List<ActiveRule>>();
		
		boolean assemblyRulesMapEmpty = true;
		for (ActiveRule activeRule : rules) {

      if (priority != null && priority != activeRule.getPriority()) {
				continue;
			}

			String key = activeRule.getConfigKey();
			String assembly = StringUtils.substringAfter(key, "@");
			//String ruleName = StringUtils.substringBefore(key, "@");
			List<ActiveRule> assemblyRules = assemblyRulesMap.get(assembly);
			if (assemblyRules == null) {
				assemblyRules = new ArrayList<ActiveRule>();
				assemblyRulesMap.put(assembly, assemblyRules);
				assemblyRulesMapEmpty = false;
			}
			assemblyRules.add(activeRule);
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
				List<ActiveRule> assemblyRules = assemblyRulesMap.get(assembly);
				appendRuleList(builder, assemblyRules);
				builder.append("\" from=\"");
				builder.append(assembly);
				builder.append("\">");
				appendRuleParams(builder, assemblyRules);
				builder.append("</rules>");
			}
			builder.append("</ruleset>");
		}
		

		return builder.toString();
	}

  private void appendRuleParams(StringBuilder builder,
      List<ActiveRule> assemblyRules) {
    for (ActiveRule activeRule : assemblyRules) {
      List<ActiveRuleParam> params = activeRule.getActiveRuleParams();
      for (ActiveRuleParam param : params) {
        String key = activeRule.getConfigKey();
        String ruleName = StringUtils.substringBefore(key, "@");
        String propertyName = param.getRuleParam().getKey();
        String propertyValue = param.getValue();
        builder
          .append("<parameter rule=\"")
          .append(ruleName)
          .append("\" property=\"")
          .append(propertyName)
          .append("\" value=\"")
          .append(propertyValue)
          .append("\" />");
      }
    }
  }

  private void appendRuleList(StringBuilder builder, List<ActiveRule> rules) {
    List<String> ruleNames = new ArrayList<String>();
    for (ActiveRule activeRule : rules) {
      String key = activeRule.getConfigKey();
      String ruleName = StringUtils.substringBefore(key, "@");
      ruleNames.add(ruleName);
    }
    builder.append(StringUtils.join(ruleNames, " | "));
  }

}
