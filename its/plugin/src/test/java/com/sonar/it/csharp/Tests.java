/*
 * SonarSource :: C# :: ITs :: Plugin
 * Copyright (C) 2011 SonarSource
 * sonarqube@googlegroups.com
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
package com.sonar.it.csharp;

import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.SonarRunner;
import com.sonar.orchestrator.locator.FileLocation;
import org.junit.ClassRule;
import org.junit.runner.RunWith;
import org.junit.runners.Suite;
import org.junit.runners.Suite.SuiteClasses;

@RunWith(Suite.class)
@SuiteClasses({
  CommentRegexpRuleTest.class,
  CoverageTest.class,
  DoNotAnalyzeTestFilesTest.class,
  FileSuffixesTest.class,
  FxCopTest.class,
  MetricsTest.class,
  NoSonarTest.class,
  UnitTestResultsTest.class
})
public class Tests {

  private static final String PLUGIN_KEY = "csharp";

  @ClassRule
  public static final Orchestrator ORCHESTRATOR = Orchestrator.builderEnv()
    .addPlugin(FileLocation.of("../../target/sonar-csharp-plugin.jar"))
    .restoreProfileAtStartup(FileLocation.of("profiles/fxcop.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/no_rule.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/class_name.xml"))
    .restoreProfileAtStartup(FileLocation.of("profiles/template_rule.xml"))
    .build();

  public static boolean is_at_least_plugin_4_1() {
    return ORCHESTRATOR.getConfiguration().getPluginVersion(PLUGIN_KEY).isGreaterThanOrEquals("4.1");
  }

  public static SonarRunner createSonarRunnerBuild() {
    SonarRunner build = SonarRunner.create();
    return build;
  }

}
