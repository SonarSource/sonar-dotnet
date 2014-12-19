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
package org.sonar.plugins.csharp.squid;

import com.sonar.csharp.checks.CheckList;
import org.sonar.api.server.rule.RulesDefinition;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.squidbridge.annotations.AnnotationBasedRulesDefinition;
import org.sonar.squidbridge.rules.ExternalDescriptionLoader;
import org.sonar.squidbridge.rules.PropertyFileLoader;
import org.sonar.squidbridge.rules.SqaleXmlLoader;

public class CSharpRuleRepository implements RulesDefinition {

  @Override
  public void define(Context context) {
    NewRepository repository = context
      .createRepository(CSharpSquidConstants.REPOSITORY_KEY, CSharpConstants.LANGUAGE_KEY)
      .setName(CSharpSquidConstants.REPOSITORY_NAME);
    AnnotationBasedRulesDefinition rules = new AnnotationBasedRulesDefinition(repository, CSharpConstants.LANGUAGE_KEY);
    rules.addRuleClasses(false, CheckList.getChecks());
    PropertyFileLoader.loadNames(repository, "/org/sonar/l10n/csharp.properties");
    ExternalDescriptionLoader.loadHtmlDescriptions(repository, "/org/sonar/l10n/csharp/rules/csharpsquid");
    SqaleXmlLoader.load(repository, "/com/sonar/sqale/csharp-model.xml");
    repository.done();
  }

}
