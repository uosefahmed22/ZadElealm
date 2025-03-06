using Microsoft.EntityFrameworkCore;
using ZadElealm.Repository.Data.Datbases;

public static class MiddlewareExtensions
{
    public static async Task ConfigureMiddlewareAsync(this WebApplication app)
    {
        app.UseStatusCodePagesWithRedirects("/errors/{0}");
        app.UseCors("Open");
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        #region Update the database automatically
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
        }
        #endregion

        app.MapControllers();
    }
}