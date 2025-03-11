using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Principal;
using System.Text;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Helpers;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Repository.Data.Datbases;
using ZadElealm.Repository.Repositories;
using ZadElealm.Service.AppServices;
using ZadElealm.Service.IdentityService;

namespace ZadElealm.Apis.Extentions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            ConfigureAuthentication(services, configuration);
            ConfigureDatabase(services, configuration);
            ConfigureCors(services);
            ConfigureDependencyInjection(services, configuration);
            ConfigureValidationErrorHandling(services);
            services.AddAutoMapper(typeof(MappingProfiles));
            services.AddMemoryCache();

            var cloudName = configuration["CloudinarySetting:CloudName"];
            var apiKey = configuration["CloudinarySetting:ApiKey"];
            var apiSecret = configuration["CloudinarySetting:ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            var cloudinary = new Cloudinary(account);
            services.AddSingleton(cloudinary);

            return services;
        }
        private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
            }).AddEntityFrameworkStores<AppDbContext>()
                .AddSignInManager<SignInManager<AppUser>>()
                .AddDefaultTokenProviders();

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
            });

            services.AddSingleton(tokenValidationParameters);
        }
        private static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });
        }
        private static void ConfigureCors(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("Open", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }
        private static void ConfigureDependencyInjection(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddSingleton<ISendEmailService, SendEmailService>();
            services.AddScoped<ICertificateService, CertificateService>();
            services.AddScoped<IQuizService, QuizService>();
            services.AddScoped<INotificationService, NotificationService>();

            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(ApplicationServiceExtensions).Assembly));
        }
        private static void ConfigureValidationErrorHandling(IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage).ToArray();

                    var errorResponse = new ApiValidationErrorResponse
                    {
                        Errors = errors
                    };

                    return new BadRequestObjectResult(errorResponse);
                };
            });
        }
    }
}
