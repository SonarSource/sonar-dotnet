/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.toolkit.squid;

import java.io.BufferedWriter;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileWriter;
import java.io.PrintStream;
import java.nio.charset.Charset;
import java.util.Collection;
import java.util.Properties;

import org.apache.commons.io.FileUtils;

import com.sonar.csharp.CSharpConfiguration;
import com.sonar.csharp.parser.CSharpParser;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.impl.ast.AstXmlPrinter;

public class CSharpParserCLI {

  private Properties configuration = new Properties();
  private final PrintStream out;
  private PrintStream err;
  private int numberOfFiles = 0;
  private int numberOfParsedFiles = 0;
  private int numberOfUnparsableFiles = 0;

  public static enum Argument {
    srcDir, astDir, astDumpActivated, charset;
  }

  public CSharpParserCLI(String... args) {
    this(System.out, args);
  }

  public CSharpParserCLI(PrintStream out, String... args) {
    this.out = out;
    if (args.length == 0) {
      printHelp();
    }
    for (String argument : args) {
      String keyValue[] = argument.split("=");
      if (keyValue.length != 2) {
        throw new IllegalArgumentException("Argument '" + argument + "' in't in desired format 'key=value'");
      }
      configuration.put(keyValue[0], keyValue[1]);
    }
  }

  public void parseAndDumpAst() {
    File astDir = null;
    if (isAstDumpActivated()) {
      astDir = removeAndCreateAstDir();
    }
    File errorLog = new File("ParsingErrors.log");
    errorLog.delete();
    try {
      err = new PrintStream(errorLog);
    } catch (FileNotFoundException e) {
      throw new IllegalStateException("Unable to open initialize error log file : " + errorLog.getAbsolutePath(), e);
    }

    CSharpConfiguration conf = new CSharpConfiguration(getCharset());
    CSharpParser parser = new CSharpParser(conf);

    @SuppressWarnings("unchecked")
    Collection<File> srcFiles = FileUtils.listFiles(getSrcDir(), new String[] { "cs" }, true);

    numberOfFiles = srcFiles.size();

    displayStartingMessage(numberOfFiles);
    long startTime = System.currentTimeMillis();
    for (File srcFile : srcFiles) {
      int progress = (numberOfParsedFiles + 1) * 100 / numberOfFiles;
      out.printf("\rParsing file: %5d / %d (%3d%%)", numberOfParsedFiles + 1, numberOfFiles, progress);
      try {
        AstNode ast = parser.parse(srcFile);
        if (isAstDumpActivated()) {
          FileWriter astNodeFile = new FileWriter(new File(astDir, srcFile.getName() + ".xml"));
          BufferedWriter bf = new BufferedWriter(astNodeFile);
          AstXmlPrinter.print(ast, bf);
          bf.flush();
          bf.close();
        }
      } catch (Exception e) {
        out.print(" << Parsing error...");
        err.println("***** Unable to parse file : " + srcFile.getAbsolutePath());
        err.println(e);
        err.println("\n\n\n");
        numberOfUnparsableFiles++;
      }
      numberOfParsedFiles++;
    }
    printFooter();
    if (numberOfUnparsableFiles != 0) {
      out.println("==> " + numberOfUnparsableFiles + " files (out of " + numberOfFiles
          + ") have not been parsed. Please refer to the \"ParsingErrors.log\" file for more detail.");
    } else {
      out.println("==> SUCCESS: all " + numberOfFiles + " files have been parsed!");
    }
    long endTime = System.currentTimeMillis();
    out.println("\nParsing time : " + ((double) (endTime - startTime) / 1000) + " seconds\n\n");
    err.flush();
    err.close();
  }

  private void displayStartingMessage(int numberOfFiles) {
    printHeader();
    out.println("                   srcDir : " + getSrcDir().getAbsolutePath());
    out.println("   number of source files : " + numberOfFiles);
    out.println("                  charset : " + getCharset().displayName());
    out.println("         astDumpActivated : " + isAstDumpActivated());
    if (isAstDumpActivated()) {
      out.println("                   astDir : " + getAstDir().getAbsolutePath());
    }
    printFooter();
  }

  private void printHeader() {
    out.println("**********************************************************************************");
    out.println("                     Welcome to Sonar C# Parser");
    out.println("**********************************************************************************");
    out.println();
  }

  private void printFooter() {
    out.println();
    out.println("**********************************************************************************");
    out.println();
  }

  private void printHelp() {
    printHeader();
    out.println("Arguments: ");
    out.println("           - srcDir=...             : absolute path of the folder that contains the source files");
    out.println("           - charset=...            : the charset to use");
    out.println("                                      (optional, defaults to the platform default charset)");
    out.println("           - astDumpActivated=...   : whether to print out in XML files the Abstract Syntac Tree generated from the source files");
    out.println("                                      (optional, defaults to \"true\")");
    out.println("           - astDir=...             : the folder where the XML files will be written to");
    out.println("                                      (optional, defaults to \"./SonarGeneratedAstDir\")");
    printFooter();
  }

  public File removeAndCreateAstDir() {
    File astDir = getAstDir();
    if (astDir.exists()) {
      for (File file : astDir.listFiles()) {
        file.delete();
      }
      astDir.delete();
    }
    astDir.mkdir();
    return astDir;
  }

  public Charset getCharset() {
    if (configuration.get(Argument.charset.name()) != null) {
      return Charset.forName((String) configuration.get(Argument.charset.name()));
    }
    return Charset.defaultCharset();
  }

  public boolean isAstDumpActivated() {
    if (configuration.get(Argument.astDumpActivated.name()) != null) {
      return Boolean.parseBoolean((String) configuration.get(Argument.astDumpActivated.name()));
    }
    return true;
  }

  public File getSrcDir() {
    if (configuration.get(Argument.srcDir.name()) == null) {
      throw new IllegalStateException("'srcDir' argument is missing.");
    }
    File srcDir = new File((String) configuration.get(Argument.srcDir.name()));
    if ( !srcDir.exists()) {
      throw new IllegalStateException("source directory '" + srcDir.getAbsolutePath() + "' doesn't exist.");
    }
    return srcDir;
  }

  public File getAstDir() {
    if (configuration.get(Argument.astDir.name()) == null) {
      return new File("./SonarGeneratedAstDir");
    }
    return new File((String) configuration.get(Argument.astDir.name()));
  }
}
