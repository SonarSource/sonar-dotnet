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

package org.sonar.plugin.dotnet.gendarme;

import java.io.Reader;
import java.io.StringReader;
import java.util.Collection;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

import org.apache.commons.lang.StringUtils;
import org.sonar.api.rules.RulePriority;
import org.sonar.plugin.dotnet.core.AbstractXmlParser;
import org.w3c.dom.Element;

public class GendarmeRuleParserImpl extends AbstractXmlParser implements GendarmeRuleParser {

	
	/* (non-Javadoc)
   * @see org.sonar.plugin.dotnet.gendarme.GendarmeRuleParser#parseRuleConfiguration(java.lang.String)
   */
	public Collection<GendarmeRule> parseRuleConfiguration(String rawConfiguration) {
		Set<GendarmeRule> rules = parseRuleConfiguration(rawConfiguration, "default", RulePriority.MAJOR);
		
		RulePriority[] priorities = RulePriority.values();
		for (RulePriority rulePriority : priorities) {
			Set<GendarmeRule> rules4priority = parseRuleConfiguration(rawConfiguration, rulePriority.name().toLowerCase(), rulePriority);
			rules.removeAll(rules4priority);
			rules.addAll(rules4priority);
    }
		
		return rules;
	}
	
	
  private Set<GendarmeRule> parseRuleConfiguration(String rawConfiguration, String ruleSet, RulePriority rulePriority) {
		
		Reader reader = new StringReader(rawConfiguration);
		Set<GendarmeRule> result = new HashSet<GendarmeRule>();
	  List<Element> ruleElements = extractElements(reader, "/gendarme/ruleset[@name=\""+ruleSet+"\"]/rules");
	  for (Element element : ruleElements) {
	    String assemblyName = element.getAttribute("from");
	    String rawRuleArray = element.getAttribute("include");
			String[] ruleArray = StringUtils.split(rawRuleArray, '|');
			for (String ruleName : ruleArray) {
	      String ruleId = StringUtils.trim(ruleName)+'@'+assemblyName;
	      GendarmeRule rule = new GendarmeRule();
	      rule.setId(ruleId);
	      rule.setPriority(rulePriority);
	      
	      List<Element> propElements = 
	        extractElements(element, "parameter[@rule=\""+StringUtils.trim(ruleName)+"\"]");
	      
	      for (Element propElement : propElements) {
          String name = evaluateAttribute(propElement, "@property");
          String value = evaluateAttribute(propElement, "@value");
          rule.addProperty(new RuleProperty(name, value));
        }
	      
	      result.add(rule);
      }
    }
		return result;
	}
	
}
