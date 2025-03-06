using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Gen;
using System.Reflection;
using System.Text;

namespace ZadElealm.Apis.Extentions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddService(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddGen(c =>
            {
                c.Doc("v1", new OpenApiInfo { Title = "ZadElealm API", Version = "v1" });
                AddSecurityDefinition(c);
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            return services;
        }
        private static void AddSecurityDefinition(GenOptions c)
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        }
        public static void UseConfiguration(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.Use();
            app.UseUI(options =>
            {
                options.Endpoint("/swagger/v1/swagger.json", "API V1");
                options.RoutePrefix = "swagger";

                options.ConfigObject.AdditionalItems["persistAuthorization"] = true;
                options.ConfigObject.AdditionalItems["defaultModelsExpandDepth"] = -1;
            });
        }
    }
}
