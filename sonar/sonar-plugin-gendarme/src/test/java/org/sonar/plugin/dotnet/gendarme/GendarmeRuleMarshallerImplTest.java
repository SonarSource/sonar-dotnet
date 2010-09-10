package org.sonar.plugin.dotnet.gendarme;

import static org.junit.Assert.*;
import static org.hamcrest.CoreMatchers.*;
import static org.junit.matchers.JUnitMatchers.*;

import java.util.ArrayList;
import java.util.List;

import org.junit.Test;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.ActiveRuleParam;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleParam;
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
		assertThat(xml, containsString("<rules"));
		assertThat(xml, containsString("from=\"MyAssembly.dll\""));
		assertThat(xml, containsString("include=\"MyRuleBlock\""));
	}
	
	
	@Test
  public void testMarshallWithParams() {
    GendarmeRuleMarshallerImpl marshaller = new GendarmeRuleMarshallerImpl();
    
    List<ActiveRule> activeRules = new ArrayList<ActiveRule>();
    
    ActiveRule activeRule = new ActiveRule();
    Rule rule = new Rule();
    rule.setConfigKey("MyRule@MyAssembly.dll");
    RuleParam ruleParam = new RuleParam();
    ruleParam.setKey("ParamName");
    ruleParam.setType("i");
    
    List<ActiveRuleParam> params = new ArrayList<ActiveRuleParam>();
    ActiveRuleParam param = new ActiveRuleParam();
    param.setRuleParam(ruleParam);
    param.setValue("123");
    params.add(param);
    activeRule.setActiveRuleParams(params);
    activeRule.setRule(rule);
    activeRules.add(activeRule);
    
    String xml = marshaller.marshall(activeRules);
    System.out.println(xml);
    assertThat(xml, containsString("<rules"));
    assertThat(xml, containsString("from=\"MyAssembly.dll\""));
    assertThat(xml, containsString("include=\"MyRule\""));
    assertThat(xml, containsString("<parameter rule=\"MyRule\" property=\"ParamName\" value=\"123\" />"));
  }

}
