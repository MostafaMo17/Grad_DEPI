using MediCare.Web.Filters;
using MediCare.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews(options =>
{
    // Every POST form must carry an anti-forgery token.
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

    // Turns 401 / 403 answers from the API into the appropriate web response.
    options.Filters.Add<ApiExceptionFilter>();
});

// The browser gets only a cookie that holds a random key. The real session,
// including the API JWT, stays in this server's memory.
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ITicketStore, MemoryCacheTicketStore>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "MediCare.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = false;
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
    .Configure<ITicketStore>((options, ticketStore) =>
    {
        options.SessionStore = ticketStore;
    });

string apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("Api:BaseUrl is not configured.");

builder.Services.AddTransient<BearerTokenHandler>();
builder.Services.AddHttpClient<MediCareApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<BearerTokenHandler>();

// Both the MVC authentication/notifications code and the doctor schedule
// controller use the same typed API client instance.
builder.Services.AddTransient<IMediCareApiClient>(serviceProvider =>
    serviceProvider.GetRequiredService<MediCareApiClient>());

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
