/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.plugins.csharp.squid;

import org.sonar.api.profiles.AnnotationProfileParser;
import org.sonar.api.profiles.ProfileDefinition;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.utils.ValidationMessages;
import org.sonar.plugins.csharp.api.CSharpConstants;

import com.sonar.plugins.csharp.squid.check.CSharpCheck;

/**
 * Creates the Sonar C# Way profile using the C# checks found by the container.
 */
public class CSharpRuleProfile extends ProfileDefinition {

  private AnnotationProfileParser annotationProfileParser;

  private CSharpCheck[] checks;

  /**
   * Creates a {@link CSharpRuleProfile}
   * 
   * @param annotationProfileParser
   *          the annotation parser
   */
  public CSharpRuleProfile(AnnotationProfileParser annotationProfileParser) {
    this(annotationProfileParser, new CSharpCheck[] {});
  }

  /**
   * Creates a {@link CSharpRuleProfile}
   * 
   * @param annotationProfileParser
   *          the annotation parser
   * @param checks
   *          the C# checks provided by the container
   */
  public CSharpRuleProfile(AnnotationProfileParser annotationProfileParser, CSharpCheck[] checks) {
    this.annotationProfileParser = annotationProfileParser;
    this.checks = checks;
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public RulesProfile createProfile(ValidationMessages validation) {
    return annotationProfileParser.parse(CSharpSquidConstants.REPOSITORY_KEY, CSharpConstants.CSHARP_WAY_PROFILE,
        CSharpConstants.LANGUAGE_KEY, CSharpCheck.toCollection(checks), validation);
  }

}
