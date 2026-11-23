using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using MediCare.API.Middleware;
using MediCare.API.Security;
using MediCare.API.Services;
using MediCare.Data;
using MediCare.Data.Seeding;
using MediCare.Services.Enums;
using MediCare.Services.Implementations;
using MediCare.Services.Interfaces;
using MediCare.Services.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);



#region //  send enums as text  
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
#endregion

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


#region // Database 

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured. Use User Secrets or environment variables.");

builder.Services.AddDataLayer(connectionString);
#endregion

// Our services 

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();

#region // JWT settings 
// Jwt:SigningKey is secret (User Secrets locally, environment variable Jwt__SigningKey in hosting)


string jwtKey = builder.Configuration["Jwt:SigningKey"] ?? string.Empty;

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Jwt:SigningKey is missing or shorter than 32 characters. Set it with User Secrets or an environment variable.");
}

#endregion

#region // Authentication (JWT Bearer )


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    // verify the token that comes with every request
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime = true
    };

    // After the normal checks, we check the user in the database (see JwtTokenEvents)
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = JwtTokenEvents.OnTokenValidated
    };
});

#endregion

#region // Authorization

builder.Services.AddAuthorization(options =>
{
    
    options.AddPolicy("ApprovedDoctor", policy =>
    {
        policy.RequireRole(RoleNames.Doctor);
        policy.RequireClaim(AppClaimTypes.DoctorApprovalStatus, DoctorApprovalStatus.Approved.ToString());
    });
});
#endregion

#region //  Rate limiting 
// login policy: at most 10 requests per minute from the same IP address. More requests get HTTP 429.

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});
#endregion


var app = builder.Build();

#region // Seed data
// roles, the optional initial Admin and the initial specializations.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<IdentitySeeder>().SeedAsync();
    await scope.ServiceProvider.GetRequiredService<SpecializationSeeder>().SeedAsync();
}
#endregion

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();