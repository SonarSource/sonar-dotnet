/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
 *
 * Sonar is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * Sonar is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Sonar; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package org.sonar.plugin.dotnet.core;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

import org.sonar.api.batch.Phase;
import org.sonar.api.batch.Sensor;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.checks.NoSonarFilter;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.resources.Project;
import org.sonar.api.utils.SonarException;
import org.sonar.plugin.dotnet.core.resource.CSharpFile;
import org.sonar.squid.measures.Metric;
import org.sonar.squid.recognizer.CamelCaseDetector;
import org.sonar.squid.recognizer.CodeRecognizer;
import org.sonar.squid.recognizer.ContainsDetector;
import org.sonar.squid.recognizer.Detector;
import org.sonar.squid.recognizer.EndWithDetector;
import org.sonar.squid.recognizer.KeywordsDetector;
import org.sonar.squid.recognizer.LanguageFootprint;
import org.sonar.squid.text.Source;

@Phase(name = Phase.Name.PRE)
// The NoSonarFilter must be fed before launching the violation engines
public class NoSonarAndCommentedOutLocSensor implements Sensor {

  private final NoSonarFilter noSonarFilter;
  private final CSharp cSharp;

  public NoSonarAndCommentedOutLocSensor(NoSonarFilter noSonarFilter, CSharp cSharp) {
    this.noSonarFilter = noSonarFilter;
    this.cSharp = cSharp;
  }

  public void analyse(Project prj, SensorContext context) {
    List<File> srcFiles = prj.getFileSystem().getSourceFiles(cSharp);
    for (File srcFile : srcFiles) {
      CSharpFile cSharpFile = CSharpFile.from(prj, srcFile, false);
      Source source = analyseSourceCode(srcFile);
      noSonarFilter.addResource(cSharpFile, source.getNoSonarTagLines());
      context.saveMeasure(cSharpFile, CoreMetrics.COMMENTED_OUT_CODE_LINES, (double) source.getMeasure(Metric.COMMENTED_OUT_CODE_LINES));
    }
  }

  protected static Source analyseSourceCode(File file) {
    try {
      return new Source(new FileReader(file), new CodeRecognizer(0.9, new CSharpLanguageFootprint()));
    } catch (FileNotFoundException e) {
      throw new SonarException("Unable to open file '" + file.getAbsolutePath() + "'", e);
    }
  }

  @Override
  public boolean shouldExecuteOnProject(Project prj) {
    String packaging = prj.getPackaging();
    // We only accept the "sln" packaging
    return "sln".equals(packaging);
  }

  private static class CSharpLanguageFootprint implements LanguageFootprint {

    private final Set<Detector> detectors = new HashSet<Detector>();

    public CSharpLanguageFootprint() {
      detectors.add(new EndWithDetector(0.95, '}', ';', '{'));
      detectors.add(new KeywordsDetector(0.7, "||", "&&"));
      detectors.add(new KeywordsDetector(0.3, "abstract", "add", "alias",
          "as", "ascending", "base", "bool", "break", "by", "byte", "case",
          "catch", "char", "checked", "class", "const", "continue", "decimal",
          "default", "delegate", "descending", "do", "double", "dynamic", "else",
          "enum", "equals", "event", "explicit", "extern", "false", "finally",
          "fixed", "float", "for", "foreach", "from", "get", "global", "goto",
          "group", "if", "implicit", "in", "int", "interface", "internal",
          "into", "is", "join", "let", "lock", "long", "namespace", "new",
          "null", "object", "on", "operator", "orderby", "out", "override",
          "params", "partial", "private", "protected", "public", "readonly",
          "ref", "remove", "return", "sbyte", "sealed", "select", "set", "short",
          "sizeof", "stackalloc", "static", "string", "struct", "switch", "this",
          "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
          "unsafe", "ushort", "using", "value", "var", "virtual", "void",
          "volatile", "where", "while", "yield"));
      detectors.add(new ContainsDetector(0.95, "++", "for(", "if(", "while(", "catch(", "switch(", "try{", "else{"));
      detectors.add(new CamelCaseDetector(0.5));
    }

    @Override
    public Set<Detector> getDetectors() {
      return detectors;
    }

  }
}
