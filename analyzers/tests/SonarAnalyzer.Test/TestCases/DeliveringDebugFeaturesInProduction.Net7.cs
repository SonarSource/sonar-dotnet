using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// https://github.com/SonarSource/sonar-dotnet/issues/6772
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

ConfigureNonProduction(app);
app.Run();

static void ConfigureNonProduction(WebApplication app)
{
    // https://sonarsource.atlassian.net/browse/NET-4818
    if (!app.Environment.IsProduction())
    {
        app.UseDeveloperExceptionPage(); // Compliant
    }
}
