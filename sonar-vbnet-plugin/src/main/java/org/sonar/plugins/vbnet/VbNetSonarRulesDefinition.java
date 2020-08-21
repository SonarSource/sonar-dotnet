/*
 * SonarVB
 * Copyright (C) 2012-2020 SonarSource SA
 * mailto:info AT sonarsource DOT com
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
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package org.sonar.plugins.vbnet;

import org.sonar.api.scanner.ScannerSide;
import org.sonarsource.dotnet.shared.plugins.AbstractRulesDefinition;

import static org.sonar.plugins.vbnet.VbNetPlugin.REPOSITORY_KEY;
import static org.sonar.plugins.vbnet.VbNetPlugin.REPOSITORY_NAME;

@ScannerSide
public class VbNetSonarRulesDefinition extends AbstractRulesDefinition {
  private static final String RULES_XML = "/org/sonar/plugins/vbnet/rules.xml";

  public VbNetSonarRulesDefinition() {
    super(REPOSITORY_KEY, REPOSITORY_NAME, VbNetPlugin.LANGUAGE_KEY, RULES_XML);
  }

  @Override
  protected String getRuleJson(String ruleKey) {
    return "/org/sonar/plugins/vbnet/" + ruleKey + "_vb.net.json";
  }
}
