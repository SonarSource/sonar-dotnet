/*
 * SonarSource :: .NET :: Shared library
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

import java.io.File;
import java.util.Collection;
import java.util.Collections;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;

import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.rule.ActiveRule;
import org.sonar.api.batch.sensor.SensorContext;

public class AnalysisInputXml {
  private AnalysisInputXml() {
    // static only
  }

  private static String escapeXml(String str) {
    return str.replace("&", "&amp;").replace("\"", "&quot;").replace("'", "&apos;").replace("<", "&lt;").replace(">", "&gt;");
  }

  private static void appendLine(StringBuilder sb, String line) {
    sb.append(line);
    sb.append("\r\n");
  }

  private static Map<String, String> effectiveParameters(ActiveRule activeRule) {
    Map<String, String> builder = new HashMap<>();

    for (Entry<String, String> param : activeRule.params().entrySet()) {
      builder.put(param.getKey(), param.getValue());
    }

    return Collections.unmodifiableMap(builder);
  }

  public static String generate(boolean includeSettings, boolean ignoreHeaderComments, boolean includeRules,
                                SensorContext context, String repositoryKey, String languageKey, String sourceEncoding) {
    StringBuilder sb = new StringBuilder();

    appendLine(sb, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
    appendLine(sb, "<AnalysisInput>");

    if (includeSettings) {
      appendLine(sb, "  <Settings>");
      appendLine(sb, "    <Setting>");
      appendLine(sb, "      <Key>sonar." + languageKey + ".ignoreHeaderComments</Key>");
      appendLine(sb, "      <Value>" + (ignoreHeaderComments ? "true" : "false") + "</Value>");
      appendLine(sb, "    </Setting>");
      appendLine(sb, "    <Setting>");
      appendLine(sb, "      <Key>sonar.sourceEncoding</Key>");
      appendLine(sb, "      <Value>" + sourceEncoding + "</Value>");
      appendLine(sb, "    </Setting>");
      appendLine(sb, "  </Settings>");
    }

    appendLine(sb, "  <Rules>");
    if (includeRules) {
      Collection<ActiveRule> activeRules = context.activeRules().findByRepository(repositoryKey);
      writeActiveRules(sb, activeRules);
    }
    appendLine(sb, "  </Rules>");

    appendLine(sb, "  <Files>");
    for (File file : filesToAnalyze(context.fileSystem(), languageKey)) {
      appendLine(sb, "    <File>" + escapeXml(file.getAbsolutePath()) + "</File>");
    }
    appendLine(sb, "  </Files>");

    appendLine(sb, "</AnalysisInput>");

    return sb.toString();
  }

  private static void writeActiveRules(StringBuilder sb, Collection<ActiveRule> activeRules) {
    for (ActiveRule activeRule : activeRules) {
      appendLine(sb, "    <Rule>");
      appendLine(sb, "      <Key>" + escapeXml(activeRule.ruleKey().rule()) + "</Key>");
      Map<String, String> parameters = effectiveParameters(activeRule);
      if (!parameters.isEmpty()) {
        appendLine(sb, "      <Parameters>");
        for (Entry<String, String> parameter : parameters.entrySet()) {
          appendLine(sb, "        <Parameter>");
          appendLine(sb, "          <Key>" + escapeXml(parameter.getKey()) + "</Key>");
          appendLine(sb, "          <Value>" + escapeXml(parameter.getValue()) + "</Value>");
          appendLine(sb, "        </Parameter>");
        }
        appendLine(sb, "      </Parameters>");
      }
      appendLine(sb, "    </Rule>");
    }
  }

  private static Iterable<File> filesToAnalyze(FileSystem fs, String languageKey) {
    return fs.files(fs.predicates().and(fs.predicates().hasType(Type.MAIN), fs.predicates().hasLanguage(languageKey)));
  }
}
