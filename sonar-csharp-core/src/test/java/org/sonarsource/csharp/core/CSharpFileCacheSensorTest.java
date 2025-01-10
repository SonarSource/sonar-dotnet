/*
 * SonarSource :: C# :: Core
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
package org.sonarsource.csharp.core;

import java.io.File;
import java.io.IOException;
import java.nio.file.Path;
import java.security.NoSuchAlgorithmException;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.RegisterExtension;
import org.junit.jupiter.api.io.TempDir;
import org.slf4j.event.Level;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.cache.WriteCache;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.config.internal.MapSettings;
import org.sonar.api.testfixtures.log.LogTesterJUnit5;
import org.sonarsource.dotnet.shared.plugins.HashProvider;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

class CSharpFileCacheSensorTest {
  @TempDir
  public Path basePath;

  @RegisterExtension
  public LogTesterJUnit5 logTester = new LogTesterJUnit5().setLevel(Level.DEBUG);

  @Test
  void execute_whenCacheIsEnabled_itAddsOnlyTheLanguageFiles() throws IOException, NoSuchAlgorithmException {
    var settings = new MapSettings();
    settings.setProperty(TestCSharpMetadata.INSTANCE.fileSuffixesKey(), ".cs");
    settings.setProperty("sonar.pullrequest.cache.basepath", new File(basePath.toString()).getCanonicalPath());
    var hashProvider = mock(HashProvider.class);
    when(hashProvider.computeHash(any())).thenReturn(new byte[]{42});
    var context = SensorContextTester.create(basePath);
    context.setCacheEnabled(true);
    context.setSettings(settings);
    context.setNextCache(mock(WriteCache.class));
    AddFile(context, basePath.toString(), "CSharp/Foo.cs", TestCSharpMetadata.INSTANCE.languageKey());
    AddFile(context, basePath.toString(), "CSharp/Foo.cshtml", TestCSharpMetadata.INSTANCE.languageKey());
    AddFile(context, basePath.toString(), "CSharp/Foo.razor", TestCSharpMetadata.INSTANCE.languageKey());
    AddFile(context, basePath.toString(), "VB/Bar.vb", "other-language-key");
    var sut = new CSharpFileCacheSensor(TestCSharpMetadata.INSTANCE, hashProvider);

    sut.execute(context);

    assertThat(logTester.logs(Level.WARN)).isEmpty();
    assertThat(logTester.logs(Level.DEBUG)).containsExactly(
      "Incremental PR analysis: Preparing to upload file hashes.",
      "Incremental PR analysis: Adding hash for 'CSharp/Foo.cs' to the cache.",
      "Incremental PR analysis: Adding hash for 'CSharp/Foo.cshtml' to the cache.",
      "Incremental PR analysis: Adding hash for 'CSharp/Foo.razor' to the cache."
    );
  }

  private static void AddFile(SensorContextTester context, String basePath, String filePath, String languageKey) {
    context.fileSystem().add(new TestInputFileBuilder("project-key", new File(basePath), new File(basePath, filePath)).setLanguage(languageKey).setType(InputFile.Type.MAIN).build());
  }
}
