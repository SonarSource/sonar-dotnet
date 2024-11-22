/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
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

using CommandLine;

namespace ITs.JsonParser;

[Verb("parse", isDefault: true, HelpText = "Parses the JSONs from 'output/' to 'actual/' and generates a diff report")]
public class CommandLineOptions
{
    [Option('p', "project", Required = false, HelpText = "The name of single project to parse. If ommited, all projects will be parsed")]
    public string Project { get; set; }

    [Option('r', "rule", Required = false, HelpText = "The key of the rule to parse, e.g. S1234. If omitted, all rules will be parsed")]
    public string RuleId { get; set; }
}
