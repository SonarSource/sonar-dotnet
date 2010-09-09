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
