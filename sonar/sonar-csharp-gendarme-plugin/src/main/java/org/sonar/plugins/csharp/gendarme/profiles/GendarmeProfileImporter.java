/*
 * Sonar C# Plugin :: Gendarme
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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

package org.sonar.plugins.csharp.gendarme.profiles;

import java.io.Reader;
import java.util.Map;

import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamException;

import org.apache.commons.lang.StringUtils;
import org.codehaus.stax2.XMLInputFactory2;
import org.codehaus.staxmate.SMInputFactory;
import org.codehaus.staxmate.in.SMHierarchicCursor;
import org.codehaus.staxmate.in.SMInputCursor;
import org.sonar.api.profiles.ProfileImporter;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleFinder;
import org.sonar.api.rules.RulePriority;
import org.sonar.api.rules.RuleQuery;
import org.sonar.api.utils.ValidationMessages;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.gendarme.GendarmeConstants;

import com.google.common.collect.Maps;

/**
 * Class that allows to import Gendarme rule definition files into a Sonar Rule Profile
 * 
 * Note: the parsing of Gendarme rule file is partial: it uses only the "include" attribute of the "rules" tag in the format "rule1 | rule2"
 * (the wildcard "*" is not supported yet)
 */
public class GendarmeProfileImporter extends ProfileImporter {

  private RuleFinder ruleFinder;
  private RuleQuery ruleQuery;

  public GendarmeProfileImporter(RuleFinder ruleFinder) {
    super(GendarmeConstants.REPOSITORY_KEY, GendarmeConstants.PLUGIN_NAME);
    setSupportedLanguages(CSharpConstants.LANGUAGE_KEY);
    this.ruleFinder = ruleFinder;
    this.ruleQuery = RuleQuery.create().withRepositoryKey(GendarmeConstants.REPOSITORY_KEY);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public RulesProfile importProfile(Reader reader, ValidationMessages messages) {
    RulesProfile profile = RulesProfile.create();
    profile.setLanguage(CSharpConstants.LANGUAGE_KEY);

    SMInputFactory inputFactory = initStax();
    try {
      SMHierarchicCursor cursor = inputFactory.rootElementCursor(reader);
      SMInputCursor rulesetCursor = cursor.advance().childElementCursor();

      parseRules(profile, rulesetCursor);

      cursor.getStreamReader().closeCompletely();
    } catch (XMLStreamException e) {
      messages.addErrorText("Failed to read the profile to import: " + e.getMessage());
    }

    return profile;
  }

  private void parseRules(RulesProfile profile, SMInputCursor rulesetCursor) throws XMLStreamException {
    Map<String, ActiveRule> activeRules = Maps.newHashMap();
    while (rulesetCursor.getNext() != null) {
      String severity = rulesetCursor.getAttrValue("name");
      RulePriority rulePriority = RulePriority.valueOfString("default".equals(severity) ? "major" : severity);

      SMInputCursor rulesCursor = rulesetCursor.childElementCursor();
      while (rulesCursor.getNext() != null) {
        String gendarmeCategory = rulesCursor.getAttrValue("from");
        createActiveRule(rulesCursor, activeRules, gendarmeCategory, profile, rulePriority);
        addParametersToActiveRules(rulesCursor, activeRules, gendarmeCategory);
      }

    }
  }

  private void createActiveRule(SMInputCursor rulesCursor, Map<String, ActiveRule> activeRules, String gendarmeCategory,
      RulesProfile profile, RulePriority rulePriority) throws XMLStreamException {
    String[] includedRules = StringUtils.split(rulesCursor.getAttrValue("include"), '|');
    for (int i = 0; i < includedRules.length; i++) {
      String configKey = includedRules[i].trim() + "@" + gendarmeCategory;
      ActiveRule activeRule = activeRules.get(configKey);
      if (activeRule == null) {
        Rule rule = ruleFinder.find(ruleQuery.withConfigKey(configKey));
        if (rule != null) {
          activeRule = profile.activateRule(rule, rulePriority);
          activeRules.put(configKey, activeRule);
        }
      } else if (activeRule.getSeverity().equals(RulePriority.MAJOR)) {
        // MAJOR is the default one: maybe another priority has been defined and we must set it
        activeRule.setSeverity(rulePriority);
      }
    }
  }

  private void addParametersToActiveRules(SMInputCursor rulesCursor, Map<String, ActiveRule> activeRules, String gendarmeCategory)
      throws XMLStreamException {
    SMInputCursor parameterCursor = rulesCursor.childElementCursor();
    while (parameterCursor.getNext() != null) {
      ActiveRule currentRule = activeRules.get(parameterCursor.getAttrValue("rule") + "@" + gendarmeCategory);
      if (currentRule != null) {
        currentRule.setParameter(parameterCursor.getAttrValue("property"), parameterCursor.getAttrValue("value"));
      }
    }
  }

  private SMInputFactory initStax() {
    XMLInputFactory xmlFactory = XMLInputFactory2.newInstance();
    xmlFactory.setProperty(XMLInputFactory.IS_COALESCING, Boolean.TRUE);
    xmlFactory.setProperty(XMLInputFactory.IS_NAMESPACE_AWARE, Boolean.FALSE);
    xmlFactory.setProperty(XMLInputFactory.SUPPORT_DTD, Boolean.FALSE);
    xmlFactory.setProperty(XMLInputFactory.IS_VALIDATING, Boolean.FALSE);
    return new SMInputFactory(xmlFactory);
  }

}
