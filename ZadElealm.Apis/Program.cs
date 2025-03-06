using ZadElealm.Apis.Extentions;
using ZadElealm.Apis.Middlwares;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.ConfigureApplicationServices(builder.Configuration);
        builder.Services.AddSwaggerService();

        var app = builder.Build();
        app.UseMiddleware<RateLimitingMiddleware>();
        app.UseMiddleware<SwaggerBasicAuthMiddleware>();
        await app.ConfigureMiddlewareAsync();
        app.UseSwaggerConfiguration(app.Configuration);
        app.Run();
    }
}