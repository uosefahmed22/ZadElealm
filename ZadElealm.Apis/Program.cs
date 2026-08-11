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
       
        var app = builder.Build();
        
        await app.ConfigureMiddlewareAsync();
        app.UseSwaggerConfiguration(app.Configuration);
        
        app.Run();
    }
}
