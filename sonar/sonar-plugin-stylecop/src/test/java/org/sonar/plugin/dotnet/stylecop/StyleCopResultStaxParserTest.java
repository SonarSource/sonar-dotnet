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

package org.sonar.plugin.dotnet.stylecop;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNotNull;

import java.io.File;
import java.net.MalformedURLException;
import java.util.List;

import org.junit.Before;
import org.junit.Test;
import org.sonar.plugin.dotnet.stylecop.stax.StyleCopResultStaxParser;

/**
 * This class was made to test the StyleCopResultStaxParser
 * 
 * @author Maxime Schneider-Dufeutrelle
 *
 */
public class StyleCopResultStaxParserTest {

  private StyleCopResultStaxParser parserStax;

  @Before
  public void setUp() {

    parserStax = new StyleCopResultStaxParser();

  }

  @Test
  public void testStaxParse() throws MalformedURLException {

    List<StyleCopViolation> violations = parserStax.parse(new File("target/test-classes","stylecop-report.xml"));
    assertEquals(106, violations.size());
    StyleCopViolation firstViolation = violations.get(0);

    for (StyleCopViolation violation : violations) {
      assertNotNull(violation);
    }

    assertEquals("8", firstViolation.getLineNumber());
    assertEquals("C:\\HOMEWARE\\Maxime\\sonarsource\\dotnet\\sonar\\sonar-plugin-stylecop\\src\\test\\resources\\solution\\Example\\Example.Core\\Alex.cs", firstViolation.getFilePath());
    assertEquals("ElementsMustBeDocumented", firstViolation.getKey());
    assertEquals("The class must have a documentation header.", firstViolation.getMessage());
  }

}
