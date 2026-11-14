using MediCare.Services.Entities;
using MediCare.Services.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediCare.Data.Seeding
{
  
    public class IdentitySeeder
    {
        private readonly AppDbContext _dbContext;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IdentitySeeder> _logger;

        public IdentitySeeder(
            AppDbContext dbContext,
            RoleManager<IdentityRole<int>> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ILogger<IdentitySeeder> logger)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await EnsureDatabaseIsMigratedAsync();
            await SeedRolesAsync();
            await SeedAdminAsync();
        }

        private async Task EnsureDatabaseIsMigratedAsync()
        {
            // Read only check
            var pending = await _dbContext.Database.GetPendingMigrationsAsync();
            if (pending.Any())
            {
                throw new InvalidOperationException(
                    "The database is missing migrations. Run: dotnet ef database update ");
            }
        }

        private async Task SeedRolesAsync()
        {
            foreach (var roleName in RoleNames.All)
            {
                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    continue;
                }

                var result = await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Could not create role '{roleName}'. Identity errors: {DescribeErrors(result)}");
                }

                _logger.LogInformation("Created role {RoleName}.", roleName);
            }
        }

        private async Task SeedAdminAsync()
        {
            var email = _configuration["Seed:Admin:Email"];
            var password = _configuration["Seed:Admin:Password"];
            var fullName = _configuration["Seed:Admin:FullName"];

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fullName))
            {
                _logger.LogWarning(
                    "Admin seed settings are missing (Seed:Admin:Email, Seed:Admin:Password, Seed:Admin:FullName). " +
                    "Skipping Admin provisioning.");
                return;
            }

            var existingAdmins = await _userManager.GetUsersInRoleAsync(RoleNames.Admin);
            if (existingAdmins.Count > 0)
            {
                _logger.LogInformation("An Admin account already exists. Skipping Admin provisioning.");
                return;
            }

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser is not null)
            {
                _logger.LogWarning(
                    "An account with the configured Admin email already exists and is not an Admin. " +
                    "It will NOT be promoted. Skipping Admin provisioning.");
                return;
            }

            // Create the account and assign the role in one transaction
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            // Normalize
            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                IsActive = true,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(admin, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not create the Admin account. Identity errors: {DescribeErrors(createResult)}");
            }

            var roleResult = await _userManager.AddToRoleAsync(admin, RoleNames.Admin);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not assign the Admin role. Identity errors: {DescribeErrors(roleResult)}");
            }

            await transaction.CommitAsync();
            _logger.LogInformation("Initial Admin account created.");
        }

        private static string DescribeErrors(IdentityResult result)
        {
            return string.Join(", ", result.Errors.Select(e => e.Code));
        }
    }
}
