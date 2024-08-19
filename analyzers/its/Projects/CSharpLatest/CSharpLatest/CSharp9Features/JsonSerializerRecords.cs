using System;
using System.Text.Json;

namespace CSharpLatest.CSharp9Features;

internal class JsonSerializerRecords
{
    void Foo()
    {
        Forecast forecast = new(DateTime.Now, 40)
        {
            Summary = "Hot!"
        };

        string forecastJson = JsonSerializer.Serialize<Forecast>(forecast);
        Console.WriteLine(forecastJson);
        Forecast? forecastObj = JsonSerializer.Deserialize<Forecast>(forecastJson);
        Console.Write(forecastObj);
    }

    public record Forecast(DateTime Date, int TemperatureC)
    {
        public string? Summary { get; init; }
    };
}
