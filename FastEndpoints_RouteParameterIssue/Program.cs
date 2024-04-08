using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var host = builder.Host;
var services = builder.Services;

// Enum Json Serialisierung
services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// Problem+Json Konfiguration
services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = (context) =>
    {

        if (context.ProblemDetails.Status == 500)
        {
            context.ProblemDetails.Status = 400;
            context.ProblemDetails.Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1";

        }
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["serverTimeUTC"] = DateTimeOffset.UtcNow;
        context.ProblemDetails.Extensions["apiVersion"] = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
    };
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen((options) =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "FastEndpoints_RouteParameterIssue", Version = "v1" });

            var xmlFileApi = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPathApi = Path.Combine(AppContext.BaseDirectory, xmlFileApi);
            options.IncludeXmlComments(xmlPathApi);
        });

services.SwaggerDocument(o =>
{
o.AutoTagPathSegmentIndex = 0;
o.UsePropertyNamingPolicy = false;
}
);

// FastEndpoints
services.AddFastEndpoints()
.SwaggerDocument();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
});

app.UseFastEndpoints(config =>
{
    config.Errors.UseProblemDetails();
}).UseSwaggerGen();

// Redirect root to Swagger
app.Map("/", context => Task.Run(() =>
    context.Response.Redirect("/swagger/index.html")));

app.UseHttpsRedirection();



app.Run();
