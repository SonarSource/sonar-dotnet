/*
 * Sonar C# Plugin :: StyleCop
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

package org.sonar.plugins.csharp.stylecop;

import org.apache.commons.lang.StringUtils;
import org.sonar.api.Properties;
import org.sonar.api.Property;
import org.sonar.api.PropertyType;
import org.sonar.api.config.Settings;
import org.sonar.api.platform.ServerFileSystem;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleRepository;
import org.sonar.api.rules.XMLRuleParser;
import org.sonar.plugins.csharp.api.CSharpConstants;

import java.io.File;
import java.io.StringReader;
import java.util.ArrayList;
import java.util.List;

/**
 * Loads the StyleCop rules configuration file.
 */
@Properties({
  @Property(key = StyleCopRuleRepository.SONAR_STYLECOP_CUSTOM_RULES_PROP_KEY,
    defaultValue = "", name = "StyleCop custom rules",
    description = "XML description of StyleCop custom rules", type = PropertyType.TEXT,
    global = true, project = false)
})
public class StyleCopRuleRepository extends RuleRepository {

  public static final String SONAR_STYLECOP_CUSTOM_RULES_PROP_KEY = "sonar.stylecop.customRules.definition";

  // for user extensions
  private ServerFileSystem fileSystem;
  private XMLRuleParser xmlRuleParser;
  private Settings settings;

  public StyleCopRuleRepository(ServerFileSystem fileSystem, XMLRuleParser xmlRuleParser, Settings settings) {
    super(StyleCopConstants.REPOSITORY_KEY, CSharpConstants.LANGUAGE_KEY);
    setName(StyleCopConstants.REPOSITORY_NAME);
    this.fileSystem = fileSystem;
    this.xmlRuleParser = xmlRuleParser;
    this.settings = settings;
  }

  @Override
  public List<Rule> createRules() {
    List<Rule> rules = new ArrayList<Rule>();

    // StyleCop rules
    rules.addAll(xmlRuleParser.parse(StyleCopRuleRepository.class.getResourceAsStream("/org/sonar/plugins/csharp/stylecop/rules/rules.xml")));

    // Custom rules:
    // - old fashion: XML files in the file system
    for (File userExtensionXml : fileSystem.getExtensions(StyleCopConstants.REPOSITORY_KEY, "xml")) {
      rules.addAll(xmlRuleParser.parse(userExtensionXml));
    }
    // - new fashion: through the Web interface
    String customRules = settings.getString(SONAR_STYLECOP_CUSTOM_RULES_PROP_KEY);
    if (StringUtils.isNotBlank(customRules)) {
      rules.addAll(xmlRuleParser.parse(new StringReader(customRules)));
    }

    return rules;
  }
}
