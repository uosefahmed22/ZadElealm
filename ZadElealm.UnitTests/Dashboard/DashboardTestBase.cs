using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.UnitTests.Dashboard
{
    public class DashboardTestBase : IDisposable
    {
        protected readonly AppDbContext DbContext;

        protected DashboardTestBase()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            DbContext = new AppDbContext(options);
        }

        protected static UserManager<AppUser> CreateUserManagerMock(AppUser? primaryAdmin = null, bool emailConfirmed = true)
        {
            var userManager = new Mock<UserManager<AppUser>>(
                new Mock<IUserStore<AppUser>>().Object,
                null, null, null, null, null, null, null, null);

            if (primaryAdmin != null)
            {
                var confirmed = emailConfirmed ? primaryAdmin : Clone(primaryAdmin, false);
                userManager.Setup(u => u.FindByEmailAsync(primaryAdmin.Email!)).ReturnsAsync(confirmed);
            }
            else
            {
                userManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((AppUser)null!);
            }

            return userManager.Object;
        }

        private static AppUser Clone(AppUser source, bool emailConfirmed)
            => new AppUser { Id = source.Id, Email = source.Email, EmailConfirmed = false };

        protected static IConfiguration BuildConfiguration(int maxAdminCount = 10)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AdminSettings:PrimaryAdminEmail"] = "primary@test.com",
                    ["AdminSettings:MaxAdminCount"] = maxAdminCount.ToString()
                })
                .Build();
        }

        public void Dispose() => DbContext.Dispose();
    }
}
