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

package org.sonar.plugin.dotnet.gendarme;

import static org.junit.Assert.*;
import static org.hamcrest.CoreMatchers.*;

import java.io.File;
import java.io.IOException;
import java.net.URISyntaxException;
import java.util.Collection;

import org.apache.commons.io.FileUtils;
import org.junit.Test;

public class GendarmeRuleParserTest {

  @Test
  public void testParseRuleConfiguration() throws Exception {
    final String fileName = "gendarme.test.config.xml";
    String xml = readFileToString(fileName);

    GendarmeRuleParser parser = new GendarmeRuleParserImpl();
    Collection<GendarmeRule> rules = parser.parseRuleConfiguration(xml);

    assertEquals(5, rules.size());

  }

  @Test
  public void testParseRuleConfigurationWithPriorities() throws Exception {
    final String fileName = "gendarme.test.config.v2.xml";
    String xml = readFileToString(fileName);

    GendarmeRuleParser parser = new GendarmeRuleParserImpl();
    Collection<GendarmeRule> rules = parser.parseRuleConfiguration(xml);

    assertEquals(5, rules.size());
    boolean blockerFound = false;
    boolean criticalFound = false;
    for (GendarmeRule gendarmeRule : rules) {
      switch (gendarmeRule.getPriority()) {
      case BLOCKER:
        assertEquals("AvoidLongMethodsRule@Gendarme.Rules.Smells.dll",
            gendarmeRule.getId());
        blockerFound = true;
        break;
      case CRITICAL:
        assertEquals("AvoidLargeClassesRule@Gendarme.Rules.Smells.dll",
            gendarmeRule.getId());
        criticalFound = true;
        break;
      default:
        break;
      }
    }
    assertTrue(blockerFound);
    assertTrue(criticalFound);

  }
  
  @Test
  public void testParseRuleConfigurationWithPrioritiesAndParams() throws Exception {
    final String fileName = "gendarme.test.config.with.params.xml";
    String xml = readFileToString(fileName);

    GendarmeRuleParser parser = new GendarmeRuleParserImpl();
    Collection<GendarmeRule> rules = parser.parseRuleConfiguration(xml);

    assertEquals(6, rules.size());
    boolean blockerFound = false;
    boolean criticalFound = false;
    for (GendarmeRule gendarmeRule : rules) {
      switch (gendarmeRule.getPriority()) {
      case BLOCKER:
        assertEquals("AvoidLongMethodsRule@Gendarme.Rules.Smells.dll",
            gendarmeRule.getId());
        blockerFound = true;
        break;
      case CRITICAL:
        assertThat(
            gendarmeRule.getId(), 
            anyOf(
                is("AvoidLargeClassesRule@Gendarme.Rules.Smells.dll"), 
                is("AvoidComplexMethodsRule@Gendarme.Rules.Smells.dll")
              )
        );
        
        if ("AvoidComplexMethodsRule@Gendarme.Rules.Smells.dll".equals(gendarmeRule.getId())) {
           assertFalse(gendarmeRule.getProperties().isEmpty());
           RuleProperty property = gendarmeRule.getProperties().get(0);
           assertEquals("SuccessThreshold", property.getName());
           assertEquals("15", property.getValue());
        }
        
        criticalFound = true;
        break;
      default:
        break;
      }
    }
    assertTrue(blockerFound);
    assertTrue(criticalFound);
  }

  private String readFileToString(final String fileName) throws IOException,
      URISyntaxException {
    String xml = FileUtils.readFileToString(new File(getClass()
        .getClassLoader().getResource(fileName).toURI()));
    return xml;
  }

}
