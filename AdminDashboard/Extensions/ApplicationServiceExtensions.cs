using AdminDashboard.Controllers;
using AdminDashboard.Helpers;
using AdminDashboard.Middlwares;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.Text;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Helpers;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Options;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Repository.Data.Datbases;
using ZadElealm.Repository.Repositories;
using ZadElealm.Service.AppServices;
using ZadElealm.Service.IdentityService;
using ZadElealm.Service.Documents;
using Microsoft.Extensions.Options;

namespace AdminDashboard.Extentions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            ConfigureAuthentication(services, configuration);
            ConfigureDatabase(services, configuration);
            ConfigureCors(services, configuration);
            ConfigureDependencyInjection(services, configuration);
            ConfigureValidationErrorHandling(services);
            services.AddMemoryCache();
            services.AddHttpClient();
            services.Configure<RateLimitOptions>(configuration.GetSection("RateLimit"));
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
                options.Cookie.Name = "XSRF-TOKEN";
            });
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
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.LoginPath = "/Admin/Login";
                options.AccessDeniedPath = "/Admin/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.AddControllersWithViews().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
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
            services.AddCors(options =>
            {
                options.AddPolicy("Restricted", builder =>
                {
                    builder.WithOrigins(configuration["FrontbaseUrl"])
                           .WithMethods("GET", "POST")
                           .WithHeaders("Content-Type", "Authorization");
                });
            });
        }

        private static void ConfigureDependencyInjection(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            //services.AddScoped<ITokenService, TokenService>();
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
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IVideoProgressService, VideoProgressService>();
            services.AddScoped<IVideoProgressReadRepository, VideoProgressReadRepository>();

            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.Configure<AdminSettings>(configuration.GetSection("AdminSettings"));
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
