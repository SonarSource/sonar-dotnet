using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// https://github.com/SonarSource/sonar-dotnet/issues/6772
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.Run();
