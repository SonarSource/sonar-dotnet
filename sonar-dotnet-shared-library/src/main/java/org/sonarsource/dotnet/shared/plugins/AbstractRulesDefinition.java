/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2018 SonarSource SA
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
/*
   * SonarC#
   * Copyright (C) 2014-2017 SonarSource SA
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

import com.google.gson.Gson;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.lang.reflect.Field;
import java.nio.charset.StandardCharsets;
import javax.annotation.Nullable;
import org.sonar.api.SonarRuntime;
import org.sonar.api.batch.ScannerSide;
import org.sonar.api.rules.RuleType;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.api.server.rule.RulesDefinitionXmlLoader;
import org.sonar.api.utils.Version;

@ScannerSide
public abstract class AbstractRulesDefinition implements RulesDefinition {

  private static final Version SQ_7_3 = Version.create(7, 3);
  private static final Gson GSON = new Gson();

  private final String repositoryKey;
  private final String repositoryName;
  private final String languageKey;
  private final String rulesXmlFilePath;
  private final boolean supportsSecurityHotspots;

  // for vb.net
  protected AbstractRulesDefinition(String repositoryKey, String repositoryName, String languageKey, String rulesXmlFilePath) {
    this(repositoryKey, repositoryName, languageKey, rulesXmlFilePath, null);
  }

  protected AbstractRulesDefinition(String repositoryKey, String repositoryName, String languageKey, String rulesXmlFilePath, @Nullable SonarRuntime sonarRuntime) {
    this.repositoryKey = repositoryKey;
    this.repositoryName = repositoryName;
    this.languageKey = languageKey;
    this.rulesXmlFilePath = rulesXmlFilePath;
    this.supportsSecurityHotspots = sonarRuntime != null && sonarRuntime.getApiVersion().isGreaterThanOrEqual(SQ_7_3);
  }

  @Override
  public void define(Context context) {
    NewRepository repository = context
      .createRepository(repositoryKey, languageKey)
      .setName(repositoryName);

    RulesDefinitionXmlLoader loader = new RulesDefinitionXmlLoader();
    loader.load(repository, new InputStreamReader(getClass().getResourceAsStream(rulesXmlFilePath), StandardCharsets.UTF_8));

    for (NewRule rule : repository.rules()) {
      updateMetadata(rule);
    }

    repository.done();
  }

  private void updateMetadata(NewRule rule) {
    if (supportsSecurityHotspots) {
      RuleMetadata ruleMetadata = readRuleMetadata(rule.key());
      for (String s : ruleMetadata.securityStandards.OWASP) {
        rule.addOwaspTop10(RulesDefinition.OwaspTop10.valueOf(s));
      }
      rule.addCwe(ruleMetadata.securityStandards.CWE);
      if (ruleMetadata.isSecurityHotspot()) {
        setSecurityHotspotType(rule);
      }
    }
  }

  private static RuleMetadata readRuleMetadata(String ruleKey) {
    String resourcePath = "/org/sonar/plugins/csharp/" + ruleKey + "_c#.json";
    try (InputStream stream = AbstractRulesDefinition.class.getResourceAsStream(resourcePath)) {
      return  stream != null
        ? GSON.fromJson(new InputStreamReader(stream, StandardCharsets.UTF_8), RuleMetadata.class)
        : new RuleMetadata();
    } catch (IOException e) {
      throw new IllegalStateException("Failed to read: " + resourcePath, e);
    }
  }

  /**
   * The rules.xml file is backwards compatible with SonarQube 6.7,
   * so we need to explicitly set the type after deserialization
   */
  private static void setSecurityHotspotType(NewRule rule) {
    try {
      Field type = rule.getClass().getDeclaredField("type");
      type.setAccessible(true);
      type.set(rule, RuleType.SECURITY_HOTSPOT);
    } catch (NoSuchFieldException|IllegalAccessException e) {
      throw new IllegalStateException("Cannot set type to SECURITY_HOTSPOT", e);
    }
  }

  private static class RuleMetadata {
    private static final String SECURITY_HOTSPOT = "SECURITY_HOTSPOT";

    String type;
    SecurityStandards securityStandards = new SecurityStandards();

    boolean isSecurityHotspot() {
      return SECURITY_HOTSPOT.equals(type);
    }
  }

  // for deserialization purposes
  @SuppressWarnings("squid:S00116")
  private static class SecurityStandards {
    int[] CWE = {};
    String[] OWASP = {};
  }
}
