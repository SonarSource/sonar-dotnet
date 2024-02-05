using ITs.JsonParser.Json;

namespace ITs.JsonParser.Reports;

/// <summary>
/// SARIF report generated during the build process.
/// </summary>
public class InputReport
{
    public string Project { get; }
    public string Assembly { get; }
    public string Tfm { get; }
    public SarifReport Sarif { get; }

    public InputReport(string path, JsonSerializerOptions options)
    {
        Console.WriteLine($"Processing {path}...");
        // .../project/assembly{-TFM}?.json
        var fileName = Path.GetFileNameWithoutExtension(path);
        var index = fileName.LastIndexOf('-');
        (Assembly, Tfm) = index >= 0
            ? (fileName.Substring(0, index), fileName.Substring(index + 1))
            : (fileName, null);

        Project = Path.GetFileName(Path.GetDirectoryName(path));
        Sarif = JsonSerializer.Deserialize<SarifReport>(File.ReadAllText(path), options);
        ConsoleHelper.WriteLineColor($"Successfully parsed {this}", ConsoleColor.Green);
    }

    public override string ToString() =>
        $"{Project}/{Assembly} [{Tfm}]";
}
