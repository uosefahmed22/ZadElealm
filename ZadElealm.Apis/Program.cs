using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using ZadElealm.Apis.Extentions;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSerilog((services, configuration) => configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console());

        builder.Services.ConfigureApplicationServices(builder.Configuration);
        builder.Services.AddSwaggerService();

        var app = builder.Build();

        await app.ConfigureMiddlewareAsync();
        app.UseSwaggerConfiguration(app.Configuration);
        app.MapScalarApiReference("/scalar", options => options
            .WithTitle("Zad Elealm API")
            .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
            .DisableAgent());

        await app.RunAsync();
    }
}
