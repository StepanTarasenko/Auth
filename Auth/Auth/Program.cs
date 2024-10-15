using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Auth.Infrastructure.DateBase;
using Auth.Infrastructure.Openiddict;
using PricePoint.API.UnitOfWork;
using Auth.Controllers;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using OpenIddict.Validation.AspNetCore;

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
    .AddDbContextPool<ApplicationDbContext>(config =>
    {
        config
            .UseNpgsql("Host=localhost;Database=authDB;Persist Security Info=True;Port=5432;Username=postgres;Password=admin;Enlist=false");
        //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    });


builder.Services.AddTransient<ApplicationUserStore>(); 
builder.Services.AddUnitOfWork<ApplicationDbContext, ApplicationUser, ApplicationRole>();
// Register the Identity services.

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
    //.AddClient(options =>
    //{
    //    //options.AddEphemeralEncryptionKey()
    //    //       .AddEphemeralSigningKey();
    //    options.AddDevelopmentEncryptionCertificate()
    //           .AddDevelopmentSigningCertificate();
    //    //options.AddEncryptionKey(new SymmetricSecurityKey(
    //    //    Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));
    //})
    .AddServer(options =>
    {
        options.SetAuthorizationEndpointUris("/auth/connect/authorize")
               .SetTokenEndpointUris("/auth/connect/token");
        //.SetUserinfoEndpointUris("/connect/userinfo")
        //.SetIntrospectionEndpointUris("/connect/introspect")
        //.SetLogoutEndpointUris("/connect/logout");

        //options.AddEphemeralEncryptionKey()
        //      .AddEphemeralSigningKey();

        //options.AddDevelopmentEncryptionCertificate()
        //       .AddDevelopmentSigningCertificate(); 
        //options.AddEncryptionKey(new SymmetricSecurityKey(
        //    Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

        // Разрешение grant types (authorization code и refresh token)
        options.AllowPasswordFlow();
        //.AllowAuthorizationCodeFlow()
        //      .AllowRefreshTokenFlow()
        //      .AllowClientCredentialsFlow()
        //      .AllowPasswordFlow();

        //var basePath = AppDomain.CurrentDomain.BaseDirectory;
        //var privateKeyPath = Path.Combine(basePath, "private_key.pem");
        //    using var rsa = RSA.Create();
        //    options.AddSigningCredentials(new SigningCredentials(
        //new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256));

        // Add a custom event handler to remove the 'x5t' claim from the JWT header.
        //options.AddEventHandler<OpenIddictServerEvents.ProcessSignin>(
        //    builder => builder.UseInlineHandler(async context =>
        //    {
        //        var tokenDescriptor = context.TokenDescriptor;
        //        if (tokenDescriptor is JwtSecurityTokenDescriptor jwtDescriptor)
        //        {
        //            jwtDescriptor.Header.Remove("x5t");
        //        }
        //    }));
        //options.AddSigningKey(key);
        //options.AddEncryptionKey(key);
        //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY="));
        //options.AddSigningKey(key);
        // Load the RSA private key (PEM format)
        var rsa = RSA.Create();
        options.SetAccessTokenLifetime(TimeSpan.FromDays(1));
        // Set the signing credentials using the raw RSA key
        options.AddSigningKey(new RsaSecurityKey(rsa)
        {
            KeyId = "DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY="  // Optionally specify a KeyId (kid)
        });

        options.AddDevelopmentEncryptionCertificate()
         .AddDevelopmentSigningCertificate();
        options.DisableAccessTokenEncryption();

        // Включение токенов для двухфакторной аутентификации
        //options.RegisterScopes("openid", "profile", "email", "offline_access", "api");

        // Настройка сертификатов
        //options.AddDevelopmentEncryptionCertificate()
        //       .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough();
        //.EnableAuthorizationEndpointPassthrough();

        //.EnableLogoutEndpointPassthrough()
        //.EnableUserinfoEndpointPassthrough
        //options.UseReferenceAccessTokens();

    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
        //options.Configure(options => options.TokenValidationParameters.IssuerSigningKey =
        //     new SymmetricSecurityKey(
        //         Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));
    });
builder.Services.AddDistributedMemoryCache();
builder.Services.AddAuthentication(options =>
{
    // Устанавливаем схему аутентификации по умолчанию
    options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); 
app.MapAuthorizationEndpoints();
using (var scope = app.Services.CreateScope())
{
    await DatabaseInitializer.Seed(scope.ServiceProvider);
}
// Инициализация скоупов OpenIddict
await OpenIdDictScopeConfig.SeedScopes(app.Services);

// Инициализация клиентов OpenIddict
await OpenIdDictClientConfig.SeedClients(app.Services);

app.Run();
