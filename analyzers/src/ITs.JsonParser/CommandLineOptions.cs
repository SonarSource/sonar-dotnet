using CommandLine;

namespace ITs.JsonParser;

[Verb("parse", isDefault: true, HelpText = "Parses the JSONs from 'output/' to 'actual/' and generates a diff report")]
internal class CommandLineOptions
{
    [Option('p', "project", Required = false, HelpText = "The name of single project to parse. If ommited, all projects will be parsed")]
    public string Project { get; set; }

    [Option('r', "rule", Required = false, HelpText = "The key of the rule to parse, e.g. S1234. If omitted, all rules will be parsed")]
    public string RuleId { get; set; }
}
