/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2019 SonarSource SA
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
import org.mockito.Mockito;
import org.sonar.api.utils.log.LogTester;

import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.ArgumentMatchers.isNull;
import static org.mockito.Mockito.inOrder;
import static org.mockito.Mockito.mock;
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
    return new JsonParser().parse(json).getAsJsonObject();
  }

  // VS 2015 Update 3
  @Test
  public void sarif_version_1_0() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    inOrder.verify(callback).onRule(anyString(), anyString(), anyString(), anyString(), anyString());
    Location location = new Location(new File(baseDir, "Foo.cs").getAbsolutePath(), "One issue per line", 1, 0, 1, 13);
    inOrder.verify(callback).onIssue("S1234", "warning", location, Collections.emptyList());
    location = new Location(new File(baseDir, "Bar.cs").getAbsolutePath(), "One issue per line", 2, 0, 2, 33);
    inOrder.verify(callback).onIssue("S1234", "warning", location, Collections.emptyList());
    verifyNoMoreInteractions(callback);
  }

  @Test
  public void sarif_version_1_0_file_level() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_file_level_issue.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    String filePath = new File(baseDir, "Program.cs").getAbsolutePath();
    inOrder.verify(callback).onFileIssue(eq("S104"), eq("warning"), eq(filePath), eq("Some dummy message"));
    inOrder.verify(callback).onFileIssue(eq("S105"), isNull(), eq(filePath), eq("Some dummy message"));
    Location location = new Location(filePath, "Some dummy message", 1, 0, 1, 1);
    inOrder.verify(callback).onIssue("S105", "warning", location, Collections.emptyList());
    location = new Location(filePath, "Some dummy message", 1, 0, 2, 0);
    inOrder.verify(callback).onIssue("S105", "warning", location, Collections.emptyList());

    inOrder.verify(callback).onFileIssue(eq("S106"), eq("warning"), eq(filePath), eq("Some dummy message"));

    verifyNoMoreInteractions(callback);
  }

  @Test
  public void sarif_version_1_0_suppressed() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_suppressed.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    Location location = new Location(new File(baseDir, "Bar.cs").getAbsolutePath(), "One issue per line", 2, 0, 2, 33);
    inOrder.verify(callback).onRule("S1234", "One issue per line", "This rule will create an issue for every source code line", "warning", "Test");
    inOrder.verify(callback).onIssue("S1234", "warning", location, Collections.emptyList());
    verifyNoMoreInteractions(callback);
  }

  // #934
  @Test
  public void sarif_version_1_0_file_name_with_illegal_char() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_file_name_with_illegal_char.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    inOrder.verify(callback).onRule("S1118", "Utility classes should not have public constructors",
      "Utility classes, which are collections of static members, are not meant to be instantiated. Even abstract utility classes, which can be extended, should not have public constructors.",
      "warning", "Sonar Code Smell");
    Location location = new Location(new File(baseDir, "ConsoleApplication1/P@!$#&+-=r^{}og_r()a m[1].cs").getAbsolutePath(),
      "Add a 'protected' constructor or the 'static' keyword to the class declaration.", 9, 10, 9, 17);
    inOrder.verify(callback).onIssue("S1118", "warning", location, Collections.emptyList());
    verifyNoMoreInteractions(callback);
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
    inOrder.verify(callback).onIssue("S1186", "warning", location, Collections.emptyList());
    location = new Location(filePath, "Remove this unused method parameter \"args\".", 26, 25, 26, 38);
    inOrder.verify(callback).onIssue("S1172", "warning", location, Collections.emptyList());
    location = new Location(filePath, "Add a \"protected\" constructor or the \"static\" keyword to the class declaration.",
      9, 17, 9, 24);
    inOrder.verify(callback).onIssue("S1118", "warning", location, Collections.emptyList());
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
    inOrder.verify(callback).onIssue("S107", "warning", location, Collections.emptyList());
    verifyNoMoreInteractions(callback);
  }

  @Test
  public void dont_fail_on_empty_report() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_empty.json"), String::toString).accept(callback);
    verify(callback, Mockito.never()).onIssue(Mockito.anyString(), Mockito.anyString(), Mockito.any(Location.class), Mockito.anyCollectionOf(Location.class));
  }

  @Test
  public void sarif_version_1_0_secondary_locations() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(null, getRoot("v1_0_secondary_locations.json"), String::toString).accept(callback);

    InOrder inOrder = inOrder(callback);
    inOrder.verify(callback).onRule(anyString(), anyString(), anyString(), anyString(), anyString());
    String filePath = new File(baseDir, "Foo.cs").getAbsolutePath();
    Location primaryLocation = new Location(filePath, "Identical sub-expressions on both sides of operator \"==\".",
      28, 34, 28, 51);
    Collection<Location> secondaryLocations = new ArrayList<>();
    secondaryLocations.add(new Location(filePath, null, 28, 13, 28, 30));
    inOrder.verify(callback).onIssue("S1764", "warning", primaryLocation, secondaryLocations);
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
    inOrder.verify(callback).onIssue("S3776", "warning", primaryLocation, secondaryLocations);
    verifyNoMoreInteractions(callback);
  }

}
