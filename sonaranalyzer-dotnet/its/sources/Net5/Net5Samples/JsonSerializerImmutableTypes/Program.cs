using System;
using System.Text.Json;
using System.Text.Json.Serialization;

var json = "{\"date\":\"2020-09-06T11:31:01.923395-07:00\",\"temperatureC\":-1,\"temperatureF\":31,\"summary\":\"Scorching\"} ";           
var options = new JsonSerializerOptions()
{
    PropertyNameCaseInsensitive = true,
    IncludeFields = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};
var forecast = JsonSerializer.Deserialize<Forecast>(json, options);

Console.WriteLine(forecast.Date);
Console.WriteLine(forecast.TemperatureC);
Console.WriteLine(forecast.TemperatureF);
Console.WriteLine(forecast.Summary);

var roundTrippedJson = JsonSerializer.Serialize<Forecast>(forecast, options);

Console.WriteLine(roundTrippedJson);

public struct Forecast{
    public DateTime Date {get;}
    public int TemperatureC {get;}
    public int TemperatureF {get;}
    public string Summary {get;}
    [JsonConstructor]
    public Forecast(DateTime date, int temperatureC, int temperatureF, string summary) => (Date, TemperatureC, TemperatureF, Summary) = (date, temperatureC, temperatureF, summary);
}