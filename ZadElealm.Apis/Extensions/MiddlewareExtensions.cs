using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZadElealm.Apis.Middlwares;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Repository.Data.Datbases;
using ZadElealm.Repository.Data.SeedData;
using Serilog;

public static class MiddlewareExtensions
{
    public static async Task ConfigureMiddlewareAsync(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseExceptionHandler();
        app.UseMiddleware<RateLimitingMiddleware>();
       //app.UseMiddleware<CsrfJwtMiddleware>();
        //app.UseMiddleware<SwaggerBasicAuthMiddleware>();
        app.UseStatusCodePages();
        app.UseCors("AllowSpecificOrigin");
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/certificates"))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            await next();
        });
        app.UseStaticFiles();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        #region Update the database automatically
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var appDbContext = services.GetRequiredService<AppDbContext>();
            if (app.Environment.IsEnvironment("Testing"))
            {
                await appDbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
                logger.LogInformation("Integration-test database created successfully.");
            }
            else
            {
                await appDbContext.Database.MigrateAsync().ConfigureAwait(false);
                await AppDbContextSeed.SeedAsync(appDbContext, logger).ConfigureAwait(false);
                logger.LogInformation("AppDbContext migrated and seeded successfully.");
            }

        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while applying migrations: {ex.Message}");
        }
        #endregion
        app.MapControllers();
    }
}
