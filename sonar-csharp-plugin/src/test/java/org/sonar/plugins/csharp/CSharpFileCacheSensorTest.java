/*
 * SonarC#
 * Copyright (C) 2014-2023 SonarSource SA
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
package org.sonar.plugins.csharp;

import java.io.File;
import java.io.IOException;
import java.security.NoSuchAlgorithmException;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.SonarEdition;
import org.sonar.api.SonarQubeSide;
import org.sonar.api.SonarRuntime;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.cache.WriteCache;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.config.internal.MapSettings;
import org.sonar.api.internal.SonarRuntimeImpl;
import org.sonar.api.testfixtures.log.LogTester;
import org.sonar.api.utils.Version;
import org.slf4j.event.Level;
import org.sonarsource.dotnet.shared.plugins.HashProvider;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class CSharpFileCacheSensorTest {
  private static final SonarRuntime RUNTIME_WITH_ANALYSIS_CACHE = SonarRuntimeImpl.forSonarQube(Version.create(9, 4), SonarQubeSide.SERVER, SonarEdition.COMMUNITY);

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Rule
  public LogTester logTester = new LogTester();

  @Before
  public void before() {
    logTester.setLevel(Level.TRACE);
  }

  @Test
  public void execute_whenCacheIsEnabled_itAddsOnlyTheLanguageFiles() throws IOException, NoSuchAlgorithmException {
    var basePath = temp.newFolder();
    var settings = new MapSettings();
    settings.setProperty(CSharpPlugin.FILE_SUFFIXES_KEY, ".cs");
    settings.setProperty("sonar.pullrequest.cache.basepath", basePath.getCanonicalPath());
    var hashProvider = mock(HashProvider.class);
    when(hashProvider.computeHash(any())).thenReturn(new byte[]{42});
    var context = SensorContextTester.create(basePath);
    context.setCacheEnabled(true);
    context.setSettings(settings);
    context.setNextCache(mock(WriteCache.class));
    AddFile(context, basePath, "CSharp/Foo.cs", CSharpPlugin.LANGUAGE_KEY);
    AddFile(context, basePath, "VB/Bar.vb", "other-language-key");
    var sut = new CSharpFileCacheSensor(new CSharp(settings.asConfig()), hashProvider, RUNTIME_WITH_ANALYSIS_CACHE);

    sut.execute(context);

    assertThat(logTester.logs(Level.WARN)).isEmpty();
    assertThat(logTester.logs(Level.DEBUG)).containsExactly(
      "Incremental PR analysis: Preparing to upload file hashes.",
      "Incremental PR analysis: Adding hash for 'CSharp/Foo.cs' to the cache."
    );
  }

  private static void AddFile(SensorContextTester context, File basePath, String filePath, String languageKey) {
    context.fileSystem().add(new TestInputFileBuilder("project-key", basePath, new File(basePath, filePath)).setLanguage(languageKey).setType(InputFile.Type.MAIN).build());
  }
}
