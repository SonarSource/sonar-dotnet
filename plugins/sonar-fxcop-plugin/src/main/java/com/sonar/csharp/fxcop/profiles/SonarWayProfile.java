/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.fxcop.profiles;

import java.io.InputStreamReader;

import org.sonar.api.profiles.ProfileDefinition;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.utils.ValidationMessages;

public final class SonarWayProfile extends ProfileDefinition {

  private FxCopProfileImporter profileImporter;

  public SonarWayProfile(FxCopProfileImporter profileImporter) {
    this.profileImporter = profileImporter;
  }

  public RulesProfile createProfile(ValidationMessages messages) {
    RulesProfile profile = profileImporter.importProfile(
        new InputStreamReader(getClass().getResourceAsStream("/com/sonar/csharp/fxcop/rules/DefaultRules.FxCop")), messages);
    profile.setName("Sonar C# Way");
    return profile;
  }
}
