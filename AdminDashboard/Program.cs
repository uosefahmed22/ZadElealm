using AdminDashboard.Extentions;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();
        builder.Services.ConfigureApplicationServices(builder.Configuration);

        var app = builder.Build();
        if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors(builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Admin}/{action=Login}/{id?}");

        app.Run();
    }
}