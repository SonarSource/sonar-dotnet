/*
 * Sonar C# Plugin :: StyleCop
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

package org.sonar.plugins.csharp.stylecop.profiles;

import org.junit.Before;
import org.junit.Ignore;
import org.junit.Test;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RulePriority;
import org.sonar.plugins.csharp.stylecop.StyleCopConstants;
import org.sonar.test.TestUtils;

import java.io.File;
import java.io.StringWriter;

@Ignore("FIXME")
public class StyleCopProfileExporterTest {

  private RulesProfile profile;

  @Before
  public void setUp() {

    profile = RulesProfile.create("Sonar way", "cs");
    profile.activateRule(
      Rule.create(StyleCopConstants.REPOSITORY_KEY, "ElementMustBeginWithUpperCaseLetter", "Element must begin with upper case letter")
        .setConfigKey("StyleCop.CSharp.NamingRules#ElementMustBeginWithUpperCaseLetter"), null);
    profile.activateRule(
      Rule.create(StyleCopConstants.REPOSITORY_KEY, "KeywordsMustBeSpacedCorrectly", "Keywords must be spaced correctly")
        .setConfigKey("StyleCop.CSharp.SpacingRules#KeywordsMustBeSpacedCorrectly").setSeverity(RulePriority.MINOR), null);

  }

  @Test
  public void should_generate_a_simple_stylecop_conf() throws Exception {

    StringWriter writer = new StringWriter();
    new StyleCopProfileExporter.RegularStyleCopProfileExporter().exportProfile(profile, writer);

    TestUtils.assertSimilarXml(TestUtils.getResourceContent("/ProfileExporter/SimpleRules.StyleCop.exported.xml"), writer.toString());
  }

  @Test
  public void should_generate_a_stylecop_conf_with_analyzer_settings() throws Exception {
    File analyzersSettings = TestUtils.getResource("/Settings.StyleCop");
    StringWriter writer = new StringWriter();
    new StyleCopProfileExporter.RegularStyleCopProfileExporter().exportProfile(profile, writer, analyzersSettings);

    TestUtils.assertSimilarXml(TestUtils.getResourceContent("/ProfileExporter/SimpleRules.AnalyzerSettings.StyleCop.exported.xml"), writer.toString());
  }

  public void testParseAnalyzerSettings()
  {

  }

}
