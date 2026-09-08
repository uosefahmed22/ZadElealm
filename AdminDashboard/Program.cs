using AdminDashboard.Extentions;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using AdminDashboard.Helpers;
using AdminDashboard.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ZadElealm.Repository.Data.Datbases;

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

        builder.Services.AddControllersWithViews(options =>
        {
            options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.ConfigureApplicationServices(builder.Configuration);
        builder.Services.AddRateLimiting(builder.Configuration);

        var app = builder.Build();

        if (app.Environment.IsProduction())
        {
            await using var scope = app.Services.CreateAsyncScope();
            var bootstrapOptions = scope.ServiceProvider
                .GetRequiredService<IOptions<AdminBootstrapOptions>>()
                .Value;

            if (bootstrapOptions.Enabled)
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await dbContext.Database.MigrateAsync();
                await scope.ServiceProvider.GetRequiredService<PrimaryAdminSeeder>().SeedAsync();
            }
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseCookiePolicy(new CookiePolicyOptions
        {
            Secure = CookieSecurePolicy.Always,
            HttpOnly = HttpOnlyPolicy.Always,
            MinimumSameSitePolicy = SameSiteMode.Strict
        });
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseSerilogRequestLogging();
        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.MapScalarApiReference("/scalar", options => options
                .WithTitle("Zad Elealm Dashboard")
                .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
                .DisableAgent());
        }

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Admin}/{action=Login}/{id?}");

        await app.RunAsync();
    }
}
