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
package org.sonarsource.dotnet.shared.plugins.protobuf;

import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.sensor.SensorContext;
import org.sonar.api.batch.sensor.issue.NewIssue;
import org.sonar.api.batch.sensor.issue.NewIssueLocation;
import org.sonar.api.rule.RuleKey;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer;
import org.sonarsource.dotnet.protobuf.SonarAnalyzer.FileIssues;

import java.util.function.Predicate;

import static org.sonarsource.dotnet.shared.plugins.SensorContextUtils.toTextRange;

class IssuesImporter extends ProtobufImporter<SonarAnalyzer.FileIssues> {

  private final SensorContext context;
  private final String repositoryKey;

  IssuesImporter(SensorContext context, String repositoryKey, Predicate<InputFile> inputFileFilter) {
    super(SonarAnalyzer.FileIssues.parser(), context, inputFileFilter, SonarAnalyzer.FileIssues::getFilePath);
    this.context = context;
    this.repositoryKey = repositoryKey;
  }

  @Override
  void consumeFor(InputFile inputFile, FileIssues message) {
    for (SonarAnalyzer.FileIssues.Issue issue : message.getIssueList()) {
      NewIssue newIssue = context.newIssue();
      NewIssueLocation location = newIssue
        .newLocation()
        .on(inputFile)
        .message(issue.getMessage());

      SonarAnalyzer.TextRange issueTextRange = issue.getLocation();

      if (issueTextRange.getStartOffset() == issueTextRange.getEndOffset() &&
          issueTextRange.getStartLine() == issueTextRange.getEndLine()) {
        // file level issue
      } else {
        location = location.at(toTextRange(inputFile, issueTextRange));
      }

      newIssue.forRule(RuleKey.of(repositoryKey, issue.getId()))
        .at(location)
        .save();
    }
  }
}
