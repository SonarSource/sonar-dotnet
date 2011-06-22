/*
 * Sonar C# Plugin :: FxCop
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

import java.io.IOException;
import java.io.Reader;
import java.util.List;

import org.apache.commons.io.IOUtils;
import org.apache.commons.lang.StringUtils;
import org.sonar.api.profiles.ProfileImporter;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleFinder;
import org.sonar.api.rules.RulePriority;
import org.sonar.api.rules.RuleQuery;
import org.sonar.api.utils.ValidationMessages;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.fxcop.FxCopConstants;
import org.sonar.plugins.csharp.fxcop.profiles.utils.FxCopRule;
import org.sonar.plugins.csharp.fxcop.profiles.utils.FxCopRuleParser;

/**
 * Class that allows to import FxCop rule definition files into a Sonar Rule Profile
 */
public class FxCopProfileImporter extends ProfileImporter {

  private RuleFinder ruleFinder;

  public FxCopProfileImporter(RuleFinder ruleFinder) {
    super(FxCopConstants.REPOSITORY_KEY, FxCopConstants.REPOSITORY_NAME);
    setSupportedLanguages(CSharpConstants.LANGUAGE_KEY);
    this.ruleFinder = ruleFinder;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public RulesProfile importProfile(Reader reader, ValidationMessages messages) {
    RulesProfile profile = RulesProfile.create();
    profile.setLanguage(CSharpConstants.LANGUAGE_KEY);

    try {
      List<FxCopRule> fxcopConfig = FxCopRuleParser.parse(IOUtils.toString(reader));

      for (FxCopRule fxCopRule : fxcopConfig) {
        String ruleName = fxCopRule.getName();
        Rule rule = ruleFinder.find(RuleQuery.create().withRepositoryKey(FxCopConstants.REPOSITORY_KEY).withKey(ruleName));

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
