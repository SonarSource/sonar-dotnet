/*
 * Sonar .NET Plugin :: Gendarme
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
package org.sonar.plugins.csharp.gendarme;

import org.sonar.api.ExtensionProvider;
import org.sonar.api.Properties;
import org.sonar.api.Property;
import org.sonar.api.PropertyType;
import org.sonar.api.ServerExtension;
import org.sonar.api.config.Settings;
import org.sonar.api.platform.ServerFileSystem;
import org.sonar.api.rules.XMLRuleParser;

import java.util.ArrayList;
import java.util.List;

/**
 * Creates Gendarme rule repositories for every language supported by FxCop.
 */
@Properties({
  @Property(key = GendarmeRuleRepositoryProvider.SONAR_GENDARME_CUSTOM_RULES_PROP_KEY,
    defaultValue = "", name = "Gendarme custom rules",
    description = "XML description of Gendarme custom rules", type = PropertyType.TEXT,
    global = true, project = false)
})
public class GendarmeRuleRepositoryProvider extends ExtensionProvider implements ServerExtension {

  public static final String SONAR_GENDARME_CUSTOM_RULES_PROP_KEY = "sonar.gendarme.customRules.definition";

  private ServerFileSystem fileSystem;
  private XMLRuleParser xmlRuleParser;
  private Settings settings;

  public GendarmeRuleRepositoryProvider(ServerFileSystem fileSystem, Settings settings) {
    this.fileSystem = fileSystem;
    this.xmlRuleParser = new XMLRuleParser();
    this.settings = settings;
  }

  @Override
  public Object provide() {
    List<GendarmeRuleRepository> extensions = new ArrayList<GendarmeRuleRepository>();

    for (String languageKey : GendarmeConstants.SUPPORTED_LANGUAGES) {
      String repoKey = GendarmeConstants.REPOSITORY_KEY;
      if (!"cs".equals(languageKey)) {
        // every repository key should be "gendarme-<language_key>", except for C# for which it is simply "fxcop" (for backward
        // compatibility)
        repoKey += "-" + languageKey;
      }
      extensions.add(new GendarmeRuleRepository(repoKey, languageKey, fileSystem, xmlRuleParser, settings));
    }

    return extensions;
  }

}
