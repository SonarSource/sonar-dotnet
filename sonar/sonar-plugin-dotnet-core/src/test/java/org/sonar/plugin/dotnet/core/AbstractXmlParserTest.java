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
