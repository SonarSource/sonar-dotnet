/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
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
package org.sonarsource.dotnet.shared.plugins;

import java.io.File;
import java.io.IOException;
import java.security.NoSuchAlgorithmException;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.slf4j.event.Level;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.cache.WriteCache;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.config.internal.MapSettings;
import org.sonar.api.testfixtures.log.LogTester;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class AbstractFileCacheSensorTest {
  private static final String LANGUAGE_KEY = "language-key";
  private static final String LANGUAGE_NAME = "Language Name";

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Rule
  public LogTester logTester = new LogTester();

  @Before
  public void before() {
    logTester.setLevel(Level.DEBUG);
  }

  @Test
  public void should_describe() {
    var sensorDescriptor = new DefaultSensorDescriptor();
    var sensor = new FileCacheSensor(new HashProvider());
    sensor.describe(sensorDescriptor);

    assertThat(sensorDescriptor.name()).isEqualTo("Language Name File Caching Sensor");
    assertThat(sensorDescriptor.languages()).containsOnly(LANGUAGE_KEY);
  }

  @Test
  public void execute_whenAnalyzingPullRequest_logsMessage() throws IOException {
    var settings = new MapSettings().setProperty("sonar.pullrequest.base", "42");
    var context = SensorContextTester.create(temp.newFolder()).setSettings(settings);
    var sensor = new FileCacheSensor(new HashProvider());

    sensor.execute(context);

    assertThat(logTester.logs(Level.WARN)).isEmpty();
    assertThat(logTester.logs(Level.DEBUG)).containsExactly("Incremental PR analysis: Cache is not uploaded for pull requests.");
  }

  @Test
  public void execute_whenPullRequestCacheBasePathIsNotConfigured_logsWarning() throws IOException {
    var context = SensorContextTester.create(temp.newFolder());
    context.setCacheEnabled(true);
    var sut = new FileCacheSensor(new HashProvider());

    sut.execute(context);

    assertThat(logTester.logs(Level.WARN)).containsExactly("Incremental PR analysis: Could not determine common base path, cache will not be computed. Consider setting 'sonar.projectBaseDir' property.");
  }

  @Test
  public void execute_whenCacheIsDisabled_logsWarning() throws IOException {
    var context = SensorContextTester.create(temp.newFolder());
    context.setCacheEnabled(false);
    var sensor = new FileCacheSensor(new HashProvider());

    sensor.execute(context);

    assertThat(logTester.logs(Level.WARN)).isEmpty();
    assertThat(logTester.logs(Level.INFO)).containsExactly("Incremental PR analysis: Analysis cache is disabled.");
  }

  @Test
  public void execute_whenCacheIsEnabled_itAddsTheFiles() throws IOException, NoSuchAlgorithmException {
    var hashProvider = mock(HashProvider.class);
    when(hashProvider.computeHash(any())).thenReturn(new byte[]{42});
    var context = CreateContextForCaching();
    var sut = new FileCacheSensor(hashProvider);

    sut.execute(context);

    assertThat(logTester.logs(Level.WARN)).isEmpty();
    assertThat(logTester.logs(Level.DEBUG)).containsExactly(
      "Incremental PR analysis: Preparing to upload file hashes.",
      "Incremental PR analysis: Adding hash for 'VB/Bar.vb' to the cache.",
      "Incremental PR analysis: Adding hash for 'CSharp/Foo.cs' to the cache."
    );
  }

  @Test
  public void execute_whenHashingFails_itLogsAnError() throws IOException, NoSuchAlgorithmException {
    var hashProvider = mock(HashProvider.class);
    when(hashProvider.computeHash(any())).thenThrow(new IOException("exception message"));
    var context = CreateContextForCaching();
    var sut = new FileCacheSensor(hashProvider);

    sut.execute(context);

    assertThat(logTester.logs(Level.WARN)).containsExactly(
      "Incremental PR analysis: An error occurred while computing the hash for VB/Bar.vb",
      "Incremental PR analysis: An error occurred while computing the hash for CSharp/Foo.cs"
    );
    assertThat(logTester.logs(Level.DEBUG)).containsExactly(
      "Incremental PR analysis: Preparing to upload file hashes.",
      "Incremental PR analysis: Adding hash for 'VB/Bar.vb' to the cache.",
      "Incremental PR analysis: Adding hash for 'CSharp/Foo.cs' to the cache."
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
    context.fileSystem().add(new TestInputFileBuilder("foo", basePath, new File(basePath, "CSharp/Foo.cs")).setLanguage(LANGUAGE_KEY).setType(InputFile.Type.MAIN).build());
    context.fileSystem().add(new TestInputFileBuilder("bar", basePath, new File(basePath, "VB\\Bar.vb")).setLanguage(LANGUAGE_KEY).setType(InputFile.Type.MAIN).build());

    return context;
  }

  private static class FileCacheSensor extends AbstractFileCacheSensor {
    public FileCacheSensor(HashProvider hashProvider) {
      super(metadata(), hashProvider);
    }
  }

  private static PluginMetadata metadata() {
    PluginMetadata mock = mock(PluginMetadata.class);
    when(mock.languageKey()).thenReturn(LANGUAGE_KEY);
    when(mock.languageName()).thenReturn(LANGUAGE_NAME);
    return mock;
  }
}
