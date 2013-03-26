/*
 * Sonar .NET Plugin :: FxCop
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
package org.sonar.plugins.csharp.fxcop;

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
 * Creates FXCop rule repositories for every language supported by FxCop.
 */
@Properties({
  @Property(key = FxCopRuleRepositoryProvider.SONAR_FXCOP_CUSTOM_RULES_PROP_KEY,
    defaultValue = "", name = "FxCop custom rules",
    description = "XML description of FxCop custom rules", type = PropertyType.TEXT,
    global = true, project = false)
})
public class FxCopRuleRepositoryProvider extends ExtensionProvider implements ServerExtension {

  public static final String SONAR_FXCOP_CUSTOM_RULES_PROP_KEY = "sonar.fxcop.customRules.definition";

  private ServerFileSystem fileSystem;
  private XMLRuleParser xmlRuleParser;
  private Settings settings;

  public FxCopRuleRepositoryProvider(ServerFileSystem fileSystem, Settings settings) {
    this.fileSystem = fileSystem;
    this.xmlRuleParser = new XMLRuleParser();
    this.settings = settings;
  }

  @Override
  public Object provide() {
    List<FxCopRuleRepository> extensions = new ArrayList<FxCopRuleRepository>();

    for (String languageKey : FxCopConstants.SUPPORTED_LANGUAGES) {
      String repoKey = FxCopConstants.REPOSITORY_KEY;
      if (!"cs".equals(languageKey)) {
        // every repository key should be "fxcop-<language_key>", except for C# for which it is simply "fxcop" (for backward compatibility)
        repoKey += "-" + languageKey;
      }
      extensions.add(new FxCopRuleRepository(repoKey, languageKey, fileSystem, xmlRuleParser, settings));
    }

    return extensions;
  }

}
