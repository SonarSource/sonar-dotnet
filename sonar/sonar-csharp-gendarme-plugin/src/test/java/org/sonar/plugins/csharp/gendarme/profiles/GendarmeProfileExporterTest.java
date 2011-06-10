/*
 * Sonar C# Plugin :: Gendarme
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

package org.sonar.plugins.csharp.gendarme.profiles;

import java.io.IOException;
import java.io.StringWriter;

import org.junit.Test;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RulePriority;
import org.sonar.plugins.csharp.gendarme.GendarmeConstants;
import org.sonar.test.TestUtils;
import org.xml.sax.SAXException;

public class GendarmeProfileExporterTest {

  @Test
  public void testSimpleGendarmeRulesToExport() throws IOException, SAXException {
    RulesProfile profile = RulesProfile.create("Sonar C# Way", "cs");
    profile.activateRule(
        Rule.create(GendarmeConstants.REPOSITORY_KEY, "DoNotUseLockedRegionOutsideMethodRule", "DoNotUseLockedRegionOutsideMethodRule")
            .setConfigKey("DoNotUseLockedRegionOutsideMethodRule@Gendarme.Rules.Concurrency.dll"), RulePriority.INFO);
    profile.activateRule(
        Rule.create(GendarmeConstants.REPOSITORY_KEY, "AvoidLongMethodsRule", "AvoidLongMethodsRule")
            .setConfigKey("AvoidLongMethodsRule@Gendarme.Rules.Smells.dll").setSeverity(RulePriority.BLOCKER), null);
    profile.activateRule(
        Rule.create(GendarmeConstants.REPOSITORY_KEY, "AvoidLargeClassesRule", "AvoidLargeClassesRule")
            .setConfigKey("AvoidLargeClassesRule@Gendarme.Rules.Smells.dll").setSeverity(RulePriority.CRITICAL), null);
    profile.activateRule(
        Rule.create(GendarmeConstants.REPOSITORY_KEY, "AvoidCodeDuplicatedInSameClassRule", "AvoidCodeDuplicatedInSameClassRule")
            .setConfigKey("AvoidCodeDuplicatedInSameClassRule@Gendarme.Rules.Smells.dll"), null);

    Rule ruleWithParam = Rule.create(GendarmeConstants.REPOSITORY_KEY, "AvoidComplexMethodsRule", "AvoidComplexMethodsRule")
        .setConfigKey("AvoidComplexMethodsRule@Gendarme.Rules.Smells.dll").setSeverity(RulePriority.CRITICAL);
    ruleWithParam.createParameter("SuccessThreshold");
    ActiveRule activeRule = profile.activateRule(ruleWithParam, null);
    activeRule.setParameter("SuccessThreshold", "13");

    StringWriter writer = new StringWriter();
    new GendarmeProfileExporter().exportProfile(profile, writer);
    TestUtils.assertSimilarXml(TestUtils.getResourceContent("/ProfileExporter/SimpleRules.Gendarme.exported.xml"), writer.toString());
  }

}
