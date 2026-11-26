
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
using MediCare.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

#region Send enums as text

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});

#endregion

#region OpenAPI

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes ??=
            new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] =
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT token."
            };

        document.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(
                    "Bearer", document)] = new List<string>()
            }
        ];

        return Task.CompletedTask;
    });
});

#endregion

#region Database

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured. Use User Secrets or environment variables.");

builder.Services.AddDataLayer(connectionString);

#endregion

#region Services

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IMedicalRecordService, MedicalRecordService>();

#endregion

#region JWT settings

string jwtKey = builder.Configuration["Jwt:SigningKey"]
    ?? string.Empty;

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Jwt:SigningKey is missing or shorter than 32 characters. Set it with User Secrets or an environment variable.");
}

#endregion

#region Authentication

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultForbidScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            ValidateLifetime = true
        };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = JwtTokenEvents.OnTokenValidated
    };
});

#endregion

#region Authorization

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApprovedDoctor", policy =>
    {
        policy.RequireRole(RoleNames.Doctor);

        policy.RequireClaim(
            AppClaimTypes.DoctorApprovalStatus,
            DoctorApprovalStatus.Approved.ToString());
    });
});

#endregion

#region Rate limiting

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",

            partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

#endregion

var app = builder.Build();

#region Seed data

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider
        .GetRequiredService<IdentitySeeder>()
        .SeedAsync();

    await scope.ServiceProvider
        .GetRequiredService<SpecializationSeeder>()
        .SeedAsync();
}

#endregion

app.UseMiddleware<ExceptionHandlingMiddleware>();

#region Swagger

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "MediCare API v1");
    });
}

#endregion

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
