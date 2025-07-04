/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonarsource.dotnet.shared.sarif;

import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import java.io.File;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Collections;
import java.util.regex.Pattern;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.mockito.InOrder;
import org.sonar.api.testfixtures.log.LogTester;
import org.slf4j.event.Level;

import static java.util.Collections.emptyList;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.anyCollection;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.ArgumentMatchers.isNull;
import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.inOrder;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.verifyNoMoreInteractions;

public class SarifParser10Test {

  @Rule
  public LogTester logTester = new LogTester();

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  private File baseDir;

  @Before
  public void prepare() throws Exception {
    baseDir = temp.newFolder();
  }

  private JsonObject getRoot(String fileName) throws IOException {
    String json = new String(Files.readAllBytes(Paths.get("src/test/resources/SarifParserTest/" + fileName)), StandardCharsets.UTF_8);
    json = json.replaceAll(Pattern.quote("%BASEDIR%"), baseDir.toURI().toASCIIString());
    return JsonParser.parseString(json).getAsJsonObject();
  }

  // VS 2015 Update 3
  @Test
  public void sarif_version_1_0() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    inOrder.verify(callback).onRule(anyString(), anyString(), anyString(), anyString(), anyString());
    Location location = new Location(new File(baseDir, "Foo.cs").getAbsolutePath(), "One issue per line", 1, 0, 1, 13);
    inOrder.verify(callback).onIssue("S1234", "warning", location, Collections.emptyList(), false);
    location = new Location(new File(baseDir, "Bar.cs").getAbsolutePath(), "One issue per line", 2, 0, 2, 33);
    inOrder.verify(callback).onIssue("S1234", "warning", location, Collections.emptyList(), false);
    verifyNoMoreInteractions(callback);
  }

  @Test
  public void sarif_version_1_0_file_level() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_file_level_issue.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    String filePath = new File(baseDir, "Program.cs").getAbsolutePath();
    inOrder.verify(callback).onFileIssue("S104", "warning", filePath, emptyList(), "Some dummy message");
    inOrder.verify(callback).onFileIssue(eq("S105"), isNull(), eq(filePath), eq(emptyList()), eq("Some dummy message"));
    Location location = new Location(filePath, "Some dummy message", 1, 0, 1, 1);
    inOrder.verify(callback).onIssue("S105", "warning", location, Collections.emptyList(), false);
    location = new Location(filePath, "Some dummy message", 1, 0, 2, 0);
    inOrder.verify(callback).onIssue("S105", "warning", location, Collections.emptyList(), false);

    inOrder.verify(callback).onFileIssue("S106", "warning", filePath, emptyList(), "Some dummy message");

    verifyNoMoreInteractions(callback);
  }

  @Test
  public void sarif_version_1_0_suppressed() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_suppressed.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    Location location = new Location(new File(baseDir, "Bar.cs").getAbsolutePath(), "One issue per line", 2, 0, 2, 33);
    inOrder.verify(callback).onRule("S1234", "One issue per line", "This rule will create an issue for every source code line", "warning", "Test");
    inOrder.verify(callback).onIssue("S1234", "warning", location, emptyList(), false);
    verifyNoMoreInteractions(callback);
  }

  // #934
  @Test
  public void sarif_version_1_0_file_name_with_illegal_char() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_file_name_with_illegal_char.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    inOrder.verify(callback).onRule("S1118", "Utility classes should not have public constructors",
      "Utility classes, which are collections of static members, are not meant to be instantiated. Even abstract utility classes, which can be extended, should not have public " +
        "constructors.",
      "warning", "Sonar Code Smell");
    Location location = new Location(new File(baseDir, "ConsoleApplication1/P@!$#&+-=r^{}og_r()a m[1].cs").getAbsolutePath(),
      "Add a 'protected' constructor or the 'static' keyword to the class declaration.", 9, 10, 9, 17);
    inOrder.verify(callback).onIssue("S1118", "warning", location, emptyList(), false);
    verifyNoMoreInteractions(callback);
  }

  @Test
  public void sarif_version_1_0_no_message() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_no_message.json"), String::toString).accept(callback);

    verify(callback).onRule(anyString(), anyString(), anyString(), anyString(), anyString());
    // In this report we have 2 issues, one without a message and another one with message.
    // We will push to SQ/SC only the occurrence with message. For the other, a warning is logged.
    verify(callback, times(1)).onIssue(eq("S1234"), eq("warning"), any(), any(), eq(false));
    verifyNoMoreInteractions(callback);

    assertThat(logTester.logs(Level.WARN)).hasSize(1);
    assertThat(logTester.logs(Level.WARN).get(0)).startsWith("Issue raised without a message for rule S1234. Content: {\"ruleId\":\"S1234\",\"level\":\"warning\"," +
      "\"locations\":[{\"resultFile\":{\"uri\":\"file:");
  }

  @Test
  public void sarif_version_1_0_no_location() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_no_location.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    inOrder.verify(callback).onRule(anyString(), anyString(), anyString(), anyString(), anyString());
    inOrder.verify(callback).onProjectIssue("S1234", "warning", null, "One issue per line");
    verifyNoMoreInteractions(callback);
  }

  @Test
  public void sarif_version_1_0_empty_location() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_empty_location.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    inOrder.verify(callback).onRule(anyString(), anyString(), anyString(), anyString(), anyString());
    inOrder.verify(callback).onProjectIssue("S1234", "warning", null, "One issue per line");
    verifyNoMoreInteractions(callback);
  }

  @Test
  public void sarif_version_1_0_more_rules() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_another.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    inOrder.verify(callback, times(3)).onRule(anyString(), anyString(), anyString(), anyString(), anyString());
    String filePath = new File(baseDir, "Program.cs").getAbsolutePath();
    Location location = new Location(filePath,
      "Add a nested comment explaining why this method is empty, throw a \"NotSupportedException\" or complete the implementation.",
      26, 20, 26, 24);
    inOrder.verify(callback).onIssue("S1186", "warning", location, emptyList(), false);
    location = new Location(filePath, "Remove this unused method parameter \"args\".", 26, 25, 26, 38);
    inOrder.verify(callback).onIssue("S1172", "warning", location, emptyList(), false);
    location = new Location(filePath, "Add a \"protected\" constructor or the \"static\" keyword to the class declaration.",
      9, 17, 9, 24);
    inOrder.verify(callback).onIssue("S1118", "warning", location, emptyList(), false);
    verifyNoMoreInteractions(callback);
  }

  @Test
  public void sarif_path_escaping() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_escaping.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    inOrder.verify(callback).onRule(anyString(), anyString(), anyString(), anyString(), anyString());
    Location location = new Location(new File(baseDir, "git/Temp Folder SomeRandom!@#$%^&()/csharp/ConsoleApplication1/Program.cs").getAbsolutePath(),
      "Method has 3 parameters, which is greater than the 2 authorized.", 52, 23, 52, 47);
    inOrder.verify(callback).onIssue("S107", "warning", location, emptyList(), false);
    verifyNoMoreInteractions(callback);
  }

  @Test
  public void dont_fail_on_empty_report() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_empty.json"), String::toString).accept(callback);
    verify(callback, never()).onIssue(anyString(), anyString(), any(Location.class), anyCollection(), eq(false));
  }

  @Test
  public void sarif_version_1_0_secondary_locations() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_secondary_locations.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    inOrder.verify(callback).onRule(eq("S1764"), anyString(), anyString(), anyString(), anyString());
    inOrder.verify(callback).onRule(eq("CS9999"), anyString(), anyString(), anyString(), anyString());
    inOrder.verify(callback).onRule(eq("S3776"), anyString(), anyString(), anyString(), anyString());
    String filePath = new File(baseDir, "Foo.cs").getAbsolutePath();
    Location primaryLocation = new Location(filePath, "Identical sub-expressions on both sides of operator \"==\".",
      28, 34, 28, 51);
    Collection<Location> secondaryLocations = new ArrayList<>();
    secondaryLocations.add(new Location(filePath, null, 28, 13, 28, 30));
    inOrder.verify(callback).onIssue("S1764", "warning", primaryLocation, secondaryLocations, false);
    inOrder.verify(callback).onFileIssue(eq("CS9999"), eq("warning"), eq(filePath), eq(secondaryLocations), anyString());
    Collection<Location> secondaryLocationsWithMessage = new ArrayList<>();
    secondaryLocationsWithMessage.add(new Location(filePath, "+1", 28, 13, 28, 30));
    inOrder.verify(callback).onFileIssue(eq("S3776"), eq("warning"), eq(filePath), eq(secondaryLocationsWithMessage), anyString());
    verifyNoMoreInteractions(callback);
  }

  @Test
  public void sarif_version_1_0_secondary_locations_messages() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_secondary_locations_messages.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    inOrder.verify(callback).onRule(anyString(), anyString(), anyString(), anyString(), anyString());
    String filePath = new File(baseDir, "Foo.cs").getAbsolutePath();
    Location primaryLocation = new Location(filePath, "Refactor this method to reduce its Cognitive Complexity from 30 to the 15 allowed",
      54, 21, 54, 24);
    Collection<Location> secondaryLocations = new ArrayList<>();
    secondaryLocations.add(new Location(filePath, "+1", 56, 12, 56, 14));
    secondaryLocations.add(new Location(filePath, "+2 (incl 1 for nesting)", 65, 16, 65, 18));
    secondaryLocations.add(new Location(filePath, "+1", 65, 52, 65, 54));
    inOrder.verify(callback).onIssue("S3776", "warning", primaryLocation, secondaryLocations, false);
    verifyNoMoreInteractions(callback);
  }

  @Test
  public void sarif_version_1_0_execution_flow() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_execution_flow.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    inOrder.verify(callback).onRule(anyString(), anyString(), anyString(), anyString(), anyString());
    String filePath = new File(baseDir, "Foo.cs").getAbsolutePath();
    Location primaryLocation = new Location(filePath, "'text' is null on at least one execution path.", 308, 29, 308, 33);
    Collection<Location> secondaryLocations = new ArrayList<>();
    secondaryLocations.add(new Location(filePath, "Learning null", 22, 16, 22, 35));
    secondaryLocations.add(new Location(filePath, "Taking assumption", 22, 16, 22, 35));
    secondaryLocations.add(new Location(filePath, "Taking assumption", 23, 42, 23, 49));
    secondaryLocations.add(primaryLocation);
    inOrder.verify(callback).onIssue("S2259", "warning", primaryLocation, secondaryLocations, true);
    verifyNoMoreInteractions(callback);
  }

  @Test
  public void sarif_version_1_0_execution_flow_invalid_value() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_invalid_execution_flow_value.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    inOrder.verify(callback).onRule(anyString(), anyString(), anyString(), anyString(), anyString());
    String filePath = new File(baseDir, "Foo.cs").getAbsolutePath();
    Location primaryLocation = new Location(filePath, "'text' is null on at least one execution path.", 308, 29, 308, 33);
    Collection<Location> secondaryLocations = new ArrayList<>();
    secondaryLocations.add(new Location(filePath, "Learning null", 22, 16, 22, 35));
    secondaryLocations.add(new Location(filePath, "Taking assumption", 22, 16, 22, 35));
    secondaryLocations.add(new Location(filePath, "Taking assumption", 23, 42, 23, 49));
    inOrder.verify(callback).onIssue("S2259", "warning", primaryLocation, secondaryLocations, false);
    verifyNoMoreInteractions(callback);
  }

  @Test
  public void sarif_version_1_0_execution_flow_no_secondary_locations() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_execution_flow_no_secondary_locations.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    inOrder.verify(callback).onRule(anyString(), anyString(), anyString(), anyString(), anyString());
    String filePath = new File(baseDir, "Foo.cs").getAbsolutePath();
    Location primaryLocation = new Location(filePath, "'text' is null on at least one execution path.", 308, 29, 308, 33);
    inOrder.verify(callback).onIssue("S2259", "warning", primaryLocation, Collections.emptyList(), false);
    verifyNoMoreInteractions(callback);
  }

  @Test
  public void sarif_version_1_0_no_execution_flow() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_no_execution_flow.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    inOrder.verify(callback).onRule(anyString(), anyString(), anyString(), anyString(), anyString());
    String filePath = new File(baseDir, "Foo.cs").getAbsolutePath();
    Location primaryLocation = new Location(filePath, "'text' is null on at least one execution path.", 308, 29, 308, 33);
    Collection<Location> secondaryLocations = new ArrayList<>();
    secondaryLocations.add(new Location(filePath, "Learning null", 22, 16, 22, 35));
    secondaryLocations.add(new Location(filePath, "Taking assumption", 22, 16, 22, 35));
    secondaryLocations.add(new Location(filePath, "Taking assumption", 23, 42, 23, 49));
    verify(callback).onIssue("S2259", "warning", primaryLocation, secondaryLocations, false);
    verifyNoMoreInteractions(callback);
  }

  @Test
  public void sarif_version_1_0_file_issue_with_execution_flow() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_file_level_issue_with_execution_flow.json"), String::toString).accept(callback);

    assertThat(logTester.logs(Level.WARN)).hasSize(1);
    assertThat(logTester.logs(Level.WARN).get(0)).startsWith("Unexpected file issue with an execution flow for rule S1234. File: ");
  }

  @Test
  public void sarif_version_1_0_relative_paths() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_relative_paths.json"), String::toString).accept(callback);
    InOrder inOrder = inOrder(callback);

    Location s1144PrimaryLocation = new Location("SourceGeneratorPOC.Generators\\SourceGeneratorPOC.SourceGenerator\\Greetings.cs", "Remove the unused private method " +
      "'UnusedMethod'.", 7, 8, 7, 46);
    inOrder.verify(callback).onIssue("S1144", "warning", s1144PrimaryLocation, new ArrayList<>(), false);

    Location s1186PrimaryLocation = new Location("SourceGeneratorPOC.Generators\\SourceGeneratorPOC.SourceGenerator\\Greetings.cs", "Add a nested comment explaining why this " +
      "method is empty, throw a 'NotSupportedException' or complete the implementation.", 7, 28, 7, 40);
    inOrder.verify(callback).onIssue("S1186", "warning", s1186PrimaryLocation, new ArrayList<>(), false);
  }

  @Test
  public void sarif_version_1_0_region_with_length() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_region_with_length.json"), String::toString).accept(callback);
    InOrder inOrder = inOrder(callback);

    Location primaryLocation = new Location("SourceGeneratorPOC.Generators\\SourceGeneratorPOC.SourceGenerator\\Greetings.cs", "Remove the unused private method 'UnusedMethod'."
      , 7, 8, 7, 46);
    inOrder.verify(callback).onIssue("S1144", "warning", primaryLocation, new ArrayList<>(), false);
  }

  // https://sonarsource.atlassian.net/browse/NET-1139
  @Test
  public void sarif_version_1_0_same_start_end_location() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_same_start_end_location.json"), String::toString).accept(callback);
    InOrder inOrder = inOrder(callback);
    Location primaryLocation = new Location("Foo.cs", "Fix formatting - middle of line", 8, 4, 8, 5);
    inOrder.verify(callback).onIssue("IDE0055", "note", primaryLocation, emptyList(), false);
    primaryLocation = new Location("Foo.cs", "Fix formatting - start of line", 8, 0, 8, 1);
    inOrder.verify(callback).onIssue("IDE0055", "note", primaryLocation, emptyList(), false);
    inOrder.verify(callback).onFileIssue(eq("IDE0055"), anyString(), eq("Foo.cs"), eq(emptyList()), eq("Fix formatting - First line"));
  }
}
