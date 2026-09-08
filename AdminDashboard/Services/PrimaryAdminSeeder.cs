using System.ComponentModel.DataAnnotations;
using AdminDashboard.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Services;

public sealed class PrimaryAdminSeeder
{
    private const string AdminRole = "Admin";

    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AdminSettings _adminSettings;
    private readonly AdminBootstrapOptions _bootstrapOptions;
    private readonly ILogger<PrimaryAdminSeeder> _logger;

    public PrimaryAdminSeeder(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<AdminSettings> adminSettings,
        IOptions<AdminBootstrapOptions> bootstrapOptions,
        ILogger<PrimaryAdminSeeder> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _adminSettings = adminSettings.Value;
        _bootstrapOptions = bootstrapOptions.Value;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (!_bootstrapOptions.Enabled)
            return;

        ValidateConfiguration();

        var email = _adminSettings.PrimaryAdminEmail.Trim();
        await EnsureAdminRoleExistsAsync();

        var admin = await _userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new AppUser
            {
                DisplayName = _bootstrapOptions.DisplayName.Trim(),
                Email = email,
                UserName = email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(admin, _bootstrapOptions.Password);
            if (!createResult.Succeeded)
            {
                // A second instance may have created the same account concurrently.
                admin = await _userManager.FindByEmailAsync(email);
                if (admin is null)
                    throw BuildIdentityException("create the primary admin", createResult);
            }
        }

        if (!await _userManager.IsInRoleAsync(admin, AdminRole))
        {
            var roleResult = await _userManager.AddToRoleAsync(admin, AdminRole);
            if (!roleResult.Succeeded)
                throw BuildIdentityException("assign the Admin role", roleResult);
        }

        _logger.LogInformation("Primary admin bootstrap completed successfully.");
    }

    private void ValidateConfiguration()
    {
        var email = _adminSettings.PrimaryAdminEmail?.Trim();
        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
            throw new InvalidOperationException(
                "AdminSettings:PrimaryAdminEmail must be a valid email when admin bootstrap is enabled.");

        if (string.IsNullOrWhiteSpace(_bootstrapOptions.DisplayName))
            throw new InvalidOperationException(
                "AdminBootstrap:DisplayName is required when admin bootstrap is enabled.");

        if (string.IsNullOrWhiteSpace(_bootstrapOptions.Password))
            throw new InvalidOperationException(
                "AdminBootstrap:Password is required when admin bootstrap is enabled.");
    }

    private async Task EnsureAdminRoleExistsAsync()
    {
        if (await _roleManager.RoleExistsAsync(AdminRole))
            return;

        var result = await _roleManager.CreateAsync(new IdentityRole(AdminRole));
        if (!result.Succeeded && !await _roleManager.RoleExistsAsync(AdminRole))
            throw BuildIdentityException("create the Admin role", result);
    }

    private static InvalidOperationException BuildIdentityException(
        string operation,
        IdentityResult result)
    {
        var errors = string.Join(", ", result.Errors.Select(error => error.Description));
        return new InvalidOperationException($"Failed to {operation}: {errors}");
    }
}
