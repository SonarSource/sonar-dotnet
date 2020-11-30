using System;
using System.Net.Http;
using System.Net.Http.Json;

string serviceURL = "https://localhost:5001/WeatherForecast";
HttpClient client = new();
Forecast[] forecasts = await client.GetFromJsonAsync<Forecast[]>(serviceURL);

foreach(Forecast forecast in forecasts)
{
    Console.WriteLine($"{forecast.Date}; {forecast.TemperatureC}C; {forecast.Summary}");
}

// {"date":"2020-09-06T11:31:01.923395-07:00","temperatureC":-1,"temperatureF":31,"summary":"Scorching"}            
public record Forecast(DateTime Date, int TemperatureC, int TemperatureF, string Summary);
