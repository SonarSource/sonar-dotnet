/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.fxcop.profiles;

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
import org.sonar.api.utils.SonarException;
import org.sonar.api.utils.ValidationMessages;

import com.sonar.csharp.fxcop.Constants;
import com.sonar.csharp.fxcop.profiles.utils.FxCopRule;
import com.sonar.csharp.fxcop.profiles.utils.FxCopRuleParser;

public class FxCopProfileImporter extends ProfileImporter {

  private final RuleFinder ruleFinder;

  public FxCopProfileImporter(RuleFinder ruleFinder) {
    super(Constants.REPOSITORY_KEY, Constants.PLUGIN_NAME);
    setSupportedLanguages(Constants.LANGUAGE_KEY);
    this.ruleFinder = ruleFinder;
  }

  // TODO This is not enough, we should look at "RuleFile ... AllRulesEnabled=True" as well.
  @Override
  public RulesProfile importProfile(Reader reader, ValidationMessages messages) {
    RulesProfile profile = RulesProfile.create();
    profile.setLanguage(Constants.LANGUAGE_KEY);

    List<FxCopRule> fxcopConfig = null;

    try {
      fxcopConfig = FxCopRuleParser.parse(IOUtils.toString(reader));
    } catch (IOException e) {
      throw new SonarException("Failed to read the profile to import.", e);
    }

    for (FxCopRule fxCopRule : fxcopConfig) {
      String ruleName = fxCopRule.getName();
      Rule rule = ruleFinder.find(RuleQuery.create().withRepositoryKey(Constants.REPOSITORY_KEY).withKey(ruleName));

      if (rule != null) {
        String rawPriority = fxCopRule.getPriority();
        RulePriority rulePriority = RulePriority.MAJOR;
        if (StringUtils.isNotEmpty(rawPriority)) {
          rulePriority = RulePriority.valueOfString(rawPriority);
        }
        profile.activateRule(rule, rulePriority);
      }
    }

    return profile;
  }
}
