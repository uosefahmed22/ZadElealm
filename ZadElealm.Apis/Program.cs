using Microsoft.AspNetCore.Http.Json;
using QuestPDF.Infrastructure;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZadElealm.Apis.Extentions;
using ZadElealm.Apis.Middlwares;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.ConfigureApplicationServices(builder.Configuration);
        builder.Services.AddSwaggerService();
        builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });
        QuestPDF.Settings.License = LicenseType.Community;
        var app = builder.Build();
        app.UseMiddleware<RateLimitingMiddleware>();
        //app.UseMiddleware<SwaggerBasicAuthMiddleware>();
        await app.ConfigureMiddlewareAsync();
        app.UseSwaggerConfiguration(app.Configuration);
        app.Run();
    }
}
