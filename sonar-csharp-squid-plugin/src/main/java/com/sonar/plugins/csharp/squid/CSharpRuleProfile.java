/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.plugins.csharp.squid;

import com.sonar.csharp.checks.CheckList;
import org.sonar.api.profiles.AnnotationProfileParser;
import org.sonar.api.profiles.ProfileDefinition;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.utils.ValidationMessages;
import org.sonar.plugins.csharp.api.CSharpConstants;

/**
 * Creates the Sonar C# Way profile using the C# checks found by the container.
 */
public class CSharpRuleProfile extends ProfileDefinition {

  private final AnnotationProfileParser annotationProfileParser;

  public CSharpRuleProfile(AnnotationProfileParser annotationProfileParser) {
    this.annotationProfileParser = annotationProfileParser;
  }

  @Override
  public RulesProfile createProfile(ValidationMessages validation) {
    return annotationProfileParser.parse(CSharpSquidConstants.REPOSITORY_KEY, CSharpConstants.CSHARP_WAY_PROFILE, CSharpConstants.LANGUAGE_KEY,
        CheckList.getChecks(), validation);
  }

}
