using AdminDashboard.Middlwares;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

namespace AdminDashboard.Extentions
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
            services.AddHttpClient();
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
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });
        }
        private static void ConfigureCors(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("Restricted", builder =>
                {
                    builder.WithOrigins() 
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
