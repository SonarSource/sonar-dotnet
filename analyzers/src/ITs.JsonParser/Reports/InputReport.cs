using ITs.JsonParser.Json;

namespace ITs.JsonParser.Reports;

/// <summary>
/// SARIF report generated during the build process.
/// </summary>
internal record InputReport(string Project, string Assembly, string TFM, SarifIssues Sarif);
