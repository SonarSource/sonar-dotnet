package org.sonar.plugin.dotnet.gendarme;

import static org.junit.Assert.*;

import java.io.File;
import java.util.Collection;

import org.apache.commons.io.FileUtils;
import org.junit.Test;

public class GendarmeRuleParserTest {

	@Test
	public void testParseRuleConfiguration() throws Exception {
		String xml =
			FileUtils.readFileToString(new File(getClass().getClassLoader().getResource("gendarme.test.config.xml").toURI()));
		
		GendarmeRuleParser parser = new GendarmeRuleParserImpl();
		Collection<GendarmeRule> rules = parser.parseRuleConfiguration(xml);
		
		assertEquals(5, rules.size());
		
	}
	
	@Test
	public void testParseRuleConfigurationWithPriorities() throws Exception {
		String xml =
			FileUtils.readFileToString(new File(getClass().getClassLoader().getResource("gendarme.test.config.v2.xml").toURI()));
		
		GendarmeRuleParser parser = new GendarmeRuleParserImpl();
		Collection<GendarmeRule> rules = parser.parseRuleConfiguration(xml);
		
		assertEquals(5, rules.size());
		boolean blockerFound = false;
		boolean criticalFound = false;
		for (GendarmeRule gendarmeRule : rules) {
	    switch (gendarmeRule.getPriority()) {
      case BLOCKER:
	      assertEquals("AvoidLongMethodsRule@Gendarme.Rules.Smells.dll", gendarmeRule.getId());
	      blockerFound = true;
	      break;
      case CRITICAL:
      	assertEquals("AvoidLargeClassesRule@Gendarme.Rules.Smells.dll", gendarmeRule.getId());
      	criticalFound = true;
	      break;
      default:
	      break;
      }
    }
		assertTrue(blockerFound);
		assertTrue(criticalFound);
		
	}

}
