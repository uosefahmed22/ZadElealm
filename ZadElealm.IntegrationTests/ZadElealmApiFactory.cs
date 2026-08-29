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
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.IntegrationTests
{
    public class ZadElealmApiFactory : WebApplicationFactory<Program>
    {
        private const string TestJwtKey = "integration-test-key-0123456789abcdef0123456789abcdef";
        private const string TestUserEmail = "user@test.com";
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
            builder.UseSetting("Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Command", "Warning");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<AppDbContext>();
                services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            using var scope = host.Services.CreateScope();
            SeedTestIdentityAsync(scope.ServiceProvider).GetAwaiter().GetResult();

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

            var userResult = await userManager.CreateAsync(user);
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
    }
}
