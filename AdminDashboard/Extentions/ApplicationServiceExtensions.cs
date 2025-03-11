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
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
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
            //services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddSingleton<ISendEmailService, SendEmailService>();
            services.AddScoped<ICertificateService, CertificateService>();
            services.AddScoped<IQuizService, QuizService>();

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
