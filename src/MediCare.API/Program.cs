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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);



#region //  send enums as text  
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
#endregion

#region //  Swagger 

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MediCare API", Version = "v1" });

    // The token is sent in the header: Authorization: Bearer <token>
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Log in, then paste the token here (without the word Bearer)."
    });

    // Apply the Bearer scheme to all endpoints 
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

#endregion

#region // Database 

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured. Use User Secrets or environment variables.");

builder.Services.AddDataLayer(connectionString);
#endregion

// Our services 

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();

#region // JWT settings 
// Jwt:SigningKey is secret (User Secrets locally, environment variable Jwt__SigningKey in hosting)


string jwtKey = builder.Configuration["Jwt:SigningKey"] ?? string.Empty;

// Stop if the key is missing or too weak
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
        OnTokenValidated = JwtTokenEvents.OnTokenValidated,
        OnChallenge = JwtTokenEvents.OnChallenge,
        OnForbidden = JwtTokenEvents.OnForbidden
    };
});

#endregion

#region // Authorization

builder.Services.AddAuthorization(options =>
{
    // [Authorize(Policy = "ApprovedDoctor")]    
    options.AddPolicy("ApprovedDoctor", policy =>
    {
        policy.RequireRole(RoleNames.Doctor);
        policy.RequireClaim(AppClaimTypes.DoctorApprovalStatus, DoctorApprovalStatus.Approved.ToString());
    });
});
#endregion

#region //  Rate limiting 
// login policy: at most 10 requests per minute from the same IP address. More requests get HTTP 429

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Return a JSON error body instead of an empty response
    options.OnRejected = async (context, cancellationToken) =>
    {
        ProblemDetails problem = new ProblemDetails();
        problem.Status = 429;
        problem.Title = "Too many requests";
        problem.Detail = "Too many requests. Please wait a minute and try again.";
        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
    };

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
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MediCare API v1");
        options.EnablePersistAuthorization();   // keep the token after refreshing the page
    });
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();