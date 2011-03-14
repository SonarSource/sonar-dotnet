/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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

package org.sonar.plugin.dotnet.gendarme;

import java.io.File;
import java.util.Collection;
import java.util.List;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RulesManager;
import org.sonar.api.rules.Violation;
import org.sonar.plugin.dotnet.core.AbstractXmlParser;
import org.sonar.plugin.dotnet.core.resource.CLRAssembly;
import org.sonar.plugin.dotnet.core.resource.CSharpFileLocator;
import org.w3c.dom.Element;

import org.apache.maven.dotnet.commons.GeneratedCodeFilter;
import org.apache.maven.dotnet.commons.project.SourceFile;

/**
 * @Deprecated Since 0.6 (Stax version of the parser)
 *
 */
@Deprecated
public class GendarmeResultParser extends AbstractXmlParser {

  private final static Logger log = LoggerFactory.getLogger(GendarmeResultParser.class);

  private final Project project;
  private final SensorContext context;
  private final RulesManager rulesManager;
  private final RulesProfile profile;
  private final CSharpFileLocator fileLocator;

  /**
   * Constructs a @link{GendarmeResultParser}.
   * 
   * @param project
   * @param context
   * @param rulesManager
   * @param profile
   */
  public GendarmeResultParser(Project project, SensorContext context,
      RulesManager rulesManager, RulesProfile profile, CSharpFileLocator fileLocator) {
    super();
    this.project = project;
    this.context = context;
    this.rulesManager = rulesManager;
    this.profile = profile;
    this.fileLocator = fileLocator;
  }

  /**
   * Parses a processed violation file.
   * 
   * @param stream
   */
  public void parse(File file) {
    List<Element> issues = extractElements(file, "/issues/issue");
    // We add each issue
    for (Element issueElement : issues) {

      String key = getNodeContent(issueElement, "key");
      String source = getNodeContent(issueElement, "source");
      String message = getNodeContent(issueElement, "message");
      String location = getNodeContent(issueElement, "location");

      final String filePath;
      final Integer lineNumber;

      if (StringUtils.isEmpty(source)
          || StringUtils.contains(source, "debugging symbols unavailable")) {
        String assemblyName = StringUtils.substringBefore(
            getNodeContent(issueElement, "assembly-name"), ",");

        if ("[across all assemblies analyzed]".equals(assemblyName)) {
          // this one will be ignored... Anyway it barely never happen
          continue;
        }

        CLRAssembly assembly = CLRAssembly.fromName(project, assemblyName);
        if (StringUtils.contains(key, "Assembly")) {
          // we assume we have a violation at the
          // assembly level
          filePath = assembly.getVisualProject().getDirectory()
          .getAbsolutePath()
            + File.separator
            + "Properties"
            + File.separator
            + "AssemblyInfo.cs";
          lineNumber = null;
        } else {
          if (StringUtils.containsNone(location, " ")) {
            // we will try to find a cs file that match with the class name
            final String className = StringUtils.substringBeforeLast(
                StringUtils.substringAfterLast(location, "."), "/");
            Collection<SourceFile> sourceFiles = assembly.getVisualProject()
              .getSourceFiles();

            SourceFile sourceFile = null;
            for (SourceFile currentSourceFile : sourceFiles) {
              if (StringUtils
                  .startsWith(currentSourceFile.getName(), className)) {
                sourceFile = currentSourceFile;
                break;
              }
            }

            if (sourceFile == null) {
              // this one will be ignored
              log.info("ignoring gendarme violation {} {} {}", new Object[] {
                key, source, message });
              continue;
            } else {
              filePath = sourceFile.getFile().getAbsolutePath();
              lineNumber = null;
            }
          } else {
            // this one will be ignored
            log.info("ignoring gendarme violation {} {} {}", new Object[] {
              key, source, message });
            continue;
          }
        }
      } else {
        DefectLocation defectLocation = DefectLocation.parse(source);
        filePath = defectLocation.getPath();
        lineNumber = defectLocation.getLineNumber();

        //
        // we do not care about violations in generated files
        //
        if (GeneratedCodeFilter.INSTANCE.isGenerated(StringUtils
            .substringAfterLast(filePath, File.separator))) {
          continue;
        }
      }

      if (lineNumber==null
          && StringUtils.contains(location, "::")) {
        // append a more specific location information
        // to the message
        String codeElement = StringUtils.substringAfter(location, "::");
        if (!StringUtils.contains(message, codeElement)) {
          message = StringUtils.substringAfter(location, "::") + " " + message;
        }
      }

      Resource<?> resource = fileLocator.getResource(project, filePath);

      Rule rule = rulesManager.getPluginRule(GendarmePlugin.KEY, key);
      if (rule == null || resource == null) {
        // We skip the rules that were not registered
        continue;
      }
      ActiveRule activeRule = profile.getActiveRule(GendarmePlugin.KEY, key);
      Violation violation = new Violation(rule, resource);
      violation.setLineId(lineNumber);
      violation.setMessage(message);
      if (activeRule != null) {
        violation.setPriority(activeRule.getPriority());
      }

      // We save the violation
      context.saveViolation(violation);
    }
  }

}
