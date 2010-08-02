package org.sonar.plugin.dotnet.stylecop;

import static org.junit.Assert.*;

import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.List;

import org.junit.Test;

public class StyleCopRuleParserTest {

	@Test
	public void testParseReader() {
		 InputStream stream =
			 getClass().getResourceAsStream("default-rules.StyleCop");
		 List<StyleCopRule> result =
			 StyleCopRuleParser.parse(new InputStreamReader(stream));
	
		 assertEquals(104, result.size());
	}

	
}
