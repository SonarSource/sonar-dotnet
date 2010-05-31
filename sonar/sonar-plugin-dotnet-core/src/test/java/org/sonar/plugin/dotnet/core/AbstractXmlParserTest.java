package org.sonar.plugin.dotnet.core;

import java.io.Reader;
import java.io.StringReader;
import java.util.List;

import org.junit.Test;
import static org.junit.Assert.*;
import org.w3c.dom.Element;


public class AbstractXmlParserTest {

	@Test
	public void testExtractElements() {
		AbstractXmlParser parser = new AbstractXmlParser() {};
		
		String xmlTest = "<doc><toto>blalbla</toto></doc>";
		Reader reader = new StringReader(xmlTest);
		
		List<Element> elements = parser.extractElements(reader, "//toto");
		assertEquals(1, elements.size());
		
		reader = new StringReader(xmlTest);
		elements = parser.extractElements(reader, "//Toto");
		assertEquals(0, elements.size());
	}
	
	
}
