using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System.Collections.Concurrent;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Helpers;
using ZadElealm.Apis.Middlwares;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Options;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Repository.Data.Datbases;
using ZadElealm.Repository.Repositories;
using ZadElealm.Service.AppServices;
using ZadElealm.Service.IdentityService;
using ZadElealm.Service.Documents;

namespace ZadElealm.Apis.Extentions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });
            // Add Swagger/OpenAPI support
            services.AddProblemDetails();
            // Add global exception handling middleware
            services.AddExceptionHandler<GlobalExceptionHandler>();

            QuestPDF.Settings.License = LicenseType.Community;

            ConfigureAuthentication(services, configuration);
            ConfigureDatabase(services, configuration);
            ConfigureCors(services, configuration);
            ConfigureDependencyInjection(services, configuration);
            ConfigureValidationErrorHandling(services);

            return services;
        }

        private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
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
                options.UseSqlServer(SqlServerConnectionString.WithoutMultipleActiveResultSets(
                    configuration.GetConnectionString("DefaultConnection")));
            });
        }

        private static void ConfigureCors(IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
                ?? new[] { "https://zad-elealm.netlify.app" };

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        builder
                            .WithOrigins(allowedOrigins)
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });
        }

        private static void ConfigureDependencyInjection(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IEnrollmentReadRepository, EnrollmentReadRepository>();
            services.AddScoped<IEnrollmentWriteRepository, EnrollmentWriteRepository>();
            services.AddScoped<IRatingReadRepository, RatingReadRepository>();
            services.AddScoped<IUserRankReadRepository, UserRankReadRepository>();
            services.AddScoped<IVideoProgressReadRepository, VideoProgressReadRepository>();
            services.AddScoped<IAchievementRepository, AchievementRepository>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddSingleton<ISendEmailService, SendEmailService>();
            services.AddScoped<ICertificateService, CertificateService>();
            services.AddOptions<CertificateStorageOptions>()
                .Bind(configuration.GetSection(CertificateStorageOptions.SectionName))
                .Validate(options =>
                    !string.IsNullOrWhiteSpace(options.PrivatePath) &&
                    !string.IsNullOrWhiteSpace(options.LegacyPublicPath) &&
                    !string.IsNullOrWhiteSpace(options.LogoPath),
                    "Certificate storage paths are required.")
                .ValidateOnStart();
            services.AddSingleton<ICertificateFileStorage>(serviceProvider =>
                new CertificateFileStorage(
                    serviceProvider.GetRequiredService<IHostEnvironment>().ContentRootPath,
                    serviceProvider.GetRequiredService<IOptions<CertificateStorageOptions>>().Value));
            services.AddScoped<IQuizService, QuizService>();
            services.AddScoped<IAssessmentService, AssessmentService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IVideoProgressService, VideoProgressService>();
            services.AddScoped<ICheckPasswordService, CheckPasswordService>();
            services.AddScoped<IUserRankCalculator, UserRankCalculator>();
            services.AddScoped<IAchievementService, AchievementService>();
            services.AddSingleton(TimeProvider.System);

            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            services.Configure<RateLimitOptions>(configuration.GetSection("RateLimit"));
            services.AddMemoryCache();
            services.AddHybridCache();
            services.AddSingleton<ConcurrentDictionary<string, ClientStatistics>>();
            services.AddHostedService<RateLimitCleanupService>();

            var cloudName = configuration["CloudinarySetting:CloudName"];
            var apiKey = configuration["CloudinarySetting:ApiKey"];
            var apiSecret = configuration["CloudinarySetting:ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            var cloudinary = new Cloudinary(account);
            services.AddSingleton(cloudinary);

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
