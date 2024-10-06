using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Auth.Infrastructure.DateBase;
using Auth.Infrastructure.Openiddict;
using PricePoint.API.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddEntityFrameworkNpgsql()
    .AddDbContextPool<ApplicationDbContext>(config =>
    {
        config
            .UseNpgsql("Host=localhost;Database=authDB;Persist Security Info=True;Port=5432;Username=postgres;Password=admin;Enlist=false");
        //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    });


builder.Services.AddTransient<ApplicationUserStore>(); 
builder.Services.AddUnitOfWork<ApplicationDbContext, ApplicationUser, ApplicationRole>();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Настройки пароля
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;

    // Настройки блокировки
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 10;

    // Настройка двухфакторной аутентификации
    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
    options.User.RequireUniqueEmail = true;
});


builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
       options.UseEntityFrameworkCore()
              .UseDbContext<ApplicationDbContext>(); 
    })
    .AddServer(options =>
    {
        options.SetAuthorizationEndpointUris("/auth/connect/authorize")
               .SetTokenEndpointUris("/auth/connect/token");
              //.SetUserinfoEndpointUris("/connect/userinfo")
              //.SetIntrospectionEndpointUris("/connect/introspect")
              //.SetLogoutEndpointUris("/connect/logout");

        // Разрешение grant types (authorization code и refresh token)
       options.AllowAuthorizationCodeFlow()
              .AllowRefreshTokenFlow()
              .AllowClientCredentialsFlow();

        // Включение токенов для двухфакторной аутентификации
       options.RegisterScopes("openid", "profile", "email", "offline_access", "api");

        // Настройка сертификатов
       options.AddDevelopmentEncryptionCertificate()
              .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough()
              .EnableAuthorizationEndpointPassthrough();
        //.EnableLogoutEndpointPassthrough()
        //.EnableUserinfoEndpointPassthrough();

    })
    .AddValidation(options =>
    {
       options.UseLocalServer();
       options.UseAspNetCore();
    });
builder.Services.AddDistributedMemoryCache(options =>
{
    // Установите лимит для кэша, например, 100 MB
    options.SizeLimit = null;
});
//builder.Services.AddAutoMapper(typeof(Startup));

//builder.Services.AddUnitOfWork<ApplicationDbContext, ApplicationUser, ApplicationRole>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers(); 
using (var scope = app.Services.CreateScope())
{
    await DatabaseInitializer.Seed(scope.ServiceProvider);
}
// Инициализация скоупов OpenIddict
await OpenIdDictScopeConfig.SeedScopes(app.Services);

// Инициализация клиентов OpenIddict
await OpenIdDictClientConfig.SeedClients(app.Services);

app.Run();
