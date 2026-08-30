using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Models;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Service;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.IntegrationTests
{
    public class ZadElealmApiFactory : WebApplicationFactory<Program>
    {
        private const string TestJwtKey = "integration-test-key-0123456789abcdef0123456789abcdef";
        private const string TestUserEmail = "user@test.com";
        public const string TestUserPassword = "IntegrationTest123!";
        private readonly SqliteConnection _connection;

        public ZadElealmApiFactory()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Jwt:Key", TestJwtKey);
            builder.UseSetting("Jwt:Issuer", "ZadElealm");
            builder.UseSetting("Jwt:Audience", "ZadElealm");
            builder.UseSetting("CloudinarySetting:CloudName", "integration-test-cloud");
            builder.UseSetting("CloudinarySetting:ApiKey", "integration-test-key");
            builder.UseSetting("CloudinarySetting:ApiSecret", "integration-test-secret");
            builder.UseSetting("CorsSettings:AllowedOrigins:0", "https://zad-elealm.netlify.app");
            builder.UseSetting("CorsSettings:AllowedOrigins:1", "http://localhost:4200");
            builder.UseSetting("Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Command", "Warning");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<AppDbContext>();
                services.RemoveAll<ISendEmailService>();
                services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
                services.AddSingleton<ISendEmailService, SuccessfulEmailService>();
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            using var scope = host.Services.CreateScope();
            SeedTestDataAsync(scope.ServiceProvider).GetAwaiter().GetResult();

            return host;
        }

        public string GenerateToken(params string[] roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, TestUserEmail),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey)),
                    SecurityAlgorithms.HmacSha256Signature),
                Expires = DateTime.UtcNow.AddMinutes(30),
                Issuer = "ZadElealm",
                Audience = "ZadElealm"
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(handler.CreateToken(tokenDescriptor));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _connection.Dispose();
            }
        }

        private static async Task SeedTestIdentityAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            if (!await roleManager.RoleExistsAsync("User"))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole("User"));
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException("Failed to create the integration-test User role.");
                }
            }

            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var user = await userManager.FindByEmailAsync(TestUserEmail);
            if (user != null)
            {
                return;
            }

            user = new AppUser
            {
                DisplayName = "Integration Test User",
                Email = TestUserEmail,
                UserName = TestUserEmail,
                EmailConfirmed = true
            };

            var userResult = await userManager.CreateAsync(user, TestUserPassword);
            if (!userResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to create the integration-test user.");
            }

            var addToRoleResult = await userManager.AddToRoleAsync(user, "User");
            if (!addToRoleResult.Succeeded)
            {
                throw new InvalidOperationException("Failed to assign the integration-test User role.");
            }
        }

        private static async Task SeedTestDataAsync(IServiceProvider services)
        {
            await SeedTestIdentityAsync(services);
            await SeedTestCatalogAsync(services);
        }

        private static async Task SeedTestCatalogAsync(IServiceProvider services)
        {
            var dbContext = services.GetRequiredService<AppDbContext>();
            if (await dbContext.Courses.AnyAsync())
            {
                return;
            }

            var quran = new Category
            {
                Name = "القرآن الكريم",
                Description = "دورات القرآن الكريم",
                ImageUrl = "https://example.test/quran-category.jpg",
                Courses = []
            };
            var seerah = new Category
            {
                Name = "السيرة النبوية",
                Description = "دورات السيرة النبوية",
                ImageUrl = "https://example.test/seerah-category.jpg",
                Courses = []
            };

            dbContext.Courses.AddRange(
                new Course
                {
                    Name = "أساسيات التجويد",
                    Description = "مدخل عملي إلى أحكام التجويد",
                    Author = "أحمد محمود",
                    CourseLanguage = "العربية",
                    CourseVideosCount = 12,
                    ImageUrl = "https://example.test/tajweed.jpg",
                    rating = 4.8m,
                    CreatedAt = new DateTime(2026, 1, 10),
                    Category = quran
                },
                new Course
                {
                    Name = "مواقف من السيرة",
                    Description = "دراسة مواقف مختارة من السيرة النبوية",
                    Author = "محمد علي",
                    CourseLanguage = "العربية",
                    CourseVideosCount = 8,
                    ImageUrl = "https://example.test/seerah.jpg",
                    rating = 4.5m,
                    CreatedAt = new DateTime(2026, 2, 15),
                    Category = seerah
                });

            await dbContext.SaveChangesAsync();
        }

        private sealed class SuccessfulEmailService : ISendEmailService
        {
            public Task<ApiDataResponse> SendEmailAsync(
                EmailMessage emailMessage,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new ApiDataResponse(200));
            }
        }
    }
}
