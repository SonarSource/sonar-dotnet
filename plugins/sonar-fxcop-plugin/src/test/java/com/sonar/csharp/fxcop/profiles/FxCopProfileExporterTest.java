/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.fxcop.profiles;

import java.io.IOException;
import java.io.StringWriter;

import org.junit.Test;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.Rule;
import org.sonar.test.TestUtils;
import org.xml.sax.SAXException;

public class FxCopProfileExporterTest {

  @Test
  public void testSimpleFxCopRulesToExport() throws IOException, SAXException {
    RulesProfile profile = RulesProfile.create("Sonar C# Way", "cs");
    profile.activateRule(Rule.create("fxcop", "AssembliesShouldHaveValidStrongNames", "Assemblies should have valid strong names")
        .setConfigKey("AssembliesShouldHaveValidStrongNames@$(FxCopDir)\\Rules\\DesignRules.dll"), null);
    profile.activateRule(
        Rule.create("fxcop", "UsePropertiesWhereAppropriate", "Use properties where appropriate").setConfigKey(
            "UsePropertiesWhereAppropriate@$(FxCopDir)\\Rules\\DesignRules.dll"), null);
    profile.activateRule(
        Rule.create("fxcop", "AvoidDuplicateAccelerators", "Avoid duplicate accelerators").setConfigKey(
            "AvoidDuplicateAccelerators@$(FxCopDir)\\Rules\\GlobalizationRules.dll"), null);

    StringWriter writer = new StringWriter();
    new FxCopProfileExporter().exportProfile(profile, writer);

    TestUtils.assertSimilarXml(TestUtils.getResourceContent("/ProfileExporter/SimpleRules.FxCop.exported"), writer.toString());
  }

}
