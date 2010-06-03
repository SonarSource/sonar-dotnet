package org.sonar.plugin.dotnet.gendarme;

import java.io.Reader;
import java.io.StringReader;
import java.util.ArrayList;
import java.util.List;

import org.apache.commons.lang.StringUtils;
import org.sonar.plugin.dotnet.core.AbstractXmlParser;
import org.w3c.dom.Element;

public class GendarmeRuleParserImpl extends AbstractXmlParser implements GendarmeRuleParser {

	
	/* (non-Javadoc)
   * @see org.sonar.plugin.dotnet.gendarme.GendarmeRuleParser#parseRuleConfiguration(java.lang.String)
   */
	public List<GendarmeRule> parseRuleConfiguration(String rawConfiguration) {
		Reader reader = new StringReader(rawConfiguration);
		List<GendarmeRule> result = new ArrayList<GendarmeRule>();
	  List<Element> ruleElements = extractElements(reader, "//rules");
	  for (Element element : ruleElements) {
	    String assemblyName = element.getAttribute("from");
	    String rawRuleArray = element.getAttribute("include");
			String[] ruleArray = StringUtils.split(rawRuleArray, '|');
			for (String ruleName : ruleArray) {
	      String ruleId = assemblyName+'$'+StringUtils.trim(ruleName);
	      GendarmeRule rule = new GendarmeRule();
	      rule.setId(ruleId);
	      result.add(rule);
      }
    }
		return result;
	}
	
}
