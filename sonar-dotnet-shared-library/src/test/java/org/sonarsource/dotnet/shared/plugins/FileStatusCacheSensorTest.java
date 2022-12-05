/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2022 SonarSource SA
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
package org.sonarsource.dotnet.shared.plugins;

import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.cache.WriteCache;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.config.internal.MapSettings;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import java.io.File;
import java.io.IOException;
import java.security.NoSuchAlgorithmException;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class FileStatusCacheSensorTest {
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Rule
  public LogTester logTester = new LogTester();

  @Test
  public void should_describe() {
    var sensorDescriptor = new DefaultSensorDescriptor();
    var sensor = new FileStatusCacheSensor(new HashProvider());
    sensor.describe(sensorDescriptor);

    assertThat(sensorDescriptor.name()).isEqualTo("File status cache sensor");
    assertThat(sensorDescriptor.languages()).isEmpty();
  }

  @Test
  public void execute_whenAnalyzingAPullRequest_logsMessage() throws IOException {
    var settings = new MapSettings().setProperty("sonar.pullrequest.key", "42");
    var context = SensorContextTester.create(temp.newFolder()).setSettings(settings);
    var sensor = new FileStatusCacheSensor(new HashProvider());

    sensor.execute(context);

    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsExactly("Incremental PR analysis: Cache is not updated for pull requests.");
  }

  @Test
  public void execute_whenPullRequestCacheBasePathIsNotConfigured_logsWarning() throws IOException {
    var context = SensorContextTester.create(temp.newFolder());
    var sut = new FileStatusCacheSensor(new HashProvider());

    sut.execute(context);

    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsExactly("Incremental PR analysis: Pull request cache base path is not configured. Skipping file status upload.");
  }

  @Test
  public void execute_whenCacheIsDisabled_logsWarning() throws IOException {
    var settings = new MapSettings().setProperty("sonar.pullrequest.cache.basepath", "C:/");
    var context = SensorContextTester.create(temp.newFolder()).setSettings(settings);
    context.setCacheEnabled(false);
    var sensor = new FileStatusCacheSensor(new HashProvider());

    sensor.execute(context);

    assertThat(logTester.logs()).containsExactly("Incremental PR analysis: Analysis cache is disabled. Skipping file status upload for incremental PR analysis.");
  }

  @Test
  public void execute_whenCacheIsEnabled_itAddsTheFiles() throws IOException, NoSuchAlgorithmException {
    var hashProvider = mock(HashProvider.class);
    when(hashProvider.computeHash(any())).thenReturn(new byte[0]);
    var context = CreateContextForCaching();
    var sut = new FileStatusCacheSensor(hashProvider);

    sut.execute(context);

    assertThat(logTester.logs(LoggerLevel.WARN)).isEmpty();
    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsExactly(
      "Incremental PR analysis: Preparing to upload file status.",
      "Incremental PR analysis: Adding VB/Bar.vb hash to cache.",
      "Incremental PR analysis: Adding CSharp/Foo.cs hash to cache."
    );
  }

  @Test
  public void execute_whenHashingFails_itLogsAnError() throws IOException, NoSuchAlgorithmException {
    var hashProvider = mock(HashProvider.class);
    when(hashProvider.computeHash(any())).thenThrow(new IOException());
    var context = CreateContextForCaching();
    var sut = new FileStatusCacheSensor(hashProvider);

    sut.execute(context);

    assertThat(logTester.logs(LoggerLevel.WARN)).containsExactly(
      "Incremental PR analysis: An error occurred while computing the hash for VB/Bar.vb",
      "Incremental PR analysis: An error occurred while computing the hash for CSharp/Foo.cs"
    );
    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsExactly(
      "Incremental PR analysis: Preparing to upload file status.",
      "Incremental PR analysis: Adding VB/Bar.vb hash to cache.",
      "Incremental PR analysis: Adding CSharp/Foo.cs hash to cache."
    );
  }

  private SensorContext CreateContextForCaching() throws IOException {
    var basePath = temp.newFolder();

    var settings = new MapSettings();
    settings.setProperty("sonar.pullrequest.cache.basepath", basePath.getCanonicalPath());

    var context = SensorContextTester.create(basePath);
    context.setCacheEnabled(true);
    context.setSettings(settings);
    context.setNextCache(mock(WriteCache.class));
    context.fileSystem().add(new TestInputFileBuilder("foo", basePath, new File(basePath, "CSharp/Foo.cs")).setLanguage("cs").setType(InputFile.Type.MAIN).build());
    context.fileSystem().add(new TestInputFileBuilder("bar", basePath, new File(basePath, "VB\\Bar.vb")).setLanguage("vbnet").setType(InputFile.Type.MAIN).build());

    return context;
  }
}
