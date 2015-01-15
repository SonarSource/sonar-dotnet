/*
 * Sonar C# Plugin :: Core
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
package org.sonar.plugins.csharp;

import org.sonar.api.rule.Severity;
import org.sonar.api.server.rule.RuleParamType;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.squidbridge.rules.ExternalDescriptionLoader;
import org.sonar.squidbridge.rules.SqaleXmlLoader;

public class CSharpSonarRulesDefinition implements RulesDefinition {

  @Override
  public void define(Context context) {
    NewRepository repository = context
      .createRepository(CSharpPlugin.REPOSITORY_KEY, CSharpPlugin.LANGUAGE_KEY)
      .setName(CSharpPlugin.REPOSITORY_NAME);

    repository.createRule("AssignmentInsideSubExpression").setName("Assignment should not be used inside sub-expressions").setSeverity(Severity.MAJOR);
    repository.createRule("AsyncAwaitIdentifier").setName("'async' and 'await' should not be used as identifier").setSeverity(Severity.MAJOR);
    repository.createRule("BreakOutsideSwitch").setName("'break' should not be used outside of 'switch'").setSeverity(Severity.MAJOR);
    repository.createRule("CommentedCode").setName("Comment should not include code").setSeverity(Severity.MINOR);
    repository.createRule("ParameterAssignedTo").setName("Parameter variable should not be assigned to").setSeverity(Severity.MAJOR);
    repository.createRule("SwitchWithoutDefault").setName("'switch' statement should have a 'default:' case").setSeverity(Severity.MAJOR);
    repository.createRule("TabCharacter").setName("Tabulation character should not be used").setSeverity(Severity.MINOR);
    repository.createRule("S127").setName("A loop's counter should not be assigned within the loop body").setSeverity(Severity.MAJOR);
    repository.createRule("S1301").setName("\"switch\" statements should have at least 3 \"case\" clauses").setSeverity(Severity.MAJOR);
    repository.createRule("S1116").setName("Empty statements should be removed").setSeverity(Severity.MAJOR);
    repository.createRule("S1145").setName("\"if\" statement conditions should not unconditionally evaluate to \"true\" or to \"false\"").setSeverity(Severity.MAJOR);
    repository.createRule("S1125").setName("Literal boolean values should not be used in condition expressions").setSeverity(Severity.MAJOR);
    repository.createRule("S126").setName("\"if ... else if\" constructs shall be terminated with an \"else\" clause").setSeverity(Severity.MAJOR);
    repository.createRule("S1109").setName("A close curly brace should be located at the beginning of a line").setSeverity(Severity.MINOR);
    repository.createRule("S121").setName("Control structures should always use curly braces").setSeverity(Severity.MAJOR);
    repository.createRule("S108").setName("Nested blocks of code should not be left empty").setSeverity(Severity.MAJOR);
    repository.createRule("S1186").setName("Methods should not be empty").setSeverity(Severity.MAJOR);
    repository.createRule("S1481").setName("Unused local variables should be removed").setSeverity(Severity.MAJOR);

    NewRule commentRegex = repository.createRule("S124").setName("Comment regular expression rule").setSeverity(Severity.MAJOR).setTemplate(true);
    commentRegex.createParam("regularExpression").setDescription("The regular expression")
      .setType(RuleParamType.STRING).setDefaultValue("");
    commentRegex.createParam("message").setDescription("The issue message")
      .setType(RuleParamType.STRING).setDefaultValue("");

    NewRule magicNumber = repository.createRule("MagicNumber").setName("Magic number should not be used").setSeverity(Severity.MINOR);
    magicNumber.createParam("exceptions").setDescription("Comma separated list of allowed values (excluding '-' and '+' signs)")
      .setType(RuleParamType.STRING).setDefaultValue("0,1,0x0,0x00,.0,.1,0.0,1.0");

    NewRule className = repository.createRule("S101").setName("Class name should comply with a naming convention").setSeverity(Severity.MAJOR);
    className.createParam("format").setDescription("Regular expression used to check the class names against")
      .setType(RuleParamType.STRING).setDefaultValue("^(?:[A-HJ-Z][a-zA-Z0-9]+|I[a-z0-9][a-zA-Z0-9]*)$");

    NewRule methodName = repository.createRule("S100").setName("Method name should comply with a naming convention").setSeverity(Severity.MAJOR);
    methodName.createParam("format").setDescription("Regular expression used to check the method names against")
      .setType(RuleParamType.STRING).setDefaultValue("^[A-Z][a-zA-Z0-9]+$");

    NewRule fileLines = repository.createRule("FileLoc").setName("File should not have too many lines").setSeverity(Severity.MAJOR);
    fileLines.createParam("maximumFileLocThreshold").setDescription("The maximum number of lines allowed in a file")
      .setType(RuleParamType.INTEGER).setDefaultValue("1000");

    NewRule methodComplexity = repository.createRule("FunctionComplexity").setName("Method complexity should not be too high").setSeverity(Severity.MAJOR);
    methodComplexity.createParam("maximumFunctionComplexityThreshold").setDescription("The maximum authorized complexity in function")
      .setType(RuleParamType.INTEGER).setDefaultValue("10");

    NewRule lineLength = repository.createRule("LineLength").setName("Lines should not be too long").setSeverity(Severity.MINOR);
    lineLength.createParam("maximumLineLength").setDescription("The maximum authorized line length")
      .setType(RuleParamType.INTEGER).setDefaultValue("200");

    NewRule switchCases = repository.createRule("S1479").setName("\"switch\" statements should not have too many \"case\" clauses").setSeverity(Severity.MAJOR);
    switchCases.createParam("maximum").setDescription("Maximum number of case")
      .setType(RuleParamType.INTEGER).setDefaultValue("30");

    NewRule expressionComplexity = repository.createRule("S1067").setName("Expressions should not be too complex").setSeverity(Severity.MAJOR);
    expressionComplexity.createParam("max").setDescription("Maximum number of allowed conditional operators in an expression")
      .setType(RuleParamType.INTEGER).setDefaultValue("3");

    NewRule functionParameters = repository.createRule("S107").setName("Functions should not have too many parameters").setSeverity(Severity.MAJOR);
    functionParameters.createParam("max").setDescription("Maximum authorized number of parameters")
      .setType(RuleParamType.INTEGER).setDefaultValue("7");

    ExternalDescriptionLoader.loadHtmlDescriptions(repository, "/org/sonar/l10n/csharp/rules/csharpsquid");
    SqaleXmlLoader.load(repository, "/com/sonar/sqale/csharp-model.xml");
    repository.done();
  }

}
