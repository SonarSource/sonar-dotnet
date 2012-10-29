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
package org.sonar.plugins.csharp.fxcop.profiles;

import org.apache.commons.io.IOUtils;
import org.apache.commons.lang.StringUtils;
import org.sonar.api.profiles.ProfileImporter;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleFinder;
import org.sonar.api.rules.RulePriority;
import org.sonar.api.rules.RuleQuery;
import org.sonar.api.utils.ValidationMessages;
import org.sonar.plugins.csharp.fxcop.FxCopConstants;
import org.sonar.plugins.csharp.fxcop.profiles.utils.FxCopRule;
import org.sonar.plugins.csharp.fxcop.profiles.utils.FxCopRuleParser;

import java.io.IOException;
import java.io.Reader;
import java.util.List;

/**
 * Class that allows to import FxCop rule definition files into a Sonar Rule Profile
 */
public class FxCopProfileImporter extends ProfileImporter {

  private RuleFinder ruleFinder;
  private String languageKey;

  public static class CSharpRegularFxCopProfileImporter extends FxCopProfileImporter {
    public CSharpRegularFxCopProfileImporter(RuleFinder ruleFinder) {
      super("cs", FxCopConstants.REPOSITORY_KEY, FxCopConstants.REPOSITORY_NAME, ruleFinder);
    }
  }

  public static class VbNetRegularFxCopProfileImporter extends FxCopProfileImporter {
    public VbNetRegularFxCopProfileImporter(RuleFinder ruleFinder) {
      super("vbnet", FxCopConstants.REPOSITORY_KEY + "-vbnet", FxCopConstants.REPOSITORY_NAME, ruleFinder);
    }
  }

  // Not used for the moment (see SONARPLUGINS-929) - Must be updated with correct language keys when reactivated
  public static class UnitTestsFxCopProfileImporter extends FxCopProfileImporter {
    public UnitTestsFxCopProfileImporter(RuleFinder ruleFinder) {
      super("update-when-SONARPLUGINS-929-activated", FxCopConstants.TEST_REPOSITORY_KEY, FxCopConstants.TEST_REPOSITORY_NAME, ruleFinder);
    }
  }

  protected FxCopProfileImporter(String languageKey, String repositoryKey, String repositoryName, RuleFinder ruleFinder) {
    super(repositoryKey, repositoryName);
    setSupportedLanguages(languageKey);
    this.ruleFinder = ruleFinder;
    this.languageKey = languageKey;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public RulesProfile importProfile(Reader reader, ValidationMessages messages) {
    RulesProfile profile = RulesProfile.create();
    profile.setLanguage(languageKey);

    try {
      List<FxCopRule> fxcopConfig = FxCopRuleParser.parse(IOUtils.toString(reader));

      for (FxCopRule fxCopRule : fxcopConfig) {
        String ruleName = fxCopRule.getName();
        Rule rule = ruleFinder.find(RuleQuery.create().withRepositoryKey(getKey()).withKey(ruleName));

        if (rule != null) {
          String rawPriority = fxCopRule.getPriority();
          RulePriority rulePriority = RulePriority.MAJOR;
          if (StringUtils.isNotEmpty(rawPriority)) {
            rulePriority = RulePriority.valueOfString(rawPriority);
          }
          profile.activateRule(rule, rulePriority);
        }
      }
    } catch (IOException e) {
      messages.addErrorText("Failed to read the profile to import: " + e.getMessage());
    }

    return profile;
  }
}
