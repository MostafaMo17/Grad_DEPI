using MediCare.Data.Repositories;
using MediCare.Data.Seeding;
using MediCare.Services.Entities;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MediCare.Data
{
    // Registers the DbContext (SQL Server) and ASP.NET Core Identity stores , the API does not need to know EF Core details.
    public static class DataServiceCollectionExtensions
    {

        // Registers the database, Identity, the repositories and the startup seeders
        //Called once from Program
        public static IServiceCollection AddDataLayer(this IServiceCollection services, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("The connection string is empty.", nameof(connectionString));
            }
            // one instance per request,(for atomic registration) 

            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

            // AddIdentityCore (not AddIdentity): the API uses tokens, so cookies are not needed
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                // Login Protection , Password rules stay at Identity defaults
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
                .AddRoles<IdentityRole<int>>()
                .AddEntityFrameworkStores<AppDbContext>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDoctorRepository, DoctorRepository>();
            services.AddScoped<IDoctorScheduleRepository, DoctorScheduleRepository>();
            services.AddScoped<ISpecializationRepository, SpecializationRepository>();

            services.AddScoped<IdentitySeeder>();
            services.AddScoped<SpecializationSeeder>();
            return services;
        }
    }
}
