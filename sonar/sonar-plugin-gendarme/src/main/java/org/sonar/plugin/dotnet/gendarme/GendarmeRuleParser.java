package org.sonar.plugin.dotnet.gendarme;

import java.io.Reader;
import java.io.StringReader;
import java.util.ArrayList;
import java.util.List;

import org.apache.commons.lang.StringUtils;
import org.sonar.plugin.dotnet.core.AbstractXmlParser;
import org.w3c.dom.Element;

public class GendarmeRuleParser extends AbstractXmlParser {

	
	public List<String> parseRuleConfiguration(String rawConfiguration) {
		Reader reader = new StringReader(rawConfiguration);
		List<String> result = new ArrayList<String>();
	  List<Element> ruleElements = extractElements(reader, "//rule");
	  for (Element element : ruleElements) {
	    String assemblyName = element.getAttribute("from");
	    String rawRuleArray = element.getAttribute("include");
			String[] ruleArray = StringUtils.split(rawRuleArray, '|');
			for (String ruleName : ruleArray) {
	      String ruleId = assemblyName+'$'+StringUtils.trim(ruleName);
	      result.add(ruleId);
      }
    }
		return result;
	}
	
}
