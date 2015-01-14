/*
 * Sonar C# Plugin :: Core
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
package org.sonar.plugins.csharp;

import org.sonar.api.profiles.ProfileDefinition;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.Rule;
import org.sonar.api.utils.ValidationMessages;

public class CSharpSonarWayProfile extends ProfileDefinition {

  @Override
  public RulesProfile createProfile(ValidationMessages validation) {
    RulesProfile profile = RulesProfile.create(CSharpPlugin.CSHARP_WAY_PROFILE, CSharpPlugin.LANGUAGE_KEY);

    activateRule(profile, "AssignmentInsideSubExpression");
    activateRule(profile, "AsyncAwaitIdentifier");
    activateRule(profile, "BreakOutsideSwitch");
    activateRule(profile, "CommentedCode");
    activateRule(profile, "ParameterAssignedTo");
    activateRule(profile, "SwitchWithoutDefault");
    activateRule(profile, "TabCharacter");
    activateRule(profile, "S127");
    activateRule(profile, "S1301");
    activateRule(profile, "S1116");
    activateRule(profile, "S1145");
    activateRule(profile, "S1125");
    activateRule(profile, "S1109");
    activateRule(profile, "S121");
    activateRule(profile, "S108");
    activateRule(profile, "S1186");
    activateRule(profile, "S1481");
    activateRule(profile, "S101");
    activateRule(profile, "S100");
    activateRule(profile, "FileLoc");
    activateRule(profile, "FunctionComplexity");
    activateRule(profile, "LineLength");
    activateRule(profile, "S1479");
    activateRule(profile, "S1067");
    activateRule(profile, "S107");

    return profile;
  }

  private static void activateRule(RulesProfile profile, String ruleKey) {
    profile.activateRule(Rule.create(CSharpPlugin.REPOSITORY_KEY, ruleKey), null);
  }

}
