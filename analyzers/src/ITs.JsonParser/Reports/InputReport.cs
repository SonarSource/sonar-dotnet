using System.IO;
using System.Text.Json;
using ITs.JsonParser.Json;

namespace ITs.JsonParser.Reports;

/// <summary>
/// SARIF report generated during the build process.
/// </summary>
internal class InputReport
{
    public string Project { get; init; }
    public string Assembly { get; init; }
    public string TFM { get; init; }
    public SarifReport Sarif { get; init; }

    public InputReport(string path, JsonSerializerOptions options)
    {
        Console.WriteLine($"Processing {path}...");
        // .../project/assembly{-TFM}?.json
        var filename = Path.GetFileNameWithoutExtension(path);
        var splitted = filename.Split('-');

        Project = Path.GetFileName(Path.GetDirectoryName(path));
        Assembly = splitted[0];
        TFM = splitted.Length > 1 ? splitted[^1] : null; // some projects have only one TFM, so it is not included in the name.
        Sarif = JsonSerializer.Deserialize<SarifReport>(File.ReadAllText(path), options);
        ConsoleHelper.WriteLineColor($"Successfully parsed {this}", ConsoleColor.Green);
    }

    public override string ToString() => $"{Project}/{Assembly} [{TFM}]";
}
