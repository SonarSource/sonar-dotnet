package org.sonar.plugin.dotnet.gendarme;

import static org.junit.Assert.*;

import java.rmi.activation.ActivateFailedException;
import java.util.ArrayList;
import java.util.List;

import org.apache.commons.lang.StringUtils;
import org.junit.Test;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RulePriority;

public class GendarmeRuleMarshallerImplTest {

	@Test
	public void testMarshall() {
		GendarmeRuleMarshallerImpl marshaller = new GendarmeRuleMarshallerImpl();
		
		List<ActiveRule> activeRules = new ArrayList<ActiveRule>();
		
		ActiveRule activeRule = new ActiveRule();
		Rule rule = new Rule();
		rule.setConfigKey("MyRule@MyAssembly.dll");
		activeRule.setRule(rule);
		activeRules.add(activeRule);
		
		ActiveRule activeRuleBlocker = new ActiveRule();
		Rule ruleBlocker = new Rule();
		ruleBlocker.setConfigKey("MyRuleBlock@MyAssembly.dll");
		activeRuleBlocker.setRule(ruleBlocker);
		activeRuleBlocker.setPriority(RulePriority.BLOCKER);
		activeRules.add(activeRuleBlocker);
		
		String xml = marshaller.marshall(activeRules);
		System.out.println(xml);
		assertTrue(StringUtils.contains(xml, "<rules"));
		assertTrue(StringUtils.contains(xml, "from=\"MyAssembly.dll\""));
		assertTrue(StringUtils.contains(xml, "include=\"MyRuleBlock\""));
	}

}
