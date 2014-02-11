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

import org.junit.Ignore;
import org.junit.Test;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.Rule;
import org.sonar.plugins.csharp.fxcop.FxCopConstants;
import org.sonar.test.TestUtils;

import java.io.StringWriter;

import static org.fest.assertions.Assertions.assertThat;

public class FxCopProfileExporterTest {

  @Ignore("FIXME")
  @Test
  public void testSimpleFxCopRulesToExport() throws Exception {
    RulesProfile profile = RulesProfile.create("Sonar way", "cs");
    profile.activateRule(
      Rule.create(FxCopConstants.REPOSITORY_KEY, "AssembliesShouldHaveValidStrongNames", "Assemblies should have valid strong names")
        .setConfigKey("AssembliesShouldHaveValidStrongNames@$(FxCopDir)\\Rules\\DesignRules.dll"), null);
    profile.activateRule(Rule.create(FxCopConstants.REPOSITORY_KEY, "UsePropertiesWhereAppropriate", "Use properties where appropriate")
      .setConfigKey("UsePropertiesWhereAppropriate@$(FxCopDir)\\Rules\\DesignRules.dll"), null);
    profile.activateRule(Rule.create(FxCopConstants.REPOSITORY_KEY, "AvoidDuplicateAccelerators", "Avoid duplicate accelerators")
      .setConfigKey("AvoidDuplicateAccelerators@$(FxCopDir)\\Rules\\GlobalizationRules.dll"), null);

    StringWriter writer = new StringWriter();
    FxCopProfileExporter exporter = new FxCopProfileExporter.CSharpRegularFxCopProfileExporter();
    assertThat(exporter.getKey()).isEqualTo("fxcop");
    assertThat(exporter.getSupportedLanguages()).containsOnly("cs");

    exporter.exportProfile(profile, writer);
    TestUtils.assertSimilarXml(TestUtils.getResourceContent("/ProfileExporter/SimpleRules.FxCop.exported.xml"), writer.toString());
  }

  @Test
  public void testExporterForVbNet() {
    FxCopProfileExporter exporter = new FxCopProfileExporter.VbNetRegularFxCopProfileExporter();
    // just test the differences with C#
    assertThat(exporter.getKey()).isEqualTo("fxcop-vbnet");
    assertThat(exporter.getSupportedLanguages()).containsOnly("vbnet");
  }

}
