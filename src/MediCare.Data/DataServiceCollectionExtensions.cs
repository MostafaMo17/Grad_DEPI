using System;
using MediCare.Services.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MediCare.Data.Seeding;

namespace MediCare.Data
{
    // Registers the DbContext (SQL Server) and ASP.NET Core Identity stores , the API does not need to know EF Core details.
    public static class DataServiceCollectionExtensions
    {
       
      
        public static IServiceCollection AddDataLayer(this IServiceCollection services, string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            // one instance per request,(for atomic registration) 

            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

            // AddIdentityCore (not AddIdentity): the API uses tokens, so no cookie authentication is needed.
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                // Login Protection
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
                .AddRoles<IdentityRole<int>>()
                .AddEntityFrameworkStores<AppDbContext>();

            services.AddScoped<IdentitySeeder>();
            services.AddScoped<SpecializationSeeder>();

            return services;
        }
    }
}
