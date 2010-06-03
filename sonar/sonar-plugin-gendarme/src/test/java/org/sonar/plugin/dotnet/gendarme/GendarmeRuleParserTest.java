package org.sonar.plugin.dotnet.gendarme;

import static org.junit.Assert.*;

import java.io.File;
import java.io.IOException;
import java.net.URISyntaxException;
import java.util.List;

import org.apache.commons.io.FileUtils;
import org.junit.Test;

public class GendarmeRuleParserTest {

	@Test
	public void testParseRuleConfiguration() throws Exception {
		String xml =
			FileUtils.readFileToString(new File(getClass().getClassLoader().getResource("gendarme.test.config.xml").toURI()));
		
		GendarmeRuleParser parser = new GendarmeRuleParserImpl();
		List<GendarmeRule> rules = parser.parseRuleConfiguration(xml);
		
		assertEquals(5, rules.size());
		
	}

}
